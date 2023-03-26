using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public float[,] GenerateMap(int width, int height, float scale, float octaves, float persistance, float lacunarity, float xOrg, float yOrg)
    {
        float[,] noiseMap = new float[width, height];
        //���߿� ũ�⸦ �θ�� �������� �ϱ� ���� �ּڰ� ����
        scale = Mathf.Max(0.0001f, scale);
        //�ִ� ��
        float maxNoiseHeight = float.MinValue;
        //�ּڰ� ��
        float minNoiseHeight = float.MaxValue;
        //������ ���ۿ� ����
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //����
                float amplitude = 1;
                //���ļ�
                float frequency = 1;
                //�޸� ������ * ���� ��
                float noiseHeight = 0;

                //��Ÿ�� ó���� ����
                for (int i = 0; i < octaves; i++)
                {
                    //xCoord�� x�࿡�� ���ο� ��ǥ�� ��Ÿ����, xOrg�� ���� ��ġ�� ��Ÿ����, 
                    //x�� scale, frequency ���� ���ο� ��ġ�� ����ϴ� �� ���Ǵ� �Ķ�����Դϴ�.
                    float xCoord = xOrg + x / scale * frequency;
                    float yCoord = yOrg + y / scale * frequency;
                    //0~1 ������ ���� ��ȯ�ϴ� �Լ��� 2�� ���ϰ� 1�� ���� -1~1 ������ ������ ��ȯ�մϴ�.
                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                    //������ �ĵ��� ���̸� ��Ÿ���Ƿ� �޸������� ���� ���Ͽ� ���� ���̸� ���մϴ�.
                    noiseHeight += perlinValue * amplitude;

                    //���� ��Ÿ��� ������ �� ������ ���ļ��� ��ȭ��
                    //������ ���Ӽ��� ���մϴ�. ���Ӽ��� 0~1�� �����ǹǷ� ������ ����
                    amplitude *= persistance;
                    //���ļ��� �������� ���մϴ�. �������� ����� �����ǹǷ� ���ļ� ����
                    frequency *= lacunarity;
                }
                //���ܻ��� ó��
                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                //�ؽ��ĸ� ����� ���� ������ �Է� �ܰ�
                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //�ؽ��ĸ� ����� ���� ������ �� ����ȭ ó��
                //lerp�� ���Լ��� �ּڰ��� �ִ��� ���հ��� ������ 0~1������ ���� ��ȯ
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
