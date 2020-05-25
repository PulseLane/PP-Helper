using Newtonsoft.Json;
using PP_Helper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PP_Helper.Data
{
    public static class StarAccCalculator
    {
        public static readonly string FILE_NAME = Path.Combine(Plugin.DIRECTORY, "AverageStarAcc.json");

        public enum CalculationType
        {
            AverageOfTopN,
            AverageOfAll,
            Max,
        }

        private const string AverageOfTopNString = "Average of Top N Scores";
        private const string AverageOfAllString = "Average of All Scores";
        private const string MaxString = "Max Score";

        public static string ToFriendlyString(CalculationType calcuationType)
        {
            switch (calcuationType)
            {
                case CalculationType.AverageOfTopN:
                    return AverageOfTopNString;
                case CalculationType.AverageOfAll:
                    return AverageOfAllString;
                case CalculationType.Max:
                    return MaxString;
                default:
                    throw new ArgumentException();
            }
        }

        public static bool ParseFriendlyString(string calculationTypeString, out CalculationType calculationType)
        {
            switch (calculationTypeString)
            {
                case AverageOfTopNString:
                    calculationType = CalculationType.AverageOfTopN;
                    return true;
                case AverageOfAllString:
                    calculationType = CalculationType.AverageOfAll;
                    return true;
                case MaxString:
                    calculationType = CalculationType.Max;
                    return true;
                default:
                    calculationType = CalculationType.AverageOfTopN;
                    return false;
            }
        }

        public static void CalculateAcc()
        {
            Logger.log.Debug("Calculating star acc");
            var range = Config.starRange;
            Dictionary<double, double> acc = new Dictionary<double, double>();
            Dictionary<double, int> counts = new Dictionary<double, int>();

            // Need SongDataCore data
            if (!SongDataCore.Plugin.Songs.IsDataAvailable())
            {
                return;
            }

            // Initialize some helper vars
            var numberOfScores = Config.starAccChoice.Equals(CalculationType.Max) ? 1 : Config.numberOfScores;
            var maxScores = new Dictionary<double, List<double>>();
            var maxStarValue = CalculateMaxStarValue();
            var averageAll = Config.starAccChoice.Equals(CalculationType.AverageOfAll);

            // Initialize dicts
            for (double star = 0.00; star < maxStarValue; star += 0.25)
            {
                star = SongDataUtils.GetRoundedDownStars(star);
                acc[star] = 0;
                counts[star] = 0;
            }

            foreach (var kvp in ProfileDataLoader.instance.songDataInfo)
            {
                var star = SongDataUtils.GetStars(kvp.Key);
                // Will definitely be updating acc
                if (averageAll || counts[star] < numberOfScores)
                {
                    acc[star] = ((acc[star] * counts[star]) + kvp.Value.acc) / (counts[star] + 1);
                    counts[star] += 1;
                    // update maxScores
                    if (!averageAll)
                    {
                        // first score for given star rating
                        if (!maxScores.ContainsKey(star))
                        {
                            maxScores[star] = new List<double>();
                        }
                        maxScores[star].Add(kvp.Value.acc);
                        maxScores[star].Sort(); // List is small enough to not matter
                    }
                }
                // Better than the lowest sore, update average acc
                else if (kvp.Value.acc > maxScores[star].First())
                {
                    var accWithoutLowerScore = acc[star] * counts[star] - maxScores[star].First();
                    acc[star] = (accWithoutLowerScore + kvp.Value.acc) / (counts[star]);
                    // update maxScores
                    maxScores[star].RemoveAt(0);
                    maxScores[star].Add(kvp.Value.acc);
                    maxScores[star].Sort();
                }
            }

            Logger.log.Debug("Finished calculating star acc");
            SaveFile(acc);
        }

        // Loops through all songs and finds the highest star difficulty saved (rounded up to nearest multiple of starRange)
        private static double CalculateMaxStarValue()
        {
            double maxStarValue = 0;
            foreach (var kvp in ProfileDataLoader.instance.songDataInfo)
            {
                if (SongDataCore.Plugin.Songs.Data.Songs.TryGetValue(kvp.Key.id, out var song))
                {
                    var difficultyStats = SongDataCore.Plugin.Songs.Data.Songs[kvp.Key.id].diffs;
                    foreach (var difficultyStat in difficultyStats)
                    {
                        if (difficultyStat.diff.Equals(SongDataUtils.GetDifficultyAsString(kvp.Key.difficulty)))
                        {
                            var star = SongDataUtils.GetRoundedUpStars(difficultyStat.star);
                            maxStarValue = star > maxStarValue ? star : maxStarValue;
                        }
                    }
                }
            }

            return Math.Round(maxStarValue, 2);
        }

        private static void SaveFile(Dictionary<double, double> starAcc)
        {
            File.WriteAllText(FILE_NAME, JsonConvert.SerializeObject(starAcc, Formatting.Indented));
            AccLoader.instance.LoadStarAcc();
        }
    }
}
