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

        public static BeatmapDifficulty GetBeatmapDifficulty(string difficulty)
        {
            foreach (var kvp in _difficultyDict)
            {
                if (kvp.Value.Equals(difficulty))
                    return kvp.Key;
            }

            throw new ArgumentException();
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
            var difficultyStat = GetDifficultyStat(songID);
            return difficultyStat.star;
        }

        public static double GetRoundedStars(SongID songID)
        {
            var star = GetStars(songID);
            return GetRoundedStars(star);
        }

        public static double GetRoundedStars(double star)
        {
            return Math.Round(Math.Round(star / Config.starRange) * Config.starRange, 2);
        }

        public static bool IsRankedSong(SongID songID)
        {
            return RawPPLoader.InDict(songID.id);
        }

        public static float GetRawPP(SongID songID)
        {
            if (RawPPLoader.InDict(songID.id))
            {
                return RawPPLoader.GetRawPP(songID);
            }
            return 0;
        }

        public static SongID GetSongID(IDifficultyBeatmap difficultyBeatmap)
        {
            var id = GetHash(difficultyBeatmap.level.levelID);
            var difficulty = difficultyBeatmap.difficulty;
            return new SongID(id, difficulty);
        }

        private static SongDataCore.BeatStar.BeatStarSongDifficultyStats GetDifficultyStat(SongID songID)
        {
            var difficultyStats = SongDataCore.Plugin.Songs.Data.Songs[songID.id].diffs;
            foreach (var difficultyStat in difficultyStats)
            {
                if (difficultyStat.diff.Equals(SongDataUtils.GetDifficultyAsString(songID.difficulty)))
                {
                    return difficultyStat;
                }
            }

            throw new ArgumentException();
        }
    }
}
