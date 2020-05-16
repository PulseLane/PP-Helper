using BS_Utils.Utilities;
using PP_Helper.JSON;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace PP_Helper
{
    class PPLevelSelectDisplay
    {
        protected static StandardLevelDetailView _standardLevelDetailView;
        protected static GameObject _playerStatsContainer;
        private readonly RectTransform _maxCombo;
        private readonly RectTransform _highscore;
        private readonly RectTransform _maxRank;
        protected static RectTransform _pp;

        public PPLevelSelectDisplay()
        {
            Logger.log.Info("Initializing pp display");
            var soloFreePlayFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().FirstOrDefault();
            var levelSelectionNavController = soloFreePlayFlowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            var levelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");

            _standardLevelDetailView = levelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            _playerStatsContainer = _standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");

            _maxCombo = _playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
            _highscore = _playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
            _maxRank = _playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");

            Logger.log.Info("Finished initializing pp display");
        }

        public void showPP(string id)
        {
            if (_pp == null)
            {
                // Setup PP display object
                _pp = UnityEngine.Object.Instantiate(_maxCombo, _playerStatsContainer.transform);
                _pp.name = "PP";
                var ppTitle = _pp.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
                ppTitle.SetText("PP");

                // Just copying this from SongPlayHistory - need to look into a smarter way to this (and also compatibility??)
                _maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -2.0f, 17.0f);
                Logger.log.Info("maxcombo");
                _highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 15.0f, 17.0f);
                _maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 32.0f, 16.0f);
                _pp.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 48.0f, 16.0f);
            }

            var ppValue = _pp.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
            if (RawPPLoader.InDict(id))
            {
                // Will fail if difficulty is unknown - set to default in '-' in that case
                try
                {
                    IDifficultyBeatmap difficultyBeatmap = _standardLevelDetailView.selectedDifficultyBeatmap;
                    float rawpp = RawPPLoader.GetRawPP(id, difficultyBeatmap);
                    ppValue.SetText(rawpp.ToString());
                }
                catch (Exception)
                {
                    ppValue.SetText("-");
                }
            }
            else
                ppValue.SetText("-");
        }

        public void hidePP()
        {
            if (_pp != null)
            {
                UnityEngine.Object.Destroy(_pp.gameObject);
                _pp = null;

                _maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 23.4f);
                _highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 23.4f, 23.3f);
                _maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 46.7f, 23.3f);
            }
        }
    }
}
