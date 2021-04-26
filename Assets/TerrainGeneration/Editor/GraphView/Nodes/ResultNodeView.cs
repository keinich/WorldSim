using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ResultNodeView : TerrainNodeView<ResultNode> {

  public ResultNodeView(TerrainGenerator tg, ResultNode resultNode) : base(tg, resultNode) {
  }

  protected override void InitProperties() {
    Button generateButton = new Button(() => {
      terrainGenerator.GenerateFromGraph();
    }) { text = "Generate" };
    mainContainer.Add(generateButton);

    // resolution 
    FloatField heightScaleField = new FloatField("Height Scale");
    heightScaleField.SetValueWithoutNotify(terrainNodeCasted.heightScale);
    heightScaleField.RegisterValueChangedCallback((p) => terrainNodeCasted.heightScale = p.newValue);
    mainContainer.Add(heightScaleField);
  }

}
