# AbyssalBot Mobile Control UI

A mobile-first, responsive web interface for controlling and monitoring the AbyssalBot from any device.

## Features

### Real-Time Bot Monitoring
- **Live Bot Status**: See if the bot is running, paused, or stopped
- **Current State Display**: Track which state the bot is in (Fight, Travel, Reload, etc.)
- **Step Counter**: Monitor bot execution progress

### Ship Status Dashboard
- **Shield/Armor/Structure HP**: Visual progress bars with percentages
- **Capacitor Level**: Real-time energy monitoring
- **Location Tracking**: Current system and Abyss status
- **Active Modules**: Count of currently active ship modules
- **Drones in Space**: Number of deployed drones

### Target Management
- **Active Targets List**: See all current targets
- **Target HP Bars**: Shield, armor, and structure status
- **Distance Display**: Range to each target
- **Priority Indicators**: Target priority levels
- **Active Target Highlight**: Clearly marked current focus

### Control Panel
- **Start/Stop Bot**: Basic bot lifecycle control
- **Pause/Resume**: Temporary pausing without full shutdown
- **Emergency Retreat**: Panic button for immediate retreat
- **Manual State Control**: Force bot into specific states:
  - Fight State
  - Travel State
  - Reload State
  - Wait State

### Action Log
- **Recent Actions**: Chronological log of bot activities
- **Color-Coded Messages**:
  - Red: Errors and emergencies
  - Yellow: Warnings
  - Green: Successes
  - Blue: Info messages
- **Expandable Log**: View more history as needed

## Technology Stack

- **Frontend**: Blazor Server with Interactive Server Components
- **Real-Time Updates**: SignalR for WebSocket communication
- **Backend**: ASP.NET Core 9.0
- **API**: RESTful API endpoints + SignalR hub
- **CSS**: Mobile-first responsive design with Bootstrap 5
- **Authentication**: ASP.NET Core Identity (already integrated)

## File Structure

```
WebUI/
├── Components/
│   ├── Pages/
│   │   └── BotControl.razor           # Main dashboard page
│   ├── BotControlPanel.razor          # Control buttons component
│   ├── ShipStatusComponent.razor      # Ship HP/Cap display
│   ├── TargetListComponent.razor      # Targets list
│   └── ActionLogComponent.razor       # Action log viewer
├── Controllers/
│   └── BotController.cs               # REST API endpoints
├── Hubs/
│   └── BotHub.cs                      # SignalR hub
├── Models/
│   └── BotStateDto.cs                 # Data transfer objects
├── Services/
│   ├── IBotStateService.cs            # Service interface
│   └── BotStateService.cs             # Bot state management
└── wwwroot/
    └── css/
        └── bot-control.css            # Mobile-responsive styles
```

## API Endpoints

### GET /api/bot/state
Get current bot state including ship status, targets, and logs.

**Response:**
```json
{
  "currentState": "AbyssalFightState",
  "isRunning": true,
  "isPaused": false,
  "shipStatus": { ... },
  "activeTargets": [ ... ],
  "recentActions": [ ... ]
}
```

### POST /api/bot/start
Start the bot.

### POST /api/bot/stop
Stop the bot.

### POST /api/bot/pause
Pause the bot.

### POST /api/bot/resume
Resume the bot.

### POST /api/bot/emergency-retreat
Trigger emergency retreat.

### POST /api/bot/command
Send custom command to the bot.

**Request Body:**
```json
{
  "commandType": "ChangeState",
  "parameters": {
    "state": "TravelState"
  }
}
```

## SignalR Hub

**Hub URL**: `/bothub`

**Events:**
- `ReceiveBotState`: Broadcast when bot state changes

## How to Access

1. **Start the WebUI application**
   ```bash
   cd src/WebUI/WebUI
   dotnet run
   ```

2. **Navigate to the Bot Control page**
   - Desktop: https://localhost:7XXX/bot-control
   - Mobile: Access the same URL from your phone (ensure both devices are on the same network)

3. **Login** (if authentication is required)
   - Use the Register link to create an account
   - Or login with existing credentials

## Mobile Usage Tips

### Touch-Friendly Design
- All buttons are large (minimum 44x44px touch targets)
- Swipe-friendly scrolling
- No hover-dependent features

### Responsive Layout
- **Mobile (< 768px)**: Single column, stacked components
- **Tablet (768px - 992px)**: Two-column grid
- **Desktop (> 992px)**: Three-column optimized layout

### Performance
- Real-time updates via WebSocket (low latency)
- Efficient rendering with Blazor Server
- Progressive loading of action log

## Security Features

### Authentication
- Requires ASP.NET Core Identity login (can be toggled)
- Cookie-based authentication
- HTTPS enforced in production

### Authorization
- Controller endpoints require `[Authorize]` attribute
- Per-user sessions tracked
- SignalR connections authenticated

### Rate Limiting
- (TODO) Add rate limiting for command endpoints
- Prevent command spam

## Integration with AbyssalBot

### Current Implementation
The current implementation includes a **mock bot state service** for demonstration purposes. To integrate with the actual bot:

### Step 1: Create Bot State Adapter
Create an adapter class in `AbyssalBot.Application` that implements `IBotStateService`:

```csharp
public class AbyssalBotAdapter : IBotStateService
{
    private readonly Bot _bot;

    public async Task<BotStateDto> GetCurrentStateAsync()
    {
        // Convert bot internal state to DTO
        return new BotStateDto
        {
            CurrentState = _bot.Strategy.CurrentState.GetType().Name,
            IsRunning = _bot.IsRunning,
            ShipStatus = MapShipStatus(_bot.MemoryMeasurement),
            // ... etc
        };
    }
}
```

### Step 2: Hook into Bot Step Loop
In the bot's main execution loop, broadcast state changes:

```csharp
public BotStepResult Step(BotStepInput input)
{
    var result = base.Step(input);

    // Broadcast to WebUI
    await _botStateService.UpdateStateAsync(MapToDto());

    return result;
}
```

### Step 3: Implement Command Handler
Process commands from the UI:

```csharp
public async Task<bool> SendCommandAsync(BotCommandDto command)
{
    switch (command.CommandType)
    {
        case BotCommandType.Start:
            _bot.Start();
            break;
        case BotCommandType.EmergencyRetreat:
            _bot.Strategy.ForceState(new RetreatState());
            break;
        // ... etc
    }
}
```

## Customization

### Adding New Commands
1. Add command type to `BotCommandType` enum
2. Add button to `BotControlPanel.razor`
3. Handle command in `BotStateService.SendCommandAsync()`

### Adding New Metrics
1. Add property to `ShipStatusDto`
2. Update `ShipStatusComponent.razor` to display it
3. Map data in bot adapter

### Styling
- Edit `/wwwroot/css/bot-control.css`
- Mobile-first approach: default styles are for mobile
- Use media queries for desktop enhancements

## Troubleshooting

### SignalR Connection Issues
- Check browser console for errors
- Verify WebSocket is allowed through firewall
- Ensure HTTPS is properly configured

### State Not Updating
- Check that bot is calling `UpdateStateAsync()`
- Verify SignalR hub is registered in Program.cs
- Check browser network tab for failed connections

### Mobile Layout Issues
- Clear browser cache
- Verify viewport meta tag is present
- Check CSS media queries

## Future Enhancements

### Planned Features
- [ ] Abyss progress tracker (room counter, timer)
- [ ] Historical statistics and graphs
- [ ] Multiple bot instance support
- [ ] Push notifications for critical events
- [ ] Configurable alert thresholds
- [ ] Module activation/deactivation controls
- [ ] Target priority adjustment
- [ ] Bookmark management
- [ ] Config file editor

### Performance Optimizations
- [ ] Redis caching for bot state
- [ ] Batched state updates
- [ ] Compression for SignalR messages
- [ ] Progressive Web App (PWA) support for offline access

## Development

### Running in Development
```bash
dotnet watch --project src/WebUI/WebUI/WebUI.csproj
```

### Testing API Endpoints
```bash
# Get bot state
curl -X GET https://localhost:7XXX/api/bot/state \
  -H "Authorization: Bearer {token}"

# Start bot
curl -X POST https://localhost:7XXX/api/bot/start \
  -H "Authorization: Bearer {token}"
```

### Testing SignalR
Use browser console:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/bothub")
    .build();

connection.on("ReceiveBotState", (state) => {
    console.log("Bot state:", state);
});

await connection.start();
```

## License

Same as AbyssalBot main project.

## Support

For issues or questions, please create an issue in the main repository.
