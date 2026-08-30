using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Steamworks;
using System;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ImprovedInterface.Content.PauseMenu;

public sealed class PauseMenuState : UIState
{
    public enum NetModeVisibility
    {
        Always,
        SinglePlayer,
        MultiplayerClient,
    }

    private readonly record struct OptionInfo(LocalizedText Text, MouseEvent OnClick, NetModeVisibility Visibility);

    private static readonly OptionInfo[] options =
    [
        new(Mods.ImprovedInterface.PauseMenu.Continue.GetText(), ClickContinue, NetModeVisibility.Always),
        new(Mods.ImprovedInterface.PauseMenu.Save.GetText(), ClickSave, NetModeVisibility.SinglePlayer),
        new(Mods.ImprovedInterface.PauseMenu.Settings.GetText(), ClickSettings, NetModeVisibility.Always),
        new(Mods.ImprovedInterface.PauseMenu.Achievements.GetText(), ClickAchievements, NetModeVisibility.Always),
        new(Mods.ImprovedInterface.PauseMenu.SaveAndQuit.GetText(), ClickQuit, NetModeVisibility.SinglePlayer),
        new(Mods.ImprovedInterface.PauseMenu.Disconnect.GetText(), ClickQuit, NetModeVisibility.MultiplayerClient),
    ];

    private static void ClickContinue(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.ingameOptionsWindow = false;
        SoundEngine.PlaySound(in SoundID.MenuClose);

        Main.playerInventory = true;

        // Irrelevant, should be handled by our settings menu
        // Main.SaveSettings();
    }

    private static void ClickSave(UIMouseEvent evt, UIElement listeningElement)
    {
        // TODO: Callback that has the text go from 'Saving...' to 'Saved' and turns grey and becomes no longer interactable
        WorldGen.saveAndPlay();
    }

    private static void ClickSettings(UIMouseEvent evt, UIElement listeningElement)
    {
        // TODO
    }

    private static void ClickAchievements(UIMouseEvent evt, UIElement listeningElement)
    {
        // TODO
    }

    private static void ClickQuit(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.menuMode = MenuID.Status;
        Main.gameMenu = true;
        WorldGen.SaveAndQuit();
    }

    private UIElement? textContainer;
    private UIElement? logoContainer;

    public override void OnInitialize()
    {
        textContainer = new UIElement();
        {
            textContainer.Left.Set(60f, 0f);
            textContainer.VAlign = 0.5f;
            textContainer.MinHeight.Set(225f, 0f);
            textContainer.Width.Set(0f, 0.2f);
        }
        Append(textContainer);

        // TODO: loc
        var header = new UIText(Mods.ImprovedInterface.PauseMenu.Paused.GetText(), 1f, true);
        textContainer.Append(header);

        var buttonList = new UIList();
        {
            var topPadding = header.MinHeight.Pixels + 26;

            buttonList.Top.Set(topPadding, 0f);
            buttonList.Height.Set(-topPadding, 1f);
            buttonList.Width.Set(0f, 1f);
            buttonList.ListPadding = 18f;
        }
        textContainer.Append(buttonList);

        AddButtons();

        logoContainer = new UIElement();
        {
            logoContainer.Left.Set(0f, 0f);
            logoContainer.Height.Set((Main.screenHeight - textContainer.Dimensions.Height) * 0.5f, 0f);
            logoContainer.Width.Set(0f, 1f);
            logoContainer.IgnoresMouseInteraction = true;
        }
        Append(logoContainer);

        var logo = new LogoElement();
        {
            logo.Width.Set(0f, 1f);

            logo.Height.Set(250f, 0f);

            logo.LogoX.Set(40f, 0.09f);

            logo.VAlign = 1f;
        }
        logoContainer.Append(logo);

        return;

        void AddButtons()
        {
            foreach (var (text, evt, visibility) in options)
            {
                var allowButton = visibility switch
                {
                    NetModeVisibility.SinglePlayer => Main.netMode == NetmodeID.SinglePlayer,
                    NetModeVisibility.MultiplayerClient => Main.netMode == NetmodeID.MultiplayerClient,
                    _ => true,
                };

                if (!allowButton)
                {
                    continue;
                }

                var button = new UIText(text, 0.45f, true);
                {
                    button.OnLeftClick += evt;
                }
                buttonList.Add(button);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (textContainer is null
         || logoContainer is null)
        {
            return;
        }

        logoContainer.Height.Set((Main.screenHeight - textContainer.Dimensions.Height) * 0.5f, 0f);
    }
}

file sealed class LogoElement : UIElement
{
    [OnLoad]
    private static void Load()
    {
        IL_Main.DoUpdate += DoUpdate_DisableLogoResetting;
    }

    private static void DoUpdate_DisableLogoResetting(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdfld<Main>(nameof(Main.logoRotationSpeed))
        );

        c.GotoPrev(
            MoveType.After,
            i => i.MatchLdsfld<Main>(nameof(Main.gameMenu))
        );

        c.EmitStaticDelegateUnsafe(static () => Main.ingameOptionsWindow);

        c.EmitOr();
    }

    public StyleDimension LogoX;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        UpdateLogo();
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var menu = MenuLoader.CurrentMenu;

        var logo = menu.Logo.Value;

        var logoOrigin = logo.Size() * 0.5f;

        var position = this.Dimensions.Left();
        position.X += LogoX.GetValue(this.Dimensions.Width);

        var color = Color.White;

        var rotation = Main.instance.logoRotation;
        var scale = Main.instance.logoScale;

        // TODO: The Zoey solution.
        if (menu.PreDrawLogo(sb, ref position, ref rotation, ref scale, ref color))
        {
            sb.Draw(logo, position, null, color, rotation, logoOrigin, scale, SpriteEffects.None, 0f);
        }
        menu.PostDrawLogo(sb, position, rotation, scale, color);
    }

    private static void UpdateLogo()
    {
        var main = Main.instance;

        if (Main.remixWorld)
        {
            Update(3.06f, false, 0.00004f, 3.06f, 3.22f, 0.00009f, 0.9f, 1f);
        }
        else if (WorldGen.drunkWorldGen && !WorldGen.notTheBees)
        {
            main.logoRotation += main.logoRotationSpeed * 0.000004f;
            if (main.logoRotationSpeed > 0f)
            {
                main.logoRotationSpeed += 1500f;
            }
            else
            {
                main.logoRotationSpeed -= 1500f;
            }
            main.logoScale -= 0.05f;
            if (main.logoScale < 0f)
            {
                main.logoScale = 0f;
            }
        }
        else
        {
            Update();
        }

        return;

        void Update(
            float fastRotationThreshold = 0.09f,
            bool rotationThresholdDirection = true,
            float rotationSpeed = 0.000004f,
            float minRotation = -0.08f,
            float maxRotation = 0.08f,
            float scaleSpeed = 0.000009f,
            float minScale = 1f,
            float maxScale = 1.35f)
        {
            // Ehh, cheap hack
            if (rotationThresholdDirection)
            {
                if (main.logoScale < 0.1f)
                {
                    main.logoRotation = 0f;
                    main.logoRotationSpeed = 0f;
                }

                if (main.logoScale < 0.98f)
                {
                    main.logoScale *= 1.05f;
                }
            }

            if (main.logoRotation > fastRotationThreshold == rotationThresholdDirection)
            {
                main.logoRotation += main.logoRotationSpeed * 0.0016f;
                if (main.logoRotationSpeed > 0f)
                {
                    main.logoRotationSpeed = 0f;
                }
            }

            main.logoRotation += main.logoRotationSpeed * rotationSpeed;
            if (main.logoRotation > maxRotation)
            {
                main.logoRotationDirection = -1f;
            }
            else if (main.logoRotation < minRotation)
            {
                main.logoRotationDirection = 1f;
            }

            if (main.logoRotationSpeed < 20f && main.logoRotationDirection > 0f)
            {
                main.logoRotationSpeed++;
            }
            else if (main.logoRotationSpeed > -20f && main.logoRotationDirection < 0f)
            {
                main.logoRotationSpeed--;
            }

            main.logoScale += main.logoScaleSpeed * scaleSpeed;
            if (main.logoScale > maxScale)
            {
                main.logoScaleDirection = -1f;
            }
            else if (main.logoScale < minScale)
            {
                main.logoScaleDirection = 1f;
            }

            if (main.logoScaleSpeed < 50f && main.logoScaleDirection > 0f)
            {
                main.logoScaleSpeed++;
            }
            else if (main.logoScaleSpeed > -50f && main.logoScaleDirection < 0f)
            {
                main.logoScaleSpeed--;
            }
        }
    }
}
