using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSaveUtility {

  private TerrainGraphView _targetGraphView;

  private List<Edge> Edges => _targetGraphView.edges.ToList();
  private List<TerrainNode> Nodes => _targetGraphView.nodes.ToList().Cast<TerrainNode>().ToList();

  public static GraphSaveUtility GetInstance(TerrainGraphView targetGraphView) {

    return new GraphSaveUtility {
      _targetGraphView = targetGraphView
    };
  }

  public void SaveGraph(string fileName) {
    if (!Edges.Any()) return;

    TerrainGraphContainer graphContainer = ScriptableObject.CreateInstance<TerrainGraphContainer>();
  }

  public void LoadGraph(string fileName) {

  }

}
