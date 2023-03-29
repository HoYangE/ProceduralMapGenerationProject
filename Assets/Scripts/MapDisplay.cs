using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material material;
    [SerializeField] private GameObject terrain;
    [SerializeField] private float antiGrayscale = 2.5f;


    [Range(0f, 1f)]
    public float[] fillPercents;
    public Color[] fillColors;

    public void DrawNoiseMap(float[,] noiseMap, float[,] gradientMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D noiseTex = new Texture2D(width, height);
        noiseTex.filterMode = FilterMode.Point;
        Color[] colorMap = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                colorMap[x * height + y] = CalcColor(noiseMap[x, y], gradientMap[x, y]);
            }
        }
        noiseTex.SetPixels(colorMap);
        noiseTex.Apply();

        spriteRenderer.sprite = Sprite.Create(noiseTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        material.SetTexture("_HeightMap", noiseTex);
        terrain.GetComponent<TerrainGenerator>().StartGenerator(width, height, noiseTex);

    }

    private Color CalcColor(float noiseValue, float gradientValue)
    {
        float value = noiseValue + gradientValue;
        value = Mathf.InverseLerp(0, antiGrayscale, value); //노이즈 맵과 그라디언트 맵을 더한 값을 0~1사이의 값으로 변환
        Color color = Color.Lerp(Color.black, Color.white, value); //변환된 값에 해당하는 색상을 그레이스케일로 저장
        
        return color;
    }
}
