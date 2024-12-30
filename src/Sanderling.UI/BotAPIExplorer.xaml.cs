// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.BotAPIExplorer
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using Bib3.FCL.GBS.Inspektor;
using BotEngine;
using BotEngine.UI;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Script;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Sanderling.UI
{
  public partial class BotAPIExplorer : UserControl, IComponentConnector
  {
    public FromProcessMeasurement TimeAndOrigin;
    internal TextBox SearchObjectFilterId;
    internal Button SearchObjectButtonSelectObject;
    public InspectTreeView TreeView;
    private bool _contentLoaded;

    public BotAPIExplorer()
    {
      this.InitializeComponent();
      this.TreeView.TreeViewView = (IAstSict) new ApiTreeViewNodeView();
      this.TreeView.Export.ZuExportiire = (Func<object>) (() =>
      {
        Baum treeView = this.TreeView.TreeView;
        if (treeView == null)
          return (object) null;
        IEnumerable<KeyValuePair<object, object>> segmentAstIdentUndWert = treeView.AuswaalPfaadListeSegmentAstIdentUndWert;
        return segmentAstIdentUndWert == null ? (object) null : segmentAstIdentUndWert.LastOrDefault<KeyValuePair<object, object>>().Value;
      });
    }

    public void Present(IHostToScript api)
    {
      this.TimeAndOrigin.Present<IMemoryMeasurement>(api?.MemoryMeasurement);
      this.ApiRoot = (object) api;
    }

    private object ApiRoot
    {
      set => this.TreeView.TreeView.Präsentiire(value);
      get => this.TreeView?.TreeView?.Wurzel;
    }

    private void SearchObjectButtonSelectObject_Click(object sender, RoutedEventArgs e) => ((Action) (() =>
    {
      string text = this.SearchObjectFilterId.Text;
      long? Id = text != null ? text.TryParseInt64() : new long?();
      Func<object, bool> pathEndNodeValuePredicate = (Func<object, bool>) (c =>
      {
        if (!(c is IObjectIdInt64 objectIdInt64_2))
          return !Id.HasValue;
        long id = objectIdInt64_2.Id;
        long? nullable = Id;
        long valueOrDefault = nullable.GetValueOrDefault();
        return id == valueOrDefault && nullable.HasValue;
      });
      InspectTreeView treeView = this.TreeView;
      IEnumerable<IEnumerable<KeyValuePair<object, object>>> keyValuePairs;
      if (treeView == null)
      {
        keyValuePairs = (IEnumerable<IEnumerable<KeyValuePair<object, object>>>) null;
      }
      else
      {
        IAstSict treeViewView = treeView.TreeViewView;
        keyValuePairs = treeViewView != null ? treeViewView.EnumeratePathToNodeSatisfyingPredicateBreadthFirst(this.ApiRoot, pathEndNodeValuePredicate) : (IEnumerable<IEnumerable<KeyValuePair<object, object>>>) null;
      }
      IEnumerable<IEnumerable<KeyValuePair<object, object>>> source = keyValuePairs;
      IEnumerable<KeyValuePair<object, object>> enumerable = source != null ? source.FirstOrDefault<IEnumerable<KeyValuePair<object, object>>>() : (IEnumerable<KeyValuePair<object, object>>) null;
      if (enumerable == null)
        throw new ArgumentException("no match for given search criteria.");
      this.TreeView?.TreeView?.ExpandPath(enumerable != null ? enumerable.Keys<object, object>() : (IEnumerable<object>) null, true);
    })).CatchNaacMessageBoxException();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Sanderling.UI;component/botapiexplorer.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.TimeAndOrigin = (FromProcessMeasurement) target;
          break;
        case 2:
          this.SearchObjectFilterId = (TextBox) target;
          break;
        case 3:
          this.SearchObjectButtonSelectObject = (Button) target;
          this.SearchObjectButtonSelectObject.Click += new RoutedEventHandler(this.SearchObjectButtonSelectObject_Click);
          break;
        case 4:
          this.TreeView = (InspectTreeView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
