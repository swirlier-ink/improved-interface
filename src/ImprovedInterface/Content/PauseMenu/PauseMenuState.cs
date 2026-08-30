using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ImprovedInterface.Content.PauseMenu;

public sealed class PauseMenuState : UIState
{
    public override void OnInitialize()
    {
        var logoContainer = new UIElement();
        {
            logoContainer.Left.Set(0f, 0f);
            logoContainer.Height.Set(0f, 0.3f);
            logoContainer.Width.Set(0f, 1f);
            logoContainer.IgnoresMouseInteraction = true;
        }
        Append(logoContainer);

        var logo = new LogoElement();
        {
            logo.Width.Set(0f, 1f);

            logo.Height.Set(0f, 0.8f);
            logo.MinHeight.Set(140f, 0f);

            logo.LogoX.Set(40f, 0.14f);

            logo.VAlign = 1f;
        }
        logoContainer.Append(logo);

        var buttonContainer = new UIElement();
        {
            buttonContainer.Left.Set(60f, 0f);
            buttonContainer.VAlign = 0.5f;
            buttonContainer.Height.Set(0f, 0.4f);
            buttonContainer.MinHeight.Set(400f, 0f);
            buttonContainer.Width.Set(0f, 0.2f);
        }
        Append(buttonContainer);

        // TODO: loc
        var header = new UIText("Paused", 1f, true);
        {
        }
        buttonContainer.Append(header);
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
