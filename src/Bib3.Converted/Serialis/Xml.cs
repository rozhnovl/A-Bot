// Decompiled with JetBrains decompiler
// Type: Bib3.Serialis.Xml
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Bib3.Serialis
{
  public static class Xml
  {
    public static XObject[] XmlVonObjekt(
      object objekt,
      out XObject[] abbildXml,
      KeyValuePair<Type, Func<object, KeyValuePair<XObject[], XObject>>>[] listeSictwaiseFürTyp)
    {
      abbildXml = (XObject[]) null;
      List<XObject> xobjectList1 = new List<XObject>();
      try
      {
        listeSictwaiseFürTyp = listeSictwaiseFürTyp ?? new KeyValuePair<Type, Func<object, KeyValuePair<XObject[], XObject>>>[0];
        xobjectList1.Add((XObject) new XAttribute((XName) "Objekt.Existent", (object) (objekt != null)));
        xobjectList1.Add((XObject) new XAttribute((XName) "ListeSictwaiseFürTyp.Length", (object) listeSictwaiseFürTyp.Length));
        if (objekt == null)
          return xobjectList1.ToArray();
        Type type1 = objekt.GetType();
        xobjectList1.Add((XObject) new XAttribute((XName) "Objekt.Typ.FullName", (object) type1.FullName));
        MemberInfo[] members = type1.GetMembers();
        List<XObject> content1 = new List<XObject>();
        List<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>> source1 = new List<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>>();
        try
        {
          foreach (MemberInfo key1 in members)
          {
            bool flag1 = false;
            XObject key2 = (XObject) null;
            List<XObject> content2 = new List<XObject>();
            try
            {
              foreach (object customAttribute in key1.GetCustomAttributes(true))
              {
                if (customAttribute is SerialisatioonWertOptionaal)
                  flag1 = true;
              }
              MemberTypes memberType = key1.MemberType;
              Type declaringType = key1.DeclaringType;
              content2.Add((XObject) new XAttribute((XName) "Name", (object) key1.Name.ToString()));
              content2.Add((XObject) new XAttribute((XName) "MemberTypeGrob", (object) memberType.ToString()));
              content2.Add((XObject) new XAttribute((XName) "DeclaringType.FullName", (object) declaringType.FullName.ToString()));
              FieldInfo fieldInfo = key1 as FieldInfo;
              PropertyInfo propertyInfo = key1 as PropertyInfo;
              content2.Add((XObject) new XAttribute((XName) "MemberAlsField.Existent", (object) (fieldInfo != (FieldInfo) null)));
              content2.Add((XObject) new XAttribute((XName) "MemberAlsProperty.Existent", (object) (propertyInfo != (PropertyInfo) null)));
              string str1 = (string) null;
              XObject[] xobjectArray1 = (XObject[]) null;
              object obj1 = (object) null;
              if (fieldInfo != (FieldInfo) null)
              {
                Type fieldType = fieldInfo.FieldType;
                content2.Add((XObject) new XAttribute((XName) "FieldType.FullName", (object) fieldType.FullName.ToString()));
                object obj2 = fieldInfo.GetValue(objekt);
                content2.Add((XObject) new XAttribute((XName) "FieldValue.Existent", (object) (obj2 != null)));
                if (obj1 == null)
                  obj1 = obj2;
              }
              if (propertyInfo != (PropertyInfo) null)
              {
                Type propertyType = propertyInfo.PropertyType;
                content2.Add((XObject) new XAttribute((XName) "PropertyType.FullName", (object) propertyType.FullName.ToString()));
                object obj3 = propertyInfo.GetValue(objekt, (object[]) null);
                content2.Add((XObject) new XAttribute((XName) "PropertyValue.Existent", (object) (obj3 != null)));
                if (obj1 == null)
                  obj1 = obj3;
              }
              if (obj1 != null)
              {
                Type MemberType = obj1.GetType();
                List<XObject> content3 = new List<XObject>();
                try
                {
                  Func<object, KeyValuePair<XObject[], XObject>> func = ((IEnumerable<KeyValuePair<Type, Func<object, KeyValuePair<XObject[], XObject>>>>) listeSictwaiseFürTyp).Where<KeyValuePair<Type, Func<object, KeyValuePair<XObject[], XObject>>>>((Func<KeyValuePair<Type, Func<object, KeyValuePair<XObject[], XObject>>>, bool>) (sictwaiseFürTyp => sictwaiseFürTyp.Key == MemberType)).FirstOrDefault<KeyValuePair<Type, Func<object, KeyValuePair<XObject[], XObject>>>>().Value;
                  content3.Add((XObject) new XAttribute((XName) "FieldTypSictwaiseAusArgument.Existent", (object) (func != null)));
                  if (func != null)
                  {
                    KeyValuePair<XObject[], XObject> keyValuePair = func(obj1);
                    content3.Add((XObject) new XElement((XName) "SictwaiseAusArgument.Bericht", (object[]) keyValuePair.Key));
                    XObject xobject = keyValuePair.Value;
                    content3.Add((XObject) new XAttribute((XName) "SictwaiseAusArgument.Abbild.Existent", (object) (xobject != null)));
                    if (xobject != null)
                    {
                      Type type2 = xobject.GetType();
                      content3.Add((XObject) new XAttribute((XName) "SictwaiseAusArgument.Abbild.Typ.FullName", (object) type2.FullName));
                      if (xobject is XAttribute xattribute)
                        str1 = xattribute.Value;
                      if (xobject is XElement xelement)
                        xobjectArray1 = xelement.Attributes().OfType<XObject>().Concat<XObject>(xelement.Elements().OfType<XObject>()).ToArray<XObject>();
                    }
                  }
                }
                catch (Exception ex)
                {
                  content3.Add((XObject) Glob.SictwaiseXElement(ex));
                }
                finally
                {
                  content2.Add((XObject) new XElement((XName) "SictwaiseAusArgument", (object) content3));
                }
                List<XObject> content4 = new List<XObject>();
                try
                {
                  content4.Add((XObject) new XAttribute((XName) "FieldType.IsArray", (object) MemberType.IsArray));
                  if (MemberType.IsArray)
                  {
                    Array array = obj1 as Array;
                    content4.Add((XObject) new XAttribute((XName) "MemberValueAlsArray.LongLength", (object) array.LongLength));
                    content4.Add((XObject) new XAttribute((XName) "MemberValueAlsArray.Rank", (object) array.Rank));
                    if (array.Rank != 1)
                      throw new NotImplementedException("MemberValueAlsArray.Rank\t!= 1");
                    List<KeyValuePair<XObject[], XObject[]>> source2 = new List<KeyValuePair<XObject[], XObject[]>>();
                    for (long index = 0; index < array.LongLength; ++index)
                    {
                      XObject[] abbildXml1 = (XObject[]) null;
                      List<XObject> xobjectList2 = new List<XObject>();
                      try
                      {
                        xobjectList2.Add((XObject) new XAttribute((XName) "Index", (object) index));
                        XObject[] xobjectArray2 = Bib3.Serialis.Xml.XmlVonObjekt(array.GetValue(index), out abbildXml1, listeSictwaiseFürTyp);
                        xobjectList2.Add((XObject) new XElement((XName) "SictXml", (object[]) xobjectArray2));
                      }
                      finally
                      {
                        source2.Add(new KeyValuePair<XObject[], XObject[]>(xobjectList2.ToArray(), abbildXml1));
                      }
                    }
                    content4.Add((XObject) new XElement((XName) "ListeElementSictXmlBerict", (object) source2.Select<KeyValuePair<XObject[], XObject[]>, XElement>((Func<KeyValuePair<XObject[], XObject[]>, XElement>) (elementSictXml => new XElement((XName) "Element", (object[]) elementSictXml.Key)))));
                    xobjectArray1 = (XObject[]) source2.Select<KeyValuePair<XObject[], XObject[]>, XElement>((Func<KeyValuePair<XObject[], XObject[]>, XElement>) (elementSictXml => new XElement((XName) "Element", (object[]) elementSictXml.Value))).ToArray<XElement>();
                  }
                }
                catch (Exception ex)
                {
                  content4.Add((XObject) Glob.SictwaiseXElement(ex));
                }
                finally
                {
                  content2.Add((XObject) new XElement((XName) "SictArray", (object) content4));
                }
                string str2 = obj1.ToString();
                if (xobjectArray1 != null)
                  key2 = (XObject) new XElement((XName) key1.Name, (object[]) xobjectArray1);
                else if (str2 != null)
                  key2 = (XObject) new XAttribute((XName) key1.Name, (object) str2);
              }
            }
            catch (Exception ex)
            {
              content2.Add((XObject) Glob.SictwaiseXElement(ex));
            }
            finally
            {
              bool flag2 = key2 != null | flag1;
              source1.Add(new KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>(key1, new KeyValuePair<XObject, bool>(key2, flag2)));
              content2.Add((XObject) new XAttribute((XName) "Erfolg", (object) flag2));
              content1.Add((XObject) new XElement((XName) "Member", (object) content2));
            }
          }
        }
        finally
        {
          xobjectList1.Add((XObject) new XElement((XName) "ListeMember", (object) content1));
        }
        KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>[] array1 = source1.Where<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>>((Func<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>, bool>) (memberSictXml => memberSictXml.Key.MemberType == MemberTypes.Field || memberSictXml.Key.MemberType == MemberTypes.Property)).ToArray<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>>();
        bool flag = ((IEnumerable<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>>) array1).Count<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>>((Func<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>, bool>) (memberAbbild => !memberAbbild.Value.Value)) < 1;
        xobjectList1.Add((XObject) new XAttribute((XName) "ListeMember.AbbildXml.Erfolg", (object) flag));
        if (flag)
          abbildXml = ((IEnumerable<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>>) array1).Select<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>, XObject>((Func<KeyValuePair<MemberInfo, KeyValuePair<XObject, bool>>, XObject>) (memberAbbild => memberAbbild.Value.Key)).ToArray<XObject>();
      }
      catch (Exception ex)
      {
        xobjectList1.Add((XObject) Glob.SictwaiseXElement(ex));
      }
      finally
      {
        xobjectList1.Add((XObject) new XAttribute((XName) "Erfolg", (object) (abbildXml != null)));
      }
      return xobjectList1.ToArray();
    }

    public static XObject[] ObjektVonXml<ObjektTyp>(
      XObject abbildXml,
      out ObjektTyp objekt,
      KeyValuePair<Type, Func<XObject, KeyValuePair<XObject[], object>>>[] listeSictwaiseFürTyp)
    {
      object objekt1;
      XObject[] xobjectArray = Bib3.Serialis.Xml.ObjektVonXml(abbildXml, typeof (ObjektTyp), out objekt1, listeSictwaiseFürTyp);
      objekt = (ObjektTyp) objekt1;
      return xobjectArray;
    }

    public static XObject[] ObjektVonXml(
      XObject abbildXml,
      Type objektTyp,
      out object objekt,
      KeyValuePair<Type, Func<XObject, KeyValuePair<XObject[], object>>>[] listeSictwaiseFürTyp)
    {
      objekt = (object) null;
      List<XObject> xobjectList1 = new List<XObject>();
      try
      {
        listeSictwaiseFürTyp = listeSictwaiseFürTyp ?? new KeyValuePair<Type, Func<XObject, KeyValuePair<XObject[], object>>>[0];
        xobjectList1.Add((XObject) new XAttribute((XName) "AbbildXml.Existent", (object) (abbildXml != null)));
        xobjectList1.Add((XObject) new XAttribute((XName) "ObjektTyp.Existent", (object) (objektTyp != (Type) null)));
        xobjectList1.Add((XObject) new XAttribute((XName) "ListeSictwaiseFürTyp.Length", (object) listeSictwaiseFürTyp.Length));
        if (abbildXml == null || objektTyp == (Type) null)
          return xobjectList1.ToArray();
        xobjectList1.Add((XObject) new XAttribute((XName) "ObjektTyp.FullName", (object) objektTyp.FullName));
        List<XObject> content1 = new List<XObject>();
        try
        {
          Func<XObject, KeyValuePair<XObject[], object>> objekt1 = ((IEnumerable<KeyValuePair<Type, Func<XObject, KeyValuePair<XObject[], object>>>>) listeSictwaiseFürTyp).Where<KeyValuePair<Type, Func<XObject, KeyValuePair<XObject[], object>>>>((Func<KeyValuePair<Type, Func<XObject, KeyValuePair<XObject[], object>>>, bool>) (sictwaiseFürTyp => sictwaiseFürTyp.Key == objektTyp)).FirstOrDefault<KeyValuePair<Type, Func<XObject, KeyValuePair<XObject[], object>>>>().Value;
          content1.Add((XObject) new XAttribute((XName) "ObjektTypSictAusArgument", (object) Glob.TypeFullNameSictString((object) objekt1)));
          if (objekt1 != null)
          {
            KeyValuePair<XObject[], object> keyValuePair = objekt1(abbildXml);
            content1.Add((XObject) new XElement((XName) "SictAusArgument.Bericht", (object[]) keyValuePair.Key));
            content1.Add((XObject) new XAttribute((XName) "SictAusArgument.Abbild.Existent", (object) (keyValuePair.Value != null)));
            object obj = keyValuePair.Value;
            if (obj != null)
            {
              Type type = obj.GetType();
              content1.Add((XObject) new XAttribute((XName) "SictAusArgument.Abbild.Typ.FullName", (object) type.FullName));
              if (!objektTyp.IsAssignableFrom(type))
                throw new ArgumentException("!ObjektTyp.IsAssignableFrom(SictAusArgumentAbbildTyp)");
              objekt = obj;
            }
          }
        }
        finally
        {
          xobjectList1.Add((XObject) new XElement((XName) "SictAusArgumentListeSict", (object) content1));
        }
        XElement objekt2 = abbildXml as XElement;
        if (objekt == null)
        {
          List<XObject> content2 = new List<XObject>();
          try
          {
            content2.Add((XObject) new XAttribute((XName) "ObjektTyp.IsArray", (object) objektTyp.IsArray));
            if (objektTyp.IsArray)
            {
              content2.Add((XObject) new XAttribute((XName) "AbbildXmlAlsXElement", (object) Glob.TypeFullNameSictString((object) objekt2)));
              if (objekt2 != null)
              {
                Type elementType = objektTyp.GetElementType();
                content2.Add((XObject) new XAttribute((XName) "ElementType", (object) Glob.TypeFullNameSictString((object) elementType)));
                content2.Add((XObject) new XAttribute((XName) "ElementType.Name", (object) elementType.Name));
                XElement[] array = objekt2.Elements().ToArray<XElement>();
                content2.Add((XObject) new XAttribute((XName) "ListeElementAbbildXml.Anzaal", (object) array.Length));
                List<object> objectList = new List<object>();
                List<XObject> xobjectList2 = new List<XObject>();
                foreach (XElement abbildXml1 in array)
                {
                  List<XObject> content3 = new List<XObject>();
                  try
                  {
                    object objekt3 = (object) null;
                    XObject[] xobjectArray = Bib3.Serialis.Xml.ObjektVonXml((XObject) abbildXml1, elementType, out objekt3, listeSictwaiseFürTyp);
                    content3.Add((XObject) new XElement((XName) "Sict", (object[]) xobjectArray));
                    content3.Add((XObject) new XElement((XName) "Objekt", objekt3));
                    objectList.Add(objekt3);
                  }
                  finally
                  {
                    xobjectList2.Add((XObject) new XElement((XName) "Element", (object) content3));
                  }
                }
                Array instance = Array.CreateInstance(elementType, objectList.Count);
                for (int index = 0; index < objectList.Count; ++index)
                  instance.SetValue(objectList[index], index);
                objekt = (object) instance;
              }
            }
          }
          finally
          {
            xobjectList1.Add((XObject) new XElement((XName) "SictArray", (object) content2));
          }
        }
        if (objekt == null)
        {
          List<XObject> content4 = new List<XObject>();
          try
          {
            content4.Add((XObject) new XAttribute((XName) "AbbildXmlAlsXElement", (object) Glob.TypeFullNameSictString((object) objekt2)));
            if (objekt2 != null)
            {
              MemberInfo[] members = objektTyp.GetMembers();
              List<XObject> content5 = new List<XObject>();
              List<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>> source = new List<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>>();
              try
              {
                foreach (MemberInfo memberInfo in members)
                {
                  MemberInfo Member = memberInfo;
                  object objekt4 = (object) null;
                  bool flag = false;
                  List<XObject> content6 = new List<XObject>();
                  try
                  {
                    MemberTypes memberType = Member.MemberType;
                    Type declaringType = Member.DeclaringType;
                    content6.Add((XObject) new XAttribute((XName) "Name", (object) Member.Name.ToString()));
                    content6.Add((XObject) new XAttribute((XName) "MemberType", (object) memberType.ToString()));
                    content6.Add((XObject) new XAttribute((XName) "DeclaringType.FullName", (object) declaringType.FullName.ToString()));
                    XAttribute[] array1 = objekt2.Attributes().Where<XAttribute>((Func<XAttribute, bool>) (atttribut => atttribut.Name.LocalName.Equals(Member.Name))).ToArray<XAttribute>();
                    XAttribute xattribute = ((IEnumerable<XAttribute>) array1).FirstOrDefault<XAttribute>();
                    XElement[] array2 = objekt2.Elements().Where<XElement>((Func<XElement, bool>) (element => element.Name.LocalName.Equals(Member.Name))).ToArray<XElement>();
                    XElement xelement = ((IEnumerable<XElement>) array2).FirstOrDefault<XElement>();
                    content6.Add((XObject) new XAttribute((XName) "Member.ListeAbbildXmlAttribut.Length", (object) array1.Length));
                    content6.Add((XObject) new XAttribute((XName) "Member.ListeAbbildXmlElement.Length", (object) array2.Length));
                    XObject abbildXml2 = (XObject) xelement ?? (XObject) xattribute;
                    content6.Add((XObject) new XAttribute((XName) "MemberAbbildXml.Existent", (object) (abbildXml2 != null)));
                    FieldInfo fieldInfo = Member as FieldInfo;
                    content6.Add((XObject) new XAttribute((XName) "MemberAlsField.Existent", (object) (fieldInfo != (FieldInfo) null)));
                    if (fieldInfo != (FieldInfo) null)
                    {
                      if (abbildXml2 == null)
                      {
                        List<XObject> content7 = new List<XObject>();
                        try
                        {
                          SerialisatioonWertSctandard objekt5 = ((IEnumerable<SerialisatioonWertSctandard>) ((IEnumerable<object>) fieldInfo.GetCustomAttributes(true)).Where<object>((Func<object, bool>) (atribut => typeof (SerialisatioonWertSctandard).IsAssignableFrom(atribut.GetType()))).Select<object, SerialisatioonWertSctandard>((Func<object, SerialisatioonWertSctandard>) (atribut => (SerialisatioonWertSctandard) atribut)).ToArray<SerialisatioonWertSctandard>()).FirstOrDefault<SerialisatioonWertSctandard>();
                          content7.Add((XObject) new XAttribute((XName) "AtributWertSctandard", (object) Glob.TypeFullNameSictString((object) objekt5)));
                          if (objekt5 != null)
                          {
                            object wertSctandard = objekt5.WertSctandard;
                            content7.Add((XObject) new XAttribute((XName) "WertSctandard", (object) Glob.TypeFullNameSictString(wertSctandard)));
                            if (wertSctandard == null)
                            {
                              if (fieldInfo.FieldType.IsValueType)
                                throw new ArgumentNullException("WertSctandard");
                              objekt4 = (object) null;
                            }
                            else
                            {
                              if (!fieldInfo.FieldType.IsAssignableFrom(wertSctandard.GetType()))
                                throw new ArgumentException("MemberAlsField.FieldType.IsAssignableFrom(WertSctandard.GetType())");
                              objekt4 = wertSctandard;
                            }
                            flag = true;
                          }
                        }
                        catch (Exception ex)
                        {
                          content7.Add((XObject) Glob.SictwaiseXElement(ex));
                        }
                        finally
                        {
                          content6.Add((XObject) new XElement((XName) "MemberSictObjektVonWertSctandard", (object) content7));
                        }
                      }
                      else
                      {
                        XObject[] xobjectArray = Bib3.Serialis.Xml.ObjektVonXml(abbildXml2, fieldInfo.FieldType, out objekt4, listeSictwaiseFürTyp);
                        content6.Add((XObject) new XElement((XName) "MemberSictObjektVonXml", (object[]) xobjectArray));
                        flag = objekt4 != null;
                      }
                    }
                  }
                  catch (Exception ex)
                  {
                    content6.Add((XObject) Glob.SictwaiseXElement(ex));
                  }
                  finally
                  {
                    source.Add(new KeyValuePair<MemberInfo, KeyValuePair<object, bool>>(Member, new KeyValuePair<object, bool>(objekt4, flag)));
                    content6.Add((XObject) new XAttribute((XName) "Erfolg", (object) flag));
                    content5.Add((XObject) new XElement((XName) "Member", (object) content6));
                  }
                }
              }
              finally
              {
                xobjectList1.Add((XObject) new XElement((XName) "ListeMember", (object) content5));
              }
              KeyValuePair<MemberInfo, KeyValuePair<object, bool>>[] array = source.Where<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>>((Func<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>, bool>) (memberAbbildObjekt => memberAbbildObjekt.Key.MemberType == MemberTypes.Field)).ToArray<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>>();
              bool flag1 = ((IEnumerable<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>>) array).Count<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>>((Func<KeyValuePair<MemberInfo, KeyValuePair<object, bool>>, bool>) (memberAbbild => !memberAbbild.Value.Value)) < 1;
              xobjectList1.Add((XObject) new XAttribute((XName) "ListeMember.AbbildObjekt.Erfolg", (object) flag1));
              if (flag1)
              {
                List<XObject> content8 = new List<XObject>();
                try
                {
                  ConstructorInfo[] constructors = objektTyp.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                  content8.Add((XObject) new XAttribute((XName) "ObjektTyp.ListeKonstruktor.Length", (object) constructors.Length));
                  ConstructorInfo constructorInfo = ((IEnumerable<ConstructorInfo>) constructors).Where<ConstructorInfo>((Func<ConstructorInfo, bool>) (konstruktor => konstruktor.GetParameters().Length < 1)).FirstOrDefault<ConstructorInfo>();
                  content8.Add((XObject) new XAttribute((XName) "ObjektTyp.KonstruktorOhneParameter.Existent", (object) (constructorInfo != (ConstructorInfo) null)));
                  objekt = !(constructorInfo == (ConstructorInfo) null) ? constructorInfo.Invoke(new object[0]) : throw new ArgumentNullException("ObjektTyp.KonstruktorOhneParameter");
                  foreach (KeyValuePair<MemberInfo, KeyValuePair<object, bool>> keyValuePair in array)
                    (keyValuePair.Key as FieldInfo).SetValue(objekt, keyValuePair.Value.Key);
                }
                catch (Exception ex)
                {
                  content8.Add((XObject) Glob.SictwaiseXElement(ex));
                }
                finally
                {
                  xobjectList1.Add((XObject) new XElement((XName) "Objekt.Konstruktioon", (object) content8));
                }
              }
            }
          }
          finally
          {
            xobjectList1.Add((XObject) new XElement((XName) "SictKomposition", (object) content4));
          }
        }
      }
      catch (Exception ex)
      {
        xobjectList1.Add((XObject) Glob.SictwaiseXElement(ex));
      }
      finally
      {
        xobjectList1.Add((XObject) new XAttribute((XName) "Erfolg", (object) (objekt != null)));
      }
      return xobjectList1.ToArray();
    }
  }
}
