using ImprovedInterface.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Achievements;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace ImprovedInterface.Content.PauseMenu;

/// <summary>
/// Simplified recreation of <see cref="UIAchievementListItem"/>
/// </summary>
public class AchievementItem : UIElement
{
    // Frame data
    private const int icon_size = 64, 
                      padded_icon_size = 66, 
                      icons_per_row = 8,
                      locked_icon_offset = 528;

    private const int height = padded_icon_size + 36;

    public Achievement Achievement { get; }
    public bool Locked => !Achievement.IsCompleted;
    
    private UIImageFramed icon;
    private UIImage border;
    private Rectangle iconFrame;
    private readonly Rectangle iconFrameLocked, iconFrameUnlocked;
    private bool modded;

    public AchievementItem(Achievement achievement)
    {
        ModAchievement modAchievement = achievement.ModAchievement;
        modded = modAchievement != null;
        Achievement = achievement;
        
        Height.Set(height, 0);
        Width.Set(0, 1f);
        PaddingTop = 8f;
        PaddingLeft = 9f;

        var iconIndex = Main.Achievements.GetIconIndex(achievement.Name);
        var moddedFrame = new Rectangle(0, iconIndex * padded_icon_size, icon_size, icon_size);
        var vanillaFrame = new Rectangle(iconIndex % icons_per_row * padded_icon_size, iconIndex / icons_per_row * padded_icon_size, icon_size, icon_size);
        var lockedFrameX = modded ? padded_icon_size : (vanillaFrame.X + locked_icon_offset);
        iconFrameUnlocked = modded ? moddedFrame : vanillaFrame;
        iconFrameLocked = iconFrameUnlocked with { X = lockedFrameX };
        UpdateIconFrame();

        var iconTexture = modAchievement?.Texture ?? Main.Assets.Request<Texture2D>("Images/UI/Achievements");
        icon = new(iconTexture, iconFrame);
        {
            icon.Left.Set(6, 0);
            icon.Top.Set(12, 0);
        }
        Append(icon);

        border = new(Main.Assets.Request<Texture2D>("Images/UI/Achievement_Borders"));
        {
            border.Left = icon.Left - (4, 0);
            border.Top = icon.Top - (4, 0);
        }
        Append(border);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        
        UpdateIconFrame();
    }

    private void UpdateIconFrame()
    {
        iconFrame = Locked ? iconFrameLocked : iconFrameUnlocked;
        icon?.SetFrame(iconFrame);
    }

    private (decimal, decimal) GetTrackerValues()
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