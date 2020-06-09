using HarmonyLib;

namespace PP_Helper.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayModifiersPanelController), "Awake")]
    class GameplayModifiersPanelControllerAwakePatch
    {
        static void Postfix()
        {
            PP_HelperController.instance.InitializeModifiers();
        }
    }
}
