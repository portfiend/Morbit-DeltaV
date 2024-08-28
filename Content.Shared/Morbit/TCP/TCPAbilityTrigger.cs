namespace Content.Shared.Morbit.TCP;

public enum TCPAbilityTrigger : byte
{
    /// <summary>
    ///     TCP must enter an "ascension" state, gains a targeted action that can be used repeatedly
    /// </summary>
    ActiveAscension,

    /// <summary>
    ///     TCP has an Action they can use against a target
    /// </summary>
    ActiveTargeted,

    /// <summary>
    ///     TCP has an Action to toggle their ability's status, often a passive effect
    /// </summary>
    ActiveStatus,

    /// <summary>
    ///     TCP has an Action to do something without a target
    /// </summary>
    ActiveSelf,

    /// <summary>
    ///     TCP performs some passive effect that can be "pulsed" to create instant effects
    /// </summary>
    PassiveWithPulse,

    /// <summary>
    ///     TCP has a passive ability only
    /// </summary>
    Passive,

    /// <summary>
    ///     The "default" ability of a TCP type. 
    /// <summary> 
    Default,

    /// <summary>
    ///     TCP has no ability
    /// </summary>
    Nullified
}
