using Newtonsoft.Json;
using PP_Helper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using static PP_Helper.Data.ProfileDataLoader;

namespace PP_Helper.Data
{
    public class AccLoader : PersistentSingleton<AccLoader>
    {
        private static readonly string SPECIFIC_ACC_FILE = Path.Combine(Plugin.DIRECTORY, "SongSpecificAcc.json");
        private Dictionary<SongID, float> _songSpecificAcc = new Dictionary<SongID, float>();
        private Dictionary<double, double> _starAcc = new Dictionary<double, double>();

        public void Initialize()
        {
            Logger.log.Debug("Beginning initialization of acc loader");
            // Try to load from file, if not available - load from scoresaber
            try
            {
                _songSpecificAcc = JsonConvert.DeserializeObject<Dictionary<SongID, float>>(File.ReadAllText(SPECIFIC_ACC_FILE));
            }
            catch (FileNotFoundException)
            {
                Logger.log.Debug("No song-specific accuracy file");
            }
            catch (Exception e)
            {
                Logger.log.Error($"Error loading song specific accuracy: {e}");
            }
            LoadStarAcc();
        }

        public float GetAcc(SongID songID)
        {
            // First check for song specific accuracy
            if (_songSpecificAcc.ContainsKey(songID))
                return RoundedAcc(_songSpecificAcc[songID]);

            double bestAcc = 0;
            // Next check for max of current best acc and star acc
            if (ProfileDataLoader.instance.songDataInfo.ContainsKey(songID))
            {
                bestAcc = ProfileDataLoader.instance.songDataInfo[songID].acc;
            }

            // Next check for star acc
            var star = SongDataUtils.GetRoundedDownStars(songID);
            if (_starAcc.ContainsKey(star))
            {
                bestAcc = Math.Max(bestAcc, _starAcc[star]);
            }

            if (bestAcc > 0)
                return RoundedAcc(bestAcc);

            // Finally resort to default
            return Config.defaultAcc;
        }

        private static float RoundedAcc(double acc)
        {
            return (float) Math.Round(acc * 100, 2);
        }

        internal void LoadStarAcc()
        {
            try
            {
                Logger.log.Debug($"Getting star acc from {StarAccCalculator.FILE_NAME}");
                _starAcc = JsonConvert.DeserializeObject<Dictionary<double, double>>(File.ReadAllText(StarAccCalculator.FILE_NAME));
            }
            catch (FileNotFoundException)
            {
                Logger.log.Debug("No star acc file");
            }
            catch (Exception e)
            {
                Logger.log.Error($"Error loading star accuracy: {e}");
            }
        }
    }
}
