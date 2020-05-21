using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP_Helper.Utils
{
    static class PPUtils
    {
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

        /// <summary>
        /// Calculate how much pp a given accuracy is worth on a map worth rawPP
        /// </summary>
        /// <param name="rawPP">the map's raw pp</param>
        /// <param name="accuracy">the accuracy score on the map</param>
        /// <returns></returns>
        public static float calculate_pp(float rawPP, float accuracy)
        {
            var output = rawPP * pp_percentage(accuracy);
            return output;
        }

        /// <summary>
        ///  calculate how much percentage of the raw pp a given percentage is worth
        /// </summary>
        /// <param name="accuracy">the accuracy score on the map</param>
        /// <returns>the percentage of raw pp the given accuracy is worth</returns>
        private static float pp_percentage(float accuracy)
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
            return lerp(lowerScore, lowerGiven, higherScore, higherGiven, accuracy, i);
        }

        // linearly interpolate to find y3 value for x3 between (x1, y1) and (x2, y2)
        private static float lerp(float x1, float y1, float x2, float y2, float x3, int i)
        {
            float m;
            if (slopes != null)
                m = slopes[i];
            else
                m = (y2 - y1) / (x2 - x1);
            return m * (x3 - x1) + y1;
        }
    }
}
