using Content.Shared.Actions;
using Content.Shared.Morbit.TCP.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Morbit.TCP.Components;

[RegisterComponent]
public sealed partial class TCPAbilityComponent : Component
{
    /// <summary>
    ///     Method of activating the ability.
    /// </summary>
    [DataField]
    public TCPAbilityTrigger AbilityTrigger = TCPAbilityTrigger.Nullified;

    /// <summary>
    ///     List of abilities that this TCP has, usually based on typing.
    /// </summary>
    [DataField]
    public List<ProtoId<TCPAbilityPrototype>> Abilities = new();

    /// <summary>
    ///     Generated by the system. A class that loads/unloads actions.
    /// </summary>
    public ITCPAbilityType? AbilityTypeClass;

    /// <summary>
    ///     Generated by the system. Actions loaded by the AbilityTypeClass.
    /// </summary>
    public List<EntityUid> Actions = new();
}

public sealed partial class ToggleAscensionEvent : InstantActionEvent
{ }

public sealed partial class UseAbilityEvent : InstantActionEvent
{ }

public sealed partial class UseAbilityTargetedEvent : EntityTargetActionEvent
{ }

public sealed partial class ToggleStatusAbilityEvent : InstantActionEvent
{ }

public sealed partial class PulseAbilityEvent : InstantActionEvent
{ }
