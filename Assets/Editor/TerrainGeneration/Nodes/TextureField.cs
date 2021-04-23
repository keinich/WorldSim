using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TextureField {

  private class TextureHolderHack : ScriptableObject {
    public Texture2D texture;
  }

  Action<Texture2D> onValueChanged;
  TextureHolderHack textureHolderHack;

  public TextureField(Texture2D t, Action<Texture2D> ovc) {

    textureHolderHack = ScriptableObject.CreateInstance<TextureHolderHack>();
    textureHolderHack.texture = t;
    onValueChanged = ovc;
  }

  public VisualElement GetVisualElement() {
    VisualElement container = new VisualElement();

    SerializedObject serializedObject = new SerializedObject(textureHolderHack);
    SerializedProperty serializedHeightMapProperty = serializedObject.FindProperty(nameof(TextureHolderHack.texture));

    PropertyField heightMapPropertyField = new UnityEditor.UIElements.PropertyField(serializedHeightMapProperty);

    Image preeviewImage = new Image();
    preeviewImage.image = textureHolderHack.texture;

    heightMapPropertyField.RegisterValueChangeCallback(t => {
      preeviewImage.image = GetPreviewImage(textureHolderHack.texture, 100);
      onValueChanged.Invoke(textureHolderHack.texture);
    });
    heightMapPropertyField.Bind(serializedObject);

    container.Add(heightMapPropertyField);
    container.Add(preeviewImage);
    return container;
  }

  private Texture GetPreviewImage(Texture2D texture2, float size) {
    if (size <= 0) return new Texture2D(10, 10);
    int textureSize = (int)size;
    Texture2D result = new Texture2D((int)size, (int)size);
    if (!texture2) return result;
    for (int i = 0; i < texture2.width; i++) {
      for (int j = 0; j < texture2.height; j++) {
        float percentX = ((float)(i)) / texture2.width;
        float percentY = ((float)(j)) / texture2.height;
        int resultI = (int)(percentX * size);
        int resultJ = (int)(percentY * size);
        result.SetPixel(resultI, resultJ, texture2.GetPixel(i, j));
        //result.SetPixel(resultI, resultJ, Color.red);
      }
    }
    result.Apply();
    return result;
  }

}
