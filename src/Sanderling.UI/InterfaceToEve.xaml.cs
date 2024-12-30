// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.InterfaceToEve
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using Bib3;
using BotEngine;
using BotEngine.Interface;
using BotEngine.UI;
using BotEngine.UI.ViewModel;
using Sanderling.Interface.MemoryStruct;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Sanderling.UI
{
  public partial class InterfaceToEve : UserControl, IComponentConnector
  {
    public readonly BotEngine.UI.ViewModel.License LicenseDataContext = new BotEngine.UI.ViewModel.License();
    public ContentAndStatusIcon ProcessHeader;
    public SictAuswaalWindowsProcess ProcessChoice;
    public ContentAndStatusIcon MeasurementLastHeader;
    internal GroupBox SessionDurationRemainingTooShortGroup;
    internal TextBlock SessionDurationRemainingTextBox;
    internal MemoryMeasurement Measurement;
    private bool _contentLoaded;

    public InterfaceToEve()
    {
      this.InitializeComponent();
      SictAuswaalWindowsProcess processChoice = this.ProcessChoice;
      if (processChoice == null)
        return;
      processChoice.PreferenceWriteToUI(new ChooseWindowProcessPreference()
      {
        FilterMainModuleFileName = "ExeFile.exe"
      });
    }

    public void Present(
      Sanderling.SimpleInterfaceServerDispatcher interfaceServerDispatcher,
      FromProcessMeasurement<IMemoryMeasurement> measurement)
    {
      ContentAndStatusIcon measurementLastHeader = this.MeasurementLastHeader;
      if (measurementLastHeader != null)
        measurementLastHeader.SetStatus(measurement.MemoryMeasurementLastStatusEnum());
      ContentAndStatusIcon processHeader = this.ProcessHeader;
      if (processHeader != null)
        processHeader.SetStatus(this.ProcessChoice.ProcessStatusEnum());
      this.LicenseDataContext.Dispatcher = (BotEngine.Interface.SimpleInterfaceServerDispatcher) interfaceServerDispatcher;
      bool flag = !((PropertyGenIntervalInt64<IMemoryMeasurement>) measurement)?.Value.SessionDurationRemainingSufficientToStayExposed();
      TextBlock remainingTextBox = this.SessionDurationRemainingTextBox;
      string str;
      if (measurement == null)
      {
        str = (string) null;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        IMemoryMeasurement imemoryMeasurement = __nonvirtual (((PropertyGenIntervalInt64<IMemoryMeasurement>) measurement).Value);
        if (imemoryMeasurement == null)
        {
          str = (string) null;
        }
        else
        {
          int? durationRemaining = imemoryMeasurement.SessionDurationRemaining;
          ref int? local = ref durationRemaining;
          str = local.HasValue ? local.GetValueOrDefault().ToString() : (string) null;
        }
      }
      if (str == null)
        str = "????";
      remainingTextBox.Text = str;
      this.SessionDurationRemainingTooShortGroup.Visibility = !flag || measurement == null ? Visibility.Hidden : Visibility.Visible;
      this.Measurement?.Present(measurement);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Sanderling.UI;component/interfacetoeve.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ProcessHeader = (ContentAndStatusIcon) target;
          break;
        case 2:
          this.ProcessChoice = (SictAuswaalWindowsProcess) target;
          break;
        case 3:
          this.MeasurementLastHeader = (ContentAndStatusIcon) target;
          break;
        case 4:
          this.SessionDurationRemainingTooShortGroup = (GroupBox) target;
          break;
        case 5:
          this.SessionDurationRemainingTextBox = (TextBlock) target;
          break;
        case 6:
          this.Measurement = (MemoryMeasurement) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
