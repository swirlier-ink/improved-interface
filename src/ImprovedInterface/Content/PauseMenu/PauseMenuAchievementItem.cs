using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace ImprovedInterface.Content.PauseMenu;

public class PauseMenuAchievementItem(Achievement achievement) : AchievementItem(achievement)
{
    private UIImageFramed progressIcon;
    public override void OnInitialize()
    {
        base.OnInitialize();
        
        var iconTexture = Achievement.ModAchievement?.Texture ?? Main.Assets.Request<Texture2D>("Images/UI/Achievements");
        progressIcon = new UIImageFramed(iconTexture, IconFrameUnlocked with { Height = 0 });
        Icon.Append(progressIcon);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        Container.Left.Percent = MathHelper.Lerp(Container.Left.Percent, IsMouseHovering ? 0.025f : 0, 0.1f);
        
        var trackerValues = GetTrackerValues();
        if (trackerValues.Item2 > 0 && Locked)
        {
            var progress = (float)(trackerValues.Item1 / trackerValues.Item2);
            var yFrame = (int)(64 * (1f - progress));
            progressIcon.SetFrame(IconFrameUnlocked with { Height = (int)(64 * progress), Y = IconFrameUnlocked.Y + yFrame });
            progressIcon.Top.Set(yFrame, 0);
            progressIcon.Recalculate();
        }
        else
        {
            progressIcon.Color = Color.Transparent;
        }

        MarginTop = MathHelper.SmoothStep(0, 16f, PauseMenuAchievements.AchievementsFade * 0.5f);
        Recalculate();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var font = FontAssets.ItemStack.Value;

        var containerDimensions = Container.GetInnerDimensions();
        var borderRight = BorderDimensions.X + BorderDimensions.Width;
        var basePosition = new Vector2(borderRight + 7f, containerDimensions.Y);
        
        var name = Achievement.FriendlyName;
        var desc = Achievement.Description;

        if (Locked && Achievement.Hidden)
            name = desc = new LocalizedText(null, "???");
        
        var nameScale = new Vector2(1f);
        var descScale = new Vector2(0.9f);
        var maxWidth = containerDimensions.Width - BorderDimensions.Width - 11f;
        var maxDescHeight = 58f;

        var descText = font.CreateWrappedText(desc.Value, (maxWidth - 20f) * (1f / descScale.X), Language.ActiveCulture.CultureInfo);
        var descSize = ChatManager.GetStringSize(font, descText, descScale, maxWidth);

        if (descSize.Y > maxDescHeight)
            descScale *= maxDescHeight / descSize.Y;

        var opacity = PauseMenuAchievements.AchievementsFade;
        var color = (Locked ? Color.Silver : Color.Gold);
        color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 0.5f : 0f);

        var position = basePosition - new Vector2(BorderDimensions.Width + 8, 16); 
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, name.Value, position, color * opacity, 0f, Vector2.Zero, nameScale, maxWidth);

        position = basePosition + new Vector2(6, 16);
        color = (Locked ? Color.DarkGray : Color.Silver);
        color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 1f : 0f);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, descText, position, color * opacity, 0f, Vector2.Zero, descScale);

        var trackerValues = GetTrackerValues();
        if (trackerValues.Item2 > 0 && Locked)
        {
            progressIcon.Color = Color.White * opacity;
            
            var text = "(" + (int)trackerValues.Item1 + "/" + (int)trackerValues.Item2 + ")";
            
            position = basePosition - new Vector2(BorderDimensions.Width, 16) + ChatManager.GetStringSize(font, name.Value, nameScale, maxWidth) * new Vector2(1, 0);
            color = (Locked ? Color.DarkGray : Color.Silver);
            
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, position, color * opacity, 0f, Vector2.Zero, nameScale, maxWidth);
        }
    }
}