using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiDiagram : MonoBehaviour
{
	public int cellWidth = 10;
	public int cellHeight = 10;
	public int textureWidth = 512;
	public int textureHeight = 512;

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

		float pointInGridX = Random.Range((numPointsWidth / cellWidth) * cellX, (numPointsWidth / cellWidth) * (cellX+1));
		float pointInGridY = Random.Range((numPointsHeight / cellHeight) * cellY, (numPointsHeight / cellHeight) * (cellY+1));

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
	
	public Texture2D GenerateVoronoiDiagram(int numPoints, int width, int height)
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

				foreach (Vector2 point in points)
				{
					float distance = Vector2.Distance(new Vector2(x, y), point);
					if (distance < minDistance)
					{
						minDistance = distance;
						closestPoint = point;
					}
				}

				texture.SetPixel(x, y, new Color(closestPoint.x / width, closestPoint.y / height, 0f));
			}
		}

		texture.Apply();

		return texture;
	}
}

