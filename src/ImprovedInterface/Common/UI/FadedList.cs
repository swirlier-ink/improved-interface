using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace ImprovedInterface.Common.UI;

// https://github.com/gold-meridian/daybreak-mod/blob/feat/uiconfiginterface/src/Daybreak/Content/UI/FadedList.cs
public class FadedList : UIList
{
    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        Assets.MiscUI.SlightListFade.Asset.Wait();

        using var rtLease = ScreenspaceTargetProvider.Shared.Create(
            Main.instance.GraphicsDevice,
            RenderTargetDescriptor.DefaultPreserveContents
        );

        spriteBatch.End(out var ss);

        using (rtLease.Scope(preserveContents: true, clearColor: Color.Transparent))
        {
            spriteBatch.Begin(ss);
            base.DrawChildren(spriteBatch);
            spriteBatch.End();
        }

        spriteBatch.Begin(ss with { SortMode = SpriteSortMode.Immediate, RasterizerState = RasterizerState.CullNone, TransformMatrix = Matrix.Identity });

        var dims = this.Dimensions;

        var position = Vector2.Transform(dims.TopLeft(), ss.TransformMatrix);
        var size = Vector2.Transform(dims.BottomRight(), ss.TransformMatrix) - position;

        const float fade_size = 32f;

        // Use the distance from each edge to control fading.
        var upperFade = MathF.Min(_scrollbar.ViewPosition, fade_size);
        var lowerFade = MathF.Min(MathF.Abs(_scrollbar.MaxViewSize - (_scrollbar.ViewPosition + _scrollbar.ViewSize)), fade_size);

        var fadeShader = Assets.MiscUI.SlightListFade.CreateFadeShader();
        fadeShader.Parameters.uPanelDimensions = new Vector4(position.X, position.Y, size.X, size.Y);
        fadeShader.Parameters.uScreenSize = new Vector2(rtLease.Target.Width, rtLease.Target.Height);
        fadeShader.Parameters.uFadeDistanceTop = upperFade;
        fadeShader.Parameters.uFadeDistanceBottom = lowerFade;
        fadeShader.Apply();

        var rect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);

        spriteBatch.Draw(rtLease.Target, rect, rect, Color.White);
        spriteBatch.Restart(ss);
    }
}