using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

  public int chunkSize = 513;

  public Terrain mainTerrain;

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

    mapSize = heightMap.GetLength(0);

    Terrain[,] terrainTiles = GetTerrainTiles();
    //return;

    float[,] dummyMap = new float[mapSize, mapSize];
    for (int i = 0; i < mapSize; i++) {
      for (int j = 0; j < mapSize; j++) {
        dummyMap[i, j] = ((float)(i + j)) / (2.0f * mapSize);
        //dummyMap[i, j] *= 10f;
      }
    }
  
    for (int i = 0; i < terrainTiles.GetLength(0); i++) {
      for (int j = 0; j < terrainTiles.GetLength(1); j++) {
        float[,] tileHeightMap = CreateTileHeightMap(i, j, heightMap);
        Terrain terrainTile = terrainTiles[i, j];
        terrainTile.terrainData.SetHeights(0, 0, tileHeightMap);
        terrainTile.Flush();
      }
    }


    return;
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

    int terrainSize = 3 * chunkSize;

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

  private float[,] CreateTileHeightMap(int tileX, int tileY, float[,] heightMap) {
    int startX = tileX * chunkSize;
    int startY = tileY * chunkSize;
    float[,] result = new float[chunkSize, chunkSize];
    for (int i = 0; i < chunkSize; i++) {
      for (int j = 0; j < chunkSize; j++) {
        result[j, i] = heightMap[startX + i, startY + j];
        float resultji = result[j, i];
      }
    }
    return result;
  }

  private Terrain[,] GetTerrainTiles() {

    int numTiles = mapSize / chunkSize;

    Terrain[,] result = new Terrain[numTiles, numTiles];

    int tilesLeft = numTiles / 2;
    int tilesRight = numTiles - tilesLeft;
    int tilesBottom = tilesLeft;
    int tilesTop = tilesRight;

    Terrain currentTerrain = mainTerrain;
    Terrain currentLeftTerrain = mainTerrain;
    result[0, 0] = currentTerrain;
    for (int j = 0; j < numTiles; j++) {
      for (int i = 0; i < numTiles; i++) {
        Terrain neighborTerrain;
        if (i < numTiles - 1) {
          neighborTerrain = currentTerrain.rightNeighbor;
          if (neighborTerrain is null) {
            neighborTerrain = Instantiate<Terrain>(currentTerrain, gameObject.transform);
            neighborTerrain.transform.position = currentTerrain.transform.position + new Vector3(
              2 * currentTerrain.terrainData.bounds.extents.x, 0, 0
            );
          }
          result[i + 1, j] = neighborTerrain;
          neighborTerrain.name = $"TerrainTile{i + 1}_{j}";
        }
        else {
          neighborTerrain = currentLeftTerrain.topNeighbor;
          if (neighborTerrain is null) {
            neighborTerrain = Instantiate<Terrain>(currentLeftTerrain, gameObject.transform);
            neighborTerrain.transform.position = currentLeftTerrain.transform.position + new Vector3(
              0, 0, 2 * currentLeftTerrain.terrainData.bounds.extents.x
            );
          }
          currentLeftTerrain = neighborTerrain;
          result[0, j + 1] = neighborTerrain;
          neighborTerrain.name = $"TerrainTile{0}_{j + 1}";
        }
        TerrainData td = Instantiate<TerrainData>(mainTerrain.terrainData);
        neighborTerrain.terrainData = td;
        neighborTerrain.terrainData.heightmapResolution = chunkSize;

        currentTerrain = neighborTerrain;
        if (j == numTiles - 1 && i == numTiles - 2) {
          break;
        }
      }
    }

    //while (indexX >= 0) {
    //  if (currentTerrain.leftNeighbor is null) { 
    //    Terrain newTerrain = Instantiate<Terrain>(mainTerrain, gameObject.transform);
    //    newTerrain.get
    //    newTerrain.transform.position = currentTerrain.transform.position - new Vector3(
    //      2 * currentTerrain.terrainData.bounds.extents.x, 0, 0
    //    );
    //    newTerrain.terrainData.heightmapResolution = chunkSize;
    //    indexX -= 1;
    //  }
    //  leftLowerMostTerrain = leftLowerMostTerrain.leftNeighbor;
    //}
    //while (true) {
    //  if (leftLowerMostTerrain.bottomNeighbor is null) break;
    //  leftLowerMostTerrain = leftLowerMostTerrain.bottomNeighbor;
    //}

    //Terrain leftNeighbor = mainTerrain.leftNeighbor;
    //if (leftNeighbor is null) {
    //  leftNeighbor = Instantiate<Terrain>(mainTerrain, gameObject.transform);
    //  leftNeighbor.transform.position = mainTerrain.transform.position - new Vector3(chunkSize, 0, 0);
    //  leftNeighbor.transform.position = mainTerrain.transform.position - new Vector3(2 * mainTerrain.terrainData.bounds.extents.x, 0, 0);
    //  leftNeighbor.terrainData.heightmapResolution = chunkSize;
    //  leftNeighbor.SetNeighbors(null, null, mainTerrain, null);
    //  mainTerrain.SetNeighbors(leftNeighbor, null, null, null);
    //}
    return result;
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