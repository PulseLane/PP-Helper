using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PP_Helper.Data.ProfileDataLoader;

namespace PP_Helper.Data
{
    public static class PlayHistoryTracker
    {
        private static readonly string FILE_NAME = Path.Combine(Plugin.DIRECTORY, "PlayHistory.json");

        private static Dictionary<SongID, double> playHistory;

        public static void Initialize()
        {
            // Already initialized
            if (playHistory != null)
                return;

            try
            {
                Logger.log.Debug("Opening play history");
                playHistory = JsonConvert.DeserializeObject<Dictionary<SongID, double>>(File.ReadAllText(FILE_NAME));
                Logger.log.Debug("Loaded play history");
                WaitForPPLoaderInit();
            }
            catch (FileNotFoundException)
            {
                Logger.log.Debug("No previous play history");
            }
            catch (Exception e)
            {
                Logger.log.Error(e.Message);
            }
        }

        private static IEnumerator WaitForPPLoaderInit()
        {
            yield return new WaitUntil(RawPPLoader.IsInit);
            CheckForRanked();
        }

        private static void CheckForRanked()
        {
            Logger.log.Debug("Comparing play history to ranked songs");
            HashSet<SongID> toDelete = new HashSet<SongID>();
            foreach (KeyValuePair<SongID, double> songPlay in playHistory)
            {
                if (RawPPLoader.InDict(songPlay.Key.id))
                {
                    ProfileDataLoader.instance.SubmitPlay(songPlay.Key, songPlay.Value);
                    toDelete.Add(songPlay.Key);
                }
            }

            foreach (SongID songID in toDelete)
            {
                playHistory.Remove(songID);
            }

            WriteToPlayHistory();
        }

        public static void UpdatePlayHistory(SongID songID, double acc)
        {
            Logger.log.Debug($"Updating play history w/ {acc} on {songID}");
            if (!playHistory.ContainsKey(songID) || acc > playHistory[songID])
            {
                playHistory[songID] = acc;
                WriteToPlayHistory();
            }
        }

        private static void WriteToPlayHistory()
        {
            Logger.log.Debug("Saving play history to file");
            File.WriteAllText(FILE_NAME, JsonConvert.SerializeObject(playHistory));
        }
    }
}
