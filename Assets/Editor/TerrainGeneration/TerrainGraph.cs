using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
  }

  private void OnDisable() {
    rootVisualElement.Remove(graphView);
  }

  private void ConstructGraphView() {
    graphView = new TerrainGraphView() {
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

    Button nodeCreateButton = new Button(
      clickEvent: () => { graphView.CreateNode("Terrain Node"); }
    );
    nodeCreateButton.text = "Create Node";
    toolbar.Add(nodeCreateButton);

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
    } else {
      saveUtility.LoadGraph(fileName);
    }
  }
}
