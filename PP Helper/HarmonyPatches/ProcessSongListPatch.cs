using HarmonyLib;
using PP_Helper.SongBrowser;
using SongBrowser;

namespace PP_Helper.HarmonyPatches
{
    [HarmonyPatch(typeof(SongBrowserModel), "ProcessSongList")]
    class ProcessSongListPatch
    {
        static bool Prefix(IBeatmapLevelPack selectedLevelPack, LevelCollectionViewController levelCollectionViewController, LevelSelectionNavigationController navController, ref SongBrowserModel __instance)
        {
            if (__instance.Settings.sortMode != SongSorting.PPGainSortMode && __instance.Settings.sortMode != SongSorting.PossiblePPSortMode)
                return true; // Not our sort mode, execute original

            SongSorting.ProcessSongList(ref __instance, selectedLevelPack, levelCollectionViewController, navController);
            return false;
        }
    }
}
