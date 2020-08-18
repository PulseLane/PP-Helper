using System;
using static PP_Helper.Data.StarAccCalculator;
using static PP_Helper.SongBrowser.SongSorting;

namespace PP_Helper
{
    class Config
    {
        public static BS_Utils.Utilities.Config config = new BS_Utils.Utilities.Config("PP Helper");
        public static bool showInfo = true;
        public static float defaultAcc = 80.0f;
        public static bool ppTop = false;
        public static float accIncrement = 0.1f;
        public static float starRange = 0.25f;
        public static CalculationType starAccChoice = CalculationType.AverageOfTopN;
        public static int numberOfScores = 3;
        public static bool accOverride = true;
        public static bool autoUpdate = true;
        public static bool playHistory = true;
        public static SortMethod sortMethod = SortMethod.PPGain;

        public static bool ignoreNoFail = true;
        public static bool hideOnStart = false;
        public static int decimalPrecision = 2;

        public static void Read()
        {
            showInfo = config.GetBool("PP Helper", "showInfo", true, true);
            defaultAcc = config.GetFloat("PP Helper", "defaultAcc", 80f, true);
            ppTop = config.GetBool("PP Helper", "ppTop", false, true);
            accIncrement = config.GetFloat("PP Helper", "accIncrement", 0.1f, true);
            starRange = config.GetFloat("PP Helper", "starRange", 0.5f, true);
            starAccChoice = Enum.TryParse(config.GetString("PP Helper", "starAccChoice", "AverageOfTopN", true), out CalculationType starChoice) ? starChoice : CalculationType.AverageOfTopN;
            numberOfScores = config.GetInt("PP Helper", "numberOfScores", 3, true);
            accOverride = config.GetBool("PP Helper", "accOverride", true, true);
            autoUpdate = config.GetBool("PP Helper", "autoUpdate", true, true);
            playHistory = config.GetBool("PP Helper", "playHistory", true, true);
            sortMethod = Enum.TryParse(config.GetString("PP Helper", "sortMethod", "PPGain", true), out SortMethod sortChoice) ? sortChoice : SortMethod.PPGain;

            ignoreNoFail = config.GetBool("PP Helper", "ignoreNoFail", true, true);
            hideOnStart = config.GetBool("PP Helper", "hideOnStart", false, true);
            decimalPrecision = config.GetInt("PP Helper", "decimalPrecision", 2, true);
        }

        public static void Write()
        {
            config.SetBool("PP Helper", "showInfo", showInfo);
            config.SetFloat("PP Helper", "defaultAcc", defaultAcc);
            config.SetBool("PP Helper", "ppTop", ppTop);
            config.SetFloat("PP Helper", "accIncrement", accIncrement);
            config.SetFloat("PP Helper", "starRange", starRange);
            config.SetString("PP Helper", "starAccChoice", starAccChoice.ToString());
            config.SetInt("PP Helper", "numberOfScores", numberOfScores);
            config.SetBool("PP Helper", "accOverride", accOverride);
            config.SetBool("PP Helper", "autoUpdate", autoUpdate);
            config.SetBool("PP Helper", "playHistory", playHistory);
            config.SetString("PP Helper", "sortMethod", sortMethod.ToString());

            config.SetBool("PP Helper", "ignoreNoFail", ignoreNoFail);
            config.SetBool("PP Helper", "hideOnStart", hideOnStart);
            config.SetInt("PP Helper", "decimalPrecision", decimalPrecision);
        }
    }
}
