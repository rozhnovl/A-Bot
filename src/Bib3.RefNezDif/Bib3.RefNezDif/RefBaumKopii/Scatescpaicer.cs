// Decompiled with JetBrains decompiler
// Type: Bib3.RefBaumKopii.Scatescpaicer
// Assembly: Bib3.RefNezDif, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84B4EE4-406B-479A-B36F-73C2C03DE5B2
// Assembly location: C:\Src\A-Bot\lib\Bib3.RefNezDif.dll

using System;
using System.Collections.Generic;

namespace Bib3.RefBaumKopii
{
  public class Scatescpaicer
  {
    private readonly Dictionary<KeyValuePair<Type, Type>, ZuMemberMengeDelegate[]> DictZuHerkunftTypeUndZiilTypeMengeDelegate = new Dictionary<KeyValuePair<Type, Type>, ZuMemberMengeDelegate[]>();
    private readonly Dictionary<Type, bool> DictTypeErfordertKopiiRekurs = new Dictionary<Type, bool>();

    public ZuMemberMengeDelegate[] ZuTypeMengeDelegate<Type>() => this.ZuHerkunftTypeUndZiilTypeMengeDelegate<Type, Type>();

    public ZuMemberMengeDelegate[] ZuHerkunftTypeUndZiilTypeMengeDelegate<HerkunftType, ZiilType>() => this.ZuHerkunftTypeUndZiilTypeMengeDelegate(typeof (HerkunftType), typeof (ZiilType));

    public ZuMemberMengeDelegate[] ZuTypeMengeDelegate(Type type) => this.ZuHerkunftTypeUndZiilTypeMengeDelegate(type, type);

    public bool TypeErfordertKopiiRekurs(Type type)
    {
      if ((Type) null == type)
        return false;
      bool flag;
      if (!this.DictTypeErfordertKopiiRekurs.TryGetValue(type, out flag))
        this.DictTypeErfordertKopiiRekurs[type] = flag = RefBaumKopiiStatic.ErfordertKopiiRekursBerecne(type);
      return flag;
    }

    public ZuMemberMengeDelegate[] ZuHerkunftTypeUndZiilTypeMengeDelegate(
      Type herkunftType,
      Type ziilType)
    {
      if ((Type) null == herkunftType || (Type) null == ziilType)
        return (ZuMemberMengeDelegate[]) null;
      KeyValuePair<Type, Type> key = new KeyValuePair<Type, Type>(herkunftType, ziilType);
      ZuMemberMengeDelegate[] memberMengeDelegateArray = (ZuMemberMengeDelegate[]) null;
      this.DictZuHerkunftTypeUndZiilTypeMengeDelegate.TryGetValue(key, out memberMengeDelegateArray);
      if (memberMengeDelegateArray == null)
        this.DictZuHerkunftTypeUndZiilTypeMengeDelegate[key] = memberMengeDelegateArray = RefBaumKopiiStatic.ZuHerkunftTypeUndZiilTypeMengeDelegateBerecne(key.Key, key.Value);
      return memberMengeDelegateArray;
    }
  }
}
