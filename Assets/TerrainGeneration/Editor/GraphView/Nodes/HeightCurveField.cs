using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class HeightCurveField {

  private class HolderHack : ScriptableObject {
    public AnimationCurve animationCurve;
  }

  Action<AnimationCurve> onValueChanged;
  HolderHack holderHack;

  public HeightCurveField(AnimationCurve ac, Action<AnimationCurve> ovc) {

    holderHack = ScriptableObject.CreateInstance<HolderHack>();
    holderHack.animationCurve = ac;
    onValueChanged = ovc;
  }

  public VisualElement GetVisualElement() {
    VisualElement container = new VisualElement();

    SerializedObject serializedObject = new SerializedObject(holderHack);
    SerializedProperty serializedHeightMapProperty = serializedObject.FindProperty(nameof(HolderHack.animationCurve));

    PropertyField heightMapPropertyField = new UnityEditor.UIElements.PropertyField(serializedHeightMapProperty);
     

    heightMapPropertyField.RegisterValueChangeCallback(ace => {
      onValueChanged.Invoke(holderHack.animationCurve);
    }); 
    heightMapPropertyField.Bind(serializedObject);

    container.Add(heightMapPropertyField);
    return container;
  } 

}
