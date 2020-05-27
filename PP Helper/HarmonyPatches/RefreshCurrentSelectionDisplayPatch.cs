using HarmonyLib;
using PP_Helper.SongBrowser;
using SongBrowser;
using SongBrowser.Internals;
using SongBrowser.UI;
using UnityEngine.UI;

namespace PP_Helper.HarmonyPatches
{
    [HarmonyPatch(typeof(SongBrowserUI), "RefreshCurrentSelectionDisplay")]
    class RefreshCurrentSelectionDisplayPatch
    {
        static void Postfix(ref SongBrowserUI __instance, ref SongBrowserModel ____model, ref Button ____sortByDisplay, ref Button ____filterByDisplay)
        {
            string sortByDisplay = null;
            if (____model.Settings.sortMode == SongSorting.PossiblePPSortMode)
            {
                sortByDisplay = SongSorting.PossiblePPName;
            }
            else if (____model.Settings.sortMode == SongSorting.PPGainSortMode)
            {
                sortByDisplay = SongSorting.PPGainName;
            }
            else
            {
                return; // No need to overrwrite
            }

            ____sortByDisplay.SetButtonText(sortByDisplay);
        }
    }
}
