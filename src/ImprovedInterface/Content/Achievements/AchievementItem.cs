using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Achievements;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using ImprovedInterface.Content.PauseMenu;

namespace ImprovedInterface.Content;

/// <summary>
/// Simplified recreation of <see cref="UIAchievementListItem"/> without visuals
/// </summary>
public abstract class AchievementItem : UIElement
{
    // Frame data
    private const int icon_size = 64, 
                      padded_icon_size = 66, 
                      icons_per_row = 8,
                      locked_icon_offset = 528;

    private const int height = padded_icon_size + 36;

    public Achievement Achievement { get; }
    public bool Locked => !Achievement.IsCompleted;
    public CalculatedStyle BorderDimensions => Border.GetInnerDimensions();

    public UIElement Container;
    public UIImageFramed Icon;
    public UIImage Border;
    public Rectangle IconFrame;
    public readonly Rectangle IconFrameLocked, IconFrameUnlocked;
    public bool Modded;

    public AchievementItem(Achievement achievement)
    {
        ModAchievement modAchievement = achievement.ModAchievement;
        Modded = modAchievement != null;
        Achievement = achievement;
        
        Height.Set(height, 0);
        Width.Set(0, 1f);
        PaddingTop = 8f;
        PaddingLeft = 9f;

        Container = new UIElement();
        {
            Container.Width.Set(0, 1);
            Container.Height.Set(0, 1);
        }
        Append(Container);

        var iconIndex = Main.Achievements.GetIconIndex(achievement.Name);
        var moddedFrame = new Rectangle(0, iconIndex * padded_icon_size, icon_size, icon_size);
        var vanillaFrame = new Rectangle(iconIndex % icons_per_row * padded_icon_size, iconIndex / icons_per_row * padded_icon_size, icon_size, icon_size);
        var lockedFrameX = Modded ? padded_icon_size : (vanillaFrame.X + locked_icon_offset);
        IconFrameUnlocked = Modded ? moddedFrame : vanillaFrame;
        IconFrameLocked = IconFrameUnlocked with { X = lockedFrameX };
        UpdateIconFrame();

        var iconTexture = modAchievement?.Texture ?? Main.Assets.Request<Texture2D>("Images/UI/Achievements");
        Icon = new(iconTexture, IconFrame);
        {
            Icon.Left.Set(6, 0);
            Icon.Top.Set(12, 0);
        }
        Container.Append(Icon);

        Border = new(Main.Assets.Request<Texture2D>("Images/UI/Achievement_Borders"));
        {
            Border.Left = Icon.Left - (4, 0);
            Border.Top = Icon.Top - (4, 0);
            Border.Color = Color.Black;
        }
        Container.Append(Border);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (PauseMenuAchievements.AchievementsFade > 0)
            base.Draw(spriteBatch);

        Icon.Color = Color.White * PauseMenuAchievements.AchievementsFade;
        Border.Color = Color.Black * PauseMenuAchievements.AchievementsFade;
        
        UpdateIconFrame();
    }

    private void UpdateIconFrame()
    {
        IconFrame = Locked ? IconFrameLocked : IconFrameUnlocked;
        Icon?.SetFrame(IconFrame);
    }

    public (decimal, decimal) GetTrackerValues()
    {
        if (!Achievement.HasTracker)
            return (0m, 0m);

        var tracker = Achievement.GetTracker();
        if (tracker.GetTrackerType() == TrackerType.Int)
        {
            var actualTracker = (AchievementTracker<int>)tracker;
            return (actualTracker.Value, actualTracker.MaxValue);
        }

        if (tracker.GetTrackerType() == TrackerType.Float)
        {
            var actualTracker = (AchievementTracker<float>)tracker;
            return ((decimal)actualTracker.Value, (decimal)actualTracker.MaxValue);
        } 

        return (0m, 0m);
    }

    public override int CompareTo(object obj)
    {
        if (!(obj is AchievementItem item))
            return 0;

        if (Achievement.IsCompleted && !item.Achievement.IsCompleted)
            return -1;

        if (!Achievement.IsCompleted && item.Achievement.IsCompleted)
            return 1;

        return Achievement.Id.CompareTo(item.Achievement.Id);
    }
}