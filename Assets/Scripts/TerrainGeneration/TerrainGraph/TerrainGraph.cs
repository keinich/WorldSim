using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TerrainGraph {

  public ResultNode resultNode;

  public TerrainGraph() {
    terrainNodes = new List<TerrainNode>();
    resultNode = new ResultNode();
  }

  public List<TerrainNode> terrainNodes;
}
