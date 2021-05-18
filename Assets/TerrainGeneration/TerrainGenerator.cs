using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

  public int chunkSize = 513;
  public int numChunks = 3;
  public  int mapSize => chunkSize * numChunks + numChunks;

  public Terrain mainTerrain;

  public TerrainGraph terrainGraph;
  public bool autoUpdate = true; 

  public ComputeShader erosion;

  private void Start() {
    GenerateFromGraph();
  }

  public void GenerateFromGraph() {

    ResultNode resultNode = terrainGraph.resultNode;
    float[,] heightMap = resultNode.GenerateHeightMap(mapSize);

    Terrain[,] terrainTiles = GetTerrainTiles();

    for (int i = 0; i < terrainTiles.GetLength(0); i++) {
      for (int j = 0; j < terrainTiles.GetLength(1); j++) {
        Terrain terrainTile = terrainTiles[i, j];
        float[,] tileHeightMap = CreateTileHeightMap(i, j, heightMap);

        terrainTile.terrainData.SetHeights(0, 0, tileHeightMap);
        terrainTile.Flush();
      }
    }  
  }

  private float[,] CreateTileHeightMap(int tileX, int tileY, float[,] heightMap) {
    int startX = tileX * chunkSize - tileX;
    int startY = tileY * chunkSize - tileY;
    float[,] result = new float[chunkSize, chunkSize];
    for (int i = 0; i < chunkSize; i++) {
      for (int j = 0; j < chunkSize; j++) {
        result[j, i] = heightMap[startX + i, startY + j];
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
    return result;
  } 

}