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

public class TestTextureContainer : ScriptableObject {
  public string texture;
  public Texture2D texture2;
}

public class HeightmapInputNodeView : TerrainNodeView {

  public Texture2D heightMap;

  public HeightmapInputNodeView(HeightMapInputNode heightMapInputNode) {
    terrainNode = heightMapInputNode;
    title = "Heightmap Input";

    Id = Guid.NewGuid().ToString();

    //Port outputPort = GeneratePort(Direction.Output, Port.Capacity.Multi);
    //outputPort.portName = "Height Output";
    //inputContainer.Add(outputPort);

    GeneratePorts();

    styleSheets.Add(Resources.Load<StyleSheet>(path: "Node"));

    TestTextureContainer testTextureContainer = new TestTextureContainer();
    SerializedObject serializedObject = new SerializedObject(testTextureContainer);
    SerializedProperty serializedProperty2 = serializedObject.FindProperty(nameof(TestTextureContainer.texture2));

    PropertyField pf = new UnityEditor.UIElements.PropertyField(serializedProperty2);
    VisualElement textureElement2 = pf;
    Image preeviewImage = new Image();
    preeviewImage.image = testTextureContainer.texture2;
    mainContainer.Add(preeviewImage);

    pf.RegisterValueChangeCallback(t => {
      preeviewImage.image = GetPreviewImage(testTextureContainer.texture2, this.contentRect.width);
      heightMapInputNode.heightMap = testTextureContainer.texture2;
    });
    textureElement2.Bind(serializedObject);
    //VisualElement extureElement2 = new UnityEngine.UIElements.Ima
    var container = new VisualElement();

    container.Add(textureElement2);

    mainContainer.Add(container);
    extensionContainer.Add(container);

    var objField = new ObjectField {
      objectType = typeof(GameObject),
      allowSceneObjects = false,
      value = null,
    };

    mainContainer.Add(objField);

    Button previewButton = new Button(() => Preview()) { text = "Preview" };

    mainContainer.Add(previewButton);

    RefreshExpandedState();
    RefreshPorts();

  }

  private void Preview() {
    TerrainGenerator tg = GameObject.FindObjectOfType<TerrainGenerator>();
  }

  private Texture GetPreviewImage(Texture2D texture2, float size) {
    if (size <= 0) return new Texture2D(10, 10);
    int textureSize = (int)size;
    Texture2D result = new Texture2D((int)size, (int)size);
    if (!texture2) return result;
    for (int i = 0; i < texture2.width; i++) {
      for (int j = 0; j < texture2.height; j++) {
        float percentX = ((float)(i)) / texture2.width;
        float percentY = ((float)(j)) / texture2.height;
        int resultI = (int)(percentX * size);
        int resultJ = (int)(percentY * size);
        result.SetPixel(resultI, resultJ, texture2.GetPixel(i, j));
        //result.SetPixel(resultI, resultJ, Color.red);
      }
    }
    result.Apply();
    return result;
  }
}

public class ResultNodeView : TerrainNodeView {

  public ResultNodeView(TerrainGenerator tg, ResultNode resultNode) {

    terrainNode = resultNode;

    title = "Result";
    Content = "Result";
    Id = Guid.NewGuid().ToString();

    GeneratePorts();

    styleSheets.Add(Resources.Load<StyleSheet>(path: "Node"));

    Button generateButton = new Button(() => {
      tg.GenerateFromGraph();
    }) { text = "Generate" };
    mainContainer.Add(generateButton);

    RefreshExpandedState();
    RefreshPorts();
  }
}