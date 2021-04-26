using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class FieldGenerator {
  public abstract VisualElement GetVisualElement();
  public abstract void Init(Func<object> g, Action<object> s);
}

public class FieldGenerator<T> : FieldGenerator {



  public Func<T> getter;
  public Action<T> setter;
  public string name;

  public FieldGenerator(string n) {
    name = n;
  }

  public override VisualElement GetVisualElement() {
    return null;
    //holder = ScriptableObject.CreateInstance<HolderHack>();
    //holder.o = getter.Invoke();
    //VisualElement container = new VisualElement();

    //SerializedObject serializedObject = new SerializedObject(holder);
    //SerializedProperty serializedProperty = serializedObject.FindProperty(nameof(holder.o));

    //PropertyField propertyField = new UnityEditor.UIElements.PropertyField(serializedProperty);
    
    //propertyField.RegisterValueChangeCallback(t => {
    //  setter.Invoke((T)holder.o);
    //});
    //propertyField.Bind(serializedObject);

    //container.Add(propertyField);
    //return container;
  }

  public override void Init(Func<object> g, Action<object> s) {
    getter = () => (T)g.Invoke();
    setter = (nv) => s.Invoke(nv);
  }
}

public class FloatFieldGenerator : FieldGenerator<float> {

  public FloatFieldGenerator(string n) : base(n) {
  }

  public override VisualElement GetVisualElement() {
    FloatField result = new FloatField(name);
    result.SetValueWithoutNotify(getter.Invoke());
    result.RegisterValueChangedCallback((p) => setter.Invoke(p.newValue));
    return result;
  }
}

public class BoolFieldGenerator : FieldGenerator<bool> {

  public BoolFieldGenerator(string n) : base(n) {
  }

  public override VisualElement GetVisualElement() {
    Toggle result = new Toggle(name);
    result.SetValueWithoutNotify(getter.Invoke());
    result.RegisterValueChangedCallback((p) => setter.Invoke(p.newValue));
    return result;
  }
}

public class IntFieldGenerator : FieldGenerator<int> {

  public IntFieldGenerator(string n) : base(n) {
  }

  public override VisualElement GetVisualElement() {
    IntegerField result = new IntegerField(name);
    result.SetValueWithoutNotify(getter.Invoke());
    result.RegisterValueChangedCallback((p) => setter.Invoke(p.newValue));
    return result;
  }
}