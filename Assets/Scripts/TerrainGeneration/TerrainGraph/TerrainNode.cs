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

  public string nodeName;

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
  protected float[] GetInput(int mapSize, string name, float defaultValue = 0) {
    HeightmapOutputReceiver heightmapInput = heightmapInputs.Find((x) => x.name == name);
    HeightmapOutput heightmapOutput = heightmapInput.output;
    float[] inputMap;
    if (heightmapOutput is null || heightmapOutput.parent is null) {
      inputMap = new float[mapSize * mapSize];
      for (int i = 0; i < mapSize * mapSize; i++) {
        inputMap[i] = defaultValue;
      }
    }
    else {
      inputMap = heightmapOutput.parent.Generate(heightmapOutput, mapSize);
    }

    return inputMap;
  }

  public virtual float[] Generate(HeightmapOutput output, int mapSize) {
    return new float[mapSize * mapSize];
  }

}
