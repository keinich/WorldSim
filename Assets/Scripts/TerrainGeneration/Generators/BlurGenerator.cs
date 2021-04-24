using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlurGenerator {

  public static float[] Apply(float[] inputMap, float resolution, float size) {
    Texture2D texture = ShaderUtilities.Blur(TextureGenerator.TextureFromHeightMap(inputMap), resolution, size);
    return TextureGenerator.ConvertTextureToHeightMap(texture);
  }

}
