using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResultNodeView : TerrainNodeView {

  public ResultNodeView(TerrainGenerator tg, ResultNode resultNode) {

    terrainNode = resultNode;

    title = "Result";
    Content = "Result";
    Id = Guid.NewGuid().ToString();

    GeneratePorts();

    styleSheets.Add(Resources.Load<StyleSheet>(path: "Node"));

    Button generateButton = new Button(() => {
      tg.GenerateFromGraph();
    }) { text = "Generate" };
    mainContainer.Add(generateButton);

    RefreshExpandedState();
    RefreshPorts();
  }
}