using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErosionParams {

  [SerializeField]
  public int erosionBrushRadius = 5;
  [SerializeField]
  public int numErosionIterations = 100000;
  [SerializeField]
  public int maxLifetime = 5;
  [SerializeField]
  public float inertia = .05f;
  [SerializeField]
  public float sedimentCapacityFactor = 4;
  [SerializeField]
  public float minSedimentCapacity = .01f;
  [SerializeField]
  public float depositSpeed = .3f;
  [SerializeField]
  public float erodeSpeed = .3f;
  [SerializeField]
  public float evaporateSpeed = .01f;
  [SerializeField]
  public float gravity = 4;
  [SerializeField]
  public float startSpeed = 1;
  [SerializeField]
  public float startWater = 1;
}

public static class ErosionGenerator {

  public static float[,] Erode(float[,] inputMap, ErosionParams p) {

    float[] input1D = HeightmapUtilities.ConvertFrom2DTo1D(inputMap);

    float[] result = Erode(input1D, inputMap.GetLength(0) - p.erosionBrushRadius * 2, p);

    return HeightmapUtilities.ConvertFrom1DTo2D(result);
  }

  public static float[] Erode(float[] map, int mapSize, ErosionParams p) {
    ComputeShader erosion = ShaderUtilities.GetErosionShader();

    //float[] map = new float[inputMap.Length];
    //for (int i = 0; i < inputMap.Length; i++) {
    //  map[i] = inputMap[i];
    //}

    int mapSizeWithBorder = mapSize + p.erosionBrushRadius * 2;

    int numThreads = p.numErosionIterations / 1024;

    // Create brush
    List<int> brushIndexOffsets = new List<int>();
    List<float> brushWeights = new List<float>();

    float weightSum = 0;
    for (int brushY = -p.erosionBrushRadius; brushY <= p.erosionBrushRadius; brushY++) {
      for (int brushX = -p.erosionBrushRadius; brushX <= p.erosionBrushRadius; brushX++) {
        float sqrDst = brushX * brushX + brushY * brushY;
        if (sqrDst < p.erosionBrushRadius * p.erosionBrushRadius) {
          brushIndexOffsets.Add(brushY * mapSize + brushX);
          float brushWeight = 1 - Mathf.Sqrt(sqrDst) / p.erosionBrushRadius;
          weightSum += brushWeight;
          brushWeights.Add(brushWeight);
        }
      }
    }
    for (int i = 0; i < brushWeights.Count; i++) {
      brushWeights[i] /= weightSum;
    }

    // Send brush data to compute shader
    ComputeBuffer brushIndexBuffer = new ComputeBuffer(brushIndexOffsets.Count, sizeof(int));
    ComputeBuffer brushWeightBuffer = new ComputeBuffer(brushWeights.Count, sizeof(int));
    brushIndexBuffer.SetData(brushIndexOffsets);
    brushWeightBuffer.SetData(brushWeights);
    erosion.SetBuffer(0, "brushIndices", brushIndexBuffer);
    erosion.SetBuffer(0, "brushWeights", brushWeightBuffer);

    // Generate random indices for droplet placement
    int[] randomIndices = new int[p.numErosionIterations];
    for (int i = 0; i < p.numErosionIterations; i++) {
      int randomX = UnityEngine.Random.Range(p.erosionBrushRadius, mapSize + p.erosionBrushRadius);
      int randomY = UnityEngine.Random.Range(p.erosionBrushRadius, mapSize + p.erosionBrushRadius);
      randomIndices[i] = randomY * mapSize + randomX;
    }

    // Send random indices to compute shader
    ComputeBuffer randomIndexBuffer = new ComputeBuffer(randomIndices.Length, sizeof(int));
    randomIndexBuffer.SetData(randomIndices);
    erosion.SetBuffer(0, "randomIndices", randomIndexBuffer);

    // Heightmap buffer
    ComputeBuffer mapBuffer = new ComputeBuffer(map.Length, sizeof(float));
    mapBuffer.SetData(map);
    erosion.SetBuffer(0, "map", mapBuffer);

    // Settings
    erosion.SetInt("borderSize", p.erosionBrushRadius);
    erosion.SetInt("mapSize", mapSizeWithBorder);
    erosion.SetInt("brushLength", brushIndexOffsets.Count);
    erosion.SetInt("maxLifetime", p.maxLifetime);
    erosion.SetFloat("inertia", p.inertia);
    erosion.SetFloat("sedimentCapacityFactor", p.sedimentCapacityFactor);
    erosion.SetFloat("minSedimentCapacity", p.minSedimentCapacity);
    erosion.SetFloat("depositSpeed", p.depositSpeed);
    erosion.SetFloat("erodeSpeed", p.erodeSpeed);
    erosion.SetFloat("evaporateSpeed", p.evaporateSpeed);
    erosion.SetFloat("gravity", p.gravity);
    erosion.SetFloat("startSpeed", p.startSpeed);
    erosion.SetFloat("startWater", p.startWater);

    // Run compute shader
    erosion.Dispatch(0, numThreads, 1, 1);
    mapBuffer.GetData(map);

    // Release buffers
    mapBuffer.Release();
    randomIndexBuffer.Release();
    brushIndexBuffer.Release();
    brushWeightBuffer.Release();

    return map;
  }

}
