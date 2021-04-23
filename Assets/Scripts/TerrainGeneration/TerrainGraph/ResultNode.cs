using System;

[Serializable]
public class ResultNode : TerrainNode {

  HeightmapOutputReceiver heightmapInput;

  public ResultNode() {
    heightmapInput = new HeightmapOutputReceiver(this) { name = "Heightmap Input" };
    heightmapInputs.Add(heightmapInput);
  }

  public float[] GenerateHeightMap(int mapSize) {
    HeightmapOutput output = heightmapInput.output;
    if (output is null) {
      return new float[mapSize];
    }
    return output.parent.Generate(output, mapSize);
  }

}
