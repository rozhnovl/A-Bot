# Quick Start Guide - AbyssalBot Mobile UI

## 5-Minute Setup

### Step 1: Build and Run (Development)

```bash
cd /home/user/A-Bot/src/WebUI/WebUI
dotnet run
```

You should see output like:
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5000
```

### Step 2: Access the UI

**From your computer:**
- Navigate to: `https://localhost:7001/bot-control`

**From your mobile device:**
1. Find your computer's local IP address:
   ```bash
   # On Linux
   ip addr show | grep "inet " | grep -v 127.0.0.1

   # On Windows
   ipconfig

   # On Mac
   ifconfig | grep "inet " | grep -v 127.0.0.1
   ```

2. Access from phone: `https://[YOUR-IP]:7001/bot-control`
   - Example: `https://192.168.1.100:7001/bot-control`

3. Accept the self-signed certificate warning (dev only)

### Step 3: Create an Account

1. Click **Register** in the navigation
2. Enter email and password
3. Click **Register** (email confirmation is disabled in dev mode)
4. You'll be automatically logged in

### Step 4: Access Bot Control

1. Click **Bot Control** in the navigation
2. You should see:
   - ✅ Green "Connected" status
   - Bot control panel
   - Ship status (with mock data)
   - Empty targets list
   - Action log with "Bot initialized" message

### Step 5: Test the Interface

**Try these controls:**

1. **Start the Bot**
   - Click the "▶️ Start Bot" button
   - Status should change to "Running" (green)
   - Action log should show "Bot started"

2. **Pause the Bot**
   - Click "⏸️ Pause"
   - Status should change to "Paused" (yellow)

3. **Change State**
   - Click "🚀 Travel" button
   - Watch the "State" badge update
   - See action log entry

4. **Emergency Retreat**
   - Click the red "🚨 EMERGENCY RETREAT" button
   - Observe state change

## Integration with Real Bot

Currently, the UI uses **mock data**. To connect it to your actual bot:

### Option 1: Quick Test Integration

Add this to your bot's main loop:

```csharp
using WebUI.Services;
using WebUI.BotIntegration;

// In your bot initialization
var botStateService = serviceProvider.GetRequiredService<IBotStateService>();
var adapter = new BotControlAdapter(bot, botStateService, logger);

// In your bot loop
while (shouldRun)
{
    if (adapter.ShouldExecuteStep())
    {
        var stepResult = bot.Step(input);

        // Update UI after each step
        await adapter.UpdateUiAfterStepAsync(currentState.GetType().Name);
    }

    await Task.Delay(100); // Or your preferred delay
}
```

### Option 2: Full Integration

1. **Add project reference:**
   ```xml
   <!-- In AbyssalBot.Application.csproj -->
   <ItemGroup>
     <ProjectReference Include="..\..\WebUI\WebUI\WebUI.csproj" />
   </ItemGroup>
   ```

2. **Register services:**
   ```csharp
   // In your bot's DI setup
   services.AddSingleton<IBotStateService, BotStateService>();
   services.AddSingleton<BotControlAdapter>();
   ```

3. **Update bot loop:**
   See `BotControlAdapter.cs` for full implementation details

## Common Issues and Solutions

### Issue 1: Certificate Warning on Mobile

**Symptom:** Browser shows "Your connection is not private"

**Solution:**
- Development only: Click "Advanced" → "Proceed to localhost (unsafe)"
- Production: Use proper SSL certificate

### Issue 2: Can't Connect from Mobile

**Symptom:** Page doesn't load from phone

**Solutions:**
1. Ensure both devices are on the same WiFi network
2. Check firewall isn't blocking port 7001
3. On Windows, allow through firewall:
   ```powershell
   netsh advfirewall firewall add rule name="ASP.NET Core" dir=in action=allow protocol=TCP localport=7001
   ```

### Issue 3: SignalR Disconnected

**Symptom:** Red "Disconnected" status

**Solutions:**
1. Check browser console for errors (F12)
2. Verify SignalR is registered in Program.cs
3. Ensure WebSocket isn't blocked by proxy/firewall
4. Try refreshing the page

### Issue 4: UI Not Updating

**Symptom:** Ship status shows mock data, doesn't change

**Solution:**
- This is expected! The current implementation uses mock data
- See "Integration with Real Bot" section above
- Check that `BotStateService.UpdateStateAsync()` is being called

### Issue 5: API Endpoints Return 401

**Symptom:** Unauthorized errors in browser console

**Solutions:**
1. Ensure you're logged in
2. Check that `[Authorize]` attribute is on controller
3. Verify authentication cookie is set

## Testing Without Bot

You can test the UI without connecting to a real bot:

```csharp
// In BotStateService.cs, modify InitializeMockState() to update periodically

private Timer _mockUpdateTimer;

public BotStateService(IHubContext<BotHub> hubContext, ILogger<BotStateService> logger)
{
    // ... existing code ...

    // Add mock updates every 2 seconds
    _mockUpdateTimer = new Timer(async _ => await SimulateBotActivity(), null, 2000, 2000);
}

private async Task SimulateBotActivity()
{
    // Simulate shield going down
    if (_currentState.ShipStatus != null)
    {
        _currentState.ShipStatus.ShieldHp = Math.Max(0, _currentState.ShipStatus.ShieldHp - 10);

        if (_currentState.ShipStatus.ShieldHp <= 0)
            _currentState.ShipStatus.ShieldHp = 1000; // Reset
    }

    // Add random log entry
    LogAction($"Simulated action {DateTime.Now.Ticks}");

    await BroadcastStateAsync();
}
```

## Production Deployment

### Step 1: Configure HTTPS

```bash
# Generate production certificate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Step 2: Update appsettings.json

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443"
      }
    }
  }
}
```

### Step 3: Deploy

```bash
# Publish
dotnet publish -c Release -o ./publish

# Run in production
cd publish
ASPNETCORE_ENVIRONMENT=Production dotnet WebUI.dll
```

### Step 4: Configure Reverse Proxy (Optional)

Using nginx:
```nginx
server {
    listen 80;
    server_name bot.yourdomain.com;

    location / {
        proxy_pass https://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
    }
}
```

## Next Steps

1. **Customize the UI**
   - Edit `/wwwroot/css/bot-control.css` for styling
   - Modify components in `/Components/` folder

2. **Add More Features**
   - Target priority adjustment
   - Module activation controls
   - Configuration editor
   - Statistics dashboard

3. **Improve Security**
   - Add rate limiting (AspNetCoreRateLimit package)
   - Enable 2FA authentication
   - Add API key authentication for bot commands

4. **Monitor Performance**
   - Add Application Insights
   - Set up logging to file/database
   - Create performance dashboard

## Support and Resources

- **Full Documentation:** See `MOBILE_UI_README.md`
- **UI Description:** See `UI_DESCRIPTION.md`
- **Integration Guide:** See `BotIntegration/` folder
- **Bot Source Code:** `/home/user/A-Bot/src/Sanderling.ABot/`

## Feedback

If you encounter issues or have suggestions, please create an issue in the repository.

Happy botting! 🤖
