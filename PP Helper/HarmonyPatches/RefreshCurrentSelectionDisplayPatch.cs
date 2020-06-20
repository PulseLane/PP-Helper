using BeatSaberMarkupLanguage;
using HarmonyLib;
using SongBrowser;
using SongBrowser.DataAccess;
using SongBrowser.UI;
using UnityEngine.UI;

namespace PP_Helper.HarmonyPatches
{
    // I'd rather not do this, but don't really have a choice...
    [HarmonyPatch(typeof(SongBrowserUI), "RefreshCurrentSelectionDisplay")]
    public class RefreshCurrentSelectionDisplayPatch
    {
        static void Postfix(ref SongBrowserUI __instance, ref Button ____sortByDisplay, ref SongBrowserModel ____model)
        {
            if (____model.Settings.sortMode == SongSortMode.Custom)
                ____sortByDisplay.SetButtonText("PP Helper");
        }
    }
}
