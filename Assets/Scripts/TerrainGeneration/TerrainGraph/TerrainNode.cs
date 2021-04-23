using System;
using System.Collections.Generic;
using UnityEngine;

public class HeightmapOutput {
  public string name;
  public TerrainNode parent;
  public Func<int, float[]> generator;

  public HeightmapOutput(TerrainNode p) {
    parent = p;
  }

}

public class HeightmapOutputReceiver {
  public TerrainNode parent;
  public string name;
  public HeightmapOutput output;

  public HeightmapOutputReceiver(TerrainNode p) {
    parent = p;
  }
}

[Serializable]
public class TerrainNode {

  public string id;

  public List<HeightmapOutput> heightmapOutputs = new List<HeightmapOutput>();
  public List<HeightmapOutputReceiver> heightmapInputs = new List<HeightmapOutputReceiver>();

}
