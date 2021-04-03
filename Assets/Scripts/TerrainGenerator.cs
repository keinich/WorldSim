using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

  public Terrain terrain;
  public bool autoUpdate;

  public Noise.NormalizeMode normalizeMode;

  [Range(0, 6)]
  public int editorPreviewLOD;
  public float noiseScale;

  [Range(0, 1)]
  public float heightScale;

  public int octaves;
  [Range(0, 1)]
  public float persistance;
  public float lacunarity;

  public int seed;
  public Vector2 offset;

  public Boolean useFalloff;

  void Start() {

  }

  void Update() {

  }

  public void DrawTerrainInEditor() {
    float[,] heightMap = Noise.GenerateNoiseMap(
      terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution, seed, noiseScale, octaves, persistance, lacunarity, offset, normalizeMode
    );
    float[,] betterHeightMap = new float[heightMap.GetLength(0), heightMap.GetLength(1)];
    for (int i = 0; i < heightMap.GetLength(0); i++) {
      for (int j = 0; j < heightMap.GetLength(1); j++) {
        betterHeightMap[i, j] = heightMap[i, j] * heightScale;
      }
    }
    terrain.terrainData.SetHeights(0, 0, betterHeightMap);
  }
}
