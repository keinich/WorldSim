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
    for (int i = 0; i < height; i++) {
      for (int j = 0; j < width; j++) {
        result[i * width + j] = inputMap[i, j];
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
    for (int i = 0; i < height; i++) {
      for (int j = 0; j < width; j++) {
        result[i, j] = map[i * width + j];
      }
    }
    return result;
  }

  internal static bool Compare(float[,] inputMap, float[,] inputMap2) {
    if (inputMap.GetLength(0) != inputMap2.GetLength(0)) return false;
    if (inputMap.GetLength(1) != inputMap2.GetLength(1)) return false;
    for (int x = 0; x < inputMap.GetLength(0); x++) {
      for (int y = 0; y < inputMap.GetLength(1); y++) {
        if (inputMap[x, y] != inputMap2[x, y]) return false;
      }
    }
    return true;
  }

  internal static void Print(float[,] inputMap) {
    for (int x = 0; x < inputMap.GetLength(0); x++) {
      string line = "";
      for (int y = 0; y < inputMap.GetLength(1); y++) {
        line += $"[{inputMap[x, y]}]";
      }
      Debug.Log(line);
    }
  }
}
