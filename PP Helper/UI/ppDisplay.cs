using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BS_Utils.Utilities;
using PP_Helper.JSON;
using PP_Helper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PP_Helper.UI
{
    public class ppDisplay : PersistentSingleton<ppDisplay>
    {
        [UIComponent("pp")]
        public TextMeshProUGUI _ppText;
        [UIObject("accuracy")]
        public GameObject _accuracyObject;
        [UIValue("accuracyValue")]
        public float _accuracy;

        private StandardLevelDetailView _standardLevelDetailView;
        private static GameObject _parentObject;

        internal void Setup()
        {
            Logger.log.Debug("Setup for UI");
            var soloFreePlayFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().FirstOrDefault();
            var levelSelectionNavController = soloFreePlayFlowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            var levelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            _standardLevelDetailView = levelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            var playerStatsContainer = _standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");

            // Add PP Helper UI
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PP_Helper.UI.ppDisplay.bsml"), playerStatsContainer.transform.parent.gameObject, this);
            _parentObject = _ppText.transform.parent.gameObject;
            
            // Resize accuracy font size
            var _accText = _accuracyObject.GetComponentsInChildren<TextMeshProUGUI>().Last();
            _accText.fontSize = 3.5f;
            _accuracy = 85f;

            // Resize accuracy adjuster
            var accTransform = (RectTransform) _accText.transform.parent.transform.parent.transform;
            accTransform.sizeDelta = new Vector2(30f, 1f);
            Logger.log.Debug("Done setup");

            // We want pp stuff above play stuff
            var playButton = _standardLevelDetailView.GetPrivateField<Button>("_playButton");
            var playContainer = playButton.transform.parent.transform.parent;
            playContainer.transform.SetAsLastSibling();
        }

        internal void Refresh(string id)
        {
            _accuracy = 85.0f;

            if (RawPPLoader.InDict(id))
            {
                // Will fail if difficulty is unknown - set to default in '-' in that case
                try
                {
                    _parentObject.SetActive(true);
                    IDifficultyBeatmap difficultyBeatmap = _standardLevelDetailView.selectedDifficultyBeatmap;
                    float rawPP = RawPPLoader.GetRawPP(id, difficultyBeatmap);
                    float ppGain = rawPP;
                    _ppText.text = ($"{rawPP}pp (<color=\"green\">+{ppGain}</color>)");
                    SetAccText();
                }
                catch (Exception)
                {
                    _ppText.SetText("-");
                }
            }
            else
            {
                _parentObject.SetActive(false);
                _ppText.SetText("-");
            }
        }

        [UIAction("accFormat")]
        public string AccFormat(float value)
        {
            return value.ToString() + "%";
        }

        private void SetAccText()
        {
            // Will fetch acc from saved accs
        }
    }
}
