using System;
using ImprovedInterface.Common;
using ImprovedInterface.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ImprovedInterface.Content.PauseMenu;

public class PauseMenuAchievements : FadedList
{
    public static bool AchievementsOpen;
    public static float AchievementsFade;
    
    public PauseMenuAchievements() : base()
    {
        SetScrollbar(new UIScrollbar());
        
        Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("LIKE_A_BOSS")));
        Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("DEFEAT_QUEEN_SLIME")));
        Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("SUPREME_HELPER_MINION")));
        Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("DEFEAT_OLD_ONES_ARMY_TIER3")));
        Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("TO_INFINITY_AND_BEYOND")));
        Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("ROLLIN_IN_YOUR_GRAVE")));
        Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("FEAR_THE_SUN")));
        for (int i = 0; i < 20; i++)
        {
            Add(new PauseMenuAchievementItem(Main.Achievements.GetAchievement("EXTRA_LIFE")));
        }
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        IgnoresMouseInteraction = !AchievementsOpen;

        AchievementsFade += AchievementsOpen.ToDirectionInt() * 0.1f;
        AchievementsFade = MathF.Saturate(AchievementsFade);
        
        PaddingLeft = MathHelper.SmoothStep(0, 25f, AchievementsFade);
        Recalculate();
    }
}