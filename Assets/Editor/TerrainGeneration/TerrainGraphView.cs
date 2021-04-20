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

    node.capabilities &= ~Capabilities.Movable;
    node.capabilities &= ~Capabilities.Deletable;

    node.RefreshExpandedState();
    node.RefreshPorts();

    node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));

    return node;

  }

  public void CreateNode(string nodeName, Vector2 position) {
    AddElement(CreateTerrainNode(nodeName, position));
  }

  internal TerrainNode CreateTerrainNode(string nodeName, Vector2 position) {
    TerrainNode terrainNode = new TerrainNode {
      title = nodeName,
      Content = nodeName,
      Id = Guid.NewGuid().ToString()
    };

    Port inputPort = GeneratePort(terrainNode, Direction.Input, Port.Capacity.Multi);
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

  public void AddChoicePort(TerrainNode terrainNode, string overridenPortName = "") {
    Port generatedPort = GeneratePort(terrainNode, Direction.Output);

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

  private void RemovePort(TerrainNode terrainNode, Port generatedPort) {
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