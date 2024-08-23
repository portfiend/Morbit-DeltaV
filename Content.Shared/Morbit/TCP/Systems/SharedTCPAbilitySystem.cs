using Content.Shared.Actions;
using Content.Shared.Morbit.TCP.Components;

namespace Content.Shared.Morbit.TCP.Systems;

public abstract class SharedTCPAbilitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TCPAbilityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<TCPAbilityComponent, ComponentShutdown>(OnShutdown);

        // Action callbacks
        SubscribeLocalEvent<TCPAbilityComponent, ToggleAscensionEvent>(OnToggleAscension);
        SubscribeLocalEvent<TCPAbilityComponent, UseAbilityEvent>(OnUseAbility);
        SubscribeLocalEvent<TCPAbilityComponent, UseAbilityTargetedEvent>(OnUseAbilityTargeted);
        SubscribeLocalEvent<TCPAbilityComponent, ToggleStatusAbilityEvent>(OnToggleAbility);
        SubscribeLocalEvent<TCPAbilityComponent, PulseAbilityEvent>(OnPulseAbility);
    }

    private void OnInit(EntityUid uid, TCPAbilityComponent component, MapInitEvent args)
    {
        LoadAbility(uid, component);
    }

    private void OnShutdown(EntityUid uid, TCPAbilityComponent component, ComponentShutdown args)
    {
        UnloadAbility(component);
    }

    private void LoadAbility(EntityUid uid, TCPAbilityComponent component)
    {
        if (component.AbilityTypeClass is not null)
            UnloadAbility(component);

        var abilityTypeClass = TCPAbilityTypeFactory
            .CreateAbilityType(component.AbilityTrigger, uid);

        component.AbilityTypeClass = abilityTypeClass;
        abilityTypeClass.Load();
    }

    private void UnloadAbility(TCPAbilityComponent component)
    {
        if (component.AbilityTypeClass is null)
            return;

        component.AbilityTypeClass.Unload();
        component.AbilityTypeClass = null;
    }

    private void OnToggleAscension(EntityUid uid, TCPAbilityComponent component, ToggleAscensionEvent args)
    {
        if (component?.AbilityTypeClass is ActiveAscensionAbility abilityTypeClass)
        {
            abilityTypeClass.ActivateSecondary();
            args.Handled = true;
        }
    }

    private void OnUseAbilityTargeted(EntityUid uid, TCPAbilityComponent component, UseAbilityTargetedEvent args)
    {
        throw new NotImplementedException();
    }

    private void OnToggleAbility(EntityUid uid, TCPAbilityComponent component, ToggleStatusAbilityEvent args)
    {
        throw new NotImplementedException();
    }

    private void OnPulseAbility(EntityUid uid, TCPAbilityComponent component, PulseAbilityEvent args)
    {
        throw new NotImplementedException();
    }

    private void OnUseAbility(EntityUid uid, TCPAbilityComponent component, UseAbilityEvent args)
    {
        throw new NotImplementedException();
    }
}
