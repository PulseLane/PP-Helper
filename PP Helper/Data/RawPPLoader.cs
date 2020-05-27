using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static PP_Helper.Data.ProfileDataLoader;

namespace PP_Helper.Data
{
    // All diffs are solostandard, but just in case some 360/90/one-handed/etc. gets ranked at some point
    public class RawPPData
    {
        public float _Easy_SoloStandard { get; set; }
        public float _Normal_SoloStandard { get; set; }
        public float _Hard_SoloStandard { get; set; }
        public float _Expert_SoloStandard { get; set; }
        public float _ExpertPlus_SoloStandard { get; set; }
    }

    static class RawPPLoader
    {
        private static bool init = false;
        // temporary
        private static string RAW_PP_FILE = Environment.CurrentDirectory + "/UserData/PP Helper/raw_pp.json";
        private static Dictionary<string, RawPPData> _songData;

        // Load up the Raw PP data
        public static void Initialize()
        {
            Logger.log.Debug("Starting raw pp data initialization");
            var ppDownloader = new GameObject("RawPPDownloader").AddComponent<RawPPDownloader>();
            ppDownloader.OnDataDownloaded += OnDataDownloaded;
            ppDownloader.StartDownloading();
        }

        private static void OnDataDownloaded(Dictionary<string, RawPPData> songData)
        {
            _songData = songData;
            Logger.log.Debug("Loaded raw pp data");
            init = true;
        }

        public static bool IsInit()
        {
            return init;
        }

        public static bool InDict(string hash)
        {
            // Just in case
            if (!init)
            {
                Logger.log.Error("Tried to use RawPPLoader when it wasn't initialized!");
                throw new Exception("Tried to use RawPPLoader when it wasn't initialized");
            }

            return _songData.ContainsKey(hash);
        }

        public static float GetRawPP(SongID songID)
        {
            if (!init)
            {
                Logger.log.Error("Tried to use RawPPLoader when it wasn't initialized!");
                throw new Exception("Tried to use RawPPLoader when it wasn't initialized");
            }

            switch (songID.difficulty)
            {
                case BeatmapDifficulty.Easy:
                    return _songData[songID.id]._Easy_SoloStandard;
                case BeatmapDifficulty.Normal:
                    return _songData[songID.id]._Normal_SoloStandard;
                case BeatmapDifficulty.Hard:
                    return _songData[songID.id]._Hard_SoloStandard;
                case BeatmapDifficulty.Expert:
                    return _songData[songID.id]._Expert_SoloStandard;
                case BeatmapDifficulty.ExpertPlus:
                    return _songData[songID.id]._ExpertPlus_SoloStandard;
                default:
                    Logger.log.Error("Unknown beatmap difficulty: " + songID.difficulty.ToString());
                    throw new Exception("Unknown difficultry");
            }
        }
    }
}
