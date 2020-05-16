using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP_Helper.Utils
{
    static class SongDataUtils
    {
        // May need this for getting star data?
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
