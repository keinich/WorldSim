using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {

  public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {
    Texture2D texture = new Texture2D(width, height);
    texture.filterMode = FilterMode.Point;
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.SetPixels(colorMap);
    texture.Apply();
    return texture;
  }

  public static Texture2D TextureFromHeightMap(float[,] heightMap) {
    int width = heightMap.GetLength(0);
    int height = heightMap.GetLength(1);

    Color[] colorMap = new Color[width * height];

    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
      }
    }

    return TextureFromColorMap(colorMap, width, height);
  }

  public static Texture2D TextureFromHeightMap(float[] heightMap) {
    int width = (int)Mathf.Sqrt(heightMap.GetLength(0));
    int height = width;

    Color[] colorMap = new Color[width * height];

    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[y * width + x]);
      }
    }

    return TextureFromColorMap(colorMap, width, height);
  }

  public static float[] ConvertTextureToHeightMap(Texture2D texture) {
    int width = texture.width;
    float[] result = new float[width * width];
    for (int i = 0; i < width; i++) {
      for (int j = 0; j < width; j++) {
        result[i * width + j] = texture.GetPixel(i, j).grayscale;
      }
    }
    return result;
  }

}
