using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGraph : EditorWindow {

  private TerrainGraphView graphView;
  private string fileName = "New Terrain";

  [MenuItem("Graph/Terrain Graph")]
  public static void OpenTerrainGraphWindow() {
    TerrainGraph window = GetWindow<TerrainGraph>();
    window.titleContent = new GUIContent(text: "Terrain Editor");
  }

  private void OnEnable() {
    ConstructGraphView();
    GenerateToolbar();
    GenerateMiniMap();
    GenerateBlackBoard();
  }

  private void GenerateBlackBoard() {
    Blackboard blackboard = new Blackboard(graphView);
    blackboard.Add(new BlackboardSection { title = "Exposed Properties" });
    blackboard.addItemRequested = bb => { graphView.AddPropertyToBlackBoard(new ExposedProperty()); };
    blackboard.editTextRequested = (bb1, element, newValue) => {
      string oldPropertyName = ((BlackboardField)element).text;
      if (graphView.exposedProperties.Any(x => x.PropertyName == newValue)) {
        EditorUtility.DisplayDialog("Error", "Variable Name already in use", "OK");
        return;
      }

      int propertyIndex = graphView.exposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
      graphView.exposedProperties[propertyIndex].PropertyName = newValue;
      ((BlackboardField)element).text = newValue;
    };

    blackboard.SetPosition(new Rect(10, 30, 200, 300));
    graphView.blackboard = blackboard;
    graphView.Add(blackboard);
  }

  private void GenerateMiniMap() {
    MiniMap miniMap = new MiniMap { anchored = true };
    Vector2 coords = graphView.contentViewContainer.WorldToLocal(p: new Vector2(x: this.position.width - 10, y: 30));
    miniMap.SetPosition(new Rect(coords.x, coords.y, 200, 140));
    //miniMap.SetPosition(new Rect(0, 0, 200, 140));
    graphView.Add(miniMap);
  }

  private void OnDisable() {
    rootVisualElement.Remove(graphView);
  }

  private void ConstructGraphView() {
    graphView = new TerrainGraphView(this) {
      name = "Terrain Graph"
    };
    graphView.StretchToParentSize();
    rootVisualElement.Add(graphView);
  }

  private void GenerateToolbar() {
    Toolbar toolbar = new Toolbar();

    TextField fileNameTextField = new TextField(label: "File Name:");
    fileNameTextField.SetValueWithoutNotify(fileName);
    fileNameTextField.MarkDirtyRepaint();
    fileNameTextField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
    toolbar.Add(fileNameTextField);

    toolbar.Add(new Button(() => SaveData()) { text = "Save Data" }); ;
    toolbar.Add(new Button(() => LoadData()) { text = "Load Data" }); ;

    rootVisualElement.Add(toolbar);

  }

  private void LoadData() {
    RequestDataOperation(false);
  }

  private void SaveData() {
    RequestDataOperation(true);
  }

  private void RequestDataOperation(bool save) {
    if (string.IsNullOrEmpty(fileName)) {
      EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
      return;
    }

    GraphSaveUtility saveUtility = GraphSaveUtility.GetInstance(graphView);
    if (save) {
      saveUtility.SaveGraph(fileName);
    }
    else {
      saveUtility.LoadGraph(fileName);
    }
  }
}
