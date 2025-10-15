using System.Collections.Generic;

namespace rt;

/// <summary>
/// Maps numeric ranges to colors for procedural texturing and data visualization.
/// Useful for creating height-based terrain coloring, temperature maps, or value-based textures.
/// Each range [from, to] is associated with a specific color returned when queried.
/// </summary>
public class ColorMap
{
    /// <summary>
    /// Lower bounds of each color range (inclusive).
    /// </summary>
    private readonly List<ushort> _from = new();

    /// <summary>
    /// Upper bounds of each color range (inclusive).
    /// </summary>
    private readonly List<ushort> _to = new();

    /// <summary>
    /// Colors corresponding to each range.
    /// Indexed in parallel with _from and _to lists.
    /// </summary>
    private readonly List<Color> _color = new();

    /// <summary>
    /// Adds a new color mapping for a numeric range.
    /// Returns this ColorMap for method chaining, allowing fluent configuration.
    /// Example: colorMap.Add(0, 100, Color.BLUE).Add(101, 200, Color.GREEN);
    /// </summary>
    /// <param name="from">Lower bound of the range (inclusive).</param>
    /// <param name="to">Upper bound of the range (inclusive).</param>
    /// <param name="color">Color to return for values within this range.</param>
    /// <returns>This ColorMap instance for chaining.</returns>
    public ColorMap Add(ushort from, ushort to, Color color)
    {
        _from.Add(from);
        _to.Add(to);
        _color.Add(color);
        return this;
    }

    /// <summary>
    /// Retrieves the color associated with a given value.
    /// Performs linear search through all ranges to find the first matching interval.
    /// Returns Color.NONE if the value doesn't fall within any defined range.
    /// Used for texture mapping based on height, temperature, or other scalar properties.
    /// </summary>
    /// <param name="value">The value to look up in the color map.</param>
    /// <returns>Color corresponding to the range containing the value, or Color.NONE if not found.</returns>
    public Color GetColor(ushort value)
    {
        for (int i = 0; i < _from.Count; i++)
        {
            if (_from[i] <= value && _to[i] >= value)
            {
                return _color[i];
            }
        }
        return Color.NONE;
    }
}
