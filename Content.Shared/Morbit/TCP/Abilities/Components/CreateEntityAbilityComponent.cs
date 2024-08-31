using Robust.Shared.Prototypes;

namespace Content.Shared.Morbit.TCP.Abilities.Components;

[RegisterComponent]
public sealed partial class CreateEntityAbilityComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Prototype = string.Empty;

    [DataField]
    public bool PickupOnSpawn = true;
}
