using Newtonsoft.Json;
using PP_Helper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PP_Helper.JSON
{
    public static class ProfileDataLoader
    {
        public class ProfileData
        {
            public long scoreId;
            public long leaderboardId;
            public long score;
            public long uScore;
            public string mods;
            public string playerId;
            public string timeset;
            public double pp;
            public double weight;
            public string id;
            public string name;
            public string songSubName;
            public string songAuthorName;
            public string levelAuthorName;
            public string diff;
            public long maxScoreEx;
            public long rank;
        }

        public class SongPage
        {
            public List<ProfileData> scores;
        }

        public class SongData
        {
            public double pp;
            public double weight;

            public SongData(double pp, double weight)
            {
                this.pp = pp;
                this.weight = weight;
            }
        }

        [TypeConverter(typeof(SongIDConverter))]
        public class SongID
        {
            public string id;
            public BeatmapDifficulty difficulty;

            public SongID(string id, string difficulty)
            {
                this.id = id;
                this.difficulty = ScoreSaberUtils.GetDifficulty(difficulty);
            }

            public SongID(string id, BeatmapDifficulty difficulty)
            {
                this.id = id;
                this.difficulty = difficulty;
            }

            public override bool Equals(System.Object obj)
            {
                return this.id == ((SongID)obj).id && this.difficulty == ((SongID)obj).difficulty;
            }

            public override int GetHashCode()
            {
                return id.GetHashCode() + difficulty.GetHashCode();
            }
        }

        internal class SongIDConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string[] v = ((string)value).Split(',');
                    return new SongID(v[0], v[1]);
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ((SongID)value).id + "," + ScoreSaberUtils.GetDifficultyAsString(((SongID)value).difficulty);
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        private static string FILE_NAME = Path.Combine(Environment.CurrentDirectory, "UserData", "PP Helper", "ProfileData.json");

        public static Dictionary<SongID, SongData> songDataInfo = new Dictionary<SongID, SongData>();
        public static Dictionary<SongID, int> songIndex = new Dictionary<SongID, int>();
        public static List<SongID> songOrder = new List<SongID>();
        // index i is the sum of weighted pp plays #(i+1) .. #N
        public static List<double> ppTopBottomSum;
        // index i is the sum of weighted pp plays #(i+1) .. #1
        public static List<double> ppBottomUpSum;
        private static DateTime _lastUpdateTime = DateTime.MinValue;

        public static void Initialize()
        {
            Logger.log.Debug("Beginning initialization of profile data");
            // Try to load from file, if not available - load from scoresaber
            try
            {
                Logger.log.Debug("Opening file");
                songDataInfo = JsonConvert.DeserializeObject<Dictionary<SongID, SongData>>(File.ReadAllText(FILE_NAME));
                Logger.log.Debug("Loaded existing profile data");
                CalculateSums();
            }
            catch (FileNotFoundException)
            {
                LoadProfileData();
            } catch (Exception e)
            {
                Logger.log.Error(e.Message);
            }
        }
        public static void LoadProfileData()
        {
            if (_lastUpdateTime.AddMinutes(1) > DateTime.Now)
            {
                Logger.log.Info("Fetched data too soon, not refetching");
                return;
            }
            var user = BS_Utils.Gameplay.GetUserInfo.GetUserID();
            Logger.log.Info($"Fetching scoresaber data for user {user}");

            // Start downloading and wait for it to finish
            var profileDownloader = new GameObject("ProfileDownloader").AddComponent<ProfileDownloader>();
            profileDownloader.OnPageFinished += OnPageFinished;
            profileDownloader.OnProfileDataFinished += OnProfileDataFinished;
        }

        private static void OnPageFinished(int page)
        {
            Logger.log.Info($"Downloaded page {page}");
        }

        private static void OnProfileDataFinished(List<SongPage> pages)
        {
            Logger.log.Debug("Finished collecting data - processing now");
            _lastUpdateTime = DateTime.Now;
            // Update dict with all of the info
            foreach (SongPage songPage in pages)
            {
                foreach (ProfileData data in songPage.scores)
                {
                    string id = data.id;
                    double acc = (double) data.score / (double) data.maxScoreEx;
                    double pp = data.pp;
                    double weight = data.weight;
                    string diff = data.diff;
                    songDataInfo[new SongID(id, diff)] = new SongData(pp, weight);
                }
            }

            CalculateSums();
            SaveSongData();
        }

        // Go through _songDataInfo and calculate the pp sums
        private static void CalculateSums()
        {
            Logger.log.Debug("Beginning to calculate sums");
            // order by smallest weight to largest
            var songDataInfoList = songDataInfo.OrderBy(x => x.Value.weight).ToList();

            if (ppTopBottomSum == null)
                ppTopBottomSum = new List<double>(new double[songDataInfoList.Count]);
            if (ppBottomUpSum == null)
                ppBottomUpSum = new List<double>(new double[songDataInfoList.Count]);

            Logger.log.Debug("Created lists");
            // First calculate top-bottom sum
            double sum = 0;
            var i = songDataInfoList.Count - 1;
            foreach (var kvp in songDataInfoList)
            {
                var songData = kvp.Value;
                double ppValue = songData.weight * songData.pp;
                sum += ppValue;
                ppTopBottomSum[i] = sum;
                i--;
            }
            Logger.log.Debug("Calculated top-bottom");

            // Now calculate bottom-up sum and store songs in weighted order (largest -> smallest)
            sum = 0;
            i = 0; // should already be 0, but for clarity
            songDataInfoList.Reverse();
            foreach (var kvp in songDataInfoList)
            {
                songIndex[kvp.Key] = i;
                songOrder.Add(kvp.Key);
                var songData = kvp.Value;
                double ppValue = songData.weight * songData.pp;
                sum += ppValue;
                ppBottomUpSum[i] = sum;
                i++;
            }
            Logger.log.Debug("Calculated bottom-up");
        }

        private static void SaveSongData()
        {
            Logger.log.Debug("Saving data to file");
            File.WriteAllText(FILE_NAME, JsonConvert.SerializeObject(songDataInfo));
        }
    }
}
