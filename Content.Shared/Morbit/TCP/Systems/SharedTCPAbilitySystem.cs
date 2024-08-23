using Content.Shared.Actions;
using Content.Shared.Morbit.TCP.Components;

namespace Content.Shared.Morbit.TCP.Systems;

public abstract class SharedTCPAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TCPAbilityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<TCPAbilityComponent, ComponentShutdown>(OnShutdown);
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
            .CreateAbilityType(component.AbilityTrigger, uid, _actionsSystem);

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
}
