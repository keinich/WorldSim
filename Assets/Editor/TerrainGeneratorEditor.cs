using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {
  public override void OnInspectorGUI() {
    TerrainGenerator terrainGen = (TerrainGenerator)target;

    if (DrawDefaultInspector()) {
      if (terrainGen.autoUpdate) {
        terrainGen.DrawTerrainInEditor();
      }
    }

    if (GUILayout.Button("Generate")) {
      terrainGen.DrawTerrainInEditor();
    }
  }
}
