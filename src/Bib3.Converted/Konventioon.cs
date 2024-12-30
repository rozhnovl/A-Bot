// Decompiled with JetBrains decompiler
// Type: Bib3.Konventioon
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System.Xml.Linq;

namespace Bib3
{
  public class Konventioon
  {
    public static XElement SictwaiseXmlElement3VonNaacrict(object naacrict)
    {
      switch (naacrict)
      {
        case null:
          return new XElement((XName) "null");
        case XElement xelement:
          return xelement;
        case XObject content:
          return new XElement((XName) "BehältnisFürXObject", (object) content);
        default:
          return new XElement((XName) "Objekt", (object[]) new XObject[2]
          {
            (XObject) new XAttribute((XName) "Type.FullName", (object) naacrict.GetType().FullName),
            (XObject) new XAttribute((XName) "ToString", (object) naacrict.ToString())
          });
      }
    }
  }
}
