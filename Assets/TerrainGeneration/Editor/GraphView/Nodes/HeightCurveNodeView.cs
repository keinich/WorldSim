using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightCurveNodeView : TerrainNodeView<HeightCurveNode> {

  public HeightCurveNodeView(TerrainGenerator tg, HeightCurveNode hn) : base(tg, hn) {
  }

  protected override void InitProperties() {
    HeightCurveField heightCurveField = new HeightCurveField(terrainNodeCasted.heightCurve, (ac) => terrainNodeCasted.heightCurve = ac);
    mainContainer.Add(heightCurveField.GetVisualElement());
  }

}
