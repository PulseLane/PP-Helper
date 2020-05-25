﻿using System.Collections.Generic;

namespace PP_Helper.Utils
{
    static class SongDataUtils
    {
        private static Dictionary<BeatmapDifficulty, string> _difficultyDict = new Dictionary<BeatmapDifficulty, string>()
        {
            {BeatmapDifficulty.Easy, "Easy" },
            {BeatmapDifficulty.Normal, "Normal" },
            {BeatmapDifficulty.Hard, "Hard" },
            {BeatmapDifficulty.Expert, "Expert" },
            {BeatmapDifficulty.ExpertPlus, "Expert+" },
        };

        public static string getDifficultyAsString(BeatmapDifficulty difficulty)
        {
            return _difficultyDict[difficulty];
        }

        public static string getHash(string levelId)
        {
            if (levelId.Contains("custom_level"))
            {
                return levelId.Substring(levelId.LastIndexOf("_") + 1);
            }
            return levelId;
        }
    }
}
