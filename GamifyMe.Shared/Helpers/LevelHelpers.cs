using System;

namespace GamifyMe.Shared.Helpers
{
    public static class LevelHelpers
    {
        public static int GetLevelFromXp(int xp)
        {
            if (xp < 100) return 1;
            
            // Formula: Level = log2(xp/100 + 1) + 1
            double val = (double)xp / 100.0 + 1.0;
            int level = (int)Math.Floor(Math.Log2(val)) + 1;
            
            return level < 1 ? 1 : level;
        }

        public static int GetTotalXpForLevel(int level)
        {
             if (level <= 1) return 0;
             return 100 * ((int)Math.Pow(2, level - 1) - 1);
        }
        
        // Helper to get progress details for UI
        public static (int currentLevel, int xpForNextLevel, int xpInCurrentLevel, double progressPercent) GetLevelDetails(int currentXp)
        {
            int level = GetLevelFromXp(currentXp);
            int totalXpForCurrent = GetTotalXpForLevel(level);
            int totalXpForNext = GetTotalXpForLevel(level + 1);
            
            int xpInCurrentLevel = currentXp - totalXpForCurrent;
            int range = totalXpForNext - totalXpForCurrent;
            
            double progress = range == 0 ? 100 : (double)xpInCurrentLevel / range * 100;

            return (level, totalXpForNext, xpInCurrentLevel, progress);
        }
    }
}
