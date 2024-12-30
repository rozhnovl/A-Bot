// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.MemoryMeasurement
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using Bib3;
using Bib3.FCL.GBS.Inspektor;
using BotEngine.Interface;
using BotEngine.UI;
using Sanderling.Interface;
using Sanderling.Interface.MemoryStruct;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Sanderling.UI
{
  public partial class MemoryMeasurement : UserControl, IComponentConnector
  {
    internal FromProcessMeasurement Summary;
    public InspectTreeView Detail;
    private bool _contentLoaded;

    public MemoryMeasurement()
    {
      this.InitializeComponent();
      this.Detail.TreeViewView = InspectTreeView.ViewRefNezDifConstruct(FromInterfaceResponse.SerialisPolicyCache, AstHeaderBackgroundBrushParam.SctandardParam, (Func<object, IEnumerable<KeyValuePair<object, object>>>) null, (Func<object, UIElement, Func<UIElement>, UIElement>) null);
    }

    public void Present(
      FromProcessMeasurement<IMemoryMeasurement> measurement)
    {
      this.Summary?.Present<IMemoryMeasurement>(measurement);
      this.Detail?.TreeView?.Präsentiire((object) new IMemoryMeasurement[1]
      {
        ((PropertyGenIntervalInt64<IMemoryMeasurement>) measurement)?.Value
      });
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Sanderling.UI;component/memorymeasurement.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Summary = (FromProcessMeasurement) target;
          break;
        case 2:
          this.Detail = (InspectTreeView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
