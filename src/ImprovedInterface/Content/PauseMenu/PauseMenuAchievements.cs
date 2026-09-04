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

        var list = Main.Achievements.CreateAchievementsList();
        foreach (var item in list)
        {
            Add(new PauseMenuAchievementItem(item));
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