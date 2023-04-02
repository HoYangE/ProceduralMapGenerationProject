using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gradient : MonoBehaviour
{
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float lux = 0.5f;

    public float[,] GenerateMap(int width, int height)
    {
        float[,] heightMap = new float[width, height];
        radius = Mathf.Max(0.0001f, radius);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //중심으로부터 픽셀의 거리
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));
                //거리를 반지름, 픽셀의 너비로 나누어 그라이언트를 생성            
                float gradient = distance / (width * radius);
                //0~1로 변환
                gradient = Mathf.Clamp01(gradient);

                //검은 바탕에 흰 그라이언트 생성
                float calcHeight = 1 - Mathf.Lerp(lux * -1, 1, gradient);
                heightMap[x, y] = calcHeight;
            }
        }

        return heightMap;
    }
}
