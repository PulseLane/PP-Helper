using BS_Utils.Utilities;
using JetBrains.Annotations;
using PP_Helper.UI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PP_Helper
{
    public class PP_HelperController : MonoBehaviour
    {
        public static PP_HelperController instance { get; private set; }
        private string _id;

        private void Awake()
        {
            if (instance != null)
            {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Logger.log?.Debug($"{name}: Awake()");
        }

        public static void OnLoad()
        {
            if (instance == null)
                new GameObject("PP_HelperController").AddComponent<PP_HelperController>();
            PPDisplay.instance.Setup();
        }

        public void Refresh()
        {
            PPDisplay.instance.Refresh(_id);
        }

        public void setId(string id)
        {
            _id = id;
        }

        public void InitializeModifiers()
        {
            PPDisplay.instance.OnFirstLoad();
        }

        public void UpdateModifiers()
        {
            PPDisplay.instance.Refresh(_id);
        }

        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
        }
    }
}
