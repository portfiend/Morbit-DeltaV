using System.Linq;
using Content.Shared.Morbit.TCP.Abilities.Components;
using Content.Shared.Morbit.TCP.Abilities.Events;

namespace Content.Shared.Morbit.TCP.Abilities.Systems;

public sealed partial class SharedApplyComponentAbilitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ApplyComponentAbilityComponent, TCPAbilityActivatedEvent>(OnAbilityActivated);
        SubscribeLocalEvent<ApplyComponentAbilityComponent, TCPAbilityDeactivatedEvent>(OnAbilityDeactivated);
    }

    public void OnAbilityActivated(EntityUid uid, ApplyComponentAbilityComponent component, TCPAbilityActivatedEvent args)
    {
        if (args.Target == null)
            return;

        EntityManager.AddComponents(args.Target.Value, component.Components);
    }

    public void OnAbilityDeactivated(EntityUid uid, ApplyComponentAbilityComponent component, TCPAbilityDeactivatedEvent args)
    {
        if (args.Target is null)
            return;

        EntityManager.RemoveComponents(args.Target.Value, component.Components);
    }
}
