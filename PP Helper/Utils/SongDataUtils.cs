using PP_Helper.Data;
using System;
using System.Collections.Generic;
using static PP_Helper.Data.ProfileDataLoader;

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

        public static string GetDifficultyAsString(BeatmapDifficulty difficulty)
        {
            return _difficultyDict[difficulty];
        }

        public static string GetHash(string levelId)
        {
            if (levelId.Contains("custom_level"))
            {
                return levelId.Substring(levelId.LastIndexOf("_") + 1);
            }
            return levelId;
        }

        public static double GetStars(SongID songID)
        {
            var difficultyStats = SongDataCore.Plugin.Songs.Data.Songs[songID.id].diffs;
            foreach (var difficultyStat in difficultyStats)
            {
                if (difficultyStat.diff.Equals(SongDataUtils.GetDifficultyAsString(songID.difficulty)))
                {
                    return difficultyStat.star;
                }
            }
            throw new ArgumentException();
        }

        public static double GetRoundedDownStars(SongID songID)
        {
            var star = GetStars(songID);
            return GetRoundedDownStars(star);
        }

        public static double GetRoundedDownStars(double star)
        {
            return Math.Round(Math.Floor(star / Config.starRange) * Config.starRange, 2);
        }

        public static double GetRoundedUpStars(SongID songID)
        {
            var star = GetStars(songID);
            return GetRoundedUpStars(star);
        }

        public static double GetRoundedUpStars(double star)
        {
            return Math.Round(Math.Ceiling(star / Config.starRange) * Config.starRange, 2);
        }

        public static bool IsRankedSong(SongID songID)
        {
            return RawPPLoader.InDict(songID.id);
        }
    }
}
