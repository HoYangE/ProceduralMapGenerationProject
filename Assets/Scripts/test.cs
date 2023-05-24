using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class test : MonoBehaviour
{
    private void Start()
    {
        Vector2Int size = new Vector2Int(512, 512);
        var riverSprite = Enumerable.Repeat(Color.white, 512 * 512).ToArray();

        //Vector2Int before = new Vector2Int(300, 300);
        //riverSprite = GradientRiver(before, riverSprite, size, 30);
        
        for (int i = 0; i < 512; i+=1)
        {
            Vector2Int before = new Vector2Int(i, i);
            riverSprite = GradientRiver(before, riverSprite, size, 30);
        }

        GetComponent<SpriteRenderer>().sprite = DrawSprite(size, riverSprite);
    }

    private Color[] GradientRiver(Vector2Int pos, Color[] currentColor, Vector2Int size, int radius)
    {
        Color[] pixelColors = currentColor;
        
        radius = Mathf.Max(1, radius);

        int minX = Mathf.Max(0, pos.x - radius);
        int maxX = Mathf.Min(size.x, pos.x + radius);
        int minY = Mathf.Max(0, pos.y - radius);
        int maxY = Mathf.Min(size.y, pos.y + radius);
        
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                if (x == pos.x && y == pos.y)
                {
                    pixelColors[y * size.x + x] = Color.black;
                    continue;
                }

                //중심으로부터 픽셀의 거리
                float distance = Vector2.Distance(new Vector2(x, y), pos);
                //거리를 반지름, 픽셀의 너비로 나누어 그라이언트를 생성
                float gradient = distance / ((maxX - minX));
                //0~1로 변환
                gradient = Mathf.Clamp01(gradient);

                //흰 바탕에 검은 그라이언트 생성
                float calcHeight = Mathf.Lerp(0.905f, 1, gradient);
                    
                if (!(calcHeight < 1)) continue;
                
                Color temp = pixelColors[y * size.x + x] * new Color(calcHeight, calcHeight, calcHeight);
                pixelColors[y * size.x + x] = temp;
            }
        }
        return pixelColors;
    }
    
    
    private Sprite DrawSprite(Vector2Int size, Color[] colorData)
    {
        var texture = new Texture2D(size.x, size.y) { filterMode = FilterMode.Point };
        texture.SetPixels(colorData);
        texture.Apply();

        var rect = new Rect(0, 0, size.x, size.y);
        var sprite = Sprite.Create(texture, rect, Vector2.one * 0.5f);
        return sprite;
    }
}
