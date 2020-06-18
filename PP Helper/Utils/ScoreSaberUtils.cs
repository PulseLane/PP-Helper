using System;
using System.Collections.Generic;

namespace PP_Helper.Utils
{
    static class ScoreSaberUtils
    {
        // Will need to adjust this logic when/if non-standard maps get ranked
        private static Dictionary<string, BeatmapDifficulty> _difficultyDict = new Dictionary<string, BeatmapDifficulty>()
        {
            {"_Easy_SoloStandard", BeatmapDifficulty.Easy},
            {"_Normal_SoloStandard", BeatmapDifficulty.Normal},
            {"_Hard_SoloStandard", BeatmapDifficulty.Hard},
            {"_Expert_SoloStandard", BeatmapDifficulty.Expert},
            {"_ExpertPlus_SoloStandard", BeatmapDifficulty.ExpertPlus},
        };

        public static BeatmapDifficulty GetDifficulty(string difficulty)
        {
            return _difficultyDict[difficulty];
        }

        public static BeatmapDifficulty GetDifficulty(int difficulty)
        {
            switch (difficulty)
            {
                case 1:
                    return BeatmapDifficulty.Easy;
                case 3:
                    return BeatmapDifficulty.Normal;
                case 5:
                    return BeatmapDifficulty.Hard;
                case 7:
                    return BeatmapDifficulty.Expert;
                case 9:
                    return BeatmapDifficulty.ExpertPlus;
                default:
                    Logger.log.Debug($"Unknown difficulty: {difficulty}");
                    throw new ArgumentException();
            }
        }

        public static string GetDifficultyAsString(BeatmapDifficulty difficulty)
        {
            foreach (var kvp in _difficultyDict)
            {
                if (kvp.Value == difficulty)
                    return kvp.Key;
            }
            // TODO - better exception
            throw new Exception();
        }
    }
}
