using BS_Utils.Utilities;
using PP_Helper.Data;
using PP_Helper.Utils;
using SongBrowser;
using SongBrowser.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PP_Helper.Data.ProfileDataLoader;

namespace PP_Helper.SongBrowser
{
    public static class SongSorting
    {
        // Hacky as all hell, but if it works...
        public static SongSortMode PPGainSortMode = SongSortMode.Favorites;
        public static SongSortMode PossiblePPSortMode = SongSortMode.Playlist;

        public static string PPGainName = "PPGain";
        public static string PossiblePPName = "PossiblePP";

        // Mostly copied from SongBrowserModel.ProcessSongList(object[] params)
        internal static void ProcessSongList(ref SongBrowserModel _model, IBeatmapLevelPack selectedLevelPack, LevelCollectionViewController levelCollectionViewController, LevelSelectionNavigationController navController)
        {
            List<IPreviewBeatmapLevel> unsortedSongs = null;
            List<IPreviewBeatmapLevel> filteredSongs = null;
            List<IPreviewBeatmapLevel> sortedSongs = null;

            // Abort
            if (selectedLevelPack == null)
            {
                Logger.log.Debug("Cannot process songs yet, no level pack selected...");
                return;
            }

            Logger.log.Debug($"Using songs from level pack: {selectedLevelPack.packID}");
            unsortedSongs = selectedLevelPack.beatmapLevelCollection.beatmapLevels.ToList();

            // filter
            Logger.log.Debug($"Starting filtering songs by {_model.Settings.filterMode}");
            Stopwatch stopwatch = Stopwatch.StartNew();

            MethodInfo method;
            switch (_model.Settings.filterMode)
            {
                case SongFilterMode.Favorites:
                    method = _model.GetType().GetMethod("FilterFavorites", BindingFlags.NonPublic | BindingFlags.Instance);
                    filteredSongs = (List<IPreviewBeatmapLevel>)method.Invoke(_model, new object[] { unsortedSongs });
                    break;
                case SongFilterMode.Search:
                    method = _model.GetType().GetMethod("FilterSearch", BindingFlags.NonPublic | BindingFlags.Instance);
                    filteredSongs = (List<IPreviewBeatmapLevel>)method.Invoke(_model, new object[] { unsortedSongs });
                    break;
                case SongFilterMode.Ranked:
                    method = _model.GetType().GetMethod("FilterRanked", BindingFlags.NonPublic | BindingFlags.Instance);
                    filteredSongs = (List<IPreviewBeatmapLevel>)method.Invoke(_model, new object[] { unsortedSongs, true, false });
                    break;
                case SongFilterMode.Unranked:
                    method = _model.GetType().GetMethod("FilterRanked", BindingFlags.NonPublic | BindingFlags.Instance);
                    filteredSongs = (List<IPreviewBeatmapLevel>)method.Invoke(_model, new object[] { unsortedSongs, false, true });
                    break;
                case SongFilterMode.Custom:
                    Logger.log.Info("Song filter mode set to custom. Deferring filter behaviour to another mod.");
                    filteredSongs = SongBrowserModel.CustomFilterHandler != null ? SongBrowserModel.CustomFilterHandler.Invoke(selectedLevelPack) : unsortedSongs;
                    break;
                case SongFilterMode.None:
                default:
                    Logger.log.Info("No song filter selected...");
                    filteredSongs = unsortedSongs;
                    break;
            }

            stopwatch.Stop();
            Logger.log.Info($"Filtering songs took {stopwatch.ElapsedMilliseconds}ms");

            // sort
            Logger.log.Debug("Starting to sort songs...");
            stopwatch = Stopwatch.StartNew();

            // PP Gain
            if (_model.Settings.sortMode == PPGainSortMode)
            {
                sortedSongs = SortPPGain(filteredSongs);
            }

            // Possible PP
            else if (_model.Settings.sortMode == PossiblePPSortMode)
            {
                sortedSongs = SortPossiblePP(filteredSongs);
            }

            else
            {
                Logger.log.Error("Should not be here");
            }

            if (_model.Settings.invertSortResults)
                sortedSongs.Reverse();

            stopwatch.Stop();
            Logger.log.Info($"Sorting songs took {stopwatch.ElapsedMilliseconds}ms");

            var packName = selectedLevelPack.packName;
            if (!packName.EndsWith("*") && _model.Settings.filterMode != SongFilterMode.None)
            {
                packName += "*";
            }
            BeatmapLevelPack levelPack = new BeatmapLevelPack(SongBrowserModel.FilteredSongsPackId, packName, selectedLevelPack.shortPackName, selectedLevelPack.coverImage, new BeatmapLevelCollection(sortedSongs.ToArray()));

            GameObject _noDataGO = levelCollectionViewController.GetPrivateField<GameObject>("_noDataInfoGO");
            //string _headerText = tableView.GetPrivateField<string>("_headerText");
            //Sprite _headerSprite = tableView.GetPrivateField<Sprite>("_headerSprite");

            bool _showPlayerStatsInDetailView = navController.GetPrivateField<bool>("_showPlayerStatsInDetailView");
            bool _showPracticeButtonInDetailView = navController.GetPrivateField<bool>("_showPracticeButtonInDetailView");

            navController.SetData(levelPack, true, _showPlayerStatsInDetailView, _showPracticeButtonInDetailView, _noDataGO);
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
    }
}
