using System.Diagnostics.CodeAnalysis;
using Content.Shared.Actions;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Morbit.TCP.Abilities.Events;
using Content.Shared.Morbit.TCP.Components;
using Content.Shared.Morbit.TCP.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Morbit.TCP;

public interface ITCPAbilityType
{
    void Load();
    void Unload();
    void Activate();
}

public interface ITCPToggleableAbilityType
{
    void Deactivate();
}

/// <summary>
///     Generic class for TCP ability types.
/// </summary>
public abstract partial class TCPAbilityType : ITCPAbilityType
{
    protected const string ABILITY_ACTION_PROTOTYPE = "ActionTCPAbility";
    protected const float HEALTH_LEVEL = 8.0f;
    private const string HEALTH_COST_DAMAGE_TYPE = "Strain";

    protected virtual TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.Nullified;
    protected readonly IEntityManager Entities;

    protected EntityUid User;

    protected TCPAbilityType(EntityUid user)
    {
        User = user;
        Entities = IoCManager.Resolve<IEntityManager>();
    }

    public virtual void Load()
    { }

    public virtual void Unload()
    {
        if (!GetAbilityComponent(out var component))
            return;

        var actions = Entities.System<SharedActionsSystem>();

        foreach (var action in component.Actions)
        {
            actions.RemoveAction(action);
        }
    }

    public virtual void Activate()
    { }

    protected bool GetAbilityComponent([NotNullWhen(true)] out TCPAbilityComponent? ability)
    {
        Entities.TryGetComponent<TCPAbilityComponent>(User, out var comp);
        ability = comp;
        return comp is not null;
    }

    private void UpdateActionCosts(float levelCost = 1.0f, ActionHealthCostComponent? comp = null)
    {
        var mobThresholdSystem = Entities.System<MobThresholdSystem>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        prototypeManager.TryIndex<DamageTypePrototype>(HEALTH_COST_DAMAGE_TYPE, out var damageType);

        if (damageType is null || comp is null)
            return;

        var damageSpecifier = new Damage.DamageSpecifier(damageType, HEALTH_LEVEL * levelCost);
        comp.Damage = damageSpecifier;

        mobThresholdSystem.TryGetThresholdForState(User, MobState.Critical, out var critThreshold);
        if (critThreshold is null)
            return;

        comp.MaximumDamage = (float)critThreshold - levelCost * HEALTH_LEVEL;
    }

    /// <summary>
    ///     Creates a basic "Ability" action and adds it to the user.
    ///     Also sets the health cost of the ability, while we're at it.
    /// </summary>
    /// <param name="actionProto">Prototype of the action to use.</param>
    /// <param name="levelCost">How many health levels the action costs.</param>
    protected EntityUid? LoadAbilityAction(float levelCost = 1.0f,
        string actionProto = ABILITY_ACTION_PROTOTYPE)
    {
        if (!GetAbilityComponent(out var comp))
            return null;

        var actions = Entities.System<SharedActionsSystem>();
        EntityUid? actionRef = null;
        actions.AddAction(User, ref actionRef, actionProto);

        if (actionRef is null)
            return actionRef;

        var action = actionRef.Value;
        var healthCost = Entities.EnsureComponent<ActionHealthCostComponent>(action);
        UpdateActionCosts(levelCost, healthCost);

        comp.Actions.Add(action);
        return action;
    }

    /// <summary>
    ///     Gets the corresponding effect from a TCPAbilityPrototype's ability list.
    ///     These effects correspond to AbilityTriggers.
    /// </summary>
    /// <param name="protoId"></param>
    /// <returns></returns>
    public ComponentRegistry? GetCompsFromPrototype(ProtoId<TCPAbilityPrototype> protoId)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        if (!prototypeManager.TryIndex(protoId, out var tcpAbility)
            || AbilityTrigger == TCPAbilityTrigger.Nullified)
            return null;

        tcpAbility.Components.TryGetValue(AbilityTrigger, out var comps);

        if (comps != null)
            return comps;

        tcpAbility.Components.TryGetValue(TCPAbilityTrigger.Default, out var defaultComps);
        return defaultComps;
    }
}

/// <summary>
///     Ability requires the user to enter an "ascended" state.
///     User can use their ability infinitely and rapidly while ascended to apply an effect.
///     Upon descending, effects of the ability are undone.
///     Staying ascended costs health.
///     
///     Applicable to Abstract types.
/// </summary>
public sealed class ActiveAscensionAbility : TCPAbilityType, ITCPToggleableAbilityType
{
    private const string ASCEND_ACTION_PROTOTYPE = "ActionTCPAscend";
    private const string ABILITY_PULSE_TARGETED_PROTOTYPE = "ActionTCPAbilityTargeted";
    public bool Enabled { get; private set; } = false;
    public EntityUid? AbilityAction = null;
    protected override TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.ActiveAscension;

    public ActiveAscensionAbility(EntityUid user) : base(user)
    { }

    public override void Load()
    {
        if (!GetAbilityComponent(out var comp))
            return;

        var actions = Entities.System<SharedActionsSystem>();
        EntityUid? ascendId = null;
        var actionAdded = actions.AddAction(User, ref ascendId, ASCEND_ACTION_PROTOTYPE);
        if (actionAdded && ascendId is not null)
            comp.Actions.Add(ascendId.Value);
    }

    public override void Unload()
    {
        Deactivate();
        base.Unload();
    }

    public void ActivateSecondary()
    {
        if (!GetAbilityComponent(out var comp))
            return;

        Enabled = !Enabled;

        if (Enabled)
        {
            var action = LoadAbilityAction(0.0f, ABILITY_PULSE_TARGETED_PROTOTYPE);
            AbilityAction = action;
            Entities.EventBus.RaiseLocalEvent(User, new TCPAscendedEvent());
        }
        else
        {
            Deactivate();
            Entities.EventBus.RaiseLocalEvent(User, new TCPDescendedEvent());
        }
    }

    public void Deactivate()
    {
        Enabled = false;
        RemoveAbilityAction();
    }

    private void RemoveAbilityAction()
    {
        if (!GetAbilityComponent(out var comp) || AbilityAction is null)
            return;

        var actions = Entities.System<SharedActionsSystem>();
        var action = AbilityAction.Value;
        actions.RemoveAction(action);
        comp.Actions.Remove(action);
    }
}

/// <summary>
///     Ability to select a target and inflict an effect on them.
///
///     Applicable to Body, Form, Nature types.
/// </summary>
public sealed class ActiveTargetedAbility : TCPAbilityType
{
    private const string ABILITY_PULSE_TARGETED_PROTOTYPE = "ActionTCPAbilityTargeted";
    protected override TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.ActiveTargeted;

    public ActiveTargetedAbility(EntityUid user) : base(user)
    { }

    public override void Load()
    {
        LoadAbilityAction(1.0f, ABILITY_PULSE_TARGETED_PROTOTYPE);
    }

    public override void Activate()
    {
    }
}

/// <summary>
///     Uses their ability on themselves without selecting a target.
///
///     Applicable to Food and some Weapon types.
/// </summary>
public sealed class ActiveSelfAbility : TCPAbilityType
{
    protected override TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.ActiveSelf;

    public ActiveSelfAbility(EntityUid user) : base(user)
    { }

    public override void Load()
    {
        LoadAbilityAction(1.0f, ABILITY_ACTION_PROTOTYPE);
    }

    public override void Activate()
    {
    }
}

/// <summary>
///     A toggleable "passive" effect. May have some cost (health or otherwise) while active.
///
///     Applicable to some Machine and Storage types.
/// </summary>
public sealed class ActiveStatusAbility : TCPAbilityType, ITCPToggleableAbilityType
{
    private const string ABILITY_PULSE_STATUS_PROTOTYPE = "ActionTCPAbilityStatus";
    public bool Enabled { get; private set; } = false;
    protected override TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.ActiveStatus;

    public ActiveStatusAbility(EntityUid user) : base(user)
    { }

    public override void Load()
    {
        LoadAbilityAction(0.0f, ABILITY_PULSE_STATUS_PROTOTYPE);
    }

    public override void Unload()
    {
        Deactivate();
        base.Unload();
    }

    public override void Activate()
    {
        Enabled = !Enabled;
    }

    public void Deactivate()
    {
        Enabled = false;
    }
}

/// <summary>
///     Exposure to the user creates "buildup" over time, but the user may also "pulse" their
///     ability for instantaneous effects - usually about 30 seconds of buildup.
///
///     Applicable to many Creature types.
/// </summary>
public sealed class PassiveWithPulseAbility : TCPAbilityType
{
    private const string ABILITY_PULSE_ACTION_PROTOTYPE = "ActionTCPAbilityPulse";
    protected override TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.PassiveWithPulse;

    public PassiveWithPulseAbility(EntityUid user) : base(user)
    { }

    public override void Load()
    {
        LoadAbilityAction(2.0f, ABILITY_PULSE_ACTION_PROTOTYPE);
    }

    public override void Activate()
    {
    }
}

/// <summary>
///     User does something passively and has no relevant action.
///
///     Applicable to some Storage types.
/// </summary>
public sealed class PassiveAbility : TCPAbilityType
{
    protected override TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.Passive;
    public PassiveAbility(EntityUid user) : base(user)
    { }
}

/// <summary>
///     No ability.
/// </summary>
public sealed class NullifiedAbility : TCPAbilityType
{
    protected override TCPAbilityTrigger AbilityTrigger => TCPAbilityTrigger.Nullified;

    /// <summary>
    ///     Constructor for NullifiedAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public NullifiedAbility(EntityUid user) : base(user)
    { }
}
