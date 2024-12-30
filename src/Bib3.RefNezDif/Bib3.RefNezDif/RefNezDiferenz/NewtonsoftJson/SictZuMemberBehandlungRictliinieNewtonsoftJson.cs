// Decompiled with JetBrains decompiler
// Type: Bib3.RefNezDiferenz.NewtonsoftJson.SictZuMemberBehandlungRictliinieNewtonsoftJson
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Bib3.RefNezDiferenz.NewtonsoftJson
{
  public class SictZuMemberBehandlungRictliinieNewtonsoftJson : IZuMemberEntscaidungBinäär
  {
    public virtual bool MemberBehandlung(MemberInfo herkunftMember)
    {
      if ((MemberInfo) null == herkunftMember)
        return false;
      object[] customAttributes1 = herkunftMember.GetCustomAttributes(true);
      if (customAttributes1 != null && ((IEnumerable<object>) customAttributes1).Any<object>((Func<object, bool>) (attribute => attribute is CompilerGeneratedAttribute)))
        return false;
      FieldInfo fieldInfo = herkunftMember as FieldInfo;
      PropertyInfo propertyInfo = herkunftMember as PropertyInfo;
      if ((PropertyInfo) null != propertyInfo && (MethodInfo) null == propertyInfo.SetMethod)
        return false;
      object[] customAttributes2 = herkunftMember.DeclaringType.GetCustomAttributes(true);
      MemberSerialization memberSerialization = (MemberSerialization) 0;
      if (customAttributes2 != null)
      {
        foreach (object obj in customAttributes2)
        {
          if (obj != null && obj is JsonObjectAttribute jsonObjectAttribute)
            memberSerialization = jsonObjectAttribute.MemberSerialization;
        }
      }
      JsonPropertyAttribute propertyAttribute1 = (JsonPropertyAttribute) null;
      if (customAttributes1 != null)
      {
        foreach (object obj in customAttributes1)
        {
          if (obj is JsonPropertyAttribute propertyAttribute2)
            propertyAttribute1 = propertyAttribute2;
        }
      }
      bool flag = false;
      if ((FieldInfo) null != fieldInfo)
      {
        if (memberSerialization == null || 2 == (int)memberSerialization)
          flag = true;
        if (1 == (int)memberSerialization && propertyAttribute1 != null)
          flag = true;
      }
      if ((PropertyInfo) null != propertyInfo)
      {
        MethodInfo setMethod = propertyInfo.SetMethod;
        MethodInfo getMethod = propertyInfo.GetMethod;
        if ((MethodInfo) null != setMethod && (MethodInfo) null != getMethod && memberSerialization == 0)
          flag = true;
        if (1 == (int)memberSerialization && propertyAttribute1 != null)
          flag = true;
      }
      return flag;
    }
  }
}
