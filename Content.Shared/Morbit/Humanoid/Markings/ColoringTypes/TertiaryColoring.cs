using Content.Shared.Humanoid.Markings;

namespace Content.Shared.Morbit.Humanoid.Markings;

/// <summary>
///     Colors layer in a skin color
/// </summary>
public sealed partial class TertiaryColoring : SecondaryColoring
{
    public override Color? GetCleanColor(Color? skin, Color? eyes, MarkingSet markingSet)
    {
        if (skin == null)
            return null;

        return ShiftHueAndDarken(skin.Value, 80.0f, 0.5f);
    }
}
