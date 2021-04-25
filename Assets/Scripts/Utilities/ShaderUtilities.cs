using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderUtilities : MonoBehaviour {

  public ComputeShader blurShader;

  static ShaderUtilities _Instance;
  
  static ShaderUtilities GetInstance() {
    if (!_Instance) {
      _Instance = FindObjectOfType<ShaderUtilities>();
    }
    return _Instance;
  }

  public static ComputeShader GetBlurShader() {
    return GetInstance().blurShader;
  }

  public static Texture2D Blur(Texture2D texture, float resolution, float size) {
    ComputeShader blurShader = GetShader("Blur");

    int width = texture.width;
    int height = texture.height;
    Texture2D result = new Texture2D(width, height);

    int kernelHandle = blurShader.FindKernel("CSMain");
    RenderTexture tex = new RenderTexture(width, height, 24);
    tex.enableRandomWrite = true;
    tex.Create();

    blurShader.SetTexture(kernelHandle, "Result", tex);
    blurShader.SetTexture(kernelHandle, "ImageInput", texture);
    //blurShader.SetFloats("iResolution", 1f, 1f, 1f);
    blurShader.SetFloats("iResolution", resolution, resolution, resolution);
    blurShader.SetFloat("Size", size);
    //blurShader.SetFloats("iFrame", 0.5f, 0.5f, 0.5f);
    //blurShader.SetFloats("iChannelResolution", 0.5f, 0.5f, 0.5f);
    //blurShader.SetFloats("iMouse", 0.5f, 0.5f, 0.5f);
    //blurShader.SetFloats("iDate", 0.5f, 0.5f, 0.5f);
    //blurShader.SetFloats("iSampleRate", 0.5f, 0.5f, 0.5f);

    blurShader.Dispatch(kernelHandle, width / 8, height / 8, 1);

    RenderTexture.active = tex;
    result.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
    result.Apply();

    return result;
  }

  internal static ComputeShader GetErosionShader() {
    return GetShader("Erosion");
  }

  public static ComputeShader GetShader(string shaderName) {
    ComputeShader[] compShaders = (ComputeShader[])Resources.FindObjectsOfTypeAll(typeof(ComputeShader));
    for (int i = 0; i < compShaders.Length; i++) {
      if (compShaders[i].name == shaderName) {
        return compShaders[i];
      }
    }
    Debug.LogError($"No shader with name {shaderName}");
    return null;
  }
}
