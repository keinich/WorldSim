using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TypeIndexer {

  private static TypeIndexer _instance;
  private static TypeIndexer instance {
    get {
      if (_instance is null) _instance = new TypeIndexer();
      return _instance;
    }
  }

  private List<Assembly> _assemblies;

  private List<Assembly> assemblies {
    get {
      if (_assemblies is null) _assemblies = new List<Assembly>();
      return _assemblies;
    }
  }

  private Dictionary<string, List<Type>> _typesByAssemblyName;
  private Dictionary<string, List<Type>> typesByAssemblyName {
    get {
      if (_typesByAssemblyName is null) _typesByAssemblyName = new Dictionary<string, List<Type>>();
      return _typesByAssemblyName;
    }
  }

  private Dictionary<string, List<Type>> _applicableTypesByTypeName;
  private Dictionary<string, List<Type>> applicableTypesByTypeName {
    get {
      if (_applicableTypesByTypeName is null) _applicableTypesByTypeName = new Dictionary<string, List<Type>>();
      return _applicableTypesByTypeName;
    }
  }

  public List<Type> AllTypes {
    get {
      List<Type> result = new List<Type>();
      foreach (var kvp in typesByAssemblyName) {
        result.AddRange(kvp.Value);
      }
      return result;
    }
  }


  public TypeIndexer() {
    Init();
  }

  public void AddAssembly(Assembly assembly) {
    if (assemblies.FindAll((a) => a.FullName == assembly.FullName).Count > 0) return;
    assemblies.Add(assembly);
    if (typesByAssemblyName.ContainsKey(assembly.FullName)) return;
    typesByAssemblyName.Add(assembly.FullName, assembly.GetTypes().ToList());
  }

  private void Init() {
    Assembly executingAssembly = Assembly.GetExecutingAssembly();
    AddAssembly(executingAssembly);
  }

  public static List<Type> GetApplicableTypes(Type t) {
    instance.AddAssembly(Assembly.GetExecutingAssembly());
    instance.AddAssembly(Assembly.GetCallingAssembly());
    if (instance.applicableTypesByTypeName.ContainsKey(t.FullName)) return instance.applicableTypesByTypeName[t.FullName];
    List<Type> result = new List<Type>();
    foreach (Type canditateType in instance.AllTypes) {
      if (t.IsAssignableFrom(canditateType)) {
        result.Add(canditateType);
      }
    }
    instance.applicableTypesByTypeName.Add(t.FullName, result);
    return result;
  }

}
