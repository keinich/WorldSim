using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainGraphContainer : ScriptableObject {

  public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
  public List<TerrainNodeData> TerrainNodeDatas = new List<TerrainNodeData>();
  public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();

}
