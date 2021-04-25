using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class MyIEdgeConnectorListener : IEdgeConnectorListener {

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

public abstract class TerrainNodeView : Node {

  public bool EntryPoint;

  public TerrainNode terrainNode;

  public TerrainGenerator terrainGenerator;

  Image previewImage;

  public TerrainNodeView(TerrainGenerator tg, TerrainNode tn) {
    terrainNode = tn;
    terrainGenerator = tg;
    title = tn.nodeName;

    GeneratePorts();

    styleSheets.Add(Resources.Load<StyleSheet>(path: "Node"));

    InitProperties();

    UpdatePreview();

    RefreshExpandedState();
    RefreshPorts();
  }

  protected abstract void InitProperties();

  public Port GeneratePort(Direction portDirection, Port.Capacity capacity = Port.Capacity.Single) {
    Port result = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
    result.AddManipulator(new EdgeConnector<Edge>(new MyIEdgeConnectorListener()));
    return result;
  }

  public Port GeneratePort(Direction portDirection, HeightmapOutput heightmapOutput, Port.Capacity capacity = Port.Capacity.Single) {
    Port result = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
    result.userData = heightmapOutput;
    result.AddManipulator(new EdgeConnector<Edge>(new MyIEdgeConnectorListener()));
    return result;
  }

  public Port GeneratePort(Direction portDirection, HeightmapOutputReceiver heightmapInput, Port.Capacity capacity = Port.Capacity.Single) {
    Port result = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type: typeof(float));
    result.userData = heightmapInput;
    result.AddManipulator(new EdgeConnector<Edge>(new MyIEdgeConnectorListener()));
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

  public override void SetPosition(Rect newPos) {
    base.SetPosition(newPos);
    terrainNode.position = newPos.position;
  }

  protected void UpdatePreview() {
    HeightmapOutput heightmapOutput = terrainNode.heightmapOutputs.Find((x) => x.name == "Heightmap Output");
    if (!(heightmapOutput is null)) {
      if (previewImage is null) {
        previewImage = new Image();
        mainContainer.Add(previewImage);
      }
      float[,] previewMap = terrainNode.Generate(heightmapOutput, terrainGenerator.mapSize);
      Texture2D previewTexture = TextureGenerator.TextureFromHeightMap(previewMap);
      previewImage.image = previewTexture;
    }
    terrainGenerator.GenerateFromGraph();
  }
}

public abstract class TerrainNodeView<T> : TerrainNodeView where T : TerrainNode {

  public TerrainNodeView(TerrainGenerator tg, T tn) : base(tg, tn) {

  }

  protected T terrainNodeCasted => (T)terrainNode;

}