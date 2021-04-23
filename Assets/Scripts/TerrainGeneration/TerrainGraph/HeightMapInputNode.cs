using System;
using UnityEngine;

[Serializable]
public class HeightMapInputNode : TerrainNode {

  public Texture2D heightMap;

  public string outputNodeId;

  public HeightMapInputNode() {
    heightmapOutputs.Add(new HeightmapOutput(this) { name = "Heightmap Output", generator = Generate });
  }

  private float[] Generate(int size) {
    float[] result = new float[size * size];
    int textureWidth = heightMap.width;
    int textureHeight = heightMap.height;
    for (int i = 0; i < size; i++) {
      for (int j = 0; j < size; j++) {
        int textureX = (int)(((float)i / size) * textureWidth);
        int textureY = (int)(((float)j / size) * textureHeight);
        result[j * size + i] = heightMap.GetPixel(textureX, textureY).grayscale;
      }
    }
    return result;
  }
}
