using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGeneratorByPerlinNoise : MonoBehaviour
{
    public int width = 256;
    public int height = 256;

    public float scale = 1.0f;
    public int octaves = 3;
    public float persistance = 0.5f;
    public float lacunarity = 2;

    private float xOrg = 0;
    private float yOrg = 0;

    public string seed;
    public bool useRandomSeed;

    public bool useColorMap;
    public bool useGradientMap;

    [SerializeField] private PerlinNoise perlinNoise;
    [SerializeField] private Gradient gradient;
    [SerializeField] private MapDisplay mapDisplay;

    private void Start()
    {
        //시드를 만들고 위치를 구한다.
        int temp = Time.time.ToString().GetHashCode();
        if (useRandomSeed) seed = temp.ToString();
        Random.InitState(temp);
        xOrg = Random.Range(0, 100000);
        yOrg = Random.Range(0, 100000);
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Start();
        }
    }

    private void GenerateMap()
    {
        float[,] noiseMap = perlinNoise.GenerateMap(width, height, scale, octaves, persistance, lacunarity, xOrg, yOrg); //노이즈 맵 생성
        float[,] gradientMap = gradient.GenerateMap(width, height);
        if (useGradientMap) mapDisplay.DrawNoiseMap(noiseMap, gradientMap, useColorMap);
        else mapDisplay.DrawNoiseMap(noiseMap, noiseMap, useColorMap);
    }
}
