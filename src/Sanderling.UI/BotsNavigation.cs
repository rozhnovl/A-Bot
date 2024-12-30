// Decompiled with JetBrains decompiler
// Type: Sanderling.UI.BotsNavigation
// Assembly: Sanderling.UI, Version=2018.324.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 08E7571D-A17F-4722-903C-771404BAB228
// Assembly location: C:\Src\A-Bot\lib\Sanderling.UI.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;

namespace Sanderling.UI
{
  public class BotsNavigation
  {
    private static (string, string) SupportEmailAddress => ("mailto:", "support@botengine.org");

    private static (string, string) ForumAddress => ("http://", "forum.botengine.de");

    private static string FindTheRightBotLink => "http://forum.botengine.de/t/how-to-automate-anything-in-eve-online/774";

    public static BotSharp.UI.Wpf.BotsNavigation.NavigationContent NavigationRoot(
      IEnumerable<(string, byte[])> botsOfferedAtRoot,
      BotSharp.UI.Wpf.BotsNavigation.Bot defaultBotForDevelopmentEnvironment)
    {
      List<(string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)> list = botsOfferedAtRoot.EmptyIfNull<(string, byte[])>().Select<(string, byte[]), (string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)>((Func<(string, byte[]), (string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)>) (botOfferedAtRoot => (botOfferedAtRoot.Item1, new BotSharp.UI.Wpf.BotsNavigation.NavigationContent()
      {
        PreviewBot = new BotSharp.UI.Wpf.BotsNavigation.Bot()
        {
          SerializedBot = botOfferedAtRoot.Item2
        }
      }))).ToList<(string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)>();
      BotSharp.UI.Wpf.BotsNavigation.NavigationContent navigationContent1 = new BotSharp.UI.Wpf.BotsNavigation.NavigationContent();
      navigationContent1.Caption = "How may I assist you?";
      BotSharp.UI.Wpf.BotsNavigation.NavigationContent navigationContent2 = navigationContent1;
      List<(string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)> first = list;
      (string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)[] second = new (string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)[2];
      BotSharp.UI.Wpf.BotsNavigation.NavigationContent navigationContent3 = new BotSharp.UI.Wpf.BotsNavigation.NavigationContent();
      navigationContent3.Caption = "Automate Anything";
      BotSharp.UI.Wpf.BotsNavigation.NavigationContent navigationContent4 = navigationContent3;
      BotSharp.UI.Wpf.BotsNavigation.NavigationContent[] navigationContentArray = new BotSharp.UI.Wpf.BotsNavigation.NavigationContent[2]
      {
        new BotSharp.UI.Wpf.BotsNavigation.NavigationContent()
        {
          FlowDocument = (Func<RoutedEventHandler, FlowDocument>) (hyperlinkClickHandler => BotsNavigation.AutomateSomethingElseGuide(defaultBotForDevelopmentEnvironment, hyperlinkClickHandler))
        },
        null
      };
      BotSharp.UI.Wpf.BotsNavigation.NavigationContent navigationContent5 = new BotSharp.UI.Wpf.BotsNavigation.NavigationContent();
      navigationContent5.SingleChoiceOptions = new (string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)[2]
      {
        ("\uD83D\uDCC2 Load Bot From File", new BotSharp.UI.Wpf.BotsNavigation.NavigationContent()
        {
          LoadBotFromFileToPreview = true
        }),
        ("Open Development Environment", new BotSharp.UI.Wpf.BotsNavigation.NavigationContent()
        {
          BotToInitDevelopmentEnvironment = defaultBotForDevelopmentEnvironment
        })
      };
      navigationContentArray[1] = navigationContent5;
      navigationContent4.Children = navigationContentArray;
      second[0] = ("Automate Something Else", navigationContent3);
      second[1] = ("Something Else", new BotSharp.UI.Wpf.BotsNavigation.NavigationContent()
      {
        Caption = "General Support",
        FlowDocument = new Func<RoutedEventHandler, FlowDocument>(BotsNavigation.SomethingElseDocument)
      });
      (string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)[] array = first.Concat<(string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)>((IEnumerable<(string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)>) second).ToArray<(string, BotSharp.UI.Wpf.BotsNavigation.NavigationContent)>();
      navigationContent2.SingleChoiceOptions = array;
      return navigationContent1;
    }

    public static FlowDocument AutomateSomethingElseGuide(
      BotSharp.UI.Wpf.BotsNavigation.Bot defaultBotForDevelopmentEnvironment,
      RoutedEventHandler hyperlinkClickHandler)
    {
      // ISSUE: variable of a compiler-generated type
      BotsNavigation.\u003C\u003Ec__DisplayClass7_0 cDisplayClass70;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass70.defaultBotForDevelopmentEnvironment = defaultBotForDevelopmentEnvironment;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass70.hyperlinkClickHandler = hyperlinkClickHandler;
      Paragraph paragraph = new Paragraph();
      BotSharp.UI.Wpf.BotsNavigation.EventHandling eventHandling = new BotSharp.UI.Wpf.BotsNavigation.EventHandling()
      {
        NavigateInto = new BotSharp.UI.Wpf.BotsNavigation.NavigationContent()
        {
          LoadBotFromFileToPreview = true
        }
      };
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      paragraph.Inlines.AddRange((IEnumerable) new Inline[10]
      {
        (Inline) new Run("To find the right bot for your use case, see the guide at "),
        BotSharp.UI.Wpf.BotsNavigation.LinkToUrlInline(("", BotsNavigation.FindTheRightBotLink), cDisplayClass70.hyperlinkClickHandler),
        (Inline) new LineBreak(),
        (Inline) new Run("When you have chosen a bot, use the "),
        BotSharp.UI.Wpf.BotsNavigation.LinkFromDisplayTextAndEventHandling("\uD83D\uDCC2 Load Bot From File", eventHandling, cDisplayClass70.hyperlinkClickHandler),
        (Inline) new Run(" button to run it."),
        (Inline) new LineBreak(),
        (Inline) new Run("You can also use the "),
        BotsNavigation.\u003CAutomateSomethingElseGuide\u003Eg__linkToDevelopmentEnvironment7_0("development environment", ref cDisplayClass70),
        (Inline) new Run(" to create a new bot from scratch.")
      });
      return new FlowDocument((Block) paragraph);
    }

    public static FlowDocument SomethingElseDocument(RoutedEventHandler hyperlinkClickHandler)
    {
      Paragraph paragraph = new Paragraph();
      paragraph.Inlines.AddRange((IEnumerable) new Inline[4]
      {
        (Inline) new Run("For general help, post on the forum at "),
        BotSharp.UI.Wpf.BotsNavigation.LinkToUrlInline(BotsNavigation.ForumAddress, hyperlinkClickHandler),
        (Inline) new Run(" or contact us via "),
        BotSharp.UI.Wpf.BotsNavigation.LinkToUrlInline(BotsNavigation.SupportEmailAddress, hyperlinkClickHandler)
      });
      return new FlowDocument((Block) paragraph);
    }

    [CompilerGenerated]
    internal static Inline \u003CAutomateSomethingElseGuide\u003Eg__linkToDevelopmentEnvironment7_0(
      string displayText,
      [In] ref BotsNavigation.\u003C\u003Ec__DisplayClass7_0 obj1)
    {
      string displayText1 = displayText;
      BotSharp.UI.Wpf.BotsNavigation.EventHandling eventHandling = new BotSharp.UI.Wpf.BotsNavigation.EventHandling();
      // ISSUE: reference to a compiler-generated field
      eventHandling.NavigateInto = new BotSharp.UI.Wpf.BotsNavigation.NavigationContent()
      {
        BotToInitDevelopmentEnvironment = obj1.defaultBotForDevelopmentEnvironment
      };
      // ISSUE: reference to a compiler-generated field
      RoutedEventHandler hyperlinkClickHandler = obj1.hyperlinkClickHandler;
      return BotSharp.UI.Wpf.BotsNavigation.LinkFromDisplayTextAndEventHandling(displayText1, eventHandling, hyperlinkClickHandler);
    }
  }
}
