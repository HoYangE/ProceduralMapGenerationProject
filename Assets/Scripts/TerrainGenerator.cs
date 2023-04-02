using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        TerrainData terrainData = new TerrainData();
        if(terrain == null)
            terrain = gameObject.AddComponent<Terrain>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
        }
        terrainCollider.terrainData = terrainData;

        gameObject.GetComponent<Terrain>().terrainData = terrainData;

        terrain.treeBillboardDistance = 1500;
        terrainData.heightmapResolution = heightmapTexture.width;
        terrainData.size = new Vector3(width, maxHeight, height);
        SetTerrainHeight(terrainData, heightmapTexture);

        terrain.materialTemplate = material;
        terrain.Flush();

        terrain.transform.position = Vector3.zero;

        Tree();
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

    public GameObject treePrefab; // assign the prefab in the Inspector

    void Tree()
    {
        for (int i = 0; i < 100; i++)
        {
            // generate random position within the terrain bounds
            float x = Random.Range(0f, terrain.terrainData.size.x);
            float z = Random.Range(0f, terrain.terrainData.size.z);
            Vector3 position = new Vector3(x, 0f, z);

            // get the terrain's height at the position
            float height = terrain.SampleHeight(position);

            // create a new tree instance at the position with the prefab
            TreeInstance treeInstance = new TreeInstance();
            // get the normalized position on the terrain
            Vector3 normalizedPosition = new Vector3(
                position.x / terrain.terrainData.size.x,
                0f,
                position.z / terrain.terrainData.size.z
            );

            // set the tree instance position
            treeInstance.position = normalizedPosition;

            // get the normalized height on the terrain
            float normalizedHeight = terrain.SampleHeight(position) / terrain.terrainData.size.y;

            // set the tree instance height
            treeInstance.position.y = normalizedHeight;
            treeInstance.widthScale = 1f;
            treeInstance.heightScale = 1f;
            treeInstance.color = Color.white;
            treeInstance.lightmapColor = Color.white;

            // apply the prefab to the tree instance
            treeInstance.prototypeIndex = 0;
            terrain.terrainData.treePrototypes = terrain.terrainData.treePrototypes
                .Concat(new TreePrototype[] { new TreePrototype() { prefab = treePrefab } }).ToArray();

            // add the tree instance to the terrain
            terrain.AddTreeInstance(treeInstance);
        }
    }
}
