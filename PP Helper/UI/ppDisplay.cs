using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BS_Utils.Utilities;
using PP_Helper.Data;
using PP_Helper.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PP_Helper.UI
{
    public class PPDisplay : PersistentSingleton<PPDisplay>
    {
        [UIParams]
        private BSMLParserParams _parserParams;

        [UIComponent("pp")]
        private TextMeshProUGUI _ppText;
        [UIObject("accuracy")]
        private GameObject _accuracyObject;
        [UIValue("accuracyValue")]
        private float _accuracy = 85f;
        [UIValue("accIncrement")]
        public float accIncrement = Config.accIncrement;

        private StandardLevelDetailView _standardLevelDetailView;
        private static GameObject _parentObject;
        private float _rawPP = 0f;
        private ProfileDataLoader.SongID _id;

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

            // Resize accuracy adjuster
            var accTransform = (RectTransform)_accText.transform.parent.transform.parent.transform;
            accTransform.sizeDelta = new Vector2(30f, 1f);

            // We want pp stuff above play stuff
            var playButton = _standardLevelDetailView.GetPrivateField<Button>("_playButton");
            var playContainer = playButton.transform.parent.transform.parent;
            playContainer.transform.SetAsLastSibling();

            Logger.log.Debug("Done setup");
        }

        internal void Refresh(string id)
        {
            try
            {
                IDifficultyBeatmap difficultyBeatmap = _standardLevelDetailView.selectedDifficultyBeatmap;
                _id = new ProfileDataLoader.SongID(id, difficultyBeatmap.difficulty);
                if (SongDataUtils.IsRankedSong(_id))
                {
                    _parentObject.SetActive(true);
                    _rawPP = SongDataUtils.GetRawPP(_id);
                    LoadAcc();
                    SetPPText(PPUtils.CalculatePP(_rawPP, _accuracy));
                }
                else
                {
                    _parentObject.SetActive(false);
                }
            }
            catch (Exception)
            {
                Logger.log.Debug($"error with difficulty for song {id}");
                _parentObject.SetActive(false);
            }
        }

        [UIAction("accFormat")]
        public string AccFormat(float value)
        {
            return Math.Round(value, 2).ToString() + "%";
        }

        [UIAction("changedAcc")]
        private void ChangedAcc(float value)
        {
            value = (float)Math.Round(value, 2);
            _accuracy = value;
            SetPPText(PPUtils.CalculatePP(_rawPP, value));
        }

        [UIAction("saveButtonPressed")]
        private void SaveButtonPressed()
        {
            Logger.log.Debug($"Saving song-specific accuracy for {_id}: {_accuracy}");
            AccLoader.instance.SaveAcc(_id, _accuracy);
        }

        [UIAction("loadButtonPressed")]
        private void LoadButtonPressed()
        {
            Logger.log.Debug($"Loading star accuracy for {_id}");
            AccLoader.instance.ClearAcc(_id);
            LoadAcc();
        }

        private void SetPPText(float ppValue)
        {
            string ppText = Math.Round(ppValue, 2).ToString("0.00");
            var ppGain = Math.Round(PPUtils.GetPPGain(ppValue, _id), 2);
            string ppGainText = ppGain.ToString("0.00");
            var color = ppGain > 0 ? "green" : "red";
            _ppText.SetText($"{ppText} (<color=\"{color}\">+{ppGain}</color>)");
        }


        private void LoadAcc()
        {
            _accuracy = AccLoader.instance.GetAcc(_id);
            _parserParams.EmitEvent("setAcc");
        }
    }
}
