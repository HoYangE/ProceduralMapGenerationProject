using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGeneratorByPerlinNoise : MonoBehaviour
{
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;

    [SerializeField] private float scale = 1.0f;
    [SerializeField] private int octaves = 3;
    [SerializeField] private float persistance = 0.5f;
    [SerializeField] private float lacunarity = 2;

    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;
    [SerializeField] private bool useGradientMap;    

    private float xOrg = 0;
    private float yOrg = 0;    

    [SerializeField] private PerlinNoise perlinNoise;
    [SerializeField] private Gradient gradient;
    [SerializeField] private MapDisplay mapDisplay;

    private void Start()
    {
        //시드를 만들고 위치를 구한다.
        int temp = Time.realtimeSinceStartup.ToString().GetHashCode();
        if (useRandomSeed) seed = temp.ToString();
        Random.InitState(temp);
        xOrg = Random.Range(0, 100000);
        yOrg = Random.Range(0, 100000);
        //xOrg += 0.1f;
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Start();
            //GenerateMap();
        }
    }

    private void GenerateMap()
    {
        //노이즈 맵 생성으로 좌표계를 일치시키기 위해 -yOrg, -xOrg을 넣어준다.
        StartCoroutine(PerlinNoiseCoroutine());
    }
    IEnumerator PerlinNoiseCoroutine()
    {
        Debug.Log("GameStart : " + Time.realtimeSinceStartup);
        yield return null;
        float[,] noiseMap = perlinNoise.GenerateMap(width, height, scale, octaves, persistance, lacunarity, -yOrg, -xOrg);
        StartCoroutine(GradientCoroutine(noiseMap));
    }
    IEnumerator GradientCoroutine(float[,] noiseMap)
    {
        Debug.Log("PerlinNoise Done : " + Time.realtimeSinceStartup);
        yield return null;
        float[,] gradientMap = gradient.GenerateMap(width, height);
        StartCoroutine(MapDisplayCoroutine(noiseMap, gradientMap));
    }
    IEnumerator MapDisplayCoroutine(float[,] noiseMap, float[,] gradientMap)
    {
        Debug.Log("Gradient Done : " + Time.realtimeSinceStartup);
        yield return null;
        if (useGradientMap) mapDisplay.DrawNoiseMap(noiseMap, gradientMap);
        else mapDisplay.DrawNoiseMap(noiseMap, noiseMap);
    }
}
