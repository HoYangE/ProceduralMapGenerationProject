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
        int temp = Time.time.ToString().GetHashCode();
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
        float[,] noiseMap = perlinNoise.GenerateMap(width, height, scale, octaves, persistance, lacunarity, -yOrg, -xOrg);
        float[,] gradientMap = gradient.GenerateMap(width, height);
        if (useGradientMap) mapDisplay.DrawNoiseMap(noiseMap, gradientMap);
        else mapDisplay.DrawNoiseMap(noiseMap, noiseMap);
    }
}
