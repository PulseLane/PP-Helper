using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PP_Helper
{
    public class PP_HelperController : MonoBehaviour
    {
        public static PP_HelperController instance { get; private set; }
        private static PPLevelSelectDisplay _ppLevelSelectDisplay;
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

            var soloFreePlayButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "SoloFreePlayButton");
            soloFreePlayButton.onClick.AddListener(() =>
            {
                Initialize();
            });
        }

        private static void Initialize()
        {
            _ppLevelSelectDisplay = new PPLevelSelectDisplay();
        }

        public void Refresh()
        {
            _ppLevelSelectDisplay.showPP(_id);
        }

        public void setId(string id)
        {
            _id = id;
        }

        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
        }
    }
}
