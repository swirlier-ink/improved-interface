using Terraria.Achievements;
using Terraria.Localization;

namespace ImprovedInterface.Common;

/// <summary>
/// Extensions to different Achievement-related types.
/// </summary>
public static class AchievementExtensions
{
    extension(AchievementCategory category)
    {
        public string GetCategoryText()
        {
            var text = " ";

            switch (category)
            {
                case AchievementCategory.Challenger:
                    text = Language.GetTextValue("Achievements.ChallengerCategory");
                    break;
                case AchievementCategory.Collector:
                    text = Language.GetTextValue("Achievements.CollectorCategory");
                    break;
                case AchievementCategory.Explorer:
                    text = Language.GetTextValue("Achievements.ExplorerCategory");
                    break;
                case AchievementCategory.Slayer:
                    text = Language.GetTextValue("Achievements.SlayerCategory");
                    break;
            }

            return text;
        }
    }
}