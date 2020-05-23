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
