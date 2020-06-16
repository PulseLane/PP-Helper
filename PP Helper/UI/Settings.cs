using BeatSaberMarkupLanguage.Attributes;
using PP_Helper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static PP_Helper.Data.StarAccCalculator;

namespace PP_Helper.UI
{
    class Settings : PersistentSingleton<Settings>
    {
        [UIValue("showInfo")]
        public bool showInfo
        {
            get => Config.showInfo;
            set
            {
                Config.showInfo = value;
                Config.Write();
            }
        }
        [UIValue("defaultAcc")]
        public float defaultAcc
        {
            get => Config.defaultAcc;
            set
            {
                Config.defaultAcc = value;
                Config.Write();
            }
        }

        [UIValue("ppTop")]
        public bool ppTop
        {
            get => Config.ppTop;
            set
            {
                Config.ppTop = value;
                Config.Write();
            }
        }

        [UIValue("accIncrement")]
        public float accIncrement
        {
            get => Config.accIncrement;
            set
            {
                value = (float) Math.Round(value, 2);
                Config.accIncrement = value;
                PPDisplay.instance.accIncrement = value;
                Config.Write();
            }
        }

        [UIValue("starRange")]
        public float starRange
        {
            get => Config.starRange;
            set
            {
                Config.starRange = value;
                Config.Write();
            }
        }

        [UIValue("starAccOptions")]
        private List<object> options = Enum.GetValues(typeof(CalculationType)).Cast<CalculationType>()
                                        .Select(x => StarAccCalculator.ToFriendlyString(x)).Cast<object>().ToList();

        [UIValue("starAccChoice")]
        public string starAccChoice
        {
            get => ToFriendlyString(Config.starAccChoice);
            set
            {
                Config.starAccChoice = StarAccCalculator.ParseFriendlyString(value, out CalculationType choice) ? choice : CalculationType.AverageOfTopN;
                Config.Write();
            }
        }

        [UIValue("numberOfScores")]
        public int scores
        {
            get => Config.numberOfScores;
            set
            {
                Config.numberOfScores = value;
                Config.Write();
            }
        }

        [UIValue("accOverride")]
        public bool accOverride
        {
            get => Config.accOverride;
            set
            {
                Config.accOverride = value;
                Config.Write();
            }
        }

        [UIValue("autoUpdate")]
        public bool autoUpdate
        {
            get => Config.autoUpdate;
            set
            {
                Config.autoUpdate = value;
                Config.Write();
            }
        }
    }
}
