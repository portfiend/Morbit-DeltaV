using Content.Shared.Morbit.Exposure.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Morbit.Exposure.Systems;

public sealed partial class SharedProximityExposureSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ProximityExposureComponent>();
        while (query.MoveNext(out var uid, out var exposure))
        {
            if (_gameTiming.CurTime < exposure.NextUpdate)
                continue;

            var inRange = GetEntitiesInRange(uid, exposure.ExposureRange);
            GiveInRangeExposure(uid, exposure, inRange);
            DecayExposure(uid, exposure, inRange);

            exposure.NextUpdate = _gameTiming.CurTime + exposure.AccumulationFrequency;
        }
    }

    public HashSet<EntityUid>? GetEntitiesInRange(EntityUid uid, float range)
    {
        if (!_xformQuery.TryGetComponent(uid, out var xform))
            return null;

        var coords = xform.Coordinates;
        return _entityLookup.GetEntitiesInRange(coords, range);
    }

    /// <summary>
    ///     Increases the Exposure values of surrounding entitities.
    /// </summary>
    /// <param name="uid">Entity whose proximity increases exposure.</param>
    /// <param name="component">Entity's `ProximityExposureComponent`.</param>
    /// <param name="inRange">List of entities that are in range.</param>
    public void GiveInRangeExposure(EntityUid uid, ProximityExposureComponent component, HashSet<EntityUid>? inRange = null)
    {
        inRange ??= GetEntitiesInRange(uid, component.ExposureRange);
        if (inRange is null)
            return;

        foreach (var entity in inRange)
        {
            if (!component.AffectedEntities.ContainsKey(entity)
            && _xformQuery.TryGetComponent(entity, out var _))
                component.AffectedEntities[entity] = 0f;

            component.AffectedEntities[entity] += component.AccumulationRate;

            var buildupEv = new ExposureBuildupEvent(uid, entity, component, component.AffectedEntities[entity]);
            var updateEv = new ExposureUpdateEvent(uid, entity, component, component.AffectedEntities[entity]);
            RaiseLocalEvent(uid, buildupEv);
            RaiseLocalEvent(uid, updateEv);
        }
    }

    /// <summary>
    ///     Decreases the Exposure values of surrounding entities, and removes entities that are no longer exposed.
    /// </summary>
    /// <param name="uid">Entity whose lack of proximity decreases exposure.</param>
    /// <param name="component">Entity's `ProximityExposureComponent`.</param>
    /// <param name="inRange">List of entities that are in range.</param>
    public void DecayExposure(EntityUid uid, ProximityExposureComponent component, HashSet<EntityUid>? inRange = null)
    {
        inRange ??= GetEntitiesInRange(uid, component.ExposureRange);
        if (inRange is null)
            return;

        var affected = component.AffectedEntities;
        var toRemove = new List<EntityUid>();

        foreach (var entity in component.AffectedEntities.Keys)
        {
            if (inRange.Contains(entity))
                continue;

            affected[entity] -= component.DecayRate;
            if (affected[entity] <= 0f)
                toRemove.Add(entity);

            var decayEv = new ExposureBuildupEvent(uid, entity, component, affected[entity]);
            var updateEv = new ExposureUpdateEvent(uid, entity, component, affected[entity]);
            RaiseLocalEvent(uid, decayEv);
            RaiseLocalEvent(uid, updateEv);
        }

        foreach (var entity in toRemove)
            affected.Remove(entity);
    }
}

[NetSerializable, Serializable]
public abstract partial class ProximityExposureEvent : EntityEventArgs
{
    /// <summary>
    ///     The entity that is creating exposure effects in the target.
    /// </summary>
    public EntityUid Source { get; }

    /// <summary>
    ///     The entity that is building exposure.
    /// </summary>
    public EntityUid Target { get; }

    /// <summary>
    ///     The source entity's proximity exposure data.
    /// </summary>
    public ProximityExposureComponent Component { get; }

    /// <summary>
    ///     The amount of exposure the target entity has.
    /// </summary>
    public float Exposure { get; }

    protected ProximityExposureEvent(EntityUid source, EntityUid target, ProximityExposureComponent component, float exposure)
    {
        Source = source;
        Target = target;
        Component = component;
        Exposure = exposure;
    }
}

public sealed partial class ExposureUpdateEvent : ProximityExposureEvent
{
    public ExposureUpdateEvent(EntityUid source,
        EntityUid target,
        ProximityExposureComponent component,
        float exposure)
        : base(source, target, component, exposure)
    { }
}

public sealed partial class ExposureBuildupEvent : ProximityExposureEvent
{
    public ExposureBuildupEvent(EntityUid source,
        EntityUid target,
        ProximityExposureComponent component,
        float exposure)
        : base(source, target, component, exposure)
    { }
}

public sealed partial class ExposureDecayEvent : ProximityExposureEvent
{
    public ExposureDecayEvent(EntityUid source,
        EntityUid target,
        ProximityExposureComponent component,
        float exposure)
        : base(source, target, component, exposure)
    { }
}
