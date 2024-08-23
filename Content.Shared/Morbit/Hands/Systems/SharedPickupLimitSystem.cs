using Content.Server.Morbit.Hands.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;

namespace Content.Shared.Morbit.Hands.Systems;

public abstract class SharedPickupLimitSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PickupSizeLimitComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<PickupSizeLimitComponent, IsEquippingAttemptEvent>(OnEquipAttempt);
    }

    public void OnPickupAttempt(EntityUid uid, PickupSizeLimitComponent component, PickupAttemptEvent args)
    {
        CheckSizeLimit(args.Item, component, args);
    }

    public void OnEquipAttempt(EntityUid uid, PickupSizeLimitComponent component, IsEquippingAttemptEvent args)
    {
        CheckSizeLimit(args.EquipTarget, component, args);
    }

    public void CheckSizeLimit(EntityUid item,
        PickupSizeLimitComponent component,
        CancellableEntityEventArgs args)
    {
        if (component.MaximumSize is null
            || !TryComp<ItemComponent>(item, out var itemComp))
            return;

        var itemSize = _item.GetSizePrototype(itemComp.Size);
        var maxSize = _item.GetSizePrototype(component.MaximumSize.Value);

        if (itemSize > maxSize)
            args.Cancel();
    }
}
