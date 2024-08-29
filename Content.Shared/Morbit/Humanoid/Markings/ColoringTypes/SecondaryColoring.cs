using Content.Shared.Humanoid.Markings;

namespace Content.Shared.Morbit.Humanoid.Markings;

/// <summary>
///     Colors layer in a skin color
/// </summary>

[Virtual]
public partial class SecondaryColoring : LayerColoringType
{
    public override Color? GetCleanColor(Color? skin, Color? eyes, MarkingSet markingSet)
    {
        if (skin == null)
            return null;

        return ShiftHueAndDarken(skin.Value, 40.0f, 0.3f);
    }

    protected Color ShiftHueAndDarken(Color color, float hueShiftDegrees, float darkenAmount)
    {
        Vector4 hsv = Color.ToHsv(color);

        hsv.X = (hsv.X + hueShiftDegrees / 360.0f) % 1.0f;

        hsv.Z = Math.Max(0.0f, hsv.Z * (1.0f - darkenAmount));

        return Color.FromHsv(hsv);
    }
}
