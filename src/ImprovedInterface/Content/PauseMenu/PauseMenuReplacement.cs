using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using Terraria.UI;

namespace ImprovedInterface.Content.PauseMenu;

public static class PauseMenuReplacement
{
    private static readonly UserInterface @interface = new UserInterface();
    private static PauseMenuState? state;

    [OnLoad]
    private static void Load()
    {
        IL_Main.DrawInterface_29_SettingsButton += DrawInterface_29_SettingsButton_ChangeText;
        IL_Main.DrawSettingButton += DrawSettingButton_Coloration;
    }

    private static void DrawSettingButton_Coloration(ILContext il)
    {
        var c = new ILCursor(il);

        var mouseOverIndex = ParameterIndex.Invalid;

        c.GotoNext(
            i => i.MatchLdarg(out mouseOverIndex),
            i => i.MatchLdindU1(),
            i => i.MatchBrfalse(out _)
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall<Color>($"get_{nameof(Color.White)}")
        );

        c.EmitPop();

        c.EmitLdarg(mouseOverIndex);
        c.EmitLdindU1();

        c.EmitStaticDelegateUnsafe(
            static (bool mouseOver) =>
            {
                if (mouseOver)
                {
                    return Main.OurFavoriteColor;
                }

                return (Color.White * ((float)Main.mouseTextColor / byte.MaxValue)) with { A = byte.MaxValue };
            }
        );
    }

    private static void DrawInterface_29_SettingsButton_ChangeText(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchCallvirt<LocalizedText>($"get_{nameof(LocalizedText.Value)}")
        );

        c.EmitPop();

        c.EmitStaticDelegateUnsafe(
            static () =>
            {
                return Main.ingameOptionsWindow
                    ? Mods.ImprovedInterface.PauseMenu.MenuButton.InMenu.GetTextValue()
                    : Mods.ImprovedInterface.PauseMenu.MenuButton.InGame.GetTextValue();
            }
        );
    }

    [ModSystemHooks.UpdateUI]
    private static void UpdateUI(GameTime gameTime)
    {
        if (!Main.ingameOptionsWindow)
        {
            return;
        }

        @interface.Update(gameTime);
    }

    [GameInterfaceLayers.Replace(GameInterfaceLayers.IN_GAME_OPTIONS, InterfaceScaleType.UI, Name = $"{nameof(ImprovedInterface)}: Pause Menu")]
    private static bool DrawPauseMenu()
    {
        if (!Main.ingameOptionsWindow)
        {
            @interface.State = null;
            state = null;

            return true;
        }

        state ??= new PauseMenuState();
        @interface.State = state;

        var sb = Main.spriteBatch;

        sb.End(out var ss);
        sb.Begin(in ss);
        {
            Main.instance.DrawInterface_16_MapOrMinimap();
        }

        // DrawInterface_16_MapOrMinimap runs PlayerInput::SetZoom_Unscaled
        PlayerInput.SetZoom_UI();

        sb.Restart(in ss);
        {
            @interface.Draw(sb, Main.instance.gameTime);

            Main.instance.DrawMouseOver();

            // Should be drawn even if vanilla conditions would disable it as to preview settings
            Main.instance.GUIBarsDraw();

            Main.DrawInterface_29_SettingsButton();
        }
        sb.Restart(ss with { SamplerState = Main.SamplerStateForCursor });
        {
            Main.DrawCursor(Main.DrawThickCursor());

            // The vanilla pause menu draws the interact icon with Main.SamplerStateForCursor, should we replicate this?
            Main.instance.DrawInterface_40_InteractItemIcon();
        }

        // Return false as the pause menu stops all further layers from rendering
        return false;
    }

    public static float PauseFade { get; set; }

    [ScreenFilter(EffectPriority.VeryHigh)]
    private static bool ScreenOverlay(SpriteBatch sb, GraphicsDevice device, RenderTarget2D screen, RenderTarget2D screenSwap)
    {
        var horizBlur = Assets.PauseMenu.ScreenBlur.CreateHorizontalShader();
        var vertBlur = Assets.PauseMenu.ScreenBlur.CreateVerticalShader();

        var increment = Main.hideUI ? 0.2f : 0.07f;

        PauseFade += (Main.ingameOptionsWindow && !Main.hideUI).ToDirectionInt() * increment;
        PauseFade = MathF.Saturate(PauseFade);

        if (PauseFade == 0f)
        {
            return false;
        }

        var blur = (1f - MathF.Pow(1f - PauseFade, 2f)) * 16f;

        var blurSize = new Vector2(blur) / new Vector2(Main.screenWidth, Main.screenHeight);

        device.SetRenderTarget(screenSwap);
        device.Clear(Color.Transparent);

        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.Identity);
        {
            horizBlur.Parameters.BlurSize = blurSize;
            horizBlur.Apply();

            sb.Draw(screen, Vector2.Zero, Color.White);
        }
        sb.End();

        device.SetRenderTarget(screen);
        device.Clear(Color.Transparent);

        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.Identity);
        {
            vertBlur.Parameters.BlurSize = blurSize;
            vertBlur.Apply();

            sb.Draw(screenSwap, Vector2.Zero, Color.White);
        }
        sb.End();
        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.Identity);
        {
            var color = Color.Black * (1f - MathF.Pow(1f - PauseFade, 2f));
            color *= 0.35f;

            sb.Draw(TextureAssets.MagicPixel.Value, device.Viewport.Bounds, color);
        }
        sb.End();

        // Return false as we're drawing back to the first target, so no swap is needed
        return false;
    }
}
