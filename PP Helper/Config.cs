﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PP_Helper.JSON.StarAccCalculator;

namespace PP_Helper
{
    class Config
    {
        public static BS_Utils.Utilities.Config config = new BS_Utils.Utilities.Config("PP Helper");
        public static float defaultAcc = 80.0f;
        public static float starRange = 0.25f;
        public static CalculationType starAccChoice = CalculationType.AverageOfTopN;
        public static int numberOfScores = 3;

        public static void Read()
        {
            defaultAcc = config.GetFloat("PP Helper", "defaultAcc", 80f, true);
            starRange = config.GetFloat("PP Helper", "starRange", 0.5f, true);
            starAccChoice = Enum.TryParse(config.GetString("PP Helper", "starAccChoice", "AverageOfTopN", true), out CalculationType choice) ? choice : CalculationType.AverageOfTopN;
            numberOfScores = config.GetInt("PP Helper", "numberOfScores", 3, true);
        }

        public static void Write()
        {
            config.SetFloat("PP Helper", "defaultAcc", defaultAcc);
            config.SetFloat("PP Helper", "starRange", starRange);
            config.SetString("PP Helper", "starAccChoice", starAccChoice.ToString());
            config.SetInt("PP Helper", "numberOfScores", numberOfScores);
        }
    }
}