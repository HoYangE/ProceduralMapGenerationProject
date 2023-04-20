using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

class MapDrawer
{
    public static Sprite DrawVoronoiToSprite(Voronoi voronoi)
    {
        // 텍스쳐 픽셀 하나하나의 색을 담고있는 배열입니다.
        // 애석하게도, Texture2D의 픽셀 정보는 1차원 배열입니다.
        var rect = voronoi.PlotBounds;
        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);
        var pixelColors = Enumerable.Repeat(Color.white, width * height).ToArray();
        var siteCoords = voronoi.SiteCoords();

        // 무게중심 그리기
        foreach (var coord in siteCoords)
        {
            var x = Mathf.RoundToInt(coord.x);
            var y = Mathf.RoundToInt(coord.y);

            var index = (y * width) + x;
            pixelColors[index] = Color.red;
        }
        
        // 원하는 셀 색칠하기
        Vector2Int test = new Vector2Int(400, 100);
        var index2 = (test.y * width) + test.x;
        pixelColors[index2] = Color.yellow;
        int target = 0;
        float max_distance = float.MaxValue;
        for (int i = 0; i < voronoi.Sites.Count; i++)
        {
            float distance = Vector2.Distance(test, voronoi.Sites[i].Coord);
            if (distance < max_distance)
            {
                max_distance = distance;
                target = i;
            }
        }
        var positions = voronoi.Sites[target].Region(rect);
        pixelColors = FillPolygonWithColor(positions, Color.cyan, pixelColors, new Vector2Int(width, height));

        // 모서리 그리기
        // 먼저 모든 폴리곤의 정보를 얻어온다.
        foreach (var site in voronoi.Sites)
        {
            // 그 폴리곤의 모든 이웃 폴리곤을 얻어온다.
            var neighbors = site.NeighborSites();
            foreach (var neighbor in neighbors)
            {
                // 이웃한 폴리곤들에게서 겹치는 가장자리(edge)를 유도해낸다.
                var edge = voronoi.FindEdgeFromAdjacentPolygons(site, neighbor);

                // 뭔진 밑에서 설명해드리겠습니다.
                if (edge.ClippedVertices is null)
                {
                    continue;
                }

                // 가장자리를 이루는 모서리 정점(vertex) 2개를 얻어온다.
                var corner1 = edge.ClippedVertices[LR.LEFT];
                var corner2 = edge.ClippedVertices[LR.RIGHT];

                // 1차 함수의 그래프를 그리듯이 텍스쳐에 가장자리 선분을 그린다.
                var targetPoint = corner1;
                var delta = 1 / (corner2 - corner1).magnitude;
                var lerpRatio = 0f;

                while ((int)targetPoint.x != (int)corner2.x ||
                    (int)targetPoint.y != (int)corner2.y)
                {
                    // 선형 보간을 통해 corner1과 corner2 사이를 lerpRatio만큼 나누는 점을 얻어온다.
                    targetPoint = Vector2.Lerp(corner1, corner2, lerpRatio);
                    lerpRatio += delta;

                    // 텍스쳐의 좌표 영역은 (0 ~ size.x - 1)이지만,
                    // 생성한 보로노이 다이어그램의 좌표 영역은 (0 ~ (float) size.x)이다.
                    var x = Mathf.Clamp((int)targetPoint.x, 0, width - 1);
                    var y = Mathf.Clamp((int)targetPoint.y, 0, height - 1);

                    var index = (y * width) + x;
                    pixelColors[index] = Color.black;
                }
            }
        }        

        // 텍스쳐화 시키고 스프라이트로 만들기
        var size = new Vector2Int(width, height);
        return DrawSprite(size, pixelColors);
    }

    public static Sprite DrawSprite(Vector2Int size, Color[] colorDatas)
    {
        var texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorDatas);
        texture.Apply();

        var rect = new Rect(0, 0, size.x, size.y);
        var sprite = Sprite.Create(texture, rect, Vector2.one * 0.5f);
        return sprite;
    }

    public static Color[] FillPolygonWithColor(List<Vector2> vertices, Color fillColor, Color[] currentColor, Vector2Int size)
    {
        // 다각형의 경계를 둘러싸는 바운딩 박스를 계산
        float minX = Mathf.Floor(vertices.Min(v => v.x));
        float maxX = Mathf.Ceil(vertices.Max(v => v.x));
        float minY = Mathf.Floor(vertices.Min(v => v.y));
        float maxY = Mathf.Ceil(vertices.Max(v => v.y));
        Color[] colors = currentColor;

        // 각 픽셀에 대해 다각형 내부에 위치한지 검사하여 색상을 변경
        for (int x = (int)minX; x < (int)maxX; x++)
        {
            for (int y = (int)minY; y < (int)maxY; y++)
            {
                Vector2 testPoint = new Vector2(x + 0.5f, y + 0.5f); // 중심점 픽셀 좌표
                int index = y * size.x + x;
                if (IsPointInPolygon(testPoint, vertices))
                {
                    if(colors[index] == Color.white)
                        colors[index] = fillColor;
                }
                else
                {
                    colors[index] = currentColor[index];
                }
            }
        }

        return colors;
    }

    // 점이 다각형 내부에 있는지 검사하는 함수
    private static bool IsPointInPolygon(Vector2 point, List<Vector2> polygonVertices)
    {
        int polygonVertexCount = polygonVertices.Count;
        bool isInside = false;

        for (int i = 0, j = polygonVertexCount - 1; i < polygonVertexCount; j = i++)
        {
            if ((polygonVertices[i].y > point.y) != (polygonVertices[j].y > point.y) &&
                (point.x < (polygonVertices[j].x - polygonVertices[i].x) * (point.y - polygonVertices[i].y) /
                          (polygonVertices[j].y - polygonVertices[i].y) + polygonVertices[i].x))
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }
}


public class VoronoiDiagram : MonoBehaviour
{
    /*
    [SerializeField]
    private int cellWidth = 10;
    [SerializeField]
    private int cellHeight = 10;
    [SerializeField]
    private int textureWidth = 512;
    [SerializeField]
    private int textureHeight = 512;

    public void StartGenerateVoronoiDiagram()
    {
        GetComponent<SpriteRenderer>().sprite = Sprite.Create(GenerateVoronoiDiagram(cellWidth * cellHeight, textureWidth, textureHeight),
            new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));
    }

    // 그리드 안에서 랜덤한 위치에 점을 생성하는 함수
    private Vector2 RandomPointInGrid(int numPointsWidth, int numPointsHeight, int num)
    {
        int cellX = num % cellWidth;
        int cellY = num / cellWidth;

        //그리드를 이용하여 점 랜덤 구하기
        float pointInGridX = Random.Range((numPointsWidth / cellWidth) * cellX, (numPointsWidth / cellWidth) * (cellX + 1));
        float pointInGridY = Random.Range((numPointsHeight / cellHeight) * cellY, (numPointsHeight / cellHeight) * (cellY + 1));

        //완전 랜덤
        //float pointInGridX = Random.Range(0, numPointsWidth);
        //float pointInGridY = Random.Range(0,numPointsHeight);

        return new Vector2(pointInGridX, pointInGridY);
    }

    // 그리드 안에 랜덤한 개수의 점들을 생성하는 함수
    private List<Vector2> GenerateRandomPointsInGrid(int numPointsWidth, int numPointsHeight, int numPoints)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < numPoints; i++)
        {
            points.Add(RandomPointInGrid(numPointsWidth, numPointsHeight, i));
        }
        return points;
    }

    private Texture2D GenerateVoronoiDiagram(int numPoints, int width, int height)
    {
        //랜덤 포인트 구하기
        List<Vector2> points = GenerateRandomPointsInGrid(width, height, numPoints);

        Texture2D texture = new Texture2D(width, height);

        //보로노이 계산
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float minDistance = float.MaxValue;
                Vector2 closestPoint = Vector2.zero;

                //가까운 점 찾기
                foreach (Vector2 point in points)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), point);
                    if (distance > minDistance)
                    {
                        minDistance = distance;
                        closestPoint = point;
                    }
                }

                texture.SetPixel(x, y, new Color(closestPoint.x / width, closestPoint.y / height, 0f));
            }
        }


        foreach (Vector2 point in points)
        {
            texture.SetPixel((int)point.x, (int)point.y, new Color(0, 0, 1));
        }


        texture.Apply();
        return texture;
    }
    */

    /*
    public Vector2Int imageDim;
    public int regionAmount;
    public bool drawByDistance = false;
    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = Sprite.Create((drawByDistance ? GetDiagramByDistance() : GetDiagram()), 
            new Rect(0, 0, imageDim.x, imageDim.y), Vector2.one * 0.5f);
    }
    Texture2D GetDiagram()
    {
        Vector2Int[] centroids = new Vector2Int[regionAmount];
        Color[] regions = new Color[regionAmount];
        for (int i = 0; i < regionAmount; i++)
        {
            centroids[i] = new Vector2Int(Random.Range(0, imageDim.x), Random.Range(0, imageDim.y));
            regions[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
        Color[] pixelColors = new Color[imageDim.x * imageDim.y];
        for (int x = 0; x < imageDim.x; x++)
        {
            for (int y = 0; y < imageDim.y; y++)
            {
                int index = x * imageDim.x + y;
                pixelColors[index] = regions[GetClosestCentroidIndex(new Vector2Int(x, y), centroids)];
            }
        }
        return GetImageFromColorArray(pixelColors);
    }
    Texture2D GetDiagramByDistance()
    {
        Vector2Int[] centroids = new Vector2Int[regionAmount];

        for (int i = 0; i < regionAmount; i++)
        {
            centroids[i] = new Vector2Int(Random.Range(0, imageDim.x), Random.Range(0, imageDim.y));
        }
        Color[] pixelColors = new Color[imageDim.x * imageDim.y];
        float[] distances = new float[imageDim.x * imageDim.y];

        //you can get the max distance in the same pass as you calculate the distances. :P oops!
        float maxDst = float.MinValue;
        for (int x = 0; x < imageDim.x; x++)
        {
            for (int y = 0; y < imageDim.y; y++)
            {
                int index = x * imageDim.x + y;
                distances[index] = Vector2.Distance(new Vector2Int(x, y), 
                    centroids[GetClosestCentroidIndex(new Vector2Int(x, y), centroids)]);
                if (distances[index] > maxDst)
                {
                    maxDst = distances[index];
                }
            }
        }

        for (int i = 0; i < distances.Length; i++)
        {
            float colorValue = distances[i] / maxDst;
            pixelColors[i] = new Color(colorValue, colorValue, colorValue, 1f);
        }
        return GetImageFromColorArray(pixelColors);
    }
   
    int GetClosestCentroidIndex(Vector2Int pixelPos, Vector2Int[] centroids)
    {
        float smallestDst = float.MaxValue;
        int index = 0;
        for (int i = 0; i < centroids.Length; i++)
        {
            if (Vector2.Distance(pixelPos, centroids[i]) < smallestDst)
            {
                smallestDst = Vector2.Distance(pixelPos, centroids[i]);
                index = i;
            }
        }
        return index;
    }
    Texture2D GetImageFromColorArray(Color[] pixelColors)
    {
        Texture2D tex = new Texture2D(imageDim.x, imageDim.y);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixelColors);
        tex.Apply();
        return tex;
    }
    */


    [SerializeField]
    private Vector2Int size;
    [SerializeField] 
    private int nodeAmount = 0;
    [SerializeField]
    private int lloydIterationCount = 0;

    private void Awake()
    {
        var voronoi = GenerateVoronoi(size, nodeAmount, lloydIterationCount);
        GetComponent<SpriteRenderer>().sprite = MapDrawer.DrawVoronoiToSprite(voronoi);
    }

    private Voronoi GenerateVoronoi(Vector2Int size, int nodeAmount, int lloydIterationCount)
    {
        var centroids = new List<Vector2>();

        // 무게 중심을 nodeAmount만큼 생성
        for (var i = 0; i < nodeAmount; ++i)
        {
            var x = Random.Range(0, size.x);
            var y = Random.Range(0, size.y);

            centroids.Add(new Vector2(x, y));
        }

        var Rect = new Rect(0f, 0f, size.x, size.y);
        var voronoi = new Voronoi(centroids, Rect, lloydIterationCount);
        return voronoi;
    }
}
