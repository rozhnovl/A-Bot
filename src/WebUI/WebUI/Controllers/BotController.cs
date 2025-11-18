using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models;
using WebUI.Services;

namespace WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class BotController : ControllerBase
{
    private readonly IBotStateService _botStateService;
    private readonly ILogger<BotController> _logger;

    public BotController(IBotStateService botStateService, ILogger<BotController> logger)
    {
        _botStateService = botStateService;
        _logger = logger;
    }

    /// <summary>
    /// Get current bot state
    /// </summary>
    [HttpGet("state")]
    [ProducesResponseType(typeof(BotStateDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BotStateDto>> GetState()
    {
        var state = await _botStateService.GetCurrentStateAsync();
        return Ok(state);
    }

    /// <summary>
    /// Send a command to the bot
    /// </summary>
    [HttpPost("command")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendCommand([FromBody] BotCommandDto command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _botStateService.SendCommandAsync(command);

        if (!success)
        {
            return BadRequest("Failed to process command");
        }

        return Ok(new { success = true, message = "Command processed successfully" });
    }

    /// <summary>
    /// Start the bot
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult> Start()
    {
        var command = new BotCommandDto { CommandType = BotCommandType.Start };
        await _botStateService.SendCommandAsync(command);
        return Ok(new { message = "Bot started" });
    }

    /// <summary>
    /// Stop the bot
    /// </summary>
    [HttpPost("stop")]
    public async Task<ActionResult> Stop()
    {
        var command = new BotCommandDto { CommandType = BotCommandType.Stop };
        await _botStateService.SendCommandAsync(command);
        return Ok(new { message = "Bot stopped" });
    }

    /// <summary>
    /// Pause the bot
    /// </summary>
    [HttpPost("pause")]
    public async Task<ActionResult> Pause()
    {
        var command = new BotCommandDto { CommandType = BotCommandType.Pause };
        await _botStateService.SendCommandAsync(command);
        return Ok(new { message = "Bot paused" });
    }

    /// <summary>
    /// Resume the bot
    /// </summary>
    [HttpPost("resume")]
    public async Task<ActionResult> Resume()
    {
        var command = new BotCommandDto { CommandType = BotCommandType.Resume };
        await _botStateService.SendCommandAsync(command);
        return Ok(new { message = "Bot resumed" });
    }

    /// <summary>
    /// Emergency retreat
    /// </summary>
    [HttpPost("emergency-retreat")]
    public async Task<ActionResult> EmergencyRetreat()
    {
        var command = new BotCommandDto { CommandType = BotCommandType.EmergencyRetreat };
        await _botStateService.SendCommandAsync(command);
        return Ok(new { message = "Emergency retreat initiated" });
    }

    /// <summary>
    /// Get action log
    /// </summary>
    [HttpGet("log")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetLog([FromQuery] int count = 50)
    {
        var log = await _botStateService.GetActionLogAsync(count);
        return Ok(log);
    }
}
