using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public enum TerrainGraphPortType {
  HeightInput,
  HeightOutput
}

public class MyIEdgeConnectorListener : IEdgeConnectorListener {

  TerrainGraphPortType terrainGraphPortType;

  public MyIEdgeConnectorListener(TerrainGraphPortType pt) {
    terrainGraphPortType = pt;
  }

  public void OnDrop(GraphView graphView, Edge edge) {
    Port inputPort = edge.input;
    Port outputPort = edge.output;
    if (!(inputPort.userData is null) && inputPort.userData.GetType() == typeof(HeightmapOutputReceiver)) {
      if (!(outputPort.userData is null) && outputPort.userData.GetType() == typeof(HeightmapOutput)) {
        HeightmapOutputReceiver heightmapInput = (HeightmapOutputReceiver)inputPort.userData;
        HeightmapOutput heightmapOutput = (HeightmapOutput)outputPort.userData;
        heightmapInput.output = heightmapOutput;
      }
    }
  }

  public void OnDropOutsidePort(Edge edge, Vector2 position) {
    Debug.Log("Port dopped outside");
  }

}

public class TerrainGraphPortInfo {

  TerrainGraphPortType terrainPortType;

  public TerrainGraphPortInfo(TerrainGraphPortType pt) {
    terrainPortType = pt;
  }

}

public class TerrainNodeView : Node {

  public string Id;

  public string Content;

  public bool EntryPoint;

  public TerrainNode terrainNode;

  public Port GeneratePort(Direction portDirection, Port.Capacity capacity = Port.Capacity.Single) {
    Port result = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
    result.AddManipulator(new EdgeConnector<Edge>(new MyIEdgeConnectorListener(TerrainGraphPortType.HeightInput)));
    return result;
  }

  public Port GeneratePort(Direction portDirection, HeightmapOutput heightmapOutput, Port.Capacity capacity = Port.Capacity.Single) {
    Port result = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
    result.userData = heightmapOutput;
    result.AddManipulator(new EdgeConnector<Edge>(new MyIEdgeConnectorListener(TerrainGraphPortType.HeightInput)));
    return result;
  }

  public Port GeneratePort(Direction portDirection, HeightmapOutputReceiver heightmapInput, Port.Capacity capacity = Port.Capacity.Single) {
    Port result = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
    result.userData = heightmapInput;
    result.AddManipulator(new EdgeConnector<Edge>(new MyIEdgeConnectorListener(TerrainGraphPortType.HeightInput)));
    return result;
  }

  public void GeneratePorts() {
    foreach (HeightmapOutput heightmapOutput in this.terrainNode.heightmapOutputs) {
      Port outputNode = GeneratePort(Direction.Output, heightmapOutput, Port.Capacity.Multi);
      outputNode.portName = heightmapOutput.name;
      outputContainer.Add(outputNode);
    }
    foreach (HeightmapOutputReceiver heightmapInput in this.terrainNode.heightmapInputs) {
      Port inputNode = GeneratePort(Direction.Input, heightmapInput, Port.Capacity.Multi);
      inputNode.portName = heightmapInput.name;
      inputContainer.Add(inputNode);
    }
  }

}
