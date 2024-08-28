using Robust.Shared.Serialization;

namespace Content.Shared.Morbit.TCP.Abilities.Events;

[NetSerializable, Serializable]
public abstract partial class TCPAbilityEvent : EntityEventArgs
{
    /// <summary>
    ///     The TCP using this ability.
    /// </summary>
    public readonly EntityUid User;

    /// <summary>
    ///     The target entity.
    /// </summary>
    public readonly EntityUid? Target;

    protected TCPAbilityEvent(EntityUid user, EntityUid? target)
    {
        User = user;
        Target = target;
    }
}

public sealed partial class TCPAbilityActivatedEvent : TCPAbilityEvent
{
    /// <summary>
    ///     The severity of the effect.
    /// </summary>
    public readonly float Strength;

    public TCPAbilityActivatedEvent(EntityUid user,
        EntityUid? target,
        float strength)
        : base(user, target)
    {
        Strength = strength;
    }
}

public sealed partial class TCPAbilityDeactivatedEvent : TCPAbilityEvent
{
    public TCPAbilityDeactivatedEvent(EntityUid user,
        EntityUid? target)
        : base(user, target)
    { }
}

public sealed partial class TCPAscendedEvent : EntityEventArgs
{ }