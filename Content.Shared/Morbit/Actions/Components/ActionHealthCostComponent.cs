using Content.Shared.Damage;

namespace Content.Shared.Morbit.TCP.Components;

[RegisterComponent]
public sealed partial class ActionHealthCostComponent : Component
{
    /// <summary>
    ///     Ability cannot be cast if the entity has more than this amount of damage.
    /// </summary>
    [DataField]
    public float MaximumDamage = 0.0f;

    /// <summary>
    ///     Damage received upon attempting to cast action.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = default!;
}
