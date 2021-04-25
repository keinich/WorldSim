using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightmapUtilities {

  public static float[,] ApplyHeightCurve(float[,] inputMap, AnimationCurve heightCurve) {
    float[,] result = new float[inputMap.GetLength(0), inputMap.GetLength(1)];
    for (int i = 0; i < result.GetLength(0); i++) {
      for (int j = 0; j < result.GetLength(1); j++) {
        result[i, j] = heightCurve.Evaluate(inputMap[i, j]);
      }
    }
    return result;
  }

  internal static float[] ConvertFrom2DTo1D(float[,] inputMap) {
    int width = inputMap.GetLength(0);
    int height = inputMap.GetLength(1);
    float[] result = new float[width * height];
    for (int j = 0; j < height; j++) {
      for (int i = 0; i < width; i++) {
        result[j * width + i] = inputMap[i, j];
      }
    }
    return result;
  }

  internal static float[,] ConvertFrom1DTo2D(float[] map, int width = 0, int height = 0) {
    
    if (width == 0 || height == 0) {
      width = (int)Mathf.Sqrt(map.Length);
      height = width;
    }
    if (width * height != map.Length) {
      Debug.LogError("Width * Length != map.Length");
      return null;
    }
    float[,] result = new float[width, height];
    for (int j = 0; j < height; j++) {
      for (int i = 0; i < width; i++) {
        result[i, j] = map[j * width + i];
      }
    }
    return result;
  }
}
