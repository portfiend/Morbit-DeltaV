using Robust.Shared.Containers;

namespace Content.Shared.Morbit.Exposure.Components;

[RegisterComponent]
public sealed partial class ProximityExposureComponent : Component
{
    /// <summary>
    ///     Maximum distance to cause buildup.
    /// </summary>
    [DataField]
    public float ExposureRange = 1.5f;

    /// <summary>
    ///     Maximum exposure value.
    /// </summary>
    [DataField]
    public float MaximumExposure = 180.0f;

    /// <summary>
    ///     Starting value of exposure.
    /// </summary>
    [DataField]
    public float MinimumExposure = 0.0f;

    /// <summary>
    ///     How much "exposure" surrounding entities gain from proximity.
    /// </summary>
    [DataField]
    public float AccumulationRate = 1.0f;

    /// <summary>
    ///     How often other entities accumulate exposure.
    /// </summary>
    [DataField]
    public TimeSpan AccumulationFrequency = TimeSpan.FromSeconds(1.0f);

    /// <summary>
    ///     How much "exposure" surrounding entities lose while not exposed.
    /// </summary>
    [DataField]
    public float DecayRate = 1.5f;

    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    ///     Maps entities to their exposure value.
    /// </summary>
    public Dictionary<EntityUid, float> AffectedEntities = new();
}
