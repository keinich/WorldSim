using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseNode : TerrainNode {

  [SerializeField]
  public PerlinNoiseParams perlinNoiseParams;

  public PerlinNoiseNode() {
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "HeightMap Input" });
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "Mask" });
    heightmapOutputs.Add(new HeightmapOutput(this) { name = "Heightmap Output" });
    nodeName = "Perlin Noise";
    perlinNoiseParams = new PerlinNoiseParams();
  }

  public override float[,] Generate(HeightmapOutput output, int mapSize) {
    float[,] inputMap = GetInput(mapSize, "HeightMap Input");
    float[,] maskMap = GetInput(mapSize, "Mask", 1f);  
    return PerlinNoiseGenerator.Apply(inputMap, maskMap, perlinNoiseParams);
  }
}
