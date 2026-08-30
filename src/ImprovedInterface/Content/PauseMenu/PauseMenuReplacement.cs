using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace ImprovedInterface.Content.PauseMenu;

public static class PauseMenuReplacement
{
    private static UserInterface pauseInterface = new UserInterface();

    [ModSystemHooks.UpdateUI]
    private static void UpdateUI(GameTime gameTime)
    {
        if (!Main.ingameOptionsWindow)
        {
            return;
        }

        pauseInterface.Update(gameTime);
    }

    [GameInterfaceLayers.Replace(GameInterfaceLayers.IN_GAME_OPTIONS, InterfaceScaleType.UI, Name = $"{nameof(ImprovedInterface)}: Pause Menu")]
    private static bool DrawPauseMenu()
    {
        if (!Main.ingameOptionsWindow)
        {
            pauseInterface.State = null;

            return true;
        }

        // pauseInterface.State = ;

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
            pauseInterface.Draw(sb, Main.instance.gameTime);

            Main.instance.DrawMouseOver();
        }
        sb.Restart(ss with { SamplerState = Main.SamplerStateForCursor });
        {
            Main.DrawCursor(Main.DrawThickCursor());

            Main.instance.DrawInterface_40_InteractItemIcon();
        }

        // Return false as the pause menu stops all further layers from rendering
        return false;
    }
}
