using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material material;

    [Range(0f, 1f)]
    public float[] fillPercents;
    public Color[] fillColors;

    public void DrawNoiseMap(float[,] noiseMap, float[,] gradientMap, bool useColorMap)
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
                colorMap[x * height + y] = CalcColor(noiseMap[x, y], gradientMap[x, y], useColorMap);
            }
        }
        noiseTex.SetPixels(colorMap);
        noiseTex.Apply();

        spriteRenderer.sprite = Sprite.Create(noiseTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        material.SetTexture("_HeightMap", noiseTex);
    }

    private Color CalcColor(float noiseValue, float gradientValue, bool useColorMap)
    {
        float value = noiseValue + gradientValue;
        value = Mathf.InverseLerp(0, 2, value); //������ �ʰ� �׶���Ʈ ���� ���� ���� 0~1������ ������ ��ȯ
        Color color = Color.Lerp(Color.black, Color.white, value); //��ȯ�� ���� �ش��ϴ� ������ �׷��̽����Ϸ� ����
        if (useColorMap)
        {
            for (int i = 0; i < fillPercents.Length; i++)
            {
                if (color.grayscale < fillPercents[i])
                {
                    color = fillColors[i]; //�̸� ������ ���� ������ ���� ���� ��ȯ
                    break;
                }
            }
        }
        return color;
    }
}
