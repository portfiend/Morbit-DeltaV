using System.Diagnostics.CodeAnalysis;
using Content.Shared.Actions;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Morbit.TCP.Components;
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

    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private readonly PrototypeManager _prototypeManager = default!;
    protected readonly SharedActionsSystem Actions;

    protected EntityUid User;

    /// <summary>
    ///     Constructor for TCPAbilityType.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public TCPAbilityType(EntityUid user, SharedActionsSystem actions)
    {
        User = user;
        Actions = actions;
    }

    public virtual void Load()
    { }

    public virtual void Unload()
    {
        if (!GetAbilityComponent(out var component))
            return;

        foreach (var action in component.Actions)
        {
            Actions.RemoveAction(action);
        }
    }

    public virtual void Activate()
    { }

    protected bool GetAbilityComponent([NotNullWhen(true)] out TCPAbilityComponent? ability)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        entityManager.TryGetComponent<TCPAbilityComponent>(User, out var comp);
        ability = comp;
        return comp is not null;
    }

    private void UpdateActionCosts(float levelCost = 1.0f, ActionHealthCostComponent? comp = null)
    {
        _prototypeManager.TryIndex<DamageTypePrototype>(HEALTH_COST_DAMAGE_TYPE, out var damageType);

        if (damageType is null || comp is null)
            return;

        var damageSpecifier = new Damage.DamageSpecifier(damageType, HEALTH_LEVEL * levelCost);
        comp.Damage = damageSpecifier;

        _mobThresholdSystem.TryGetThresholdForState(User, MobState.Critical, out var critThreshold);
        if (critThreshold is null)
            return;

        comp.MaximumDamage = (float) critThreshold - levelCost * HEALTH_LEVEL;
    }

    /// <summary>
    ///     Creates a basic ability action.
    /// </summary>
    /// <param name="levelCost">How many health levels the action costs.</param>
    /// <param name="actionProto">Prototype to use.</param>
    /// <returns></returns>
    protected EntityUid CreateAbilityAction(float levelCost = 1.0f,
        string? actionProto = ABILITY_ACTION_PROTOTYPE)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var entity = entityManager.Spawn(actionProto);

        var comp = entityManager.EnsureComponent<ActionHealthCostComponent>(User);
        UpdateActionCosts(levelCost, comp);

        return entity;
    }

    /// <summary>
    ///     Creates a basic "Ability" action and adds it to the user.
    /// </summary>
    /// <param name="actionProto">Prototype of the action to use.</param>
    /// <param name="levelCost">How many health levels the action costs.</param>
    protected void LoadAbilityAction(float levelCost = 1.0f,
        string actionProto = ABILITY_ACTION_PROTOTYPE)
    {
        if (!GetAbilityComponent(out var comp))
            return;

        var ability = CreateAbilityAction(levelCost, actionProto);
        Actions.AddAction(User, ability, ability);

        comp.Actions.Add(ability);
    }
}

public sealed class ActiveAscensionAbility : TCPAbilityType, ITCPToggleableAbilityType
{
    private const string ASCEND_ACTION_PROTOTYPE = "ActionTCPAscend";
    public bool Enabled { get; private set; } = false;
    public EntityUid? AbilityAction = null;

    /// <summary>
    ///     Constructor for ActiveAscensionAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public ActiveAscensionAbility(EntityUid user, SharedActionsSystem actions) : base(user, actions)
    { }

    public override void Load()
    {
        if (!GetAbilityComponent(out var comp))
            return;

        EntityUid? ascendId = null;
        var actionAdded = Actions.AddAction(User, ref ascendId, ASCEND_ACTION_PROTOTYPE);
        if (actionAdded && ascendId is not null)
            comp.Actions.Add(ascendId.Value);
    }

    public override void Activate()
    {
    }

    public void ActivateSecondary()
    {
        if (!GetAbilityComponent(out var comp))
            return;

        Enabled = true;

        var action = CreateAbilityAction(0.0f);
        Actions.AddAction(User, action, action);
        comp.Actions.Add(action);
        AbilityAction = action;
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

        var action = AbilityAction.Value;
        Actions.RemoveAction(action);
        comp.Actions.Remove(action);
    }
}

public sealed class ActiveTargetedAbility : TCPAbilityType
{
    private const string ABILITY_PULSE_TARGETED_PROTOTYPE = "ActionTCPAbilityTargeted";

    /// <summary>
    ///     Constructor for ActiveTargetedAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public ActiveTargetedAbility(EntityUid user, SharedActionsSystem actions) : base(user, actions)
    { }

    public override void Load()
    {
        LoadAbilityAction(1.0f, ABILITY_PULSE_TARGETED_PROTOTYPE);
    }

    public override void Activate()
    {
    }
}

public sealed class ActiveSelfAbility : TCPAbilityType
{
    /// <summary>
    ///     Constructor for ActiveSelfAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public ActiveSelfAbility(EntityUid user, SharedActionsSystem actions) : base(user, actions)
    { }

    public override void Load()
    {
        LoadAbilityAction(1.0f, ABILITY_ACTION_PROTOTYPE);
    }

    public override void Activate()
    {
    }
}

public sealed class ActiveStatusAbility : TCPAbilityType, ITCPToggleableAbilityType
{
    private const string ABILITY_PULSE_STATUS_PROTOTYPE = "ActionTCPAbilityStatus";
    public bool Enabled { get; private set; } = false;

    /// <summary>
    ///     Constructor for ActiveStatusAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public ActiveStatusAbility(EntityUid user, SharedActionsSystem actions) : base(user, actions)
    { }

    public override void Load()
    {
        LoadAbilityAction(0.0f, ABILITY_PULSE_STATUS_PROTOTYPE);
    }

    public override void Activate()
    {
    }

    public void ActivateSecondary()
    {
        Enabled = true;
    }

    public void Deactivate()
    {
        Enabled = false;
    }
}

public sealed class PassiveWithPulseAbility : TCPAbilityType
{
    private const string ABILITY_PULSE_ACTION_PROTOTYPE = "ActionTCPAbilityPulse";

    /// <summary>
    ///     Constructor for PassiveWithPulseAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public PassiveWithPulseAbility(EntityUid user, SharedActionsSystem actions) : base(user, actions)
    { }

    public override void Load()
    {
        LoadAbilityAction(2.0f, ABILITY_PULSE_ACTION_PROTOTYPE);
    }

    public override void Activate()
    {
    }
}

public sealed class PassiveAbility : TCPAbilityType
{
    /// <summary>
    ///     Constructor for PassiveAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public PassiveAbility(EntityUid user, SharedActionsSystem actions) : base(user, actions)
    { }
}

public sealed class NullifiedAbility : TCPAbilityType
{
    /// <summary>
    ///     Constructor for NullifiedAbility.
    /// </summary>
    /// <param name="user">The user entity UID.</param>
    /// <param name="actions">The SharedActionsSystem instance.</param>
    public NullifiedAbility(EntityUid user, SharedActionsSystem actions) : base(user, actions)
    { }
}
