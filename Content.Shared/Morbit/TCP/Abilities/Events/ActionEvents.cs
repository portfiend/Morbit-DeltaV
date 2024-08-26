using Content.Shared.Actions;

namespace Content.Shared.Morbit.TCP.Abilities.Events;

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
