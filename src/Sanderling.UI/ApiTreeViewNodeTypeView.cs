// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.ApiTreeViewNodeTypeView
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using Fasterflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.UI
{
  public class ApiTreeViewNodeTypeView
  {
    public ApiTreeViewNodeTypeMemberView[] MemberView;
    public bool AsSequence;

    public IEnumerable<KeyValuePair<object, object>> ListeContainedNodeIdAndValue(object nodeValue)
    {
      if (nodeValue == null)
        return (IEnumerable<KeyValuePair<object, object>>) null;
      if (this.AsSequence)
      {
        IEnumerable<KeyValuePair<object, object>> keyValuePairs;
        if (!(nodeValue is IEnumerable source1))
        {
          keyValuePairs = (IEnumerable<KeyValuePair<object, object>>) null;
        }
        else
        {
          IEnumerable<object> source = source1.OfType<object>();
          keyValuePairs = source != null ? source.Select<object, KeyValuePair<object, object>>((Func<object, int, KeyValuePair<object, object>>) ((elementValue, index) => new KeyValuePair<object, object>((object) index, elementValue))) : (IEnumerable<KeyValuePair<object, object>>) null;
        }
        return keyValuePairs;
      }
      object NodeValueWrapped = ValueTypeExtensions.WrapIfValueType(nodeValue);
      ApiTreeViewNodeTypeMemberView[] memberView1 = this.MemberView;
      return memberView1 != null ? ((IEnumerable<ApiTreeViewNodeTypeMemberView>) memberView1).Select<ApiTreeViewNodeTypeMemberView, KeyValuePair<object, object>>((Func<ApiTreeViewNodeTypeMemberView, KeyValuePair<object, object>>) (memberView => new KeyValuePair<object, object>(memberView?.Id, memberView?.Getter?.Invoke(NodeValueWrapped)))) : (IEnumerable<KeyValuePair<object, object>>) null;
    }
  }
}
