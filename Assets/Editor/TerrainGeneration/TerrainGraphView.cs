using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGraphView : GraphView {

  private readonly Vector2 defaultNodeSize = new Vector2(150, 200);

  public TerrainGraphView() {

    styleSheets.Add(Resources.Load<StyleSheet>(path: "TerrainGraph"));
    SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

    this.AddManipulator(new ContentDragger());
    this.AddManipulator(new SelectionDragger());
    this.AddManipulator(new RectangleSelector());

    GridBackground grid = new GridBackground();
    Insert(index: 0, grid);
    grid.StretchToParentSize();

    AddElement(GenerateEntryPointNode());
  }

  public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
    List<Port> compatiblePorts = new List<Port>();

    ports.ForEach(funcCall: (port) => {
      if (startPort != port && startPort.node!=port.node) {
        compatiblePorts.Add(port);
      }
    });

    return compatiblePorts;
  }

  private Port GeneratePort(TerrainNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single) {
    return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
  }

  private TerrainNode GenerateEntryPointNode() {

    TerrainNode node = new TerrainNode() {
      title = "Start",
      Id = Guid.NewGuid().ToString(),
      Content = "ENTRYPOINT",
      EntryPoint = true
    };

    Port generatedPort = GeneratePort(node, Direction.Output);
    generatedPort.portName = "Next";
    node.outputContainer.Add(generatedPort);

    node.RefreshExpandedState();
    node.RefreshPorts();

    node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));

    return node;

  }

  public void CreateNode(string nodeName) {
    AddElement(CreateTerrainNode(nodeName));
  }

  internal TerrainNode CreateTerrainNode(string nodeName) {
    TerrainNode terrainNode = new TerrainNode {
      title = nodeName,
      Content = nodeName,
      Id = Guid.NewGuid().ToString()
    };

    Port inputPort = GeneratePort(terrainNode, Direction.Input, Port.Capacity.Multi);
    inputPort.portName = "Input";
    terrainNode.inputContainer.Add(inputPort);

    Button button = new Button(clickEvent: () => { AddChoicePort(terrainNode); });
    button.text = "New Choice";
    terrainNode.titleContainer.Add(button);

    terrainNode.RefreshExpandedState();
    terrainNode.RefreshPorts();
    terrainNode.SetPosition(new Rect(position: Vector2.zero, defaultNodeSize));

    return terrainNode;
  }

  private void AddChoicePort(TerrainNode terrainNode) {
    Port generatedPort = GeneratePort(terrainNode, Direction.Output);

    int outputPortCount = terrainNode.outputContainer.Query(name: "connector").ToList().Count;
    string outputPortName = $"Choice {outputPortCount}";

    terrainNode.outputContainer.Add(generatedPort);
    terrainNode.RefreshExpandedState();
    terrainNode.RefreshPorts();
  }

}
