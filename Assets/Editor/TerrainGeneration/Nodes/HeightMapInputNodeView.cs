using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class HeightmapInputNodeView : TerrainNodeView<HeightMapInputNode> {

  public class TestTextureContainer : ScriptableObject {
    public string texture;
    public Texture2D texture2;
  }

  public HeightmapInputNodeView(TerrainGenerator tg, HeightMapInputNode heightMapInputNode) {
    terrainNode = heightMapInputNode;
    title = "Heightmap Input";

    GeneratePorts();

    styleSheets.Add(Resources.Load<StyleSheet>(path: "Node"));

    TextureField textureField = new TextureField(heightMapInputNode.heightMap, (t) => heightMapInputNode.heightMap = t);
    mainContainer.Add(textureField.GetVisualElement());

    var objField = new ObjectField {
      objectType = typeof(GameObject),
      allowSceneObjects = false,
      value = null,
    };

    mainContainer.Add(objField);

    RefreshExpandedState();
    RefreshPorts();

  }
}
