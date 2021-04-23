using System;
using System.Collections.Generic;

[Serializable]
public class TerrainGraph {

  public TerrainGraph() {
    ResultNode resultNode = new ResultNode();
    terrainNodes = new List<TerrainNode>();
    terrainNodes.Add(resultNode);
  }

  public List<TerrainNode> terrainNodes; 

}
