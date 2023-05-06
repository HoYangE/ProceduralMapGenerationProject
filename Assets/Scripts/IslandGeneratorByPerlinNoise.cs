using System;
using System.Collections;
using System.Collections.Generic;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;

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

    private float[,] noiseMap;
    private float[,] gradientMap;
    private Tuple<float[,],Voronoi> voronoiDiagram;
    private bool noiseMapDone = false;
    private bool gradientMapDone = false;
    private bool voronoiDone = false;

    [SerializeField] private GameObject voronoi;
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
        StartCoroutine(VoronoiDiagramCoroutine());
        StartCoroutine(PerlinNoiseCoroutine());
        StartCoroutine(GradientCoroutine());
        StartCoroutine(MapDisplayCoroutine());
    }
    
    IEnumerator VoronoiDiagramCoroutine()
    {
        Debug.Log("Voronoi Start : " + Time.realtimeSinceStartup);
        yield return null;
        voronoiDiagram = voronoi.GetComponent<VoronoiDiagram>().StartGenerateVoronoiDiagram();
        voronoiDone = true;
    }
    IEnumerator PerlinNoiseCoroutine()
    {
        Debug.Log("PerlinNoise Start : " + Time.realtimeSinceStartup);
        yield return null;
        noiseMap = perlinNoise.GenerateMap(width, height, scale, octaves, persistance, lacunarity, -yOrg, -xOrg);
        noiseMapDone = true;
    }
    IEnumerator GradientCoroutine()
    {
        Debug.Log("Gradient Start : " + Time.realtimeSinceStartup);
        yield return null;
        gradientMap = gradient.GenerateMap(width, height);
        gradientMapDone = true;
    }
    IEnumerator MapDisplayCoroutine()
    {
        yield return new WaitUntil(() => noiseMapDone && gradientMapDone && voronoiDone);
        Debug.Log("MapDisplay Start : " + Time.realtimeSinceStartup);
        yield return null;
        mapDisplay.DrawNoiseMap(noiseMap, useGradientMap ? gradientMap : noiseMap, voronoiDiagram);
    }
}
