using Content.Shared.DoAfter;
using Content.Shared.Morbit.TCP.Abilities.Events;
using Content.Shared.Morbit.TCP.Components;
using Content.Shared.Morbit.TCP.Prototypes;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared.Morbit.TCP.Systems;

public abstract class SharedTCPAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();

        // Component lifecycle events
        SubscribeLocalEvent<TCPAbilityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<TCPAbilityComponent, ComponentShutdown>(OnShutdown);

        // Action events
        SubscribeLocalEvent<TCPAbilityComponent, ToggleAscensionEvent>(OnToggleAscension);
        SubscribeLocalEvent<TCPAbilityComponent, UseAbilityEvent>(OnUseAbility);
        SubscribeLocalEvent<TCPAbilityComponent, UseAbilityTargetedEvent>(OnUseAbilityTargeted);
        SubscribeLocalEvent<TCPAbilityComponent, ToggleStatusAbilityEvent>(OnToggleStatusAbility);
        SubscribeLocalEvent<TCPAbilityComponent, PulseAbilityEvent>(OnPulseAbility);

        // DoAfter events
        SubscribeLocalEvent<TCPAbilityComponent, ActivateTCPAbilityDoAfterEvent>(OnDoAfterUseAbility);

        // Entity events
        SubscribeLocalEvent<TCPAbilityComponent, TCPDescendedEvent>(OnTCPDescended);
    }

    private void OnInit(EntityUid uid, TCPAbilityComponent component, MapInitEvent args)
    {
        component.AbilityHolders = _containerSystem.EnsureContainer<Container>(uid, component.ContainerKey);

        LoadAbilityTrigger(uid, component);
        LoadAbilityEffects(component);
    }

    private void OnShutdown(EntityUid uid, TCPAbilityComponent component, ComponentShutdown args)
    {
        UnloadAbilityTrigger(component);
        UnloadAbilityEffects(component);
    }

    private void LoadAbilityTrigger(EntityUid uid, TCPAbilityComponent component)
    {
        if (component.AbilityTypeClass != null)
            UnloadAbilityTrigger(component);

        component.AbilityTypeClass = TCPAbilityTypeFactory.CreateAbilityType(component.AbilityTrigger, uid);
        component.AbilityTypeClass?.Load();
    }

    private void UnloadAbilityTrigger(TCPAbilityComponent component)
    {
        component.AbilityTypeClass?.Unload();
        component.AbilityTypeClass = null;
    }

    private void LoadAbilityEffects(TCPAbilityComponent component)
    {
        if (component.AbilityHolders.ContainedEntities.Count > 0)
            UnloadAbilityEffects(component);

        foreach (var protoId in component.Abilities)
        {
            if (!_prototypeManager.TryIndex(protoId, out var ability))
                continue;

            ability.Components.TryGetValue(component.AbilityTrigger, out var comps);

            if (comps == null)
                continue;

            var holder = EntityManager.Spawn(component.HolderProtoId);
            EntityManager.AddComponents(holder, comps);
            _containerSystem.Insert(holder, component.AbilityHolders);
        }
    }

    private void UnloadAbilityEffects(TCPAbilityComponent component)
    {
        var abilityHolders = component.AbilityHolders.ContainedEntities;
        foreach (var holder in abilityHolders)
        {
            _containerSystem.TryRemoveFromContainer(holder, force: true);
            QueueDel(holder);
        }
    }

    private void OnToggleStatusAbility(EntityUid uid, TCPAbilityComponent component, ToggleStatusAbilityEvent args)
    {
        if (component.AbilityTypeClass is ActiveStatusAbility ability)
        {
            ability.Activate();
            args.Handled = true;
        }
    }

    private void OnToggleAscension(EntityUid uid, TCPAbilityComponent component, ToggleAscensionEvent args)
    {
        if (component.AbilityTypeClass is ActiveAscensionAbility ability)
        {
            ability.ActivateSecondary();
            args.Handled = true;
        }
    }

    private void OnUseAbilityTargeted(EntityUid uid, TCPAbilityComponent component, UseAbilityTargetedEvent args)
    {
        if (!HasComp<TransformComponent>(args.Target))
            return;

        StartDoAfter(uid, args.Target, 1.0f, 3.0f);
        args.Handled = true;
    }

    private void OnPulseAbility(EntityUid uid, TCPAbilityComponent component, PulseAbilityEvent args)
    {
        if (!_xformQuery.TryGetComponent(uid, out var xform))
            return;

        var range = 1.5f;
        var entitiesInRange = _entityLookup.GetEntitiesInRange(xform.Coordinates, range);

        foreach (var entity in entitiesInRange)
            ActivateTCPAbility(uid, component, uid, entity, 0.5f);

        args.Handled = true;
    }

    private void OnUseAbility(EntityUid uid, TCPAbilityComponent component, UseAbilityEvent args)
    {
        ActivateTCPAbility(uid, component, uid, uid, 1.0f);
        args.Handled = true;
    }

    private void OnDoAfterUseAbility(EntityUid uid, TCPAbilityComponent component, ActivateTCPAbilityDoAfterEvent args)
    {
        ActivateTCPAbility(uid, component, args.User, args.Target, args.Strength);
    }

    private void StartDoAfter(EntityUid user, EntityUid target, float strength, double delaySeconds)
    {
        var doAfterEvent = new ActivateTCPAbilityDoAfterEvent(strength);
        var doAfterArgs = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(delaySeconds), doAfterEvent, target, user);
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnTCPDescended(EntityUid uid, TCPAbilityComponent component, TCPDescendedEvent args)
    {
        DeactivateTCPAbilityForAll(uid, component);
    }

    private void ActivateTCPAbility(EntityUid uid,
        TCPAbilityComponent? component,
        EntityUid user,
        EntityUid? target = null,
        float strength = 1.0f)
    {
        if (!Resolve(uid, ref component))
            return;

        var abilityEvent = new TCPAbilityActivatedEvent(user, target, strength);
        foreach (var holder in component.AbilityHolders.ContainedEntities)
            RaiseLocalEvent(holder, abilityEvent);
    }

    private void DeactivateTCPAbility(EntityUid uid, TCPAbilityComponent? component, EntityUid user, EntityUid? target = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var abilityEvent = new TCPAbilityDeactivatedEvent(user, target);
        foreach (var holder in component.AbilityHolders.ContainedEntities)
            RaiseLocalEvent(holder, abilityEvent);
    }

    private void DeactivateTCPAbilityForAll(EntityUid uid, TCPAbilityComponent? component)
    {
        if (!Resolve(uid, ref component))
            return;

        var affectedEntities = component.AffectedEntities;
        foreach (var ent in affectedEntities)
            DeactivateTCPAbility(uid, component, uid, ent);
    }
}

public sealed partial class ActivateTCPAbilityDoAfterEvent : DoAfterEvent
{
    public float Strength { get; }

    public override ActivateTCPAbilityDoAfterEvent Clone() => this;

    public ActivateTCPAbilityDoAfterEvent(float strength)
    {
        Strength = strength;
    }
}
