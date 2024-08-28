using Robust.Shared.Prototypes;

namespace Content.Shared.Morbit.TCP.Abilities.Components;

[RegisterComponent]
public sealed partial class ApplyComponentAbilityComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
