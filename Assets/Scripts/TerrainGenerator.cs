using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Material material;
    public int maxHeight = 100;

    private Texture2D heightmapTexture;
    private Terrain terrain;
    private TerrainCollider terrainCollider;

    public void StartGenerator(int width, int height, Texture2D texture2D) 
    {
        heightmapTexture = texture2D;
        //터레인 데이터 생성
        TerrainData terrainData = new TerrainData();
        if(terrain == null)
            terrain = gameObject.AddComponent<Terrain>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
        }
        terrainCollider.terrainData = terrainData;

        gameObject.GetComponent<Terrain>().terrainData = terrainData;

        terrainData.heightmapResolution = heightmapTexture.width;
        terrainData.size = new Vector3(width, maxHeight, height);
        SetTerrainHeight(terrainData, heightmapTexture);

        terrain.materialTemplate = material;
        terrain.Flush();

        terrain.transform.position = Vector3.zero;
    }

    void SetTerrainHeight(TerrainData terrainData, Texture2D heightmapTexture)
    {
        int width = heightmapTexture.width;
        int height = heightmapTexture.height;
        float[,] heights = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float heightValue = heightmapTexture.GetPixel(y, x).grayscale;
                heightValue /= 255;
                heights[x, y] = heightValue * maxHeight;
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }
}
