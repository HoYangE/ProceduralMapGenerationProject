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
        // Create a new terrain object
        TerrainData terrainData = new TerrainData();
        if(terrain == null)
            terrain = gameObject.AddComponent<Terrain>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
            terrainCollider.terrainData = terrainData;
        }
        gameObject.GetComponent<Terrain>().terrainData = terrainData;

        // Set the terrain's heightmap from the texture
        terrainData.heightmapResolution = heightmapTexture.width;
        terrainData.size = new Vector3(width, maxHeight, height); // set the size of the terrain to suit your needs
        SetTerrainHeight(terrainData, heightmapTexture);

        // Set other properties of the terrain as needed       
        terrain.materialTemplate = material;
        terrain.Flush(); // save the terrain changes

        // Add the terrain to the scene
        terrain.transform.position = Vector3.zero;
    }

    void SetTerrainHeight(TerrainData terrainData, Texture2D heightmapTexture)
    {
        int width = heightmapTexture.width;
        int height = heightmapTexture.height;
        float[,] heights = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Get the grayscale color value of the pixel in the texture
                float heightValue = heightmapTexture.GetPixel(x, y).grayscale;

                // Normalize the height value to be between 0 and 1
                heightValue /= 255;

                // Set the corresponding height in the terrain data
                heights[x, y] = heightValue * maxHeight;
            }
        }

        // Apply the heights to the terrain data
        terrainData.SetHeights(0, 0, heights);
    }
}
