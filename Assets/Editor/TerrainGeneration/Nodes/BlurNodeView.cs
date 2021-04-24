using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BlurNodeView : TerrainNodeView<BlurNode> {

  public BlurNodeView(TerrainGenerator tg, BlurNode bn) : base(tg, bn) {
  }

  protected override void InitProperties() {
    // resolution 
    FloatField resolutionField = new FloatField("Resolution");
    resolutionField.SetValueWithoutNotify(terrainNodeCasted.resolution);
    resolutionField.RegisterValueChangedCallback((p) => terrainNodeCasted.resolution = p.newValue);
    mainContainer.Add(resolutionField);

    // size 
    FloatField sizeField = new FloatField("Size");
    sizeField.SetValueWithoutNotify(terrainNodeCasted.size);
    sizeField.RegisterValueChangedCallback((p) => terrainNodeCasted.size = p.newValue);
    mainContainer.Add(sizeField);
  }

}
