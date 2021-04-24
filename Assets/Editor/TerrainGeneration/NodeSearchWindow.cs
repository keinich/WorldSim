using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider {

  private TerrainGraphView graphView;
  private EditorWindow window;

  public void Init(EditorWindow w, TerrainGraphView gv) {
    graphView = gv;
    window = w;
  }

  public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {

    List<SearchTreeEntry> tree = new List<SearchTreeEntry> {
      new SearchTreeGroupEntry(new GUIContent("Create Elements"),level:0),
      new SearchTreeGroupEntry(new GUIContent("Natural"),level:1),
      new SearchTreeEntry(new GUIContent("Beach")) { level = 2},
      new SearchTreeGroupEntry(new GUIContent("Input"),level:1),
      new SearchTreeEntry(new GUIContent("Heightmap")) {
        userData=new HeightmapInputNodeView(graphView.terrainGenerator, ScriptableObject.CreateInstance<HeightMapInputNode>()), level=2
      },
      new SearchTreeGroupEntry(new GUIContent("Generators"),level:1),
      new SearchTreeEntry(new GUIContent("Perlin Noise")) {
        userData=new PerlinNoiseNodeView(graphView.terrainGenerator, CreateInstance<PerlinNoiseNode>()), level = 2
      },
      new SearchTreeEntry(new GUIContent("Blur")) {
        userData=new BlurNodeView(graphView.terrainGenerator, CreateInstance<BlurNode>()), level = 2
      },
      new SearchTreeEntry(new GUIContent("Height Curve")) {
        userData=new HeightCurveNodeView(graphView.terrainGenerator, CreateInstance<HeightCurveNode>()), level = 2
      }
    };

    return tree;
  }

  public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {

    var worldMousePosition = window.rootVisualElement.ChangeCoordinatesTo(
      window.rootVisualElement.parent,
      context.screenMousePosition - window.position.position
    );
    var localMousePosition = graphView.contentViewContainer.WorldToLocal(worldMousePosition);

    graphView.AddNode((TerrainNodeView)SearchTreeEntry.userData, localMousePosition);

    return true;
  }

}
