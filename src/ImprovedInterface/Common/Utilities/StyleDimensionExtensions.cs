using Terraria.UI;

namespace ImprovedInterface.Common;

/// <summary>
///     Extensions for <see cref="StyleDimension"/>s.
/// </summary>
public static class StyleDimensionExtensions
{
    extension(StyleDimension)
    {
        public static StyleDimension operator +(StyleDimension a, StyleDimension b) => new(a.Pixels + b.Pixels, a.Percent + b.Percent);
        public static StyleDimension operator -(StyleDimension a, StyleDimension b) => new(a.Pixels - b.Pixels, a.Percent - b.Percent);
        
        public static StyleDimension operator +(StyleDimension a, (float pixels, float percent) b) => new(a.Pixels + b.pixels, a.Percent + b.percent);
        public static StyleDimension operator -(StyleDimension a, (float pixels, float percent) b) => new(a.Pixels - b.pixels, a.Percent - b.percent);
    }

    extension(ref StyleDimension styleDimension)
    {
        /// <summary>
        /// Adds to both values of this <see cref="StyleDimension"/>
        /// <para/>Does not return a value.
        /// </summary>
        public void Add(StyleDimension b) => styleDimension += b;
        
        /// <inheritdoc cref="StyleDimensionExtensions.Add(ref Terraria.UI.StyleDimension,Terraria.UI.StyleDimension)"/>
        public void Add(float pixels, float percent) => styleDimension += new StyleDimension(pixels, percent);
        
        /// <summary>
        /// Subtracts from both values of this <see cref="StyleDimension"/>
        /// <para/>Does not return a value.
        /// </summary>
        public void Sub(StyleDimension b) => styleDimension -= b;
        
        /// <inheritdoc cref="StyleDimensionExtensions.Sub(ref Terraria.UI.StyleDimension,Terraria.UI.StyleDimension)"/>
        public void Sub(float pixels, float percent) => styleDimension -= new StyleDimension(pixels, percent);
    }
}