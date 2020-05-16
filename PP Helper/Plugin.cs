using System;
using System.Collections.Generic;
using HarmonyLib;
using IPA;
using PP_Helper.JSON;
using PP_Helper.Utils;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace PP_Helper
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
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
            harmony = new Harmony("com.PulseLane.BeatSaber.PP_Helper");
            try
            {
                harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
            } catch (Exception ex)
            {
                Logger.log.Error($"Failed to apply harmony patches! {ex}");
            }

            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.levelSelected += OnLevelSelected;
        }

        public void OnMenuSceneLoadedFresh(ScenesTransitionSetupDataSO transitionSetupDataSO)
        {

            new GameObject("PP_HelperController").AddComponent<PP_HelperController>();
            RawPPLoader.Initialize();
        }

        public void OnLevelSelected(LevelCollectionViewController levelCollectionViewController, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            PP_HelperController.instance.setId(SongDataUtils.getHash(previewBeatmapLevel.levelID));
        }

        [OnDisable]
        public void OnDisable()
        {
            if (PluginController != null)
                GameObject.Destroy(PluginController);
            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh -= OnMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.levelSelected -= OnLevelSelected;
            harmony.UnpatchAll("com.PulseLane.BeatSaber.PP_Helper");
        }
    }
}
