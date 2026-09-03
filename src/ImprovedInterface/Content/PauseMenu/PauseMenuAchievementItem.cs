using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Achievements;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace ImprovedInterface.Content.PauseMenu;

public class PauseMenuAchievementItem(Achievement achievement) : AchievementItem(achievement)
{
    public override void OnInitialize()
    {
        base.OnInitialize();
        
        MarginTop = 16f;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        Container.Left.Percent = MathHelper.Lerp(Container.Left.Percent, IsMouseHovering ? 0.025f : 0, 0.1f);
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
        
        var color = (Locked ? Color.Silver : Color.Gold);
        color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 0.5f : 0f);

        var position = basePosition - new Vector2(BorderDimensions.Width + 8, 16); 
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, name.Value, position, color, 0f, Vector2.Zero, nameScale, maxWidth);

        position = basePosition + new Vector2(6, 16);
        color = (Locked ? Color.DarkGray : Color.Silver);
        color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 1f : 0f);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, descText, position, color, 0f, Vector2.Zero, descScale);
    }
}