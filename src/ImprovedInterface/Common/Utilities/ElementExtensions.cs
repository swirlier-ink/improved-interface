using Microsoft.Xna.Framework;
using Terraria.UI;

namespace ImprovedInterface.Common;

/// <summary>
///     Extensions for <see cref="UIElement"/>s.
/// </summary>
public static class ElementExtensions
{
    extension(UIElement element)
    {
        /// <summary>
        ///     The <paramref name="element"/>'s dimensions as a
        ///     <see cref="Rectangle"/>.<br></br>
        ///     <inheritdoc cref="UIElement.GetDimensions"/>
        /// </summary>
        public Rectangle Dimensions => element.GetDimensions().ToRectangle();

        /// <summary>
        ///     The <paramref name="element"/>'s inner dimensions as a
        ///     <see cref="Rectangle"/>.<br></br>
        ///     <inheritdoc cref="UIElement.GetInnerDimensions"/>
        /// </summary>
        public Rectangle InnerDimensions => element.GetInnerDimensions().ToRectangle();

        /// <summary>
        ///     The <paramref name="element"/>'s outer dimensions as a
        ///     <see cref="Rectangle"/>.<br></br>
        ///     <inheritdoc cref="UIElement.GetOuterDimensions"/>
        /// </summary>
        public Rectangle OuterDimensions => element.GetOuterDimensions().ToRectangle();

        /// <summary>
        ///     Attempts to get the dimensions of this
        ///     <paramref name="element"/> based on the dimensions of a parent
        ///     element.
        ///     <br />
        ///     If the element has no parent, <see cref="Dimensions"/> is
        ///     returned directly.
        /// </summary>
        public Rectangle ParentRelativeDimensions => element.Parent is not { } parent
            ? element.Dimensions
            : element.GetDimensionsBasedOnParentDimensions(parent.GetInnerDimensions()).ToRectangle();

        /// <summary>
        /// The <paramref name="element"/>'s top, height, and vertical margin values combined.
        /// </summary>
        public StyleDimension Bottom => element.Top + element.Height + (element.MarginTop + element.MarginBottom, 0);
        
        /// <summary>
        /// The <paramref name="element"/>'s left, width, and horizontal margin values combined.
        /// </summary>
        public StyleDimension Right => element.Left + element.Width + (element.MarginLeft + element.MarginRight, 0);
    }
}
