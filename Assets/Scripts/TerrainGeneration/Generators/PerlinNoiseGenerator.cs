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

  public static float[] Apply(float[] inputMap, float[] maskMap, PerlinNoiseParams p) {
    float[] map = new float[inputMap.Length];
    int mapSize = (int)Mathf.Sqrt(inputMap.Length);

    int seedUsed = (p.randomizeSeed) ? Random.Range(-10000, 10000) : p.seed;
    var prng = new System.Random(seedUsed);

    Vector2[] offsets = new Vector2[p.numOctaves];
    for (int i = 0; i < p.numOctaves; i++) {
      offsets[i] = new Vector2(prng.Next(-1000, 1000), prng.Next(-1000, 1000));
    }

    float minValue = float.MaxValue;
    float maxValue = float.MinValue;

    for (int y = 0; y < mapSize; y++) {
      for (int x = 0; x < mapSize; x++) {
        float noiseValue = 0;
        float scale = p.initialScale;
        float weight = 1;
        for (int i = 0; i < p.numOctaves; i++) {
          Vector2 percent = offsets[i] + new Vector2(x / (float)mapSize, y / (float)mapSize) * scale;
          float perlinNoiseValue = Mathf.PerlinNoise(percent.x, percent.y);
          perlinNoiseValue = (perlinNoiseValue - 0.5f) * weight;
          //perlinNoiseValue = perlinNoiseValue  * weight;
          noiseValue += perlinNoiseValue;
          weight *= p.persistence;
          scale *= p.lacunarity;
        }
        //float height = noiseValue;
        noiseValue *= maskMap[y * mapSize + x];
        map[y * mapSize + x] = inputMap[y * mapSize + x] + noiseValue;
        minValue = Mathf.Min(noiseValue, minValue);
        maxValue = Mathf.Max(noiseValue, maxValue);
      }
    }

    // Normalize
    if (maxValue != minValue) {
      for (int i = 0; i < map.Length; i++) {
        map[i] = (map[i] - minValue) / (maxValue - minValue);
      }
    }

    return map;
  }

}
