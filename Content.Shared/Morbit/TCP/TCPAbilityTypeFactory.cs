using Content.Shared.Actions;

namespace Content.Shared.Morbit.TCP;

public static class TCPAbilityTypeFactory
{
    public static ITCPAbilityType CreateAbilityType(TCPAbilityTrigger trigger,
        EntityUid user)
    {
        return trigger switch
        {
            TCPAbilityTrigger.ActiveAscension => new ActiveAscensionAbility(user),
            TCPAbilityTrigger.ActiveTargeted => new ActiveTargetedAbility(user),
            TCPAbilityTrigger.ActiveStatus => new ActiveStatusAbility(user),
            TCPAbilityTrigger.ActiveSelf => new ActiveSelfAbility(user),
            TCPAbilityTrigger.PassiveWithPulse => new PassiveWithPulseAbility(user),
            TCPAbilityTrigger.Passive => new PassiveAbility(user),
            _ => new NullifiedAbility(user)
        };
    }
}
