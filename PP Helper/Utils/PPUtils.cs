using BeatSaberMarkupLanguage;
using PP_Helper.JSON;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP_Helper.Utils
{
    static class PPUtils
    {
        public const double FALLOFF_RATE = 0.965;
        private static (float, float)[] ppCurve = new (float, float)[]
        {
            (0f, 0),
            (45f, .015f),
            (50f, .03f),
            (55f, .06f),
            (60f, .105f),
            (65f, .16f),
            (68f, .24f),
            (70f, .285f),
            (80f, .563f),
            (84f, .695f),
            (88f, .826f),
            (94.5f, 1.015f),
            (95f, 1.046f),
            (100f, 1.12f),
            (110f, 1.18f),
            (114f, 1.25f)
        };

        // Pre-compute to save on division operator
        // Other optimizations available, good enough for now
        private static float[] slopes;

        public static void Initialize()
        {
            if (slopes == null)
            {
                slopes = new float[ppCurve.Length - 1];
                for (var i = 0; i < ppCurve.Length - 1; i++)
                {
                    var x1 = ppCurve[i].Item1;
                    var y1 = ppCurve[i].Item2;
                    var x2 = ppCurve[i + 1].Item1;
                    var y2 = ppCurve[i + 1].Item2;

                    var m = (y2 - y1) / (x2 - x1);
                    slopes[i] = m;
                }
            }
        }

        // Calculate how much pp a given accuracy is worth on a map worth rawPP
        public static float CalculatePP(float rawPP, float accuracy)
        {
            var output = rawPP * PPPercentage(accuracy);
            return output;
        }

        public static float GetPPGain(float pp, ProfileDataLoader.SongID id)
        {
            if (ProfileDataLoader.instance.ppTopBottomSum == null || ProfileDataLoader.instance.ppBottomUpSum == null)
            {
                return 0;
            }

            var ppTotal = ProfileDataLoader.instance.ppTopBottomSum[0];
            int oldIndex = -1;

            Logger.log.Debug($"old total: {ppTotal}");
            // See if song has already been played
            if (ProfileDataLoader.instance.songIndex.ContainsKey(id))
            {
                Logger.log.Debug("Song has been played before");
                // Not a higher play, worth nothing
                if (pp <= ProfileDataLoader.instance.songDataInfo[id].pp)
                {
                    return 0;
                }
                oldIndex = ProfileDataLoader.instance.songIndex[id];
            }
            // Find first song that it's worth more than

            var i = 0;
            foreach (var song in ProfileDataLoader.instance.songOrder)
            {
                var songWorth = ProfileDataLoader.instance.songDataInfo[song].pp;
                if (songWorth < pp)
                {
                    var newSum = CalculateNewPPTotal(pp, i, oldIndex);
                    return (float) (newSum - ppTotal);
                }
                i++;
            }

            // Not worth more than any song already played
            return (float)(Math.Pow(FALLOFF_RATE, ProfileDataLoader.instance.songOrder.Count) * pp);
        }

        // calculate how much percentage of the raw pp a given percentage is worth
        private static float PPPercentage(float accuracy)
        {
            if (accuracy >= 114)
                return 1.25f;
            if (accuracy <= 0)
                return 0f;

            var i = -1;
            foreach ((float score, float given) in ppCurve)
            {
                if (score > accuracy)
                    break;
                i += 1;
            }

            var lowerScore = ppCurve[i].Item1;
            var higherScore = ppCurve[i + 1].Item1;
            var lowerGiven = ppCurve[i].Item2;
            var higherGiven = ppCurve[i + 1].Item2;
            return Lerp(lowerScore, lowerGiven, higherScore, higherGiven, accuracy, i);
        }

        // linearly interpolate to find y3 value for x3 between (x1, y1) and (x2, y2)
        private static float Lerp(float x1, float y1, float x2, float y2, float x3, int i)
        {
            float m;
            if (slopes != null)
                m = slopes[i];
            else
                m = (y2 - y1) / (x2 - x1);
            return m * (x3 - x1) + y1;
        }

        private static double CalculateNewPPTotal(float pp, int index, int oldIndex)
        {
            if (oldIndex != -1 && index > oldIndex)
                throw new ArgumentException();
            double smallerPlays;
            // Last song - nothing is worth less
            if (oldIndex == index && index == ProfileDataLoader.instance.songIndex.Count - 1)
            {
                smallerPlays = 0;
            }
            else
            {
                // If a new play, then push down every song below it
                if (oldIndex == -1)
                {
                    smallerPlays = FALLOFF_RATE * ProfileDataLoader.instance.ppTopBottomSum[index];
                }
                // otherwise, remove old play and push down songs below new index
                else
                {
                    var oldValues = oldIndex < ProfileDataLoader.instance.songIndex.Count - 1 ? ProfileDataLoader.instance.ppTopBottomSum[oldIndex + 1] : 0;
                    var midValues = ProfileDataLoader.instance.ppTopBottomSum[index] - ProfileDataLoader.instance.ppTopBottomSum[oldIndex];
                    smallerPlays = FALLOFF_RATE * midValues + oldValues;
                }
            }
            Logger.log.Debug($"Smaller play: {smallerPlays}");

            double largerPlays;
            // First song - nothing is worth more
            if (index == 0)
            {
                largerPlays = 0;
            }
            else
                largerPlays = ProfileDataLoader.instance.ppBottomUpSum[index - 1];

            Logger.log.Debug($"Larger play: {largerPlays}");

            Logger.log.Debug($"index: {index}");
            double newSum = (Math.Pow(FALLOFF_RATE, index) * pp) + smallerPlays + largerPlays;
            Logger.log.Debug($"newSum: {newSum}");
            return newSum;
        }
    }
}
