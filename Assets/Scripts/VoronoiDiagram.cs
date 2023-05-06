using System;
using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

internal abstract class MapDrawer
{
    public static Sprite DrawVoronoiToSprite(Voronoi voronoi)
    {
        // 텍스쳐 픽셀 하나하나의 색을 담고있는 배열입니다.
        var rect = voronoi.PlotBounds;
        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);
        var pixelColors = Enumerable.Repeat(Color.white, width * height).ToArray();
        var points = voronoi.SiteCoords();

        // 무게중심 그리기
        foreach (var point in points)
        {
            var x = Mathf.RoundToInt(point.x);
            var y = Mathf.RoundToInt(point.y);

            var pointIndex = (y * width) + x;
            pixelColors[pointIndex] = Color.red;
        }

        // 원하는 셀 색칠하기
        //pixelColors = CellColor(new Vector2Int(400, 100), pixelColors, voronoi,rect, width, height);

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

                // 텍스쳐의 모서리를 처리한다.
                if (edge.ClippedVertices is null) continue;

                // 가장자리를 이루는 모서리 정점(vertex) 2개를 얻어온다.
                var corner1 = edge.ClippedVertices[LR.LEFT];
                var corner2 = edge.ClippedVertices[LR.RIGHT];

                // 1차 함수의 그래프를 그리듯이 텍스쳐에 가장자리 선분을 그린다.
                var targetPoint = corner1;
                var delta = 1 / (corner2 - corner1).magnitude;
                var lerpRatio = 0f;

                while ((int)targetPoint.x != (int)corner2.x || (int)targetPoint.y != (int)corner2.y)
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

    private static Sprite DrawSprite(Vector2Int size, Color[] colorData)
    {
        var texture = new Texture2D(size.x, size.y) { filterMode = FilterMode.Point };
        texture.SetPixels(colorData);
        texture.Apply();

        var rect = new Rect(0, 0, size.x, size.y);
        var sprite = Sprite.Create(texture, rect, Vector2.one * 0.5f);
        return sprite;
    }

    private static Color[] CellColor(Vector2Int point, Color[] currentColor, Voronoi voronoi, Rect rect, int width,int height)
    {
        var pixelColors = currentColor;
        var cellIndex = (point.y * width) + point.x;
        pixelColors[cellIndex] = Color.yellow;
        int target = 0;
        float maxDistance = float.MaxValue;
        // 가장 가까운 무게중심 찾기
        for (int i = 0; i < voronoi.Sites.Count; i++)
        {
            float distance = Vector2.Distance(point, voronoi.Sites[i].Coord);
            if (distance < maxDistance)
            {
                maxDistance = distance;
                target = i;
            }
        }
        // 가장 가까운 무게중심의 셀의 모서리 픽셀들을 구함
        var positions = voronoi.Sites[target].Region(rect);
        pixelColors = FillPolygonWithColor(positions, Color.cyan, pixelColors, new Vector2Int(width, height));
        return pixelColors;
    }
    
    private static Color[] FillPolygonWithColor(List<Vector2> vertices, Color fillColor, Color[] currentColor, Vector2Int size)
    {
        // 다각형의 경계를 둘러싸는 바운딩 박스를 계산
        float minX = Mathf.Floor(vertices.Min(v => v.x));
        float maxX = Mathf.Ceil(vertices.Max(v => v.x));
        float minY = Mathf.Floor(vertices.Min(v => v.y));
        float maxY = Mathf.Ceil(vertices.Max(v => v.y));

        // 각 픽셀에 대해 다각형 내부에 위치한지 검사하여 색상을 변경
        for (int x = (int)minX; x < (int)maxX; x++)
        {
            for (int y = (int)minY; y < (int)maxY; y++)
            {
                // 중심점 픽셀 좌표
                Vector2 testPoint = new Vector2(x + 0.5f, y + 0.5f);
                int index = y * size.x + x;
                if (!IsPointInPolygon(testPoint, vertices)) continue;
                if(currentColor[index] == Color.white)
                    currentColor[index] = fillColor;
            }
        }

        return currentColor;
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
                isInside = !isInside;
        }

        return isInside;
    }
}


public class VoronoiDiagram : MonoBehaviour
{
    [SerializeField]
    private Vector2Int size;
    [SerializeField] 
    private int nodeAmount = 0;
    [SerializeField]
    private int lloydIterationCount = 0;

    public Tuple<float[,],Voronoi> StartGenerateVoronoiDiagram()
    {
        var voronoi = GenerateVoronoi(size, nodeAmount, lloydIterationCount);
        var sprite = MapDrawer.DrawVoronoiToSprite(voronoi);
        GetComponent<SpriteRenderer>().sprite = sprite;
        
        Texture2D texture = sprite.texture;
        Color32[] colors = texture.GetPixels32();
        float[,] floatArray = new float[texture.width, texture.height];

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color32 color = colors[x + y * texture.width];
                // 픽셀 값을 0과 1 사이의 값으로 변환
                floatArray[x, y] = (color.r + color.g + color.b) / 765.0f;
            }
        }
        return new Tuple<float[,], Voronoi>(floatArray, voronoi);
    }

    private Voronoi GenerateVoronoi(Vector2Int vector2Int, int amount, int count)
    {
        var point = new List<Vector2>();

        // 무게 중심을 nodeAmount만큼 생성
        for (var i = 0; i < amount; ++i)
        {
            var x = Random.Range(0, vector2Int.x);
            var y = Random.Range(0, vector2Int.y);

            point.Add(new Vector2(x, y));
        }

        var rect = new Rect(0f, 0f, vector2Int.x, vector2Int.y);
        var voronoi = new Voronoi(point, rect, count);
        return voronoi;
    }
}
