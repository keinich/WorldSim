using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseParams {
  public bool randomizeSeed = true;
  public int numOctaves = 4;
  public float initialScale = 10f;
  public float persistence = .16f;
  public float lacunarity = 2.0f;
  public int seed = 1;
}

public static class PerlinNoiseGenerator {

  public static float[,] Apply(float[,] inputMap, float[,] maskMap, PerlinNoiseParams p) {
    float[,] map = new float[inputMap.GetLength(0), inputMap.GetLength(1)];
    int mapSizeX = inputMap.GetLength(0);
    int mapSizeY = inputMap.GetLength(1);

    int seedUsed = (p.randomizeSeed) ? Random.Range(-10000, 10000) : p.seed;
    var prng = new System.Random(seedUsed);

    Vector2[] offsets = new Vector2[p.numOctaves];
    for (int i = 0; i < p.numOctaves; i++) {
      offsets[i] = new Vector2(prng.Next(-1000, 1000), prng.Next(-1000, 1000));
    }

    float minValue = float.MaxValue;
    float maxValue = float.MinValue;

    for (int x = 0; x < mapSizeX; x++) {
      for (int y = 0; y < mapSizeY; y++) {
        float noiseValue = 0;
        float scale = p.initialScale;
        float weight = 1;
        for (int i = 0; i < p.numOctaves; i++) {
          Vector2 percent = offsets[i] + new Vector2(x / (float)mapSizeX, y / (float)mapSizeY) * scale;
          float perlinNoiseValue = Mathf.PerlinNoise(percent.x, percent.y) * weight;
          //perlinNoiseValue = (perlinNoiseValue - 0.5f) * weight;
          //perlinNoiseValue = perlinNoiseValue  * weight;
          noiseValue += perlinNoiseValue;
          weight *= p.persistence;
          scale *= p.lacunarity;
        }
        //float height = noiseValue;
        noiseValue *= maskMap[x, y];
        map[x, y] = inputMap[x, y] + noiseValue;
        minValue = Mathf.Min(noiseValue, minValue);
        maxValue = Mathf.Max(noiseValue, maxValue);
      }
    }

    // Normalize
    if (maxValue != minValue) {
      for (int i = 0; i < map.GetLength(0); i++) {
        for (int j = 0; j < map.GetLength(1); j++) {
          map[i, j] = (map[i, j] - minValue) / (maxValue - minValue);
        }
      }
    }

    return map;
  }

}
