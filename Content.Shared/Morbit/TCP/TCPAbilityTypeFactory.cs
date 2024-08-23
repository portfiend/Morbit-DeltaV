using Content.Shared.Actions;

namespace Content.Shared.Morbit.TCP;

public static class TCPAbilityTypeFactory
{
    public static ITCPAbilityType CreateAbilityType(TCPAbilityTrigger trigger,
        EntityUid user,
        SharedActionsSystem actionsSystem)
    {
        return trigger switch
        {
            TCPAbilityTrigger.ActiveAscension => new ActiveAscensionAbility(user, actionsSystem),
            TCPAbilityTrigger.ActiveTargeted => new ActiveTargetedAbility(user, actionsSystem),
            TCPAbilityTrigger.ActiveStatus => new ActiveStatusAbility(user, actionsSystem),
            TCPAbilityTrigger.ActiveSelf => new ActiveSelfAbility(user, actionsSystem),
            TCPAbilityTrigger.PassiveWithPulse => new PassiveWithPulseAbility(user, actionsSystem),
            TCPAbilityTrigger.Passive => new PassiveAbility(user, actionsSystem),
            _ => new NullifiedAbility(user, actionsSystem)
        };
    }
}
