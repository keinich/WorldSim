using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PerlinNoiseNodeView : TerrainNodeView<PerlinNoiseNode>{

  public PerlinNoiseNodeView(TerrainGenerator tg, PerlinNoiseNode pn) : base(tg, pn) {
  }

  protected override void InitProperties() {

    // Persistance 
    FloatField persistanceField = new FloatField("Persistance");
    persistanceField.SetValueWithoutNotify(terrainNodeCasted.perlinNoiseParams.persistence);
    persistanceField.RegisterValueChangedCallback((p) => terrainNodeCasted.perlinNoiseParams.persistence = p.newValue);
    mainContainer.Add(persistanceField);

    // seed 
    IntegerField seedField = new IntegerField("Seed");
    seedField.SetValueWithoutNotify(terrainNodeCasted.perlinNoiseParams.seed);
    seedField.RegisterValueChangedCallback((p) => terrainNodeCasted.perlinNoiseParams.seed = p.newValue);
    mainContainer.Add(seedField);

    // lacunarity 
    FloatField lacunarityField = new FloatField("Lacunarity");
    lacunarityField.SetValueWithoutNotify(terrainNodeCasted.perlinNoiseParams.lacunarity);
    lacunarityField.RegisterValueChangedCallback((p) => terrainNodeCasted.perlinNoiseParams.lacunarity = p.newValue);
    mainContainer.Add(lacunarityField);

    // scale 
    FloatField scaleField = new FloatField("Initial Scale");
    scaleField.SetValueWithoutNotify(terrainNodeCasted.perlinNoiseParams.initialScale);
    scaleField.RegisterValueChangedCallback((p) => terrainNodeCasted.perlinNoiseParams.initialScale = p.newValue);
    mainContainer.Add(scaleField);
  }

}
