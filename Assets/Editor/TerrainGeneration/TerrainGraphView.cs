using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGraphView : GraphView {

  public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

  public Blackboard blackboard;
  public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
  private NodeSearchWindow nodeSearchWindow;
  private TerrainGenerator terrainGenerator;

  public TerrainGraphView(EditorWindow editorWindow) {

    styleSheets.Add(Resources.Load<StyleSheet>(path: "TerrainGraph"));
    SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

    this.AddManipulator(new ContentDragger());
    this.AddManipulator(new SelectionDragger());
    this.AddManipulator(new RectangleSelector());

    GridBackground grid = new GridBackground();
    Insert(index: 0, grid);
    grid.StretchToParentSize();

    AddElement(GenerateEntryPointNode());
    AddSearchWindow(editorWindow);
  }

  public void ClearBlackBoardAndExposedProperties() {
    exposedProperties.Clear();
    blackboard.Clear();
  }

  internal void AddPropertyToBlackBoard(ExposedProperty exposedProperty) {

    string localPropertyName = exposedProperty.PropertyName;
    string localPropertyValue = exposedProperty.PropetyValue;
    while (exposedProperties.Any(x => x.PropertyName == localPropertyName)) {
      localPropertyName = $"{localPropertyName}(1)";
    }

    ExposedProperty property = new ExposedProperty();
    property.PropertyName = localPropertyName;
    property.PropetyValue = localPropertyValue;
    exposedProperties.Add(property);

    VisualElement container = new VisualElement();
    BlackboardField blackboardField = new BlackboardField { text = property.PropertyName, typeText = "string property" };
    container.Add(blackboardField);

    TextField propertyValueTextField = new TextField(label: "Value:") {
      value = localPropertyValue
    };
    propertyValueTextField.RegisterValueChangedCallback(
      evt => {
        int changingPropertyIndex = exposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
        exposedProperties[changingPropertyIndex].PropetyValue = evt.newValue;
      }
    );
    BlackboardRow blackboardRow = new BlackboardRow(item: blackboardField, propertyView: propertyValueTextField);
    container.Add(blackboardRow);

    blackboard.Add(container);

  }

  internal void buildGraph(TerrainGenerator tg) {
    this.terrainGenerator = tg;
    if(tg.terrainGraph.resultNode is null) {
      tg.terrainGraph.resultNode = ScriptableObject.CreateInstance<ResultNode>();
    }
    this.CreateNode(tg.terrainGraph.resultNode, Vector2.zero);
    foreach (TerrainNode terrainNode in terrainGenerator.terrainGraph.terrainNodes) {
      this.CreateNode(terrainNode, Vector2.zero);
    }
  }

  private void AddSearchWindow(EditorWindow editorWindow) {
    nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
    nodeSearchWindow.Init(editorWindow, this);
    nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), nodeSearchWindow);
  }

  public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
    List<Port> compatiblePorts = new List<Port>();

    ports.ForEach(funcCall: (port) => {
      if (startPort != port && startPort.node != port.node) {
        compatiblePorts.Add(port);
      }
    });

    return compatiblePorts;
  }

  private TerrainNodeView GenerateEntryPointNode() {

    TerrainNodeView node = new TerrainNodeView() {
      title = "Start",
      Content = "ENTRYPOINT",
      EntryPoint = true
    };

    //Port generatedPort = GeneratePort(node, Direction.Output);
    //generatedPort.portName = "Next";
    //node.outputContainer.Add(generatedPort);

    node.capabilities &= ~Capabilities.Movable;
    node.capabilities &= ~Capabilities.Deletable;

    node.RefreshExpandedState();
    node.RefreshPorts();

    node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));

    return node;

  }

  public void AddNode(TerrainNodeView node, Vector2 position) {
    terrainGenerator.terrainGraph.terrainNodes.Add(node.terrainNode);
    node.SetPosition(new Rect(position: position, defaultNodeSize));
    AddElement(node);
  }

  public void CreateNode(TerrainNode terrainNode, Vector2 position) {
    switch (terrainNode) {
      case ResultNode resultNode:
        TerrainNodeView node = new ResultNodeView(terrainGenerator, resultNode);
        node.SetPosition(new Rect(position: position, defaultNodeSize));
        AddElement(node);
        break;
      case HeightMapInputNode heightmapInputNode:
        HeightmapInputNodeView nodeView = new HeightmapInputNodeView(heightmapInputNode);
        nodeView.SetPosition(new Rect(position: position, defaultNodeSize));
        AddElement(nodeView);
        break;
    }
  }

  internal TerrainNodeView CreateTerrainNodeOld(string nodeName, Vector2 position) {
    TerrainNodeView terrainNode = new TerrainNodeView {
      title = nodeName,
      Content = nodeName
    };

    Port inputPort = terrainNode.GeneratePort(Direction.Input, Port.Capacity.Multi);
    inputPort.portName = "Input";
    terrainNode.inputContainer.Add(inputPort);

    terrainNode.styleSheets.Add(Resources.Load<StyleSheet>(path: "Node"));

    Button button = new Button(clickEvent: () => { AddChoicePort(terrainNode); });
    button.text = "New Choice";
    terrainNode.titleContainer.Add(button);

    TextField textField = new TextField(string.Empty);
    textField.RegisterValueChangedCallback(
      evt => {
        terrainNode.Content = evt.newValue;
        terrainNode.title = evt.newValue;
      }
    );
    textField.SetValueWithoutNotify(terrainNode.title);
    terrainNode.mainContainer.Add(textField);

    terrainNode.RefreshExpandedState();
    terrainNode.RefreshPorts();
    terrainNode.SetPosition(new Rect(position: position, defaultNodeSize));

    return terrainNode;
  }

  public void AddChoicePort(TerrainNodeView terrainNode, string overridenPortName = "") {
    Port generatedPort = terrainNode.GeneratePort(Direction.Output);

    VisualElement oldLabel = generatedPort.contentContainer.Q<Label>("type");
    generatedPort.contentContainer.Remove(oldLabel);

    int outputPortCount = terrainNode.outputContainer.Query(name: "connector").ToList().Count;

    string choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Choice {outputPortCount}" : overridenPortName;

    TextField textField = new TextField {
      name = string.Empty,
      value = choicePortName
    };
    textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
    generatedPort.contentContainer.Add(new Label("  "));
    generatedPort.contentContainer.Add(textField);
    Button deleteButton = new Button(() => RemovePort(terrainNode, generatedPort)) {
      text = "X"
    };
    generatedPort.contentContainer.Add(deleteButton);

    generatedPort.portName = choicePortName;
    terrainNode.outputContainer.Add(generatedPort);
    terrainNode.RefreshExpandedState();
    terrainNode.RefreshPorts();
  }

  private void RemovePort(TerrainNodeView terrainNode, Port generatedPort) {
    IEnumerable<Edge> targetEdges = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

    if (targetEdges.Any()) {
      Edge targetEdge = targetEdges.First();
      targetEdge.input.Disconnect(targetEdge);
      RemoveElement(targetEdges.First());
    }

    terrainNode.outputContainer.Remove(generatedPort);
    terrainNode.RefreshPorts();
    terrainNode.RefreshExpandedState();

  }

}
