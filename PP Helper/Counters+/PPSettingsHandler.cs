using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PP_Helper.Counters
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
	public class PPSettingsHandler : MonoBehaviour
    {
        [UIValue("ignoreNoFail")]
        public bool ignoreNoFail
        {
            get => Config.ignoreNoFail;
            set
            {
                Config.ignoreNoFail = value;
                Config.Write();
            }
        }

        [UIValue("hideOnStart")]
        public bool hideOnStart
        {
            get => Config.hideOnStart;
            set
            {
                Config.hideOnStart = value;
                Config.Write();
            }
        }

        [UIValue("decimalPrecision")]
        public int decimalPrecision
        {
            get => Config.decimalPrecision;
            set
            {
                Config.decimalPrecision = value;
                Config.Write();
            }
        }
    }
}
