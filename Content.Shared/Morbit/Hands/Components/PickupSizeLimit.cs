using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Server.Morbit.Hands.Components;

[RegisterComponent]
public sealed partial class PickupSizeLimitComponent : Component
{
    /// <summary>
    ///    Maximum item size for pickup checks
    /// </summary>
    [DataField]
    public ProtoId<ItemSizePrototype>? MaximumSize = null;
}
