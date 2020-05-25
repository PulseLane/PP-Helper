using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using IPA.Loader;
using PP_Helper.Data;
using PP_Helper.UI;
using PP_Helper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using CountersPlus.Custom;
using PP_Helper.Counters;

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
            new GameObject("PP Counter").AddComponent<PPCounter>();
        }

        [OnDisable]
        public void OnDisable()
        {
            if (PluginController != null)
                GameObject.Destroy(PluginController);

            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh -= OnMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.levelSelected -= OnLevelSelected;
            BS_Utils.Utilities.BSEvents.gameSceneActive -= OnGameSceneActive;

            BSMLSettings.instance.RemoveSettingsMenu(Settings.instance);
            harmony.UnpatchAll("com.PulseLane.BeatSaber.PP_Helper");
            // TODO: Clean up singletones
            // TODO: remove counter
        }

        private void AddPPCounter()
        {
            CustomCounter counter = new CustomCounter
            {
                SectionName = "PPHelperCounter",
                Name = "PP",
                BSIPAMod = PluginManager.EnabledPlugins.First(x => x.Name == Name),
                Counter = "PP Counter",
                Description = "Shows how much pp your current accuracy is worth on a ranked map"
            };
            CustomCounterCreator.Create(counter);
        }

    }
}
