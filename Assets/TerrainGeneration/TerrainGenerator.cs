using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

  public TerrainGraph terrainGraph;

  public bool printTimers;
  public bool autoUpdate = true;
  public bool useErosion = true;

  [Header("Mesh Settings")]
  public int mapSize = 255;
  public float scale = 20;
  public float elevationScale = 10;
  public Material material;

  [Header("Layout")]
  public Texture2D mountainMap;
  public static ComputeShader blurShader;
  public float mountainMapBlurResolution;
  public float mountainMapBlurSize;

  [Header("Perlin Noise")]
  public int seed;
  public bool randomizeSeed;

  public int numOctaves = 7;
  public float persistence = .5f;
  public float lacunarity = 2;
  public float initialScale = 2;

  [Header("Erosion")]
  [Range(0, 1)]
  public float inertia = .05f; // At zero, water will instantly change direction to flow downhill. At 1, water will never change direction. 
  public float sedimentCapacityFactor = 4; // Multiplier for how much sediment a droplet can carry
  public float minSedimentCapacity = .01f; // Used to prevent carry capacity getting too close to zero on flatter terrain
  [Range(0, 1)]
  public float erodeSpeed = .3f;
  [Range(0, 1)]
  public float depositSpeed = .3f;
  [Range(0, 1)]
  public float evaporateSpeed = .01f;
  public float gravity = 4;


  public ComputeShader erosion;
  public int numErosionIterations = 50000;
  public int erosionBrushRadius = 3;

  public int maxLifetime = 30;

  public float startSpeed = 1;
  public float startWater = 1;

  // Internal
  float[] map;

  private void Start() {
    GenerateHeightMap();
  }

  public void GenerateFromGraph() {
    ResultNode resultNode = terrainGraph.resultNode;
    float[,] heightMap = resultNode.GenerateHeightMap();



    //return;
    Terrain terrain = FindObjectOfType<Terrain>();

    Terrain leftLowerMostTerrain = terrain;
    while (true) {
      if (leftLowerMostTerrain.leftNeighbor is null) break;
      leftLowerMostTerrain = leftLowerMostTerrain.leftNeighbor;
    }
    while (true) {
      if (leftLowerMostTerrain.bottomNeighbor is null) break;
      leftLowerMostTerrain = leftLowerMostTerrain.bottomNeighbor;
    }

    int chunkSize = 513;
    int terrainSize = 3 * 513;
    float[,] dummyMap = new float[terrainSize, terrainSize];
    for (int i = 0; i < terrainSize; i++) {
      for (int j = 0; j < terrainSize; j++) {
        dummyMap[i, j] = ((float)(i + j)) / (2.0f * terrainSize);
      }
    }
    dummyMap = heightMap;
    Terrain currentTerrain = leftLowerMostTerrain;
    Terrain currentBottomTerrain = leftLowerMostTerrain;
    for (int i = 0; i < terrainSize; i += chunkSize) {
      for (int j = 0; j < terrainSize; j += chunkSize) {
        float[,] chunkMap = new float[chunkSize, chunkSize];
        float leftBottomHeight = dummyMap[i, j];
        for (int ii = 0; ii < chunkSize; ii++) {
          for (int jj = 0; jj < chunkSize; jj++) {
            chunkMap[jj, ii] = dummyMap[i + ii, j + jj];
            //chunkMap[ii, jj] = dummyMap[ii,jj];
            //chunkMap[jj, ii] = dummyMap[ii,jj];
          }
        }
        currentTerrain.terrainData.heightmapResolution = chunkSize;
        currentTerrain.terrainData.SetHeights(0, 0, chunkMap);
        currentTerrain.drawInstanced = false;
        if (!(currentTerrain.topNeighbor is null)) {
          currentTerrain = currentTerrain.topNeighbor;
        }
      }
      if (!(currentBottomTerrain.rightNeighbor is null)) {
        currentBottomTerrain = currentBottomTerrain.rightNeighbor;
        currentTerrain = currentBottomTerrain;
      }
    }
    foreach (Terrain t in FindObjectsOfType<Terrain>()) {
      t.Flush();
    }
    //terrain.terrainData.heightmapResolution = heightMap.GetLength(0);
    //terrain.terrainData.SetHeights(0, 0, heightMap);
    //terrain.drawInstanced = false;
    //terrain.Flush();
  }

  public void GenerateHeightMap() {
    int mapSizeWithBorder = mapSize;
    mapSizeWithBorder = mapSize + erosionBrushRadius * 2;
    map = HeightmapGenerator.GenerateHeightMap(
      mapSizeWithBorder, seed, randomizeSeed, numOctaves, initialScale, persistence, lacunarity, mountainMap, mountainMapBlurResolution, mountainMapBlurSize
    );
    if (useErosion) {
      //Erosion.Erode(
      //map, mapSize, numErosionIterations, erosionBrushRadius, erosion, maxLifetime, inertia, depositSpeed, minSedimentCapacity, 
      //  evaporateSpeed, sedimentCapacityFactor, erodeSpeed, startSpeed, startWater, gravity
      //);
      map = HeightmapUtilities.ConvertFrom2DTo1D(ErosionGenerator.Erode(
        HeightmapUtilities.ConvertFrom1DTo2D(map), new ErosionParams {
          numErosionIterations = numErosionIterations,
          erosionBrushRadius = erosionBrushRadius,
          maxLifetime = maxLifetime,
          inertia = inertia,
          depositSpeed = depositSpeed,
          minSedimentCapacity = minSedimentCapacity,
          evaporateSpeed = evaporateSpeed,
          sedimentCapacityFactor = sedimentCapacityFactor,
          erodeSpeed = erodeSpeed,
          startSpeed = startSpeed,
          startWater = startWater,
          gravity = gravity
        }));
      //map = HeightmapUtilities.ConvertFrom2DTo1D(HeightmapUtilities.ConvertFrom1DTo2D(map));
      //map = ErosionGenerator.Erode(
      //  map, mapSize, new ErosionParams {
      //    numErosionIterations = numErosionIterations,
      //    erosionBrushRadius = erosionBrushRadius,
      //    maxLifetime = maxLifetime,
      //    inertia = inertia,
      //    depositSpeed = depositSpeed,
      //    minSedimentCapacity = minSedimentCapacity,
      //    evaporateSpeed = evaporateSpeed,
      //    sedimentCapacityFactor = sedimentCapacityFactor,
      //    erodeSpeed = erodeSpeed,
      //    startSpeed = startSpeed,
      //    startWater = startWater,
      //    gravity = gravity
      //  });
      //map = ErosionGenerator.Erode(
      //  HeightmapUtilities.ConvertFrom2DTo1D(HeightmapUtilities.ConvertFrom1DTo2D(map)),
      //  mapSize, 
      //  new ErosionParams {
      //    numErosionIterations = numErosionIterations,
      //    erosionBrushRadius = erosionBrushRadius,
      //    maxLifetime = maxLifetime,
      //    inertia = inertia,
      //    depositSpeed = depositSpeed,
      //    minSedimentCapacity = minSedimentCapacity,
      //    evaporateSpeed = evaporateSpeed,
      //    sedimentCapacityFactor = sedimentCapacityFactor,
      //    erodeSpeed = erodeSpeed,
      //    startSpeed = startSpeed,
      //    startWater = startWater,
      //    gravity = gravity
      //  }
      //);
      //map = HeightmapUtilities.ConvertFrom2DTo1D(HeightmapUtilities.ConvertFrom1DTo2D(map));
    }
  }

  public void ConstructTerrain() {
    Terrain terrain = FindObjectOfType<Terrain>();
    terrain.terrainData.heightmapResolution = mapSize;
    float[,] heightMap = GetTerrainHeightMap(map);

    //heightMap = HeightmapUtilities.ConvertFrom1DTo2D(map);
    terrain.terrainData.SetHeights(0, 0, heightMap);
    terrain.drawInstanced = false;
    terrain.Flush();
  }

  private float[,] GetTerrainHeightMap(float[] map) {
    int s = (int)Mathf.Sqrt(map.Length);
    float[,] result = new float[s, s];
    for (int i = 0; i < s * s; i++) {
      int x = i % s;
      int y = i / s;
      result[x, y] = map[i] * 0.2f;
    }

    return result;
  }

  private float[,] GetHeightMap(float[] map) {
    int s = (int)Mathf.Sqrt(map.Length);
    float[,] result = new float[s, s];
    for (int i = 0; i < s * s; i++) {
      int x = i % s;
      int y = i / s;
      result[x, y] = map[i] * 0.1f;
    }

    return result;
  }

}