using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGraphView : GraphView {

  public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

  public Blackboard blackboard;
  public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
  private NodeSearchWindow nodeSearchWindow;
  public TerrainGenerator terrainGenerator;

  public TerrainGraphView(EditorWindow editorWindow) {

    styleSheets.Add(Resources.Load<StyleSheet>(path: "TerrainGraph"));
    SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

    this.AddManipulator(new ContentDragger());
    this.AddManipulator(new SelectionDragger());
    this.AddManipulator(new RectangleSelector());

    GridBackground grid = new GridBackground();
    Insert(index: 0, grid);
    grid.StretchToParentSize();

    //AddElement(GenerateEntryPointNode());
    AddSearchWindow(editorWindow);

    graphViewChanged = TerrainGraphViewChanged;
  }

  private GraphViewChange TerrainGraphViewChanged(GraphViewChange graphViewChange) {
    if (!(graphViewChange.elementsToRemove is null)) {
      foreach (GraphElement ge in graphViewChange.elementsToRemove) {
        switch (ge) {
          case TerrainNodeView nodeToRemove:
            terrainGenerator.terrainGraph.terrainNodes.Remove(nodeToRemove.terrainNode);
            break;
          case Edge edgeToRemove:
            Port inputPort = edgeToRemove.input;
            if (!(inputPort.userData is null) && inputPort.userData.GetType() == typeof(HeightmapOutputReceiver)) {
              HeightmapOutputReceiver heightmapInput = (HeightmapOutputReceiver)inputPort.userData;
              heightmapInput.output = null;
            }
            Debug.Log("Edge removed");
            break;
        }


        //TODO remove edges
      }
    }
    return graphViewChange;
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

    if (tg.terrainGraph.resultNode is null) {
      tg.terrainGraph.resultNode = ScriptableObject.CreateInstance<ResultNode>();
    }
    this.CreateNode(tg.terrainGraph.resultNode, tg.terrainGraph.resultNode.position);
    foreach (TerrainNode terrainNode in terrainGenerator.terrainGraph.terrainNodes) {
      this.CreateNode(terrainNode, terrainNode.position);
    }
    GenerateConnections(tg.terrainGraph.resultNode);
    foreach (TerrainNode terrainNode in terrainGenerator.terrainGraph.terrainNodes) {
      GenerateConnections(terrainNode);
    }
  }

  private void GenerateConnections(TerrainNode terrainNode) {
    foreach (HeightmapOutputReceiver heightmapInput in terrainNode.heightmapInputs) {
      HeightmapOutput heightmapOutput = heightmapInput.output;
      if (heightmapOutput is null) continue;
      TerrainNode outputNode = heightmapOutput.parent;
      if (outputNode is null) continue;
      TerrainNodeView outputNodeView = this.nodes.Cast<TerrainNodeView>().Where((n) => n.terrainNode.id == outputNode.id).FirstOrDefault();
      TerrainNodeView inputNodeView = this.nodes.Cast<TerrainNodeView>().Where((n) => n.terrainNode.id == terrainNode.id).First();
      if (outputNodeView is null) continue;
      Port inputPort = null;
      Port outputPort = null;
      for (int i = 0; i < outputNodeView.outputContainer.childCount; i++) {
        outputPort = outputNodeView.outputContainer[i].Query<Port>().Build().Where((p) => p.portName == heightmapOutput.name).FirstOrDefault();
        if (!(outputPort is null)) break;
      }
      for (int i = 0; i < inputNodeView.inputContainer.childCount; i++) {
        inputPort = inputNodeView.inputContainer[i].Query<Port>().Build().Where((p) => p.portName == heightmapInput.name).FirstOrDefault();
        if (!(inputPort is null)) break;
      }
      if (inputPort is null || outputPort is null) continue;
      LinkNodes(outputPort, inputPort);
    }
  }

  private void LinkNodes(Port output, Port input) {
    Edge tempEdge = new Edge { output = output, input = input };
    tempEdge.input.Connect(tempEdge);
    tempEdge.output.Connect(tempEdge);
    this.Add(tempEdge);
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

  public void AddNode(TerrainNodeView node, Vector2 position) {
    terrainGenerator.terrainGraph.terrainNodes.Add(node.terrainNode);
    node.SetPosition(new Rect(position: position, defaultNodeSize));
    AddElement(node);
  }

  public void CreateNode(TerrainNode terrainNode, Vector2 position) {
    TerrainNodeView nodeView1 = CreateTerrainNodeView(terrainNode);

    nodeView1.SetPosition(new Rect(position: position, defaultNodeSize));
    AddElement(nodeView1);
    return;
  }

  private TerrainNodeView CreateTerrainNodeView(TerrainNode terrainNode) {
    IEnumerable<Type> terrainNodeViewTypes = Assembly.GetExecutingAssembly().GetTypes().Where((t) => t.BaseType.BaseType == typeof(TerrainNodeView));
    foreach (Type terrainNodeViewType in terrainNodeViewTypes) {
      Type typeArgument = terrainNodeViewType.BaseType.GetGenericArguments()[0];
      if (typeArgument == terrainNode.GetType()) {
        object[] p = { terrainGenerator, terrainNode };
        return (TerrainNodeView)Activator.CreateInstance(terrainNodeViewType, p);
      }
    }
    return null;
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
