using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

  public bool printTimers;

  [Header("Mesh Settings")]
  public int mapSize = 255;
  public float scale = 20;
  public float elevationScale = 10;
  public Material material;

  // Internal
  float[] map;  

  static HeightmapGenerator heightmapGenerator;
  
  private void Start() {
    heightmapGenerator = GetComponent<HeightmapGenerator>();
    if (!heightmapGenerator) {
      Debug.Log("No HeightmapGenerator!");
    }
    CreateTerrain();
  }

  public void CreateTerrain() {
    GenerateHeightMap();
    Erosion erosion = GetComponent<Erosion>();
    if (erosion) {
      erosion.ErodeGpu(map, mapSize);
    }
  }

  private void OnValidate() {
  }

  public void GenerateHeightMap() {
    Erosion erosion = GetComponent<Erosion>();
    int mapSizeWithBorder = mapSize;
    if (erosion) {
      mapSizeWithBorder = mapSize + erosion.erosionBrushRadius * 2;
    }
    map = FindObjectOfType<HeightmapGenerator>().GenerateHeightMap(mapSizeWithBorder);
    erosion.ErodeGpu(map, mapSize);
  }

  public void ConstructTerrain() {
    Terrain terrain = FindObjectOfType<Terrain>();
    terrain.terrainData.heightmapResolution = mapSize;
    float[,] heightMap = GetTerrainHeightMap(map);
    terrain.terrainData.SetHeights(0, 0, heightMap);
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