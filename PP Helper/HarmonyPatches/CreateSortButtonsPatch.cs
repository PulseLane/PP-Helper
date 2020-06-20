using HarmonyLib;
using PP_Helper.SongBrowser;
using SongBrowser.DataAccess;
using SongBrowser.Internals;
using SongBrowser.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PP_Helper.HarmonyPatches
{
    [HarmonyPatch(typeof(SongBrowserUI), "CreateSortButtons")]
    class CreateSortButtonsPatch
    {
        static void Postfix(ref SongBrowserUI __instance, ref List<SongSortButton> ____sortButtonGroup, ref BeatSaberUIController ____beatUi)
        {
            SongSorting.Init();
        }
    }
}
