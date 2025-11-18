# AbyssalBot Mobile UI - Interface Mockup Description

## Mobile Phone View (Screenshot Description)

Imagine viewing this on your phone in portrait mode:

---

### **Screen 1: Bot Control Dashboard (Scrollable)**

```
╔═══════════════════════════════════════╗
║  ☰  🤖 AbyssalBot Control             ║
║                      🟢 Connected     ║
╠═══════════════════════════════════════╣
║                                       ║
║  ┌───────────────────────────────┐   ║
║  │ 🎮 Control Panel              │   ║
║  ├───────────────────────────────┤   ║
║  │                               │   ║
║  │ ┌───────────────────────────┐ │   ║
║  │ │ Status: Running           │ │   ║
║  │ │ [Green background]        │ │   ║
║  │ └───────────────────────────┘ │   ║
║  │ ┌───────────────────────────┐ │   ║
║  │ │ State: AbyssalFightState  │ │   ║
║  │ │ [Blue background]         │ │   ║
║  │ └───────────────────────────┘ │   ║
║  │ ┌───────────────────────────┐ │   ║
║  │ │ Step: 1247                │ │   ║
║  │ │ [Gray background]         │ │   ║
║  │ └───────────────────────────┘ │   ║
║  │                               │   ║
║  │ ┌─────────────────────────┐   │   ║
║  │ │     ⏸️  Pause           │   │   ║
║  │ │  [Yellow button, large] │   │   ║
║  │ └─────────────────────────┘   │   ║
║  │ ┌─────────────────────────┐   │   ║
║  │ │     ⏹️  Stop            │   │   ║
║  │ │  [Red button, large]    │   │   ║
║  │ └─────────────────────────┘   │   ║
║  │                               │   ║
║  │ ━━━━━━━━━━━━━━━━━━━━━━━━━━   │   ║
║  │                               │   ║
║  │ ┌─────────────────────────┐   │   ║
║  │ │ 🚨 EMERGENCY RETREAT    │   │   ║
║  │ │   [Pulsing red]         │   │   ║
║  │ └─────────────────────────┘   │   ║
║  │                               │   ║
║  │ ━━━━━━━━━━━━━━━━━━━━━━━━━━   │   ║
║  │                               │   ║
║  │ Manual State Control          │   ║
║  │ ┌────────┐  ┌────────┐        │   ║
║  │ │⚔️ Fight│  │🚀Travel│        │   ║
║  │ └────────┘  └────────┘        │   ║
║  │ ┌────────┐  ┌────────┐        │   ║
║  │ │🔄Reload│  │⏳ Wait │        │   ║
║  │ └────────┘  └────────┘        │   ║
║  │                               │   ║
║  └───────────────────────────────┘   ║
║                                       ║
║  ┌───────────────────────────────┐   ║
║  │ 🚀 Ship Status                │   ║
║  ├───────────────────────────────┤   ║
║  │                               │   ║
║  │ 📍 Location: Abyss - Room 2   │   ║
║  │ 🎯 Maneuver: Orbit            │   ║
║  │ 🌀 In Abyss: Yes              │   ║
║  │                               │   ║
║  │ Shield                        │   ║
║  │ ██████████████████░░  85.0%   │   ║
║  │ 850 / 1000                    │   ║
║  │ [Blue progress bar]           │   ║
║  │                               │   ║
║  │ Armor                         │   ║
║  │ ████████████████████ 100.0%   │   ║
║  │ 500 / 500                     │   ║
║  │ [Orange progress bar]         │   ║
║  │                               │   ║
║  │ Structure                     │   ║
║  │ ████████████████████ 100.0%   │   ║
║  │ 300 / 300                     │   ║
║  │ [Red progress bar]            │   ║
║  │                               │   ║
║  │ Capacitor                     │   ║
║  │ ██████████████████░░  90.0%   │   ║
║  │ 450 / 500                     │   ║
║  │ [Cyan progress bar]           │   ║
║  │                               │   ║
║  │      ┌──┐         ┌──┐        │   ║
║  │      │5 │         │3 │        │   ║
║  │      └──┘         └──┘        │   ║
║  │   Active        Drones        │   ║
║  │   Modules     in Space        │   ║
║  │                               │   ║
║  └───────────────────────────────┘   ║
║                                       ║
║  [Scroll down for more...]            ║
║                                       ║
╚═══════════════════════════════════════╝
```

**User scrolls down...**

```
╔═══════════════════════════════════════╗
║                                       ║
║  ┌───────────────────────────────┐   ║
║  │ 🎯 Active Targets             │   ║
║  ├───────────────────────────────┤   ║
║  │                               │   ║
║  │ ┌─────────────────────────┐   │   ║
║  │ │ 🔴 Frigate - Starving   │   │   ║
║  │ │    Tanoo                │   │   ║
║  │ │ [Red border, highlighted]│  │   ║
║  │ │                         │   │   ║
║  │ │ [NPC] [Priority: 5]     │   │   ║
║  │ │                         │   │   ║
║  │ │ Distance: 8.5 km        │   │   ║
║  │ │                         │   │   ║
║  │ │ Shield  ████████░░ 80%  │   │   ║
║  │ │ Armor   ████████░░ 80%  │   │   ║
║  │ │ Hull    ██████████ 100% │   │   ║
║  │ └─────────────────────────┘   │   ║
║  │                               │   ║
║  │ ┌─────────────────────────┐   │   ║
║  │ │ Cruiser - Ravenous      │   │   ║
║  │ │ Overmind                │   │   ║
║  │ │ [Gray border]           │   │   ║
║  │ │                         │   │   ║
║  │ │ [NPC] [Priority: 3]     │   │   ║
║  │ │                         │   │   ║
║  │ │ Distance: 12.3 km       │   │   ║
║  │ │                         │   │   ║
║  │ │ Shield  ██████░░░░ 60%  │   │   ║
║  │ │ Armor   ██████████ 100% │   │   ║
║  │ │ Hull    ██████████ 100% │   │   ║
║  │ └─────────────────────────┘   │   ║
║  │                               │   ║
║  └───────────────────────────────┘   ║
║                                       ║
║  ┌───────────────────────────────┐   ║
║  │ 📋 Action Log      [Expand]   │   ║
║  ├───────────────────────────────┤   ║
║  │                               │   ║
║  │ [12:35:20] Target destroyed   │   ║
║  │ [Green left border]           │   ║
║  │                               │   ║
║  │ [12:35:15] Shield at 85%      │   ║
║  │ [Blue left border]            │   ║
║  │                               │   ║
║  │ [12:35:02] Weapons activated  │   ║
║  │ [Blue left border]            │   ║
║  │                               │   ║
║  │ [12:35:01] Locked target      │   ║
║  │ [Blue left border]            │   ║
║  │                               │   ║
║  │ [12:34:57] Entering fight     │   ║
║  │ [Blue left border]            │   ║
║  │                               │   ║
║  │ [12:34:56] Bot started        │   ║
║  │ [Green left border]           │   ║
║  │                               │   ║
║  │       [Show More]             │   ║
║  │                               │   ║
║  └───────────────────────────────┘   ║
║                                       ║
╚═══════════════════════════════════════╝
```

---

## Tablet View (Landscape - iPad)

```
╔═══════════════════════════════════════════════════════════════════╗
║  ☰  🤖 AbyssalBot Control                        🟢 Connected    ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║  ┌─────────────────────────────────────────────────────────────┐ ║
║  │ 🎮 Control Panel                                            │ ║
║  ├─────────────────────────────────────────────────────────────┤ ║
║  │ [Status: Running] [State: AbyssalFightState] [Step: 1247]  │ ║
║  │                                                             │ ║
║  │      [⏸️ Pause]         [⏹️ Stop]                            │ ║
║  │                                                             │ ║
║  │               [🚨 EMERGENCY RETREAT]                        │ ║
║  │                                                             │ ║
║  │  [⚔️ Fight]  [🚀 Travel]  [🔄 Reload]  [⏳ Wait]             │ ║
║  └─────────────────────────────────────────────────────────────┘ ║
║                                                                   ║
║  ┌─────────────────────────────┐  ┌──────────────────────────┐  ║
║  │ 🚀 Ship Status              │  │ 🎯 Active Targets        │  ║
║  ├─────────────────────────────┤  ├──────────────────────────┤  ║
║  │ 📍 Abyss - Room 2           │  │ 🔴 Frigate - Starving    │  ║
║  │ 🎯 Orbit  🌀 In Abyss       │  │    Tanoo [Priority: 5]   │  ║
║  │                             │  │ Distance: 8.5 km         │  ║
║  │ Shield  ████████████░░ 85%  │  │ Shield ████████░░ 80%    │  ║
║  │ Armor   ████████████ 100%   │  │ Armor  ████████░░ 80%    │  ║
║  │ Structure ██████████ 100%   │  │ Hull   ██████████ 100%   │  ║
║  │ Capacitor ████████░░ 90%    │  │                          │  ║
║  │                             │  │ Cruiser - Ravenous       │  ║
║  │ Active Modules: 5           │  │ Overmind [Priority: 3]   │  ║
║  │ Drones in Space: 3          │  │ Distance: 12.3 km        │  ║
║  │                             │  │ Shield ██████░░░░ 60%    │  ║
║  └─────────────────────────────┘  │ Armor  ██████████ 100%   │  ║
║                                   │ Hull   ██████████ 100%   │  ║
║  ┌─────────────────────────────┐  │                          │  ║
║  │ 📋 Action Log    [Expand]   │  └──────────────────────────┘  ║
║  ├─────────────────────────────┤                                ║
║  │ [12:35:20] Target destroyed │                                ║
║  │ [12:35:15] Shield at 85%    │                                ║
║  │ [12:35:02] Weapons activated│                                ║
║  │ [12:35:01] Locked target    │                                ║
║  │ [12:34:57] Entering fight   │                                ║
║  │ [12:34:56] Bot started      │                                ║
║  │        [Show More]          │                                ║
║  └─────────────────────────────┘                                ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

## Desktop View (Full Screen - 1920x1080)

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║ ☰ Navigation     🤖 AbyssalBot Control                        🟢 Connected       ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                   ║
║  ┌───────────────────────────────────────────────────────────────────────────┐   ║
║  │ 🎮 Control Panel                                                          │   ║
║  ├───────────────────────────────────────────────────────────────────────────┤   ║
║  │                                                                           │   ║
║  │  [Status: Running]  [State: AbyssalFightState]  [Step: 1247]             │   ║
║  │                                                                           │   ║
║  │          [⏸️ Pause]              [⏹️ Stop]                                 │   ║
║  │                                                                           │   ║
║  │                      [🚨 EMERGENCY RETREAT]                               │   ║
║  │                                                                           │   ║
║  │  Manual State Control:                                                   │   ║
║  │    [⚔️ Fight]     [🚀 Travel]     [🔄 Reload]     [⏳ Wait]                │   ║
║  │                                                                           │   ║
║  └───────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                   ║
║  ┌─────────────────────────────────────────────────┐  ┌──────────────────────┐  ║
║  │ 🚀 Ship Status                                  │  │ 🎯 Active Targets    │  ║
║  ├─────────────────────────────────────────────────┤  ├──────────────────────┤  ║
║  │                                                 │  │                      │  ║
║  │ 📍 Location: Abyss - Room 2                     │  │ 🔴 Frigate -         │  ║
║  │ 🎯 Maneuver: Orbit                              │  │    Starving Tanoo    │  ║
║  │ 🌀 In Abyss: Yes                                │  │ [Highlighted]        │  ║
║  │                                                 │  │ [NPC] [Priority: 5]  │  ║
║  │ Shield                                          │  │ Distance: 8.5 km     │  ║
║  │ ████████████████████████░░░░░░  850/1000  85.0% │  │ Shield ████████░░    │  ║
║  │                                                 │  │ Armor  ████████░░    │  ║
║  │ Armor                                           │  │ Hull   ██████████    │  ║
║  │ ████████████████████████████  500/500  100.0%   │  │                      │  ║
║  │                                                 │  │ ───────────────────  │  ║
║  │ Structure                                       │  │                      │  ║
║  │ ████████████████████████████  300/300  100.0%   │  │ Cruiser - Ravenous   │  ║
║  │                                                 │  │ Overmind             │  ║
║  │ Capacitor                                       │  │ [NPC] [Priority: 3]  │  ║
║  │ ████████████████████████░░░░  450/500   90.0%   │  │ Distance: 12.3 km    │  ║
║  │                                                 │  │ Shield ██████░░░░    │  ║
║  │         ┌───┐              ┌───┐                │  │ Armor  ██████████    │  ║
║  │         │ 5 │              │ 3 │                │  │ Hull   ██████████    │  ║
║  │         └───┘              └───┘                │  │                      │  ║
║  │    Active Modules     Drones in Space           │  │                      │  ║
║  │                                                 │  │                      │  ║
║  └─────────────────────────────────────────────────┘  │                      │  ║
║                                                        │                      │  ║
║  ┌─────────────────────────────────────────────────┐  │                      │  ║
║  │ 📋 Action Log                       [Expand]    │  │                      │  ║
║  ├─────────────────────────────────────────────────┤  └──────────────────────┘  ║
║  │                                                 │                            ║
║  │ ┃ [12:35:20] Target destroyed [Green border]   │                            ║
║  │ ┃ [12:35:15] Shield at 85% [Blue border]       │                            ║
║  │ ┃ [12:35:02] Weapons activated [Blue border]   │                            ║
║  │ ┃ [12:35:01] Locked target: Frigate [Blue]     │                            ║
║  │ ┃ [12:34:57] Entering fight state [Blue]       │                            ║
║  │ ┃ [12:34:56] Bot started [Green border]        │                            ║
║  │                                                 │                            ║
║  │                    [Show More]                  │                            ║
║  │                                                 │                            ║
║  └─────────────────────────────────────────────────┘                            ║
║                                                                                   ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

---

## Interaction Examples

### 1. Starting the Bot (Mobile)

**Before tap:**
```
┌─────────────────────┐
│  ▶️  Start Bot      │
│  [Green button]     │
└─────────────────────┘
```

**During tap:**
```
┌─────────────────────┐
│  ▶️  Start Bot      │
│  [Pressed, 95%]     │
└─────────────────────┘
```

**After tap (state updates):**
```
Status badge changes:
[Stopped - Red] → [Running - Green]

Button changes:
[▶️ Start Bot] → [⏸️ Pause] + [⏹️ Stop]

Action log adds:
┃ [12:36:00] Bot started [Green]
```

### 2. Emergency Retreat

**Button appearance:**
```
╔═══════════════════════════╗
║   🚨 EMERGENCY RETREAT    ║
║   [Pulsing red effect]    ║
╚═══════════════════════════╝

Animation:
Opacity: 100% → 80% → 100% (repeat)
Duration: 2 seconds
```

**After tap:**
```
State changes to: "RetreatState"
Action log:
┃ [12:36:05] EMERGENCY RETREAT INITIATED [Red]
┃ [12:36:05] Bot paused [Yellow]
```

### 3. Real-Time HP Updates

**Shield taking damage:**
```
Frame 1:
Shield ████████████████░░░░ 80%

Frame 2 (0.3s later):
Shield █████████████░░░░░░░ 65%

Frame 3 (0.3s later):
Shield ██████████░░░░░░░░░░ 50%
[Percentage turns yellow]

Frame 4 (0.3s later):
Shield ████░░░░░░░░░░░░░░░░ 20%
[Percentage turns red, bold]
```

**Action log shows:**
```
┃ [12:36:10] Shield at 50% [Yellow]
┃ [12:36:11] Shield at 20% [Red]
┃ [12:36:12] Shield booster activated [Blue]
```

### 4. Target Lock Animation

**New target appears:**
```
┌─────────────────────────┐
│ 🔴 Frigate - Starving   │ ← Blinking red dot
│    Tanoo                │
│ [Red glowing border]    │
│                         │
│ [NPC] [Priority: 5]     │
│                         │
│ Distance: 8.5 km        │
└─────────────────────────┘

Action log:
┃ [12:36:15] Locked target: Frigate - Starving Tanoo [Blue]
```

---

## Color Palette Reference

### Status Colors
- **Running**: `#28a745` (Green)
- **Paused**: `#ffc107` (Yellow/Amber)
- **Stopped**: `#dc3545` (Red)
- **Connected**: `#28a745` (Green)
- **Disconnected**: `#dc3545` (Red)

### HP Bar Colors
- **Shield**: `#007bff` (Blue)
- **Armor**: `#ffc107` (Orange)
- **Structure**: `#dc3545` (Red)
- **Capacitor**: `#17a2b8` (Cyan)

### Alert Colors
- **Success**: `#d4edda` (Light green)
- **Warning**: `#fff3cd` (Light yellow)
- **Error**: `#f8d7da` (Light red)
- **Info**: `#d1ecf1` (Light blue)

### Background Colors
- **Card**: `#ffffff` (White)
- **Card Header**: `#f8f9fa` (Light gray)
- **Body**: `#ffffff` (White)
- **Border**: `#dee2e6` (Gray)

---

This mockup represents the visual appearance of the AbyssalBot mobile control interface across different device sizes, showing how the responsive design adapts to provide an optimal user experience on any device.
