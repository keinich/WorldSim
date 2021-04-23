using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility {

  private TerrainGraphView _targetGraphView;
  private TerrainGraphContainer _container;

  private List<Edge> Edges => _targetGraphView.edges.ToList();
  private List<TerrainNodeView> Nodes => _targetGraphView.nodes.ToList().Cast<TerrainNodeView>().ToList();

  public static GraphSaveUtility GetInstance(TerrainGraphView targetGraphView) {

    return new GraphSaveUtility {
      _targetGraphView = targetGraphView
    };
  }

  public void SaveGraph(string fileName) {

    TerrainGraphContainer graphContainer = ScriptableObject.CreateInstance<TerrainGraphContainer>();
    if (!SaveNodes(graphContainer)) return;

    SaveExposedProperties(graphContainer);

    if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
      AssetDatabase.CreateFolder("Assets", "Resources");
    }

    AssetDatabase.CreateAsset(graphContainer, $"Assets/Resources/{fileName}.asset");
    AssetDatabase.SaveAssets();
  }

  private void SaveExposedProperties(TerrainGraphContainer terrainGraphContainer) {
    terrainGraphContainer.ExposedProperties.AddRange(_targetGraphView.exposedProperties);
  }

  private bool SaveNodes(TerrainGraphContainer terrainGraphContainer) {

    if (!Edges.Any()) return false;


    Edge[] connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
    for (int i = 0; i < connectedPorts.Length; i++) {
      TerrainNodeView outputNode = connectedPorts[i].output.node as TerrainNodeView;
      TerrainNodeView inputNode = connectedPorts[i].input.node as TerrainNodeView;

      terrainGraphContainer.NodeLinks.Add(
        new NodeLinkData {
          BaseNodeId = outputNode.Id,
          PortName = connectedPorts[i].output.name,
          TargetNodeId = inputNode.Id
        }
      );
    }

    foreach (TerrainNodeView terrainNode in Nodes.Where(node => !node.EntryPoint)) {
      terrainGraphContainer.TerrainNodeDatas.Add(
        new TerrainNodeData {
          NodeId = terrainNode.Id,
          Content = terrainNode.Content,
          Position = terrainNode.GetPosition().position
        }
      );
    }

    return true;
  }

  public void LoadGraph(string fileName) {
    _container = Resources.Load<TerrainGraphContainer>(fileName);
    if (_container == null) {
      EditorUtility.DisplayDialog("File Not Found", "File does not exist!", "OK");
      return;
    }

    ClearGraph();
    CreateNodes();
    ConnectNodes();
    CreateExposedProperties();

  }

  private void CreateExposedProperties() {

    _targetGraphView.ClearBlackBoardAndExposedProperties();

    foreach (ExposedProperty exposedProperty in _container.ExposedProperties) {
      _targetGraphView.AddPropertyToBlackBoard(exposedProperty);
    }
  }

  private void ClearGraph() {
    Nodes.Find(x => x.EntryPoint).Id = _container.NodeLinks[0].BaseNodeId;

    foreach (TerrainNodeView node in Nodes) {
      if (node.EntryPoint) continue;
      Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

      _targetGraphView.RemoveElement(node);
    }
  }

  private void CreateNodes() {
    foreach (TerrainNodeData nodeData in _container.TerrainNodeDatas) {
      TerrainNodeView tempNode = _targetGraphView.CreateTerrainNodeOld(nodeData.Content, Vector2.zero);
      tempNode.Id = nodeData.NodeId;
      _targetGraphView.AddElement(tempNode);

      List<NodeLinkData> nodePorts = _container.NodeLinks.Where(x => x.BaseNodeId == nodeData.NodeId).ToList();
      nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.PortName));
    }
  }

  private void ConnectNodes() {
    for (int i = 0; i < Nodes.Count; i++) {
      List<NodeLinkData> connections = _container.NodeLinks.Where(x => x.BaseNodeId == Nodes[i].Id).ToList();
      for (int j = 0; j < connections.Count; j++) {
        string targetNodeId = connections[j].TargetNodeId;
        TerrainNodeView targetNode = Nodes.First(x => x.Id == targetNodeId);
        LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

        targetNode.SetPosition(
          new Rect(_container.TerrainNodeDatas.First(x => x.NodeId == targetNode.Id).Position, _targetGraphView.defaultNodeSize)
        );
      }
    }
  }

  private void LinkNodes(Port output, Port input) {
    Edge tempEdge = new Edge { output = output, input = input };
    tempEdge.input.Connect(tempEdge);
    tempEdge.output.Connect(tempEdge);
    _targetGraphView.Add(tempEdge);
  }
}
