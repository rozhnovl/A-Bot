namespace AbyssalBot.Domain.Enums;

/// <summary>
/// Represents types of electronic warfare
/// </summary>
public enum EwarType
{
    /// <summary>
    /// No EWAR active
    /// </summary>
    None,

    /// <summary>
    /// ECM Jammed - cannot lock targets
    /// </summary>
    Jammed,

    /// <summary>
    /// Sensor dampened - reduced lock range
    /// </summary>
    Dampened,

    /// <summary>
    /// Tracking disrupted - reduced weapon tracking
    /// </summary>
    TrackingDisrupted,

    /// <summary>
    /// Webbed - speed reduced
    /// </summary>
    Webbed,

    /// <summary>
    /// Warp scrambled - cannot warp
    /// </summary>
    Scrambled,

    /// <summary>
    /// Energy neutralized - capacitor drained
    /// </summary>
    Neutralized
}
