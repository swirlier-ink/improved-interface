using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Reflection;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.Social;
using Terraria.UI;

namespace ImprovedInterface.Content.PauseMenu;

public sealed class PauseMenuState : UIState
{
    public enum NetModeVisibility
    {
        Always,
        SinglePlayer,
        MultiplayerClient,
        CanInvite,
    }

    private readonly record struct OptionInfo(LocalizedText Text, MouseEvent OnClick, NetModeVisibility Visibility);

    private static readonly OptionInfo[] options =
    [
        new(Mods.ImprovedInterface.PauseMenu.Continue.GetText(), ClickContinue, NetModeVisibility.Always),
        new(Mods.ImprovedInterface.PauseMenu.Save.GetText(), ClickSave, NetModeVisibility.SinglePlayer),
        new(Mods.ImprovedInterface.PauseMenu.Settings.GetText(), ClickSettings, NetModeVisibility.Always),
        new(Mods.ImprovedInterface.PauseMenu.Achievements.GetText(), ClickAchievements, NetModeVisibility.Always),
        new(Mods.ImprovedInterface.PauseMenu.InvitePlayers.GetText(), ClickInvitePlayers, NetModeVisibility.CanInvite),
        new(Mods.ImprovedInterface.PauseMenu.SaveAndQuit.GetText(), ClickQuit, NetModeVisibility.SinglePlayer),
        new(Mods.ImprovedInterface.PauseMenu.Disconnect.GetText(), ClickQuit, NetModeVisibility.MultiplayerClient),
    ];

    private static void ClickContinue(UIMouseEvent evt, UIElement listeningElement)
    {
        PauseMenuAchievements.AchievementsOpen = false;
        Main.ingameOptionsWindow = false;
        SoundEngine.PlaySound(in SoundID.MenuClose);

        Main.playerInventory = true;

        // Irrelevant, should be handled by our settings menu
        // Main.SaveSettings();
    }

    private static void ClickSave(UIMouseEvent evt, UIElement listeningElement)
    {
        WorldFile.SetTempToOngoing();
        ThreadPool.QueueUserWorkItem(WrappedSaveAndPlay, 1);

        SoundEngine.PlaySound(in SoundID.MenuOpen);

        return;

        void WrappedSaveAndPlay(object? threadContext)
        {
            if (listeningElement is not TextOption option)
            {
                WorldGen.saveAndPlayCallBack(threadContext);

                return;
            }

            try
            {
                option.SetText(Mods.ImprovedInterface.PauseMenu.Save.Saving.GetText());

                option.Working = true;

                WorldGen.saveAndPlayCallBack(threadContext);

                option.SetText(Mods.ImprovedInterface.PauseMenu.Save.Saved.GetText());
            }
            catch
            {
                // TODO
            }
            finally
            {
                option.Working = false;
            }
        }
    }

    private static void ClickSettings(UIMouseEvent evt, UIElement listeningElement)
    {
        // TODO
        PauseMenuAchievements.AchievementsOpen = false;
    }

    private static void ClickAchievements(UIMouseEvent evt, UIElement listeningElement)
    {
        // TODO
        PauseMenuAchievements.AchievementsOpen = !PauseMenuAchievements.AchievementsOpen;
        
        SoundEngine.PlaySound(in SoundID.MenuOpen);
    }

    private static void ClickInvitePlayers(UIMouseEvent evt, UIElement listeningElement)
    {
        SocialAPI.Network.OpenInviteInterface();

        SoundEngine.PlaySound(in SoundID.MenuOpen);
    }

    private static void ClickQuit(UIMouseEvent evt, UIElement listeningElement)
    {
        PauseMenuAchievements.AchievementsOpen = false;
        PauseMenuAchievements.AchievementsFade = 0;
        Main.menuMode = MenuID.Status;
        Main.gameMenu = true;
        WorldGen.SaveAndQuit();

        SoundEngine.PlaySound(in SoundID.MenuClose);
    }

    private UIElement? textContainer;
    private UIElement? logoContainer;
    
    private PauseMenuAchievements? achievementsContainer;

    public override void OnInitialize()
    {
        textContainer = new UIElement();
        {
            textContainer.Left.Set(60f, 0f);
            textContainer.VAlign = 0.5f;
            textContainer.MinHeight.Set(255f, 0f);
            textContainer.Width.Set(0f, 0.1f);
            textContainer.MinWidth.Set(200f, 0f);
        }
        Append(textContainer);

        var header = new UIText(Mods.ImprovedInterface.PauseMenu.Paused.GetText(), 1f, true);
        {
            header.TextColor = Color.Transparent;
            header.OnUpdateExt += OnUpdate_Header;
        }
        textContainer.Append(header);

        var topPadding = header.MinHeight.Pixels + 24;

        var buttonList = new UIList();
        {
            buttonList.Top.Set(topPadding, 0f);
            buttonList.Height.Set(-topPadding, 1f);
            buttonList.Width.Set(0f, 1f);
            buttonList.ListPadding = 2f;

            buttonList.OverflowHidden = false;
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

            logo.VAlign = 1f;
        }
        logoContainer.Append(logo);

        achievementsContainer = new();
        {
            achievementsContainer.Left = textContainer.Right + (40, 0.05f);
            
            achievementsContainer.Width.Set(0, 1);
            achievementsContainer.Width.Sub(achievementsContainer.Left + (60, 0.15f));
            
            achievementsContainer.Top.Set(30, 0.05f);
            achievementsContainer.Height.Set(-60, 0.9f);
        }
        Append(achievementsContainer);

        return;

        static void OnUpdate_Header(UIText text)
        {
            text.TextColor = Color.White * ((float)Main.mouseTextColor / byte.MaxValue) * PauseMenuReplacement.PauseFade;

            // Refresh the color
            text.InternalSetText(text._text, text._textScale, text._isLarge);
        }

        void AddButtons()
        {
            foreach (var (text, evt, visibility) in options)
            {
                var allowButton = visibility switch
                {
                    NetModeVisibility.SinglePlayer => Main.netMode == NetmodeID.SinglePlayer,
                    NetModeVisibility.MultiplayerClient => Main.netMode == NetmodeID.MultiplayerClient,
                    NetModeVisibility.CanInvite => SocialAPI.Network != null && SocialAPI.Network.CanInvite(),
                    _ => true,
                };

                if (!allowButton)
                {
                    continue;
                }

                var button = new TextOption(text, evt, 0.5f, true);
                {
                    button.Width.Set(0f, 1f);
                    button.TextOriginX = 0f;

                    button.Height.Set(35f, 0f);

                    button.TextColor = Color.Transparent;
                    button.InternalSetText(button._text, button._textScale, button._isLarge);

                    button.Index = (byte)buttonList.Count;
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

file sealed class TextOption : UIText
{
    private const float base_scale = 0.5f;
    private const float hover_scale = 0.6f;

    private static readonly Color default_color = Color.White;
    private static readonly Color hover_color = Main.OurFavoriteColor;
    private static readonly Color working_color = (Color.White * 0.5f) with { A = byte.MaxValue };

    private object textCache;

    public byte Index;

    public bool Working
    {
        get;
        set
        {
            IgnoresMouseInteraction = value;

            field = value;

            if (value)
            {
                textCache = _text;
            }
        }
    }

    public TextOption(LocalizedText text, MouseEvent evt, float textScale = 1, bool large = false)
        : base(text, textScale, large)
    {
        OnLeftClick += evt;

        textCache = text;

        TextOriginY = 0.5f;
        TextColor = default_color;
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);

        SoundEngine.PlaySound(in SoundID.MenuTick);
    }

    private float scaleInterpolator;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        scaleInterpolator += 0.15f * (IsMouseHovering && !Working).ToDirectionInt();
        scaleInterpolator = MathF.Saturate(scaleInterpolator);

        _textScale = MathF.Lerp(base_scale, hover_scale, 1f - MathF.Pow(1f - scaleInterpolator, 2f));

        if (Working)
        {
            TextColor = working_color * ((float)Main.mouseTextColor / byte.MaxValue);

            var frame = (int)(Main.GlobalTimeWrappedHourly * 2.5f);

            SetText($"{textCache}{new string('.', (frame % 3) + 1)}");

            return;
        }

        TextColor = IsMouseHovering
            ? hover_color
            : (default_color * ((float)Main.mouseTextColor / byte.MaxValue));

        TextColor *= PauseMenuReplacement.PauseFade - (Index * 0.05f);

        // Refresh the color
        InternalSetText(_text, _textScale, _isLarge);
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

        c.EmitStaticDelegateUnsafe(static () => Main.ingameOptionsWindow || PauseMenuReplacement.PauseFade > 0f);

        c.EmitOr();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        UpdateLogo();
    }

    private bool modMenuImplementsPreDraw;

    public override void OnActivate()
    {
        var info = MenuLoader.CurrentMenu.GetType().GetMethod(
            nameof(ModMenu.PreDrawLogo),
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance
        );

        modMenuImplementsPreDraw = info is not null;
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var menu = MenuLoader.CurrentMenu;

        var logo = menu.Logo.Value;

        var logoOrigin = logo.Size() * 0.5f;

        var position = this.Dimensions.Left();
        position.X += logoOrigin.X * 1.35f;

        var multiplier = (1f + ((float)Main.tileColor.R / byte.MaxValue) * 2f) / 3f;

        var color = Color.White * multiplier;
        color.A = byte.MaxValue;

        var rotation = Main.instance.logoRotation;
        var scale = Main.instance.logoScale;

        if (!modMenuImplementsPreDraw
         || (menu
          is MenuOldVanilla
          or MenuBiggerAndBoulder
          or MenuJourneysEnd
          or MenutML))
        {
            color *= PauseMenuReplacement.PauseFade;

            if (menu.PreDrawLogo(sb, ref position, ref rotation, ref scale, ref color))
            {
                sb.Draw(logo, position, null, color, rotation, logoOrigin, scale, SpriteEffects.None, 0f);
            }
            menu.PostDrawLogo(sb, position, rotation, scale, color);
        }
        else
        {
            DrawModdedLogo();
        }

        if (menu
         is MenuOldVanilla 
         or MenuBiggerAndBoulder
         or MenuJourneysEnd)
        {
            DrawVanillaLogo();
        }

        return;

        void DrawModdedLogo()
        {
            // Should use the center alignment as modders cannot be trusted evidently.
            position = new Vector2(Main.screenWidth * 0.5f, 100f);

            using var lease = ScreenspaceTargetProvider.Shared.Create(Main.graphics.GraphicsDevice);

            using (sb.Scope())
            using (lease.Scope(clearColor: Color.Transparent))
            {
                var prior = Main.Rasterizer;

                Main.Rasterizer = RasterizerState.CullCounterClockwise;

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
                {
                    if (menu.PreDrawLogo(sb, ref position, ref rotation, ref scale, ref color))
                    {
                        sb.Draw(logo, position, null, color, rotation, logoOrigin, scale, SpriteEffects.None, 0f);
                    }
                    menu.PostDrawLogo(sb, position, rotation, scale, color);
                }
                sb.End();

                Main.Rasterizer = prior;
            }

            sb.End(out var ss);
            sb.Begin(ss with { SortMode = SpriteSortMode.Immediate});
            {
                var effect = Assets.PauseMenu.LogoFade.CreateLogoFadeShader();

                var dest = this.Dimensions;
                dest.Width = (int)MathF.Max((logo.Width * 1.35f) + 140, 250f);

                var source = Utils.CenteredRectangle(position, dest.Size());
                source.X += 65;

                effect.Parameters.Source = new Vector4(dest.Size(), dest.X, dest.Y);

                effect.Apply();

                sb.Draw(lease.Target, dest, source, Color.White * PauseMenuReplacement.PauseFade);
            }
            sb.Restart(in ss);
        }

        void DrawVanillaLogo()
        {
            var logoDay = menu switch
            {
                MenuOldVanilla => TextureAssets.Logo3.Value,
                MenuBiggerAndBoulder => TextureAssets.Logo5.Value,
                _ => TextureAssets.Logo.Value,
            };

            var logoNight = menu switch
            {
                MenuOldVanilla => TextureAssets.Logo4.Value,
                MenuBiggerAndBoulder => TextureAssets.Logo6.Value,
                _ => TextureAssets.Logo2.Value,
            };

            logoOrigin = logoDay.Size() * 0.5f;

            var colorDay = color * ((float)Main.LogoA / byte.MaxValue);
            var colorNight = color * ((float)Main.LogoB / byte.MaxValue);

            position = this.Dimensions.Left();
            position.X += logoOrigin.X * 1.35f;

            sb.Draw(logoDay, position, null, colorDay, rotation, logoOrigin, scale, SpriteEffects.None, 0f);
            sb.Draw(logoNight, position, null, colorNight, rotation, logoOrigin, scale, SpriteEffects.None, 0f);
        }
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

        if (Main.dayTime && !Main.remixWorld)
        {
            Main.LogoA += 2;
            if (Main.LogoA > 255)
            {
                Main.LogoA = 255;
            }
            Main.LogoB--;
            if (Main.LogoB < 0)
            {
                Main.LogoB = 0;
            }
        }
        else
        {
            Main.LogoB += 2;
            if (Main.LogoB > 255)
            {
                Main.LogoB = 255;
            }

            Main.LogoA--;

            if (Main.LogoA < 0)
            {
                Main.LogoA = 0;
                // Main.LogoT = true; - Unused, should this still be set?
            }
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
            float maxScale = 1.35f
        )
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
