// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.ApiTreeViewNodeView
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using Bib3;
using Bib3.FCL.GBS.Inspektor;
using Bib3.Terz.GBS.Inspektor;
using Fasterflect;
using Sanderling.Interface;
using Sanderling.Script.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sanderling.UI
{
  public class ApiTreeViewNodeView : IAstSict
  {
    private IAstSict Base;
    private readonly SictScatenscpaicerDict<Type, ApiTreeViewNodeTypeView> CacheTypeView = new SictScatenscpaicerDict<Type, ApiTreeViewNodeTypeView>();

    public ApiTreeViewNodeView() => this.Base = (IAstSict) new AstSictRefNezDif(FromInterfaceResponse.SerialisPolicyCache);

    public static bool IsLeaf(Type type) => (Type) null == type || type.IsPrimitive || type.IsEnum || typeof (string) == type;

    private ApiTreeViewNodeTypeView TypeView(Type type)
    {
      if ((Type) null == type)
        return (ApiTreeViewNodeTypeView) null;
      return this.CacheTypeView?.ValueFürKey(type, new Func<Type, ApiTreeViewNodeTypeView>(this.TypeViewConstruct));
    }

    private static bool MemberVisible(MemberInfo member)
    {
      if ((MemberInfo) null == member)
        return false;
      PropertyInfo propertyInfo = member as PropertyInfo;
      if ((PropertyInfo) null != propertyInfo)
      {
        if (!propertyInfo.CanRead)
          return false;
        MethodInfo getMethod = propertyInfo.GetMethod;
        // ISSUE: explicit non-virtual call
        if ((object) getMethod != null && !__nonvirtual (getMethod.IsPublic))
          return false;
      }
      Type reflectedType1 = member.ReflectedType;
      if ((object) reflectedType1 != null && TypeExtensions.InheritsOrImplements<ITimespanInt64>(reflectedType1))
      {
        if (((IEnumerable<string>) new string[2]
        {
          "Up",
          "Low"
        }).Contains<string>(member.Name))
          return false;
      }
      Type reflectedType2 = member.ReflectedType;
      if ((object) reflectedType2 != null && reflectedType2.InheritsOrImplementsOrEquals<HostToScript>())
      {
        if (((IEnumerable<string>) new string[2]
        {
          "MemoryMeasurementFunc",
          "MotionExecuteFunc"
        }).Contains<string>(member.Name))
          return false;
      }
      switch (member.MemberType)
      {
        case MemberTypes.Field:
        case MemberTypes.Property:
          return true;
        default:
          return false;
      }
    }

    private Fasterflect.MemberGetter MemberGetter(MemberInfo member)
    {
      PropertyInfo propertyInfo = member as PropertyInfo;
      Fasterflect.MemberGetter propertyValue = (object) propertyInfo != null ? PropertyInfoExtensions.DelegateForGetPropertyValue(propertyInfo) : (Fasterflect.MemberGetter) null;
      if (propertyValue != null)
        return propertyValue;
      FieldInfo fieldInfo = member as FieldInfo;
      return (object) fieldInfo == null ? (Fasterflect.MemberGetter) null : FieldInfoExtensions.DelegateForGetFieldValue(fieldInfo);
    }

    private ApiTreeViewNodeTypeView TypeViewConstruct(Type type)
    {
      if (ApiTreeViewNodeView.IsLeaf(type))
        return (ApiTreeViewNodeTypeView) null;
      if (!type.IsArray)
        ;
      if (TypeExtensions.Implements(type, typeof (IEnumerable)))
        return new ApiTreeViewNodeTypeView()
        {
          AsSequence = true
        };
      MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public);
      MemberInfo[] memberInfoArray;
      if (members == null)
      {
        memberInfoArray = (MemberInfo[]) null;
      }
      else
      {
        IEnumerable<MemberInfo> source = ((IEnumerable<MemberInfo>) members).Where<MemberInfo>(new Func<MemberInfo, bool>(ApiTreeViewNodeView.MemberVisible));
        memberInfoArray = source != null ? source.ToArray<MemberInfo>() : (MemberInfo[]) null;
      }
      MemberInfo[] source1 = memberInfoArray;
      ApiTreeViewNodeTypeView viewNodeTypeView = new ApiTreeViewNodeTypeView();
      ApiTreeViewNodeTypeMemberView[] nodeTypeMemberViewArray;
      if (source1 == null)
      {
        nodeTypeMemberViewArray = (ApiTreeViewNodeTypeMemberView[]) null;
      }
      else
      {
        IEnumerable<ApiTreeViewNodeTypeMemberView> source2 = ((IEnumerable<MemberInfo>) source1).Select<MemberInfo, ApiTreeViewNodeTypeMemberView>((Func<MemberInfo, ApiTreeViewNodeTypeMemberView>) (member => new ApiTreeViewNodeTypeMemberView()
        {
          Id = (object) member?.Name,
          Getter = this.MemberGetter(member)
        }));
        nodeTypeMemberViewArray = source2 != null ? source2.ToArray<ApiTreeViewNodeTypeMemberView>() : (ApiTreeViewNodeTypeMemberView[]) null;
      }
      viewNodeTypeView.MemberView = nodeTypeMemberViewArray;
      return viewNodeTypeView;
    }

    public bool AstIdentGlaicwertig(object id0, object id1) => this.Base.AstIdentGlaicwertig(id0, id1);

    public object HeaderContent(object nodeId, object nodeValue, object headerContentPrev) => this.Base?.HeaderContent(nodeId, nodeValue, headerContentPrev);

    public IEnumerable<KeyValuePair<object, object>> ListeAstEnthalteInAstIdentUndWert(
      object nodeValue)
    {
      return this.TypeView(nodeValue?.GetType())?.ListeContainedNodeIdAndValue(nodeValue);
    }
  }
}
