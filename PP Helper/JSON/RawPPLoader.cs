using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace PP_Helper.JSON
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
            // Use a file for now - offload to something else, likely s3, later
            using (StreamReader file = File.OpenText(RAW_PP_FILE))
            {
                JsonSerializer serializer = new JsonSerializer();
                _songData = (Dictionary<string, RawPPData>)serializer.Deserialize(file, typeof(Dictionary<string, RawPPData>));
            }
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

        public static float GetRawPP(string hash, IDifficultyBeatmap difficultyBeatmap)
        {
            // Should never happen, but why not
            if (!init)
            {
                Logger.log.Error("Tried to use RawPPLoader when it wasn't initialized!");
                throw new Exception("Tried to use RawPPLoader when it wasn't initialized");
            }

            BeatmapDifficulty difficulty = difficultyBeatmap.difficulty;
            // Will need to check more than just difficulty if non-standard song is ever ranked
            switch (difficulty)
            {
                case BeatmapDifficulty.Easy:
                    return _songData[hash]._Easy_SoloStandard;
                case BeatmapDifficulty.Normal:
                    return _songData[hash]._Normal_SoloStandard;
                case BeatmapDifficulty.Hard:
                    return _songData[hash]._Hard_SoloStandard;
                case BeatmapDifficulty.Expert:
                    return _songData[hash]._Expert_SoloStandard;
                case BeatmapDifficulty.ExpertPlus:
                    return _songData[hash]._ExpertPlus_SoloStandard;
                default:
                    Logger.log.Error("Unknown beatmap difficulty: " + difficulty.ToString());
                    throw new Exception("Unknown difficultry");
            }
        }
    }
}
