using UnityEngine;

public static class HeightmapGenerator {

  public static float[] GenerateHeightMap(
    int mapSize,
    int seed,
    bool randomizeSeed,
    int numOctaves,
    float initialScale,
    float persistence,
    float lacunarity,
    Texture2D mountainMap,
    float mountainBlurResolution,
    float mountainBlurSize
  ) {

    var map = new float[mapSize * mapSize];
    seed = (randomizeSeed) ? Random.Range(-10000, 10000) : seed;
    var prng = new System.Random(seed);

    Vector2[] offsets = new Vector2[numOctaves];
    for (int i = 0; i < numOctaves; i++) {
      offsets[i] = new Vector2(prng.Next(-1000, 1000), prng.Next(-1000, 1000));
    }

    float minValue = float.MaxValue;
    float maxValue = float.MinValue;

    Texture2D bluredMountainMap = ShaderUtilities.Blur(mountainMap, mountainBlurResolution, mountainBlurSize);

    float[] mountainMaskArray = new float[mapSize * mapSize];
    for (int x = 0; x < mapSize; x++) {
      for (int y = 0; y < mapSize; y++) {
        //int u = x * (mapSize / mountainMap.width);
        //int v = y * (mapSize / mountainMap.height);
        int u = x * (bluredMountainMap.width / mapSize);
        int v = y * (bluredMountainMap.height / mapSize);
        Color mountainIntensityColor = bluredMountainMap.GetPixel(u, v);
        float mountainIntensity = mountainIntensityColor.r;
        //mountainMask[x * mapSize + y] = Mathf.PerlinNoise(p.x, p.y);
        mountainMaskArray[x * mapSize + y] = mountainIntensity;
      }
    }


    //return mountainMaskArray;

    for (int y = 0; y < mapSize; y++) {
      for (int x = 0; x < mapSize; x++) {
        float noiseValue = 0;
        float scale = initialScale;
        float weight = 1;
        for (int i = 0; i < numOctaves; i++) {
          Vector2 p = offsets[i] + new Vector2(x / (float)mapSize, y / (float)mapSize) * scale;
          noiseValue += Mathf.PerlinNoise(p.x, p.y) * weight;
          weight *= persistence;
          scale *= lacunarity;
        }
        float height = noiseValue * mountainMaskArray[y * mapSize + x];
        //float height = noiseValue;
        map[y * mapSize + x] = height;
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

    //return mountainMask;
    return map;
  }
}