using HarmonyLib;

namespace PP_Helper.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayModifiersPanelController), "RefreshTotalMultiplierAndRankUI")]
    class RefreshTotalMultiplierAndRankUIPatch
    {
        static void Postfix()
        {
            PP_HelperController.instance.UpdateModifiers();
        }
    }
}
