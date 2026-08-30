using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace ImprovedInterface.Content.PauseMenu;

public static class PauseMenuReplacement
{
    private static readonly UserInterface @interface = new UserInterface();
    private static PauseMenuState? state;

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
}
