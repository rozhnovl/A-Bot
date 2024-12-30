// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.Extension
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using Bib3;
using Bib3.FCL.UI;
using BotEngine.Interface;
using BotEngine.UI;
using Sanderling.Interface;
using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;

namespace Sanderling.UI
{
  public static class Extension
  {
    public static StatusIcon.StatusEnum LicenseStatusEnum(
      this Sanderling.SimpleInterfaceServerDispatcher dispatcher)
    {
      return dispatcher == null || !((BotEngine.Interface.SimpleInterfaceServerDispatcher) dispatcher).AppInterfaceAvailable ? StatusIcon.StatusEnum.Reject : StatusIcon.StatusEnum.Accept;
    }

    public static StatusIcon.StatusEnum ProcessStatusEnum(
      this SictAuswaalWindowsProcess processChoice)
    {
      return ((int) processChoice?.ChoosenProcessAtTime.Wert?.BewertungMainModuleDataiNaamePasend ?? 0) == 0 ? StatusIcon.StatusEnum.None : StatusIcon.StatusEnum.Accept;
    }

    public static StatusIcon.StatusEnum MemoryMeasurementLastStatusEnum(
      this FromProcessMeasurement<IMemoryMeasurement> measurement,
      long measurementTimeMin)
    {
      long num = measurementTimeMin;
      long? begin = ((PropertyGenTimespanInt64<IMemoryMeasurement>) measurement)?.Begin;
      long valueOrDefault = begin.GetValueOrDefault();
      if ((num < valueOrDefault ? (begin.HasValue ? 1 : 0) : 0) == 0 || !((PropertyGenIntervalInt64<IMemoryMeasurement>) measurement)?.Value.EnumMengeRefAusNezAusWurzel(FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache).CountAtLeast<object>(1L))
        return StatusIcon.StatusEnum.Reject;
      bool? nullable;
      if (measurement == null)
      {
        nullable = new bool?();
      }
      else
      {
        // ISSUE: explicit non-virtual call
        IMemoryMeasurement measurement1 = __nonvirtual (((PropertyGenIntervalInt64<IMemoryMeasurement>) measurement).Value);
        nullable = measurement1 != null ? new bool?(measurement1.SessionDurationRemainingSufficientToStayExposed()) : new bool?();
      }
      return ((int) nullable ?? 0) == 0 ? StatusIcon.StatusEnum.Warn : StatusIcon.StatusEnum.Accept;
    }

    public static StatusIcon.StatusEnum MemoryMeasurementLastStatusEnum(
      this FromProcessMeasurement<IMemoryMeasurement> measurement)
    {
      return measurement.MemoryMeasurementLastStatusEnum(Glob.StopwatchZaitMiliSictInt() - 8000L);
    }

    public static StatusIcon.StatusEnum InterfaceStatusEnum(this InterfaceToEve view)
    {
      StatusIcon.StatusEnum[] statusComponent = new StatusIcon.StatusEnum[2];
      StatusIcon.StatusEnum? status = view?.ProcessHeader?.Status;
      statusComponent[0] = (StatusIcon.StatusEnum) ((int) status ?? 2);
      status = view?.MeasurementLastHeader?.Status;
      statusComponent[1] = (StatusIcon.StatusEnum) ((int) status ?? 2);
      return ((IEnumerable<StatusIcon.StatusEnum>) statusComponent).AggregateStatus().FirstOrNull<StatusIcon.StatusEnum>() ?? StatusIcon.StatusEnum.Reject;
    }
  }
}
