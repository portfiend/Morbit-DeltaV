using Robust.Shared.Prototypes;

namespace Content.Shared.Morbit.TCP.Prototypes;

[Prototype("tcpAbility")]
public sealed partial class TCPAbilityPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     A representation of possible effects an ability can have, depending on ability type.
    /// </summary>
    [DataField]
    public Dictionary<TCPAbilityTrigger, string> Abilities = new();
}
