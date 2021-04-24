using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseNode : TerrainNode {

  public PerlinNoiseParams perlinNoiseParams;

  public PerlinNoiseNode() {
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "HeightMap Input" });
    heightmapInputs.Add(new HeightmapOutputReceiver(this) { name = "Mask" });
    heightmapOutputs.Add(new HeightmapOutput(this) { name = "HeightMap Output" });
    nodeName = "Perlin Noise";
    perlinNoiseParams = new PerlinNoiseParams();
  }

  public override float[] Generate(HeightmapOutput output, int mapSize) {
    HeightmapOutputReceiver heightmapInput = heightmapInputs.Find((x) => x.name == "HeightMap Input");
    HeightmapOutput heightmapOutput = heightmapInput.output;
    float[] inputMap;
    if (heightmapOutput is null || heightmapOutput.parent is null) {
      inputMap = new float[mapSize * mapSize];
    }
    else {
      inputMap = heightmapOutput.parent.Generate(heightmapOutput, mapSize);
    }
    HeightmapOutputReceiver maskInput = heightmapInputs.Find((x) => x.name == "Mask");
    HeightmapOutput maskOutput = maskInput.output;
    float[] maskMap;
    if (maskOutput is null) {
      maskMap = new float[mapSize * mapSize];
      for (int i = 0; i < mapSize * mapSize; i++) {
        maskMap[i] = 1f;
      }
    }
    else {
      maskMap = maskOutput.parent.Generate(maskOutput, mapSize);
    }
    return PerlinNoiseGenerator.Apply(inputMap, maskMap, perlinNoiseParams);
  }

}
