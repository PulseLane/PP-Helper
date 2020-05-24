using System;
using System.Collections.Generic;
using System.IO;
using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using PP_Helper.JSON;
using PP_Helper.UI;
using PP_Helper.Utils;
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
                harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
            } catch (Exception e)
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
            BSMLSettings.instance.AddSettingsMenu("PP Helper", "PP_Helper.UI.settings.bsml", Settings.instance);

            PP_HelperMenuUI.CreateUI();
        }

        public void OnMenuSceneLoadedFresh(ScenesTransitionSetupDataSO transitionSetupDataSO)
        {
            PP_HelperController.OnLoad();
            if (!RawPPLoader.IsInit())
                RawPPLoader.Initialize();
            PPUtils.Initialize();
            ProfileDataLoader.instance.Initialize();
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
            BSMLSettings.instance.RemoveSettingsMenu(Settings.instance);
            harmony.UnpatchAll("com.PulseLane.BeatSaber.PP_Helper");
        }
    }
}
