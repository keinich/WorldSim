using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainMeshGenerator  {

  public static Mesh ContructMesh() {

    Mesh mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule).GetComponent<Mesh>();
    return mesh;
  }

}
