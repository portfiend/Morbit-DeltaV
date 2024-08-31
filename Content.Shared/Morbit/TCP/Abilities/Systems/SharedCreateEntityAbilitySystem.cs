using Content.Shared.Hands.EntitySystems;
using Content.Shared.Morbit.TCP.Abilities.Components;
using Content.Shared.Morbit.TCP.Abilities.Events;

namespace Content.Shared.Morbit.TCP.Abilities.Systems;

public sealed partial class SharedCreateEntityAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CreateEntityAbilityComponent, TCPAbilityModifyTargetEvent>(OnModifyTarget);
    }

    public void OnModifyTarget(EntityUid uid, CreateEntityAbilityComponent component, TCPAbilityModifyTargetEvent args)
    {
        if (args.Handled || component.Prototype == string.Empty)
            return;

        var ent = Spawn(component.Prototype);
        if (component.PickupOnSpawn && args.User == args.Target)
        {
            _handsSystem.PickupOrDrop(args.Target, ent);
        }
        else
        {
            var targetedEntity = args.Target ?? args.User;
            _transformSystem.DropNextTo(ent, targetedEntity);
            _transformSystem.AttachToGridOrMap(ent);
        }

        args.Target = ent;
        args.Handled = true;
    }
}
