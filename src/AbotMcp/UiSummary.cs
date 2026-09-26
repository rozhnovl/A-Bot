using System.Text.RegularExpressions;
using Eve64;
using PythonStructures;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.Interface.MemoryStruct;
using OverviewEntry = Sanderling.Interface.MemoryStruct.IOverviewEntry;

namespace AbotMcp;

/// <summary>
/// Compact, operator-readable projections of a parsed UI. Every clickable thing carries its element
/// <c>id</c>, which the click/context_menu tools accept. Sections are independent and individually
/// guarded so one parser oddity never blanks the whole answer.
/// </summary>
internal static class UiSummary
{
    public static readonly string[] DefaultSections = { "ship", "targets", "overview", "menu", "windows" };
    public static readonly string[] AllSections =
        { "ship", "targets", "overview", "menu", "windows", "drones", "inventory", "messages", "probe", "chat" };

    public static string Clean(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? ""
            : Regex.Replace(text, "<[^>]+>", " ").Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static T? Try<T>(Func<T> f) { try { return f(); } catch { return default; } }

    private static long? IdOf(object? o) => o is IUIElement e ? e.Id : null;

    public static bool IsEnemy(OverviewEntry e)
    {
        var color = e.IconSpriteColorPercent;
        return color is not null &&
               color.BPercent < color.RPercent / 3 &&
               color.GPercent < color.RPercent / 3 &&
               color.RPercent > 80;
    }

    private static IEnumerable<OverviewEntry> OverviewEntries(IMemoryMeasurement m) =>
        (m.WindowOverview ?? Array.Empty<IWindowOverview>())
        .SelectMany(w => w?.Entries ?? new List<OverviewEntry>())
        .Where(e => e is not null);

    private static IEnumerable<IMenu> Menus(IMemoryMeasurement m) =>
        (m.Menu ?? Array.Empty<IMenu>()).Where(x => x?.Entry?.Any() == true);

    // --- element index --------------------------------------------------------

    /// <summary>Register every element the summaries expose, so ids round-trip into clicks.</summary>
    public static void IndexElements(ParsedUserInterface m, Action<IUIElement?> reg)
    {
        void Safe(Action a) { try { a(); } catch { /* one bad section must not stop indexing */ } }

        Safe(() =>
        {
            foreach (var e in OverviewEntries(m)) { reg(e as IUIElement); reg(e.UiElement); }
        });
        Safe(() => { foreach (var t in m.Target ?? Array.Empty<IShipUiTarget>()) reg(t); });
        Safe(() =>
        {
            foreach (var menu in Menus(m))
            {
                reg(menu);
                foreach (var entry in menu.Entry) reg(entry);
            }
        });
        Safe(() =>
        {
            if (m.ShipUi is ShipUi ship)
            {
                reg(ship);
                foreach (var b in ship.ModuleButtons ?? new List<ShipUIModuleButton>())
                {
                    reg(b.UINode);
                    reg(b.SlotUINode);
                }
                reg(ship.StopButton);
                reg(ship.MaxSpeedButton);
            }
        });
        Safe(() =>
        {
            foreach (var w in m.WindowOther ?? Array.Empty<IWindow>())
            {
                reg(w);
                foreach (var b in w.ButtonText ?? Array.Empty<IUIElementText>()) reg(b);
                foreach (var b in w.HeaderButton ?? Array.Empty<ISprite>()) reg(b);
                foreach (var l in w.LabelText ?? Array.Empty<IUIElementText>()) reg(l);
            }
        });
        Safe(() =>
        {
            foreach (var w in m.WindowStation ?? Array.Empty<IWindowStation>())
            {
                reg(w as IUIElement);
                reg(w.UndockButton);
            }
        });
        Safe(() => reg(m.Neocom?.InventoryButton));
        Safe(() =>
        {
            reg(m.InfoPanelButtonCurrentSystem);
            reg(m.InfoPanelButtonRoute);
            reg(m.InfoPanelButtonMissions);
        });
        Safe(() =>
        {
            var dv = m.WindowDroneView;
            if (dv is null) return;
            reg(dv as IUIElement);
            foreach (var g in dv.DroneGroups ?? new List<IDronesWindowEntryGroupStructure>())
            {
                reg(g.Header as IUIElement);
                foreach (var c in (IEnumerable<IDronesWindowEntryDrone>?)g.Children ?? Enumerable.Empty<IDronesWindowEntryDrone>())
                    reg(c.Entry as IUIElement);
            }
        });
        Safe(() =>
        {
            foreach (var wi in m.WindowInventory ?? Array.Empty<IWindowInventory>())
            {
                reg(wi as IUIElement);
                foreach (var it in wi.SelectedContainerInventory?.ItemsView ?? new List<IInventoryItemsListViewEntry>())
                    reg(it.Element);
            }
        });
        Safe(() =>
        {
            foreach (var ps in m.WindowProbeScanner ?? Array.Empty<IWindowProbeScanner>())
            {
                reg(ps as IUIElement);
                foreach (var e in ps.ScanResultView?.Entry ?? Array.Empty<IListEntry>()) reg(e);
            }
        });
        Safe(() =>
        {
            foreach (var ch in m.WindowChatChannel ?? Array.Empty<WindowChatChannel>())
            {
                reg(ch);
                reg(ch.MessageInput);
            }
        });
    }

    // --- quick state (status / watch) ----------------------------------------

    // Speed and incoming-EWar are omitted: the Eve64 parser does not read them yet (the legacy
    // ShipUi members for them are always null — see the [Obsolete] marks on IShipUi).
    public sealed record QuickState(
        bool InSpace, string? System, int? Shield, int? Armor, int? Struct, int? Cap,
        string? Maneuver, string? ManeuverTarget, int Locked, string? SelectedTarget,
        int Enemies, int AttackingMe, string? NearestEnemy, int ModulesActive, int MenuLevels)
    {
        public string Line() =>
            !InSpace ? $"docked/loading sys={System}" :
            $"sys={System} S{Shield}/A{Armor}/H{Struct} cap={Cap} {Maneuver}{(ManeuverTarget is null ? "" : "->" + ManeuverTarget)} " +
            $"locked={Locked}{(SelectedTarget is null ? "" : " sel=" + SelectedTarget)} enemies={Enemies} attackingMe={AttackingMe}" +
            $"{(NearestEnemy is null ? "" : " nearest=" + NearestEnemy)} modOn={ModulesActive}{(MenuLevels > 0 ? " menu=" + MenuLevels : "")}";
    }

    public static QuickState Quick(ParsedUserInterface m)
    {
        var ship = m.ShipUi as ShipUi;
        var hp = ship?.HitpointsPercent;
        var overview = Try(() => OverviewEntries(m).ToArray()) ?? Array.Empty<OverviewEntry>();
        var enemies = overview.Where(IsEnemy).ToArray();
        var nearest = enemies.Where(e => e.ObjectDistanceInMeters is not null)
            .OrderBy(e => e.ObjectDistanceInMeters).FirstOrDefault();
        var targets = m.Target ?? Array.Empty<IShipUiTarget>();
        var selected = targets.FirstOrDefault(t => t?.IsSelected == true);
        return new QuickState(
            InSpace: ship is not null,
            System: Try(() => m.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName),
            Shield: hp?.Shield, Armor: hp?.Armor, Struct: hp?.Structure,
            Cap: ship?.Capacitor?.LevelFromPmarksPercent,
            Maneuver: ship?.Indication?.ManeuverType?.ToString(),
            ManeuverTarget: Clean(ship?.Indication?.ManeuverTarget) is { Length: > 0 } mt ? mt : null,
            Locked: targets.Length,
            SelectedTarget: selected is null ? null : TargetLabel(selected),
            Enemies: enemies.Length,
            AttackingMe: enemies.Count(e => e.CommonIndications?.AttackingMe == true),
            NearestEnemy: nearest is null ? null : $"{nearest.ObjectName ?? nearest.ObjectType}@{nearest.ObjectDistanceInMeters}m",
            ModulesActive: ship?.ModuleButtons?.Count(b => b.IsActive == true) ?? 0,
            MenuLevels: Try(() => Menus(m).Count()));
    }

    private static string TargetLabel(IShipUiTarget t) =>
        Clean(t.LabelText is null ? "" : string.Join(" ", t.LabelText));

    // --- sections ------------------------------------------------------------

    public static Dictionary<string, object?> Summarize(ParsedUserInterface m, IEnumerable<string> sections, int maxOverview)
    {
        var result = new Dictionary<string, object?>();
        foreach (var raw in sections)
        {
            var s = raw.Trim().ToLowerInvariant();
            if (s.Length == 0) continue;
            result[s] = s switch
            {
                "ship" => Try(() => Ship(m)) ?? new { error = "ship section failed" },
                "targets" => Try(() => Targets(m)) ?? new { error = "targets section failed" },
                "overview" => Try(() => Overview(m, maxOverview)) ?? new { error = "overview section failed" },
                "menu" => Try(() => Menu(m)) ?? new { error = "menu section failed" },
                "windows" => Try(() => Windows(m)) ?? new { error = "windows section failed" },
                "drones" => Try(() => Drones(m)) ?? new { error = "drones section failed" },
                "inventory" => Try(() => Inventory(m)) ?? new { error = "inventory section failed" },
                "messages" => Try(() => Messages(m)) ?? new { error = "messages section failed" },
                "probe" => Try(() => Probe(m)) ?? new { error = "probe section failed" },
                "chat" => Try(() => Chat(m)) ?? new { error = "chat section failed" },
                _ => new { error = $"unknown section '{s}'; known: {string.Join(",", AllSections)}" },
            };
        }
        return result;
    }

    private static object Ship(ParsedUserInterface m)
    {
        if (m.ShipUi is not ShipUi ship)
            return new { inSpace = false, system = Try(() => m.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName) };
        var hp = ship.HitpointsPercent;
        return new
        {
            inSpace = true,
            system = Try(() => m.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName),
            shield = hp?.Shield, armor = hp?.Armor, structure = hp?.Structure,
            capacitor = ship.Capacitor?.LevelFromPmarksPercent,
            // speed / incoming-ewar / timers omitted: not parsed by Eve64 yet (legacy members always null)
            maneuver = ship.Indication?.ManeuverType?.ToString(),
            maneuverTarget = Clean(ship.Indication?.ManeuverTarget),
            modules = ship.ModuleButtons?.Select(b => new
            {
                id = IdOf(b.UINode),
                slot = $"{b.Rack}{b.SlotIndex}",
                typeId = b.ModuleInfo?.ModuleId,
                name = b.ModuleInfo?.ModuleId is int typeId ? ModuleTypes.Lookup(typeId)?.Name : null,
                active = b.IsActive,
                busy = b.IsBusy ? true : (bool?)null,
                hilite = b.IsHiliteVisible ? true : (bool?)null,
                ramp = b.RampRotationMilli,
                chargeTypeId = b.ModuleInfo?.ChargeTypeId,
                charges = b.ModuleInfo is { MaxCharges: > 0 } mi ? $"{mi.ChargeCount}/{mi.MaxCharges}" : null,
            }).ToArray(),
            buttons = new
            {
                stop = IdOf(ship.StopButton),
                maxSpeed = IdOf(ship.MaxSpeedButton),
            },
        };
    }

    private static object Targets(ParsedUserInterface m) =>
        (m.Target ?? Array.Empty<IShipUiTarget>()).Select(t => new
        {
            id = t.Id,
            label = TargetLabel(t),
            distance = t.Distance,
            shieldPermille = t.Hitpoints?.Shield,
            armorPermille = t.Hitpoints?.Armor,
            hullPermille = t.Hitpoints?.Struct,
            selected = t.IsSelected,
        }).ToArray();

    private static object Overview(ParsedUserInterface m, int max)
    {
        var entries = OverviewEntries(m)
            .OrderBy(e => e.ObjectDistanceInMeters ?? int.MaxValue)
            .Select(e =>
            {
                var stat = Try(() => NpcStats.Lookup(e.ObjectType ?? e.ObjectName));
                return new
                {
                    id = e.Id,
                    name = e.ObjectName,
                    type = e.ObjectType,
                    distanceM = e.ObjectDistanceInMeters,
                    enemy = IsEnemy(e) ? true : (bool?)null,
                    attackingMe = e.CommonIndications?.AttackingMe == true ? true : (bool?)null,
                    targetingMe = e.CommonIndications?.Targeting == true ? true : (bool?)null,
                    lockedByMe = e.CommonIndications?.TargetedByMe == true ? true : (bool?)null,
                    jammingMe = e.CommonIndications?.IsJammingMe == true ? true : (bool?)null,
                    warpDisruptingMe = e.CommonIndications?.IsWarpDisruptingMe == true ? true : (bool?)null,
                    ehp = stat?.EhpTh ?? stat?.EhpOmni,
                    dps = stat?.Dps,
                    npcEwar = stat?.Ewar is { Length: > 0 } ew ? ew : null,
                    hints = e.RightAlignedIconsHints is { } h && h.Any() ? h : null,
                };
            })
            .ToArray();
        return new
        {
            total = entries.Length,
            shown = Math.Min(max, entries.Length),
            entries = entries.Take(max).ToArray(),
        };
    }

    private static object Menu(ParsedUserInterface m) =>
        Menus(m).Select((menu, level) => new
        {
            level,
            entries = menu.Entry.Select(e => new
            {
                id = IdOf(e),
                text = Clean(e?.Text),
                highlighted = e?.HighlightVisible == true ? true : (bool?)null,
            }).ToArray(),
        }).ToArray();

    private static object Windows(ParsedUserInterface m) => new
    {
        other = (m.WindowOther ?? Array.Empty<IWindow>()).Select(w => new
        {
            id = w.Id,
            caption = Clean(w.Caption),
            modal = w.isModal,
            buttons = w.ButtonText?.Where(b => b is not null)
                .Select(b => new { id = b!.Id, text = Clean(b.Text) }).ToArray(),
            labels = w.LabelText?.Select(l => Clean(l?.Text)).Where(t => t.Length > 0).Take(12).ToArray(),
            inputs = w.InputText?.Select(i => new { id = i.Id, text = Clean(i.Text) }).ToArray(),
        }).ToArray(),
        station = (m.WindowStation ?? Array.Empty<IWindowStation>()).Select(w => new
        {
            id = IdOf(w),
            undockButton = IdOf(w.UndockButton),
        }).ToArray(),
        overview = (m.WindowOverview ?? Array.Empty<IWindowOverview>()).Length,
        inventory = (m.WindowInventory ?? Array.Empty<IWindowInventory>()).Length,
        droneView = m.WindowDroneView is not null,
        probeScanner = (m.WindowProbeScanner ?? Array.Empty<IWindowProbeScanner>()).Count(),
        chat = (m.WindowChatChannel ?? Array.Empty<WindowChatChannel>()).Select(c => Clean(c.Caption)).ToArray(),
        neocomInventoryButton = IdOf(m.Neocom?.InventoryButton),
        systemMenu = m.SystemMenu is null ? null : (long?)m.SystemMenu.Id,
    };

    private static object? Drones(ParsedUserInterface m)
    {
        var dv = m.WindowDroneView;
        if (dv is null) return new { open = false };
        object Group(IDronesWindowEntryGroupStructure? g) => g is null
            ? new { }
            : new
            {
                id = IdOf(g.Header),
                header = Clean(g.Header?.MainText),
                drones = g.Children?.Select(c => new { id = IdOf(c.Entry), text = Clean(c.Entry?.MainText) }).ToArray(),
            };
        return new
        {
            open = true,
            inBay = Group(dv.DroneGroupInBay),
            inSpace = Group(dv.DroneGroupInSpace),
            groups = dv.DroneGroups?.Select(Group).ToArray(),
        };
    }

    private static object Inventory(ParsedUserInterface m) =>
        (m.WindowInventory ?? Array.Empty<IWindowInventory>()).Select(wi => new
        {
            id = IdOf(wi),
            subCaption = Clean(wi.SubCaptionLabelText),
            items = wi.SelectedContainerInventory?.ItemsView?.Select(it => new
            {
                id = IdOf(it.Element),
                cells = it.CellsTexts is null
                    ? null
                    : string.Join(" | ", it.CellsTexts.Select(kv => $"{kv.Key}={Clean(kv.Value)}")),
            }).Take(80).ToArray(),
        }).ToArray();

    private static object Messages(ParsedUserInterface m) => new
    {
        abovemain = m.AbovemainMessage?.Select(t => Clean(t?.Text)).Where(t => t.Length > 0).ToArray(),
        tooltips = m.Tooltip?.Select(t => string.Join(" / ",
            (t?.LabelText ?? Array.Empty<IUIElementText>()).Select(l => Clean(l?.Text)).Where(x => x.Length > 0)))
            .Where(t => t.Length > 0).ToArray(),
        moduleTooltip = m.ModuleButtonTooltip is null
            ? null
            : string.Join(" / ", (m.ModuleButtonTooltip.LabelText ?? Array.Empty<IUIElementText>())
                .Select(l => Clean(l?.Text)).Where(x => x.Length > 0)),
    };

    private static object Probe(ParsedUserInterface m) =>
        (m.WindowProbeScanner ?? Array.Empty<IWindowProbeScanner>()).Select(ps => new
        {
            id = IdOf(ps),
            columns = ps.ScanResultView?.ColumnHeader?.Select(c => Clean((c as IUIElementText)?.Text)).ToArray(),
            results = ps.ScanResultView?.Entry?.Select(e => new
            {
                id = e.Id,
                text = string.Join(" | ", ((e as IContainer)?.LabelText ?? Array.Empty<IUIElementText>())
                    .Select(l => Clean(l?.Text)).Where(x => x.Length > 0)),
            }).ToArray(),
        }).ToArray();

    private static object Chat(ParsedUserInterface m) =>
        (m.WindowChatChannel ?? Array.Empty<WindowChatChannel>()).Select(c => new
        {
            id = c.Id,
            caption = Clean(c.Caption),
            participants = Try(() => c.Participant?.Count()),
            inputId = IdOf(c.MessageInput),
            lastMessages = Try(() => c.Message?.TakeLast(8).Select(msg => Clean(msg?.ToString())).ToArray()),
        }).ToArray();

    // --- raw tree search -----------------------------------------------------

    public sealed record RawHit(long Id, string Type, string? Name, string? Text, int X, int Y, int W, int H, int Depth, int Children);

    public static List<RawHit> SearchRaw(UITreeNodeWithDisplayRegion root, string query, int limit)
    {
        var hits = new List<RawHit>();
        void Walk(UITreeNodeWithDisplayRegion node, int depth)
        {
            if (hits.Count >= limit) return;
            var n = node.UiNode;
            var type = n.PythonObjectTypeName ?? "";
            var name = n.NameProperty;
            var text = TextOf(n);
            if (Matches(type, query) || Matches(name, query) || Matches(text, query))
            {
                var r = node.TotalDisplayRegionVisible ?? node.TotalDisplayRegion;
                hits.Add(new RawHit((long)n.pythonObjectAddress, type, name, text,
                    (int)(r?.X ?? 0), (int)(r?.Y ?? 0), (int)(r?.Width ?? 0), (int)(r?.Height ?? 0),
                    depth, node.Children?.Count ?? 0));
            }
            foreach (var c in node.Children ?? new List<ChildOfNodeWithDisplayRegion>())
                if (c.NodeWithRegion is { } child) Walk(child, depth + 1);
        }
        Walk(root, 0);
        return hits;
    }

    private static bool Matches(string? value, string query) =>
        !string.IsNullOrEmpty(value) && value.Contains(query, StringComparison.OrdinalIgnoreCase);

    public static string? TextOf(UITreeNode n)
    {
        var d = n.DictEntriesOfInterest;
        if (d is null) return null;
        foreach (var key in new[] { "_setText", "_text", "text", "_displayText", "_hint" })
            if (d.TryGetValue(key, out var v) && v is not null)
            {
                var s = Clean(v.ToString());
                if (s.Length > 0) return s;
            }
        return null;
    }
}
