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

  public HeightmapInputNodeView(TerrainGenerator tg, HeightMapInputNode heightMapInputNode) : base(tg, heightMapInputNode) { 
  }

  protected override void InitProperties() {
    TextureField textureField = new TextureField(terrainNodeCasted.heightMap, (t) => terrainNodeCasted.heightMap = t);
    mainContainer.Add(textureField.GetVisualElement());

    var objField = new ObjectField {
      objectType = typeof(GameObject),
      allowSceneObjects = false,
      value = null,
    };

    mainContainer.Add(objField);
  }

}
