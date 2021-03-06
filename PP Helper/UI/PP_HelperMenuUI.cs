﻿using BeatSaberMarkupLanguage.MenuButtons;
using PP_Helper.Data;

namespace PP_Helper.UI
{
    class PP_HelperMenuUI
    {
        public static void CreateUI()
        {
            MenuButton scoreSaberButton = new MenuButton("PP Helper Profile Refresh", "Get latest profile data from scoresaber", ProfileRefresh);
            MenuButton starButton = new MenuButton("PP Helper Data Refresh", "Recalculate star accuracy", StarRefresh);
            MenuButtons.instance.RegisterButton(scoreSaberButton);
            MenuButtons.instance.RegisterButton(starButton);
        }

        internal static void ProfileRefresh()
        {
            ProfileDataLoader.instance.LoadProfileData();
        }

        internal static void StarRefresh()
        {
            StarAccCalculator.CalculateAcc();
        }
    }
}
