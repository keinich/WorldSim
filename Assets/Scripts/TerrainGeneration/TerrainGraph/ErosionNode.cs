using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErosionNode : TerrainNode {
  
  [SerializeField]
  public ErosionParams erosionParams;

  public ErosionNode() {
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "Heightmap Input" });
    heightmapOutputs.Add(new HeightmapOutput(this) { name = "Heightmap Output" });
    nodeName = "Erosion";
    erosionParams = new ErosionParams();
  }

  public override float[,] Generate(HeightmapOutput output, int mapSize) {
    float[,] inputMap = GetInput(mapSize, "Heightmap Input");
    float[,] result = ErosionGenerator.Erode(inputMap, erosionParams);
    return result;
  }

}
