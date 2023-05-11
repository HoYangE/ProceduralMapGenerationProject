using System;
using System.Collections;
using System.Collections.Generic;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer newVoronoiSpriteRenderer;
    [SerializeField] private Material material;
    [SerializeField] private GameObject terrain;
    [SerializeField] private float antiGrayscale = 2.5f;
    [SerializeField] private float waterHeight = 0.0f;
    [SerializeField] private int riverStartPoint = 100;
    [SerializeField] private int riverLength = 30;

    public void DrawNoiseMap(float[,] noiseMap, float[,] gradientMap, Tuple<float[,],Voronoi> voronoiDiagram)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        //스프라이트 이미지를 만들기 위해 텍스쳐를 만든다.
        Texture2D noiseTex = new Texture2D(width, height);
        //텍스쳐를 포인트로 채우려고 한다.
        noiseTex.filterMode = FilterMode.Point;
        
        Color[] colorMap = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //모든 픽셀에 대해 연산 진행
                colorMap[x * height + y] = CalcColor(noiseMap[x, y], gradientMap[x, y]);
            }
        }

        WaterLayer(voronoiDiagram, colorMap, width, height);

        //colorMap을 이용하여 텍스쳐 제작
        noiseTex.SetPixels(colorMap);
        noiseTex.Apply();
        
        //텍스쳐를 기반으로 스프라이트 생성
        spriteRenderer.sprite = Sprite.Create(noiseTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        material.SetTexture("_HeightMap", noiseTex);
        StartCoroutine(TerrainCoroutine(width, height, noiseTex));
    }

    private Color CalcColor(float noiseValue, float gradientValue)
    {
        float value = noiseValue + gradientValue;
        //노이즈 맵과 그라디언트 맵을 더한 값을 0~1사이의 값으로 변환
        value = Mathf.InverseLerp(0, antiGrayscale, value);
        //변환된 값에 해당하는 색상을 그레이스케일로 저장
        Color color = Color.Lerp(Color.black, Color.white, value);
        
        return color;
    }

    private void WaterLayer(Tuple<float[,],Voronoi> voronoiDiagram, Color[] colorMap, int width, int height)
    {
        //물 높이 이하의 보로노이는 파랑으로 채운다.
        var points = new List<Vector2>();
        foreach (var site in voronoiDiagram.Item2.Sites)
        {
            if (colorMap[(int)site.Coord.y * height + (int)site.Coord.x].r < waterHeight)
            {
                points.Add(new Vector2Int((int)site.Coord.x, (int)site.Coord.y));
            }
        }
        
        //습도 보로노이 스프라이트 생성
        newVoronoiSpriteRenderer.sprite = MapDrawer.DrawRiverToSprite(MapDrawer.DrawVoronoiToSprite(voronoiDiagram.Item2, points), 
            voronoiDiagram.Item2, colorMap, riverStartPoint, riverLength);
    }
    
    IEnumerator TerrainCoroutine(int width, int height, Texture2D noiseTex)
    {
        Debug.Log("Terrain Start : " + Time.realtimeSinceStartup);
        yield return null;
        terrain.GetComponent<TerrainGenerator>().StartGenerator(width, height, noiseTex);
        Debug.Log("Terrain End : " + Time.realtimeSinceStartup);
    }
}
