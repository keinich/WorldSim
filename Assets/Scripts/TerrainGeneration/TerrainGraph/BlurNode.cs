using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurNode : TerrainNode {

  public float resolution = 1;
  public float size = 1;

  public BlurNode() {
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "Heightmap Input" });
    heightmapOutputs.Add(new HeightmapOutput(this) { name = "Heightmap Output" });
    nodeName = "Blur";
  }

  public override float[,] Generate(HeightmapOutput output, int mapSize) {
    float[,] inputMap = GetInput(mapSize, "Heightmap Input");
    return BlurGenerator.Apply(inputMap, resolution, size);
  }

}
