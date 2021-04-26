using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightCurveNode : TerrainNode {

  public AnimationCurve heightCurve;

  public HeightCurveNode() {
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "Heightmap Input" });
    heightmapOutputs.Add(new HeightmapOutput(this) { name = "Heightmap Output" });
    heightCurve = new AnimationCurve();
  }

  public override float[,] Generate(HeightmapOutput output, int mapSize) {
    float[,] inputMap = GetInput(mapSize, "Heightmap Input");
    float[,] result = HeightmapUtilities.ApplyHeightCurve(inputMap, heightCurve);
    return result;
  }

}
