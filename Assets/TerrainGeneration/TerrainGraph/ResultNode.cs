using System;
using UnityEngine;

[Serializable]
public class ResultNode : TerrainNode {

  [SerializeField]
  public float heightScale = 500;

  public ResultNode() {
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "Heightmap Input" });
    nodeName = "Result";
  }

  public float[,] GenerateHeightMap(int mapSize) {

    float[,] input = GetInput(mapSize, "Heightmap Input");
    float[,] result = new float[mapSize, mapSize];

    for (int x = 0; x < mapSize; x++) {
      for (int y = 0; y < mapSize; y++) {
        result[x, y] = heightScale * input[x, y];
      }
    }

    return result;
  }

}
