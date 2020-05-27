using HarmonyLib;
using PP_Helper.SongBrowser;
using SongBrowser.DataAccess;
using SongBrowser.Internals;
using SongBrowser.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PP_Helper.HarmonyPatches
{
    [HarmonyPatch(typeof(SongBrowserUI), "CreateSortButtons")]
    class CreateSortButtonsPatch
    {
        static void Postfix(ref SongBrowserUI __instance, ref List<SongSortButton> ____sortButtonGroup, ref BeatSaberUIController ____beatUi)
        {
            //Logger.log.Debug("Inside postfix");
            float sortButtonFontSize = 2.15f;
            float sortButtonX = -23.0f;
            float sortButtonWidth = 12.0f;
            float buttonSpacing = 0.25f;
            float buttonY = 37f;
            float buttonHeight = 5.0f;

            string[] sortButtonNames = new string[]
            {
                "PP Gain", "Possible PP"
            };

            var numberOfBaseSortModes = ____sortButtonGroup.Count;
            var enumSize = Enum.GetNames(typeof(SongSortMode)).Length;

            SongSortMode[] sortModes = new SongSortMode[]
            {
                SongSorting.PPGainSortMode,
                SongSorting.PossiblePPSortMode
            };

            MethodInfo sortMethod = __instance.GetType().GetMethod("OnSortButtonClickEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo refreshMethod = __instance.GetType().GetMethod("RefreshOuterUIState", BindingFlags.NonPublic | BindingFlags.Instance);

            var copy = __instance;

            for (int i = numberOfBaseSortModes; i < sortButtonNames.Length + numberOfBaseSortModes; i++)
            {
                float curButtonX = sortButtonX + (sortButtonWidth * i) + (buttonSpacing * i);
                SongSortButton sortButton = new SongSortButton();
                var index = i - numberOfBaseSortModes;
                sortButton.SortMode = sortModes[index];
                sortButton.Button = ____beatUi.LevelCollectionViewController.CreateUIButton("ApplyButton",
                    new Vector2(curButtonX, buttonY), new Vector2(sortButtonWidth, buttonHeight),
                    () =>
                    {
                        sortMethod.Invoke(copy, new object[] { sortButton.SortMode });
                        refreshMethod.Invoke(copy, new object[] { UIState.Main });
                    },
                    sortButtonNames[index]);
                //Logger.log.Debug("After button");
                sortButton.Button.SetButtonTextSize(sortButtonFontSize);
                sortButton.Button.GetComponentsInChildren<HorizontalLayoutGroup>().First(btn => btn.name == "Content").padding = new RectOffset(4, 4, 2, 2);
                sortButton.Button.ToggleWordWrapping(false);

                // probably a better way to handle this, but do this for now
                string name = "";
                if (sortButton.SortMode == SongSorting.PPGainSortMode)
                    name = SongSorting.PPGainName;
                else if (sortButton.SortMode == SongSorting.PossiblePPSortMode)
                    name = SongSorting.PossiblePPName;
                sortButton.Button.name = "Sort" + name + "Button";

                ____sortButtonGroup.Add(sortButton);
            }
        }
    }
}
