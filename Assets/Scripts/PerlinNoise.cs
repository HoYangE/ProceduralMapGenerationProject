using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public float[,] GenerateMap(int width, int height, float scale, float octaves, float persistance, float lacunarity, float xOrg, float yOrg)
    {
        float[,] noiseMap = new float[width, height];
        //나중에 크기를 부모로 나눗셈을 하기 위해 최솟값 지정
        scale = Mathf.Max(0.0001f, scale);
        //최댓값 용
        float maxNoiseHeight = float.MinValue;
        //최솟값 용
        float minNoiseHeight = float.MaxValue;
        //노이즈 제작용 루프
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //진폭
                float amplitude = 1;
                //주파수
                float frequency = 1;
                //펄린 노이즈 * 진폭 값
                float noiseHeight = 0;

                //옥타브 처리용 루프
                for (int i = 0; i < octaves; i++)
                {
                    //xCoord는 x축에서 새로운 좌표를 나타내며, xOrg는 이전 위치를 나타내며, 
                    //x와 scale, frequency 값은 새로운 위치를 계산하는 데 사용되는 파라미터입니다.
                    float xCoord = xOrg + x / scale * frequency;
                    float yCoord = yOrg + y / scale * frequency;
                    //0~1 사이의 값을 반환하는 함수로 2를 곱하고 1을 빼서 -1~1 사이의 값으로 변환합니다.
                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                    //진폭이 파동의 높이를 나타내므로 펄린노이즈 값에 곱하여 최종 높이를 구합니다.
                    noiseHeight += perlinValue * amplitude;

                    //다음 옥타브로 진행할 때 진폭과 주파수의 변화값
                    //진폭에 지속성을 곱합니다. 지속성은 0~1로 관리되므로 진폭의 감소
                    amplitude *= persistance;
                    //주파수에 간결함을 곱합니다. 간결함은 양수로 관리되므로 주파수 증가
                    frequency *= lacunarity;
                }
                //예외사항 처리
                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                //텍스쳐를 만들기 위한 데이터 입력 단계
                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //텍스쳐를 만들기 위한 데이터 값 정규화 처리
                //lerp의 역함수로 최솟값과 최댓값의 사잇값을 넣으면 0~1사이의 값을 반환
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
