using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PerlinNoiseNodeView : TerrainNodeView<PerlinNoiseNode>{

  public PerlinNoiseNodeView(TerrainGenerator tg, PerlinNoiseNode pn) : base(tg, pn) {
  }

  protected override void InitProperties() {
    FloatField persistanceField = new FloatField("Persistance");
    persistanceField.SetValueWithoutNotify(terrainNodeCasted.perlinNoiseParams.persistence);
    persistanceField.RegisterValueChangedCallback((p) => terrainNodeCasted.perlinNoiseParams.persistence = p.newValue);
    mainContainer.Add(persistanceField);
  }

}
