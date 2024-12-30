// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.Main
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using Bib3.FCL.GBS;
using BotEngine.Common;
using BotEngine.UI.ViewModel;
using BotSharp.UI.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Sanderling.UI
{
  public partial class Main : UserControl, IComponentConnector
  {
    public static ISingleValueStore<string> LicenseKeyStore;
    public ContentAndStatusIcon InterfaceHeader;
    public InterfaceToEve Interface;
    public ContentAndStatusIcon BotHeader;
    internal Grid PanelMeasureDesiredHeight;
    public ToggleButtonHorizBinär ToggleButtonMotionEnable;
    public BotSharp.UI.Wpf.BotsNavigation BotsNavigation;
    public BotAPIExplorer DevToolsAPIExplorer;
    private bool _contentLoaded;

    public Main()
    {
      this.InitializeComponent();
      this.Interface.LicenseDataContext.LicenseKeyStore = Main.LicenseKeyStore;
    }

    public IDE DevelopmentEnvironment => this.BotsNavigation?.DevelopmentEnvironment;

    public void BotMotionDisable()
    {
      ToggleButtonHorizBinär buttonMotionEnable = this.ToggleButtonMotionEnable;
      if (buttonMotionEnable == null)
        return;
      buttonMotionEnable.LeftButtonDown();
    }

    public void BotMotionEnable()
    {
      ToggleButtonHorizBinär buttonMotionEnable = this.ToggleButtonMotionEnable;
      if (buttonMotionEnable == null)
        return;
      buttonMotionEnable.RightButtonDown();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Sanderling.UI;component/main.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.InterfaceHeader = (ContentAndStatusIcon) target;
          break;
        case 2:
          this.Interface = (InterfaceToEve) target;
          break;
        case 3:
          this.BotHeader = (ContentAndStatusIcon) target;
          break;
        case 4:
          this.PanelMeasureDesiredHeight = (Grid) target;
          break;
        case 5:
          this.ToggleButtonMotionEnable = (ToggleButtonHorizBinär) target;
          break;
        case 6:
          this.BotsNavigation = (BotSharp.UI.Wpf.BotsNavigation) target;
          break;
        case 7:
          this.DevToolsAPIExplorer = (BotAPIExplorer) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
