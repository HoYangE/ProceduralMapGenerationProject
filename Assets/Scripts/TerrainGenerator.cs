using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private GameObject waterPlane;
    [SerializeField] private Material material;
    [SerializeField] private int maxHeight = 100;
    [SerializeField] private float waterHeight = 0.0f;

    private Texture2D heightmapTexture;
    private Texture2D waterTexture;
    private Terrain terrain;
    private TerrainCollider terrainCollider;

    public void StartGenerator(int width, int height, Texture2D heightTexture2D, Texture2D waterTexture2D) 
    {
        heightmapTexture = heightTexture2D;
        waterTexture = waterTexture2D;
        
        TerrainData terrainData = new TerrainData();
        
        //오브젝트를 재활용 할것이니 컴포넌트가 없으면 추가해준다.
        if(terrain == null)
            terrain = gameObject.AddComponent<Terrain>();
        
        //콜라이더 추가 및 데이터 적용
        if (terrainCollider == null)
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
        
        terrainCollider.terrainData = terrainData;

        gameObject.GetComponent<Terrain>().terrainData = terrainData;

        terrain.treeBillboardDistance = 2000;
        terrain.heightmapPixelError = 15;
        
        //터레인의 최대 높이를 지정
        terrainData.heightmapResolution = heightmapTexture.width;
        terrainData.size = new Vector3(width, maxHeight, height);
        SetTerrainHeight(terrainData, heightmapTexture);

        //원하는 머테리얼 적용
        terrain.materialTemplate = material;
        terrain.Flush();

        terrain.transform.position = Vector3.zero;

        TreeGenerator();

        waterPlane.transform.position = new Vector3(width / 2.0f,  terrain.GetComponent<Terrain>().terrainData.size.y * waterHeight, height / 2.0f);
    }

    void SetTerrainHeight(TerrainData terrainData, Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        float[,] heights = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //픽셀의 색의 값을 0~255로 만들기
                //텍스쳐의 좌표계와 터레인의 좌표계가 다른 문제가 있어 y, x로 적용
                float heightValue = texture.GetPixel(y, x).grayscale;
                //255로 나눠서 높이값을 구하기
                heightValue /= 255;
                //원하는 터레인의 버텍스 높이위치를 계산
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
        treeInstance.widthScale = 20;
        treeInstance.heightScale = 20;
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
            
            float water = waterTexture.GetPixel((int)position.x, (int)position.z).b;
            //Trees에서 몇번째 나무를 사용할지
            if (normalizedPosition.y - blendMidHighErrorRange > 1 - terrain.materialTemplate.GetFloat("_BlendMidHigh"))
            {
                //가장 높은 곳
                if (water > 0)
                {
                    if (water < 0.5f)
                    {
                        //파란꽃
                        treeInstance.prototypeIndex = 1;
                    }
                    else
                    {
                        //빨간꽃
                        treeInstance.prototypeIndex = 0;
                    }
                }
                else
                {
                    //돌
                    treeInstance.prototypeIndex = Random.Range(7, 12);
                }
            }
            else if (normalizedPosition.y + blendLowMidErrorRange < terrain.materialTemplate.GetFloat("_BlendLowMid"))
            {
                //가장 낮은 곳
                //돌~버섯
                treeInstance.prototypeIndex = Random.Range(7,14);
            }
            else
            {
                //중간 높은 곳
                if (water > 0)
                {
                    if (waterTexture.GetPixel((int)position.x, (int)position.z).b < 0.3f)
                    {
                        //나무
                        treeInstance.prototypeIndex = Random.Range(2, 7);
                    }
                    else
                    {
                        //나무~돌
                        treeInstance.prototypeIndex = Random.Range(2, 12);
                    }
                }
                else
                {
                    //돌
                    treeInstance.prototypeIndex = Random.Range(7, 12);
                }
            }

            //추가
            terrain.AddTreeInstance(treeInstance);
        }
    }
}
