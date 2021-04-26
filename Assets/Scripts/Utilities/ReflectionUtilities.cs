using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class ObjectCrawlParams {
  public Func<PropertyInfo,object, bool> OnPropertyFound;

  public Func<FieldInfo, object, bool> OnFieldFound;
}

public static class ReflectionUtilities {

  public static void CrawlProperties(object o, ObjectCrawlParams p) {
    List<int> recursionStack = new List<int>();
    CrawlProperties(o, p, recursionStack);
  }
  private static void CrawlProperties(object o, ObjectCrawlParams p, List<int> recursionStack) {

    if (o is null) return;
    if (o.GetType().IsValueType) return;
    if (typeof(IList).IsAssignableFrom(o.GetType())) return;

    if (recursionStack.Contains(o.GetHashCode())) return;
    recursionStack.Add(o.GetHashCode());

    PropertyInfo[] properties = o.GetType().GetProperties();
    foreach (PropertyInfo propertyInfo in properties) {
      if (!propertyInfo.CanWrite) continue;

      if (p.OnPropertyFound.Invoke(propertyInfo, o)) continue;

      object propertyValue = propertyInfo.GetValue(o);
      CrawlProperties(propertyValue, p);
    }

    FieldInfo[] fieldInfos = o.GetType().GetFields();
    foreach (FieldInfo fieldInfo in fieldInfos) {

      if (fieldInfo.Name == "Empty") continue;

      if (p.OnFieldFound.Invoke(fieldInfo, o)) continue;

      object fieldValue = fieldInfo.GetValue(o);
      CrawlProperties(fieldValue, p);
    }

    recursionStack.Remove(o.GetHashCode());
  }

}
