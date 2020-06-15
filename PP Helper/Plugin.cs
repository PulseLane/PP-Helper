using BeatSaberMarkupLanguage.Settings;
using CountersPlus.Custom;
using HarmonyLib;
using IPA;
using IPA.Loader;
using PP_Helper.Counters;
using PP_Helper.Data;
using PP_Helper.HarmonyPatches;
using PP_Helper.UI;
using PP_Helper.Utils;
using SongBrowser;
using SongBrowser.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace PP_Helper
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static readonly string DIRECTORY = Path.Combine(Environment.CurrentDirectory, "UserData", "PP Helper");
        // To be used at some point?
        List<SongDataCore.BeatStar.BeatStarSongDifficultyStats> stats;

        internal static Plugin instance { get; private set; }
        internal static string Name => "PP Helper";
        internal static PP_HelperController PluginController { get { return PP_HelperController.instance; } }
        internal static Harmony harmony;

        [Init]
        public Plugin(IPALogger logger)
        {
            instance = this;
            Logger.log = logger;
            Logger.log.Debug("Logger initialized.");
        }

        [OnEnable]
        public void OnEnable()
        {
            Config.Read();

            harmony = new Harmony("com.PulseLane.BeatSaber.PP_Helper");
            try
            {
                HarmonyPatchBase();

                // SongBrowser integration
                if (PluginManager.EnabledPlugins.Any(x => x.Id == "SongBrowser"))
                {
                    Logger.log.Info("SongBrowser installed, using harmony patches");
                    HarmonyPatchSongBrowser();
                }
                else
                    Logger.log.Debug("SongBrowser not installed");
            }
            catch (Exception e)
            {
                Logger.log.Error($"Failed to apply harmony patches! {e}");
            }

            if (!Directory.Exists(DIRECTORY))
            {
                Logger.log.Info("Didn't find PP Helper directory in UserData, creating now");
                try
                {
                    var di = Directory.CreateDirectory((DIRECTORY));
                    Logger.log.Info("Successfully created folder");
                }
                catch (Exception e)
                {
                    Logger.log.Error($"Failed to create directory: {e}");
                }
            }

            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.levelSelected += OnLevelSelected;
            BS_Utils.Utilities.BSEvents.gameSceneActive += OnGameSceneActive;
            BS_Utils.Utilities.BSEvents.levelCleared += OnLevelCleared;

            BSMLSettings.instance.AddSettingsMenu("PP Helper", "PP_Helper.UI.settings.bsml", Settings.instance);

            PP_HelperMenuUI.CreateUI();

            // Counters+ integration
            if (PluginManager.EnabledPlugins.Any(x => x.Id == "Counters+"))
            {
                Logger.log.Info("Counters+ installed, setting up PP counter");
                AddPPCounter();
            }
            else
                Logger.log.Debug("Counters+ not installed");
        }

        public void OnMenuSceneLoadedFresh(ScenesTransitionSetupDataSO transitionSetupDataSO)
        {
            PP_HelperController.OnLoad();
            if (!RawPPLoader.IsInit())
                RawPPLoader.Initialize();
            PPUtils.Initialize();
            ProfileDataLoader.instance.Initialize();
            AccLoader.instance.Initialize();
        }

        public void OnLevelSelected(LevelCollectionViewController levelCollectionViewController, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            PP_HelperController.instance.setId(SongDataUtils.GetHash(previewBeatmapLevel.levelID));
        }

        public void OnGameSceneActive()
        {
            if (PluginManager.EnabledPlugins.Any(x => x.Id == "Counters+"))
            {
                Logger.log.Debug("Counters+ installed");
                MainConfigModel model = ConfigLoader.LoadPPConfig();
                if (model.ppConfig.Enabled)
                {
                    new GameObject("PP Counter").AddComponent<PPCounter>();
                }
            }
        }
        public void OnLevelCleared(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO, LevelCompletionResults levelCompletionResults)
        {
            ProfileDataLoader.instance.LevelCleared(standardLevelScenesTransitionSetupDataSO, levelCompletionResults);
        }

        [OnDisable]
        public void OnDisable()
        {
            if (PluginController != null)
                GameObject.Destroy(PluginController);

            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh -= OnMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.levelSelected -= OnLevelSelected;
            BS_Utils.Utilities.BSEvents.gameSceneActive -= OnGameSceneActive;
            BS_Utils.Utilities.BSEvents.levelCleared -= OnLevelCleared;

            BSMLSettings.instance.RemoveSettingsMenu(Settings.instance);
            harmony.UnpatchAll("com.PulseLane.BeatSaber.PP_Helper");

            ProfileDataLoader.instance.Dispose();
            RawPPLoader.Dispose();

            // TODO: remove counter
        }

        private void AddPPCounter()
        {
            CustomCounter counter = new CustomCounter
            {
                SectionName = "PPHelper Counter",
                Name = "PP",
                BSIPAMod = PluginManager.EnabledPlugins.First(x => x.Name == Name),
                Counter = "PP Counter",
                Description = "Shows how much pp your current accuracy is worth on a ranked map",
                Icon_ResourceName = "PP_Helper.Assets.pp.png",
                CustomSettingsResource = "PP_Helper.Counters_.settings.bsml",
                CustomSettingsHandler = typeof(PPSettingsHandler)
            };
            CustomCounterCreator.Create(counter);
        }

        private void HarmonyPatchBase()
        {
            // RefreshContent
            var originalRefreshContent = typeof(StandardLevelDetailView).GetMethod("RefreshContent");
            HarmonyMethod harmonyRefreshContent = new HarmonyMethod(typeof(StandardLevelDetailViewRefreshContent).GetMethod("Postfix", (BindingFlags)(-1)));
            harmony.Patch(originalRefreshContent, postfix: harmonyRefreshContent);

            // RefreshTotalMultiplierAndRankUI
            var originalRefreshTotalMultiplierAndRankUI = typeof(GameplayModifiersPanelController).GetMethod("RefreshTotalMultiplierAndRankUI");
            HarmonyMethod harmonyRefreshTotalMultiplierAndRankUI = new HarmonyMethod(typeof(RefreshTotalMultiplierAndRankUIPatch).GetMethod("Postfix", (BindingFlags)(-1)));
            harmony.Patch(originalRefreshTotalMultiplierAndRankUI, postfix: harmonyRefreshTotalMultiplierAndRankUI);

            // GameplayModifiersPanelControllerAwake
            var originalGameplayModifiersPanelControllerAwake = typeof(GameplayModifiersPanelController).GetMethod("Awake");
            HarmonyMethod harmonyGameplayModifiersPanelControllerAwake = new HarmonyMethod(typeof(GameplayModifiersPanelControllerAwakePatch).GetMethod("Postfix", (BindingFlags)(-1)));
            harmony.Patch(originalGameplayModifiersPanelControllerAwake, postfix: harmonyGameplayModifiersPanelControllerAwake);
        }

        private void HarmonyPatchSongBrowser()
        {
            // CreateSortButtons
            var originalCreateSortButtons = typeof(SongBrowserUI).GetMethod("CreateSortButtons", (BindingFlags)(-1));
            HarmonyMethod harmonyCreateSortButtons = new HarmonyMethod(typeof(CreateSortButtonsPatch).GetMethod("Postfix", (BindingFlags)(-1)));
            harmony.Patch(originalCreateSortButtons, postfix: harmonyCreateSortButtons);

            // RefreshCurrentSelectionDisplay
            var originalRefreshCurrentSelectionDisplay = typeof(SongBrowserUI).GetMethod("RefreshCurrentSelectionDisplay", (BindingFlags)(-1));
            HarmonyMethod harmonyRefreshCurrentSelectionDisplay = new HarmonyMethod(typeof(RefreshCurrentSelectionDisplayPatch).GetMethod("Postfix", (BindingFlags)(-1)));
            harmony.Patch(originalRefreshCurrentSelectionDisplay, postfix: harmonyRefreshCurrentSelectionDisplay);

            // ProcessSongList
            var originalProcessSongList = typeof(SongBrowserModel).GetMethod("ProcessSongList", (BindingFlags)(-1));
            HarmonyMethod harmonyProcessSongList = new HarmonyMethod(typeof(ProcessSongListPatch).GetMethod("Prefix", (BindingFlags)(-1)));
            harmony.Patch(originalProcessSongList, prefix: harmonyProcessSongList);
        }

    }
}
