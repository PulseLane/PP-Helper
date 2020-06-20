using BS_Utils.Utilities;
using PP_Helper.Data;
using PP_Helper.Utils;
using SongBrowser;
using SongBrowser.DataAccess;
using SongBrowser.Internals;
using SongBrowser.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static PP_Helper.Data.ProfileDataLoader;

namespace PP_Helper.SongBrowser
{
    public static class SongSorting
    {
        public enum SortMethod
        {
            PPGain,
            PossiblePP,
        }

        public static string PPGainName = "PPGain";
        public static string PossiblePPName = "PossiblePP";

        private const float SORT_BUTTON_WIDTH = 12.0f;
        private const float SORT_BUTTON_HEIGHT = 5.0f;
        private const float SORT_BUTTON_FONT_SIZE = 2.15f;
        private const float SORT_BUTTON_X = -23.0f;
        private const float SORT_BUTTON_Y = 37f;
        private const float SORT_BUTTON_SPACING = 0.25f;

        private static SongBrowserUI _songBrowserUI;
        private static LevelCollectionViewController _levelCollectionViewController;

        public static void Init()
        {
            _songBrowserUI = Resources.FindObjectsOfTypeAll<SongBrowserUI>().First();
            var sortButtonGroup = _songBrowserUI.GetPrivateField<List<SongSortButton>>("_sortButtonGroup");
            _levelCollectionViewController = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().First();

            float ppHelperButtonX = GetButtonX(sortButtonGroup.Count);
            SongSortButton ppHelperButton = CreateSortButton(ppHelperButtonX, "PP Helper", "PPHelper", SortHandler);
            sortButtonGroup.Add(ppHelperButton);
        }

        private static SongSortButton CreateSortButton(float buttonX, string buttonName, string internalButtonName,
                                                Func<List<IPreviewBeatmapLevel>, List<IPreviewBeatmapLevel>> sortHandler)
        {
            SongSortButton sortButton = new SongSortButton();
            sortButton.SortMode = SongSortMode.Custom;
            sortButton.Button = _levelCollectionViewController.CreateUIButton("ApplyButton",
                new Vector2(buttonX, SORT_BUTTON_Y), new Vector2(SORT_BUTTON_WIDTH, SORT_BUTTON_HEIGHT),
                () =>
                {
                    SongBrowserModel.CustomSortHandler = sortHandler;
                    var onSortButtonClickEvent = Utils.ReflectionUtils.GetPrivateMethod(_songBrowserUI, "OnSortButtonClickEvent");
                    var refreshOuterUIState = Utils.ReflectionUtils.GetPrivateMethod(_songBrowserUI, "RefreshOuterUIState");
                    onSortButtonClickEvent.Invoke(_songBrowserUI, new object[] { sortButton.SortMode});
                    refreshOuterUIState.Invoke(_songBrowserUI, new object[] { UIState.Main });
                },
                buttonName);

            sortButton.Button.SetButtonTextSize(SORT_BUTTON_FONT_SIZE);
            sortButton.Button.GetComponentsInChildren<HorizontalLayoutGroup>().First(btn => btn.name == "Content").padding = new RectOffset(4, 4, 2, 2);
            sortButton.Button.ToggleWordWrapping(false);
            sortButton.Button.name = "Sort" + internalButtonName + "Button";

            return sortButton;
        }

        private static List<IPreviewBeatmapLevel> SortHandler(List<IPreviewBeatmapLevel> levels)
        {
            switch (Config.sortMethod)
            {
                case SortMethod.PPGain:
                    return SortPPGain(levels);
                case SortMethod.PossiblePP:
                    return SortPossiblePP(levels);
                default:
                    throw new ArgumentException("Unrecognized sort method");
            }
        }

        private static List<IPreviewBeatmapLevel> SortPossiblePP(List<IPreviewBeatmapLevel> levels)
        {
            Logger.log.Info("Sorting song list by possible pp...");

            return levels
                .OrderByDescending(x =>
                {
                    return GetHighestPP(x, false);
                })
                .ToList();
        }

        private static List<IPreviewBeatmapLevel> SortPPGain(List<IPreviewBeatmapLevel> levels)
        {
            Logger.log.Info("Sorting song list by possible pp gain...");

            return levels
                .OrderByDescending(x =>
                {
                    return GetHighestPP(x, true);
                })
                .ToList();
        }

        // Purposefully ignore modifiers here - only 2 maps have them enabled and it doesn't make much sense to just filter to those two
        private static float GetHighestPP(IPreviewBeatmapLevel previewBeatmapLevel, bool ppGain)
        {
            float maxPP = 0;
            var id = SongDataUtils.GetHash(previewBeatmapLevel.levelID);
            // Check if in SDC
            if (SongDataCore.Plugin.Songs.Data.Songs.ContainsKey(id))
            {
                // Loop through each diff
                foreach (var diff in SongDataCore.Plugin.Songs.Data.Songs[id].diffs)
                {
                    var difficulty = SongDataUtils.GetBeatmapDifficulty(diff.diff);
                    var songID = new SongID(id, difficulty);
                    // Only go through ranked songs
                    if (SongDataUtils.IsRankedSong(songID))
                    {
                        float pp = PPUtils.CalculatePP(songID, AccLoader.instance.GetAcc(songID));
                        if (ppGain)
                            pp = PPUtils.GetPPGain(pp, songID);
                        maxPP = pp > maxPP ? pp : maxPP;
                    }

                }
                return maxPP;
            }
            return 0;
        }

        private static float GetButtonX(int numberOfButtons)
        {
            return SORT_BUTTON_X + (SORT_BUTTON_WIDTH * numberOfButtons) + (SORT_BUTTON_SPACING * numberOfButtons);
        }
    }
}
