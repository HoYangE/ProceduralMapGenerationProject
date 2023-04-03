using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private int maxHeight = 100;

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

        TreeGenerator();
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

    [SerializeField] private GameObject[] treePrefab;
    [SerializeField] private int treeCount;
    [SerializeField] private float blendLowMidErrorRange;
    [SerializeField] private float blendMidHighErrorRange;

    void TreeGenerator()
    {
        //나무 구조체
        TreeInstance treeInstance = new TreeInstance();

        //프리팹을 이용하여 Trees에 추가
        for(int i = 0; i < treePrefab.Length; i++)
        {
            terrain.terrainData.treePrototypes = terrain.terrainData.treePrototypes
            .Concat(new TreePrototype[] { new TreePrototype() { prefab = treePrefab[i] } })
            .ToArray();
        }   

        //Scale이나 Color를 조절할 수 있음
        treeInstance.widthScale = 100;
        treeInstance.heightScale = 100;
        treeInstance.color = Color.white;
        treeInstance.lightmapColor = Color.white;

        for (int i = 0; i < treeCount; i++)
        {
            //랜덤으로 위치 잡기
            Vector3 position = new Vector3(
                Random.Range(0f, terrain.terrainData.size.x),
                0f,
                Random.Range(0f, terrain.terrainData.size.z));

            //위치를 노말라이즈 시키기
            Vector3 normalizedPosition = new Vector3(
                position.x / terrain.terrainData.size.x,
                terrain.SampleHeight(position) / terrain.terrainData.size.y,
                position.z / terrain.terrainData.size.z);

            //위치를 노말라이즈 시킨 값으로 설정
            treeInstance.position = normalizedPosition;         

            //Trees에서 몇번째 나무를 사용할지
            if (normalizedPosition.y - blendMidHighErrorRange > 1 - terrain.materialTemplate.GetFloat("_BlendMidHigh"))
                treeInstance.prototypeIndex = 0;
            else if(normalizedPosition.y + blendLowMidErrorRange < terrain.materialTemplate.GetFloat("_BlendLowMid"))
                treeInstance.prototypeIndex = 1;
            else
                treeInstance.prototypeIndex = 2;

            //추가
            terrain.AddTreeInstance(treeInstance);
        }
    }
}
