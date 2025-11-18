# AbyssalBot Mobile UI - Visual Description

## Overall Layout

The interface uses a clean, card-based design optimized for mobile devices with a responsive grid layout that adapts to different screen sizes.

### Color Scheme
- **Primary Colors**: Bootstrap's standard color palette
  - Success (Green): Running status, successful actions
  - Danger (Red): Stopped status, errors, emergency controls
  - Warning (Yellow): Paused status, warnings
  - Info (Blue): Shield HP, informational messages
- **Background**: Light gray (#f8f9fa) for cards, white for main background
- **Borders**: Soft gray (#dee2e6) for card separations

## Mobile View (< 768px)

### Layout Structure
```
┌─────────────────────────┐
│  🤖 AbyssalBot Control  │ 🟢 Connected
├─────────────────────────┤
│                         │
│   Bot Control Panel     │
│   [Stacked vertically]  │
│                         │
├─────────────────────────┤
│                         │
│   Ship Status           │
│   [Full width]          │
│                         │
├─────────────────────────┤
│                         │
│   Active Targets        │
│   [Full width]          │
│                         │
├─────────────────────────┤
│                         │
│   Action Log            │
│   [Full width]          │
│                         │
└─────────────────────────┘
```

## Desktop View (> 992px)

### Layout Structure
```
┌───────────────────────────────────────────────────────┐
│  🤖 AbyssalBot Control            🟢 Connected        │
├───────────────────────────────────────────────────────┤
│                                                       │
│              Bot Control Panel                        │
│              [Full width, 3 columns]                  │
│                                                       │
├─────────────────────────────────┬─────────────────────┤
│                                 │                     │
│      Ship Status                │   Active Targets    │
│      [2 columns width]          │   [1 column width]  │
│                                 │                     │
├─────────────────────────────────┤   [Scrollable]      │
│                                 │                     │
│      Action Log                 │                     │
│      [2 columns width]          │                     │
│                                 │                     │
└─────────────────────────────────┴─────────────────────┘
```

## Component Details

### 1. Header Section

**Desktop:**
```
╔═══════════════════════════════════════════════════════╗
║  🤖 AbyssalBot Control            🟢 Connected        ║
╚═══════════════════════════════════════════════════════╝
```

**Mobile:**
```
╔═══════════════════╗
║ 🤖 AbyssalBot     ║
║ Control           ║
║                   ║
║  🟢 Connected     ║
╚═══════════════════╝
```

- Left side: Large "AbyssalBot Control" heading
- Right side: Connection status badge (green when connected, red when disconnected)
- Font: Bold, 1.5rem on mobile, 2rem on desktop

### 2. Bot Control Panel Card

```
┌─────────────────────────────────────────┐
│ 🎮 Control Panel                        │
├─────────────────────────────────────────┤
│                                         │
│  ┌────────────────────────────────────┐ │
│  │ Status: Running  State: Fight      │ │
│  │ Step: 1247                         │ │
│  └────────────────────────────────────┘ │
│                                         │
│  [  ⏸️ Pause  ]  [  ⏹️ Stop  ]          │
│                                         │
│  ┌────────────────────────────────────┐ │
│  │  🚨 EMERGENCY RETREAT               │ │
│  └────────────────────────────────────┘ │
│                                         │
│  Manual State Control                   │
│  [ ⚔️ Fight ] [ 🚀 Travel ]              │
│  [ 🔄 Reload ] [ ⏳ Wait ]               │
│                                         │
└─────────────────────────────────────────┘
```

**Status Badges:**
- Running: Green background, green border
- Paused: Yellow background, orange border
- Stopped: Red background, red border

**Buttons:**
- Large, touch-friendly (minimum 60px height on mobile)
- Primary controls: Full width on mobile, side-by-side on desktop
- Emergency button: Red, pulsing animation, full width
- State control buttons: Grid layout, 2x2 on mobile

### 3. Ship Status Component

```
┌─────────────────────────────────────────┐
│ 🚀 Ship Status                          │
├─────────────────────────────────────────┤
│                                         │
│ 📍 Location: Abyss - Room 2             │
│ 🎯 Maneuver: Orbit                      │
│ 🌀 In Abyss: Yes                        │
│                                         │
│ Shield                                  │
│ ████████████████░░░░  850/1000  85.0%   │
│                                         │
│ Armor                                   │
│ ████████████████████  500/500  100.0%   │
│                                         │
│ Structure                               │
│ ████████████████████  300/300  100.0%   │
│                                         │
│ Capacitor                               │
│ ████████████████████  450/500   90.0%   │
│                                         │
│     ┌───┐              ┌───┐            │
│     │ 5 │              │ 3 │            │
│     └───┘              └───┘            │
│ Active Modules     Drones in Space      │
│                                         │
└─────────────────────────────────────────┘
```

**HP Bars:**
- Height: 30px for easy reading
- Colors:
  - Shield: Blue (#007bff)
  - Armor: Orange/Yellow (#ffc107)
  - Structure: Red (#dc3545)
  - Capacitor: Cyan (#17a2b8)
- Text: White, centered, shows current/max values
- Percentage: Right-aligned below bar
  - Green if > 50%
  - Yellow if 25-50%
  - Red if < 25%

**Info Boxes:**
- Light gray background (#f8f9fa)
- Flexbox layout on desktop (3 columns)
- Stacked on mobile

### 4. Target List Component

```
┌─────────────────────────────────────────┐
│ 🎯 Active Targets                       │
├─────────────────────────────────────────┤
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ 🔴 Frigate - Starving Tanoo         │ │
│ │ [NPC] [Priority: 5]                 │ │
│ │                                     │ │
│ │ Distance: 8.5 km                    │ │
│ │                                     │ │
│ │ Shield  ████████░░  80%             │ │
│ │ Armor   ████████░░  80%             │ │
│ │ Hull    ██████████  100%            │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ Cruiser - Ravenous Overmind         │ │
│ │ [NPC] [Priority: 3]                 │ │
│ │                                     │ │
│ │ Distance: 12.3 km                   │ │
│ │                                     │ │
│ │ Shield  ██████░░░░  60%             │ │
│ │ Armor   ██████████  100%            │ │
│ │ Hull    ██████████  100%            │ │
│ └─────────────────────────────────────┘ │
│                                         │
└─────────────────────────────────────────┘
```

**Active Target Highlighting:**
- Red border (2px solid #dc3545)
- Light red background (#fff5f5)
- Glowing shadow effect
- Blinking red indicator (🔴) next to name

**Target Cards:**
- White background
- Gray border for inactive targets
- Rounded corners (8px)
- Padding for touch-friendly spacing

**Mini HP Bars:**
- Height: 8px (smaller than ship status)
- Same color scheme
- Label on left (60px width)

### 5. Action Log Component

```
┌─────────────────────────────────────────┐
│ 📋 Action Log               [Expand]    │
├─────────────────────────────────────────┤
│                                         │
│ ┃ [12:34:56] Bot started                │
│ ┃ [12:34:57] Entering fight state       │
│ ┃ [12:35:01] Locked target: Frigate     │
│ ┃ [12:35:02] Weapons activated          │
│ ┃ [12:35:15] Shield at 85%              │
│ ┃ [12:35:20] Target destroyed           │
│                                         │
│            [Show More]                  │
│                                         │
└─────────────────────────────────────────┘
```

**Log Entry Colors:**
- **Error/Emergency**: Red background, red left border (4px)
  ```
  ┃ [12:35:30] ERROR: Emergency retreat!
  ```
- **Warning**: Yellow background, yellow left border
  ```
  ┃ [12:35:25] WARNING: Shield low
  ```
- **Success**: Green background, green left border
  ```
  ┃ [12:35:20] Target destroyed
  ```
- **Info**: Blue background, blue left border
  ```
  ┃ [12:35:15] Shield at 85%
  ```

**Features:**
- Monospace font for timestamps
- Auto-scroll to newest entries
- Expand/collapse functionality
- "Show More" button to load older entries
- Max height: 300px (collapsed), 600px (expanded)

## Animations and Interactions

### Loading State
```
┌─────────────────────────────────────────┐
│                                         │
│         ⭕ (spinning)                   │
│                                         │
│      Connecting to bot...               │
│                                         │
└─────────────────────────────────────────┘
```

### Button Press Animation
- Scale down to 95% on touch
- Duration: 200ms
- Ease-out transition

### Emergency Button
- Continuous pulse animation
- Opacity oscillates between 1.0 and 0.8
- Duration: 2s infinite

### Active Target Blink
- Opacity: 1.0 → 0.3 → 1.0
- Duration: 1s infinite
- Applies to the 🔴 indicator

### Connection Status Transitions
- Fade in/out when changing state
- Color change animation (300ms)

## Accessibility Features

### Touch Targets
- Minimum size: 44x44px (Apple HIG standard)
- Buttons: 60px height on mobile
- Padding: Generous spacing between interactive elements

### Text Contrast
- All text meets WCAG AA standards
- Important values (HP percentages) are bold
- Color is never the only indicator (text + icons)

### Mobile Gestures
- Swipe to scroll (no conflicting gestures)
- Pull to refresh (optional feature)
- Tap highlight disabled (CSS: -webkit-tap-highlight-color: transparent)

## Dark Mode Support

When system prefers dark mode:

```
┌─────────────────────────────────────────┐
│ Background: #1a1a1a (dark gray)         │
│ Cards: #2a2a2a (lighter gray)           │
│ Text: #e0e0e0 (light gray)              │
│ Borders: #444 (medium gray)             │
└─────────────────────────────────────────┘
```

- Preserves color-coded status indicators
- Reduces eye strain in low-light conditions
- Automatically activates based on system preference

## Performance Optimizations

### Visual Feedback
- HP bars: CSS transitions (300ms) for smooth updates
- State changes: Instant visual feedback
- Real-time updates: No visible lag (< 100ms via SignalR)

### Smooth Scrolling
- Native scrolling (no JavaScript scroll hijacking)
- Momentum scrolling on iOS
- Sticky headers for context

## Browser Support

Tested and optimized for:
- ✅ Chrome Mobile (Android)
- ✅ Safari (iOS)
- ✅ Chrome Desktop
- ✅ Edge Desktop
- ✅ Firefox Desktop

Minimum requirements:
- Modern browser with WebSocket support
- JavaScript enabled
- Cookies enabled (for authentication)
