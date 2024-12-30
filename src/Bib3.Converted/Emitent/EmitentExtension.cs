// Decompiled with JetBrains decompiler
// Type: Bib3.Emitent.EmitentExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib3.Emitent
{
  public static class EmitentExtension
  {
    private static readonly IDictionary<WeakReference, EmitentExtension.EmitentInfo> VerzaicnisEmitent = (IDictionary<WeakReference, EmitentExtension.EmitentInfo>) new Dictionary<WeakReference, EmitentExtension.EmitentInfo>();
    private static readonly object LockVerzaicnis = new object();

    public static void FüügeAinNaacrict(this IEmitent emitent, object nacricht) => emitent.FüügeAinListeNaacrict((IEnumerable<object>) new List<object>()
    {
      nacricht
    });

    public static void FüügeAinListeNaacrict(
      this IEmitent emitent,
      IEnumerable<object> listeNaacrict)
    {
      if (listeNaacrict == null || listeNaacrict.Count<object>() < 1)
        return;
      EmitentExtension.EmitentInfo emitentInfo = EmitentExtension.GibInfoFürEmitent(emitent);
      lock (emitentInfo.ListeNaacrict)
      {
        foreach (object obj in listeNaacrict)
          emitentInfo.ListeNaacrict.Add(obj);
      }
    }

    public static IList<object> NimListeNaacrict(this IEmitent emitent)
    {
      if (emitent == null)
        return (IList<object>) null;
      EmitentExtension.EmitentInfo emitentInfo = EmitentExtension.GibInfoFürEmitent(emitent);
      lock (emitentInfo.ListeNaacrict)
      {
        List<object> objectList = new List<object>((IEnumerable<object>) emitentInfo.ListeNaacrict);
        emitentInfo.ListeNaacrict.Clear();
        return (IList<object>) objectList;
      }
    }

    public static object NimÄltesteNaacrict(this IEmitent emitent)
    {
      if (emitent == null)
        return (object) null;
      EmitentExtension.EmitentInfo emitentInfo = EmitentExtension.GibInfoFürEmitent(emitent);
      lock (emitentInfo.ListeNaacrict)
      {
        if (emitentInfo.ListeNaacrict.Count < 1)
          return (object) null;
        object obj = emitentInfo.ListeNaacrict.First<object>();
        emitentInfo.ListeNaacrict.RemoveAt(0);
        return obj;
      }
    }

    private static EmitentExtension.EmitentInfo GibInfoFürEmitent(IEmitent emitent)
    {
      lock (EmitentExtension.LockVerzaicnis)
      {
        int num = 0;
        foreach (KeyValuePair<WeakReference, EmitentExtension.EmitentInfo> keyValuePair in (IEnumerable<KeyValuePair<WeakReference, EmitentExtension.EmitentInfo>>) EmitentExtension.VerzaicnisEmitent)
        {
          object target = keyValuePair.Key.Target;
          if (target == emitent)
            return EmitentExtension.VerzaicnisEmitent[keyValuePair.Key];
          if (target == null)
            ++num;
        }
        if (EmitentExtension.VerzaicnisEmitent.Count / 2 + 10 < num)
          EmitentExtension.BerainigeVerzaicnisEmitent();
        EmitentExtension.EmitentInfo emitentInfo = new EmitentExtension.EmitentInfo();
        EmitentExtension.VerzaicnisEmitent[new WeakReference((object) emitent)] = emitentInfo;
        return emitentInfo;
      }
    }

    private static void BerainigeVerzaicnisEmitent()
    {
      List<WeakReference> source = new List<WeakReference>();
      foreach (KeyValuePair<WeakReference, EmitentExtension.EmitentInfo> keyValuePair in (IEnumerable<KeyValuePair<WeakReference, EmitentExtension.EmitentInfo>>) EmitentExtension.VerzaicnisEmitent)
      {
        if (!keyValuePair.Key.IsAlive)
          source.Add(keyValuePair.Key);
      }
      while (source.Count > 0)
      {
        EmitentExtension.VerzaicnisEmitent.Remove(source.First<WeakReference>());
        source.RemoveAt(0);
      }
    }

    private class EmitentInfo
    {
      public readonly IList<object> ListeNaacrict = (IList<object>) new List<object>();
    }
  }
}
