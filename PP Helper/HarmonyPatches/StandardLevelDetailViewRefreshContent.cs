using HarmonyLib;

namespace PP_Helper.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelDetailView), "RefreshContent")]
    class StandardLevelDetailViewRefreshContent
    {
        static void Postfix()
        {
            PP_HelperController.instance.Refresh();
        }
    }
}
