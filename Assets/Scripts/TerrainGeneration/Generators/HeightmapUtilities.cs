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

}
