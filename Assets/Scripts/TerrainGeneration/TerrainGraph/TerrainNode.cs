using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HeightmapOutput {
  public string name;
  public TerrainNode parent;

  public HeightmapOutput(TerrainNode p) {
    parent = p;
  }

}

[Serializable]
public class HeightmapOutputReceiver {
  public TerrainNode parent;
  public string name;
  public HeightmapOutput output;

  public HeightmapOutputReceiver(TerrainNode p) {
    parent = p;
  }
}

[Serializable]
public class TerrainNode : ScriptableObject {

  [SerializeField]
  private string _id;

  [SerializeField]
  public string id {
    get {
      if (string.IsNullOrEmpty(_id)) {
        _id = Guid.NewGuid().ToString();
      }
      return _id;
    }
    set { _id = value; }
  }

  public Vector2 position;

  public List<HeightmapOutput> heightmapOutputs = new List<HeightmapOutput>();
  public List<HeightmapOutputReceiver> heightmapInputs = new List<HeightmapOutputReceiver>();

  public virtual float[] Generate(HeightmapOutput output, int mapSize) {
    return new float[mapSize * mapSize];
  }

}
