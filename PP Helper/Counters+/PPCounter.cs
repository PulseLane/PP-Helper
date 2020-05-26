using BeatSaberMarkupLanguage;
using PP_Helper.Data;
using PP_Helper.Utils;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static PP_Helper.Data.ProfileDataLoader;

namespace PP_Helper.Counters
{
	public class PPCounter : MonoBehaviour
    {
        private TextMeshProUGUI _counter;
        private int _totalNotes;
        private int _score;
        private IDifficultyBeatmap _difficultyBeatmap;

        private BeatmapObjectManager _beatmapObjectManager;
        private ScoreController _scoreController;
        private SongID _songID;

        // Mostly just copying from Deviation Counter
        void Start()
        {
            StartCoroutine(FindBeatMapObjectManager());
            StartCoroutine(FindScoreController());
            StartCoroutine(FindDifficultyBeatmap());

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            CanvasScaler cs = gameObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 10.0f;
            cs.dynamicPixelsPerUnit = 10f;
            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1f);
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1f);

            _counter = BeatSaberUI.CreateText(canvas.transform as RectTransform, $"", Vector2.zero);
            _counter.alignment = TextAlignmentOptions.Center;
            _counter.fontSize = 3f;
            _counter.color = Color.white;
            _counter.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1f);
            _counter.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1f);
            _counter.enableWordWrapping = false;

            _totalNotes = 0;
            _score = 0;
        }

        IEnumerator FindBeatMapObjectManager()
        {
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().Any());

            _beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().First();
        }

        IEnumerator FindDifficultyBeatmap()
        {
            yield return new WaitUntil(() => BS_Utils.Plugin.LevelData.IsSet);

            _difficultyBeatmap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;
            _songID = new SongID(SongDataUtils.GetHash(_difficultyBeatmap.level.levelID), _difficultyBeatmap.difficulty);

            // Only update for ranked songs
            if (SongDataUtils.IsRankedSong(_songID))
            {
                yield return new WaitUntil(() => _beatmapObjectManager != null);
                _beatmapObjectManager.noteWasCutEvent += OnNoteCut;
                _beatmapObjectManager.noteWasMissedEvent += OnNoteMissed;

                yield return new WaitUntil(() => _scoreController != null);
                _scoreController.scoreDidChangeEvent += OnScoreChange;
                UpdateCounter();
            }
        }

        IEnumerator FindScoreController()
        {
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<ScoreController>().Any());

            _scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().First();
        }

        private void OnScoreChange(int scoreBeforeMultiplier, int scoreAfterMultiplier)
        {
            _score = scoreBeforeMultiplier;
            UpdateCounter();
        }

        private void OnNoteMissed(INoteController data)
        {
            if (data.noteData.noteType != NoteType.Bomb) _totalNotes--;
            UpdateCounter();
        }

        private void OnNoteCut(INoteController data, NoteCutInfo info)
        {
            if (data.noteData.noteType != NoteType.Bomb) _totalNotes++;
            UpdateCounter();
        }

        private void UpdateCounter()
        {
            var maxScore = ScoreModel.MaxRawScoreForNumberOfNotes(_totalNotes);
            float acc;
            if (maxScore == 0)
                acc = 1;
            else
                acc = (float) _score / (float) maxScore;

            var rawPP = RawPPLoader.GetRawPP(_songID.id, _difficultyBeatmap);
            var pp = PPUtils.CalculatePP(rawPP, acc * 100);
            _counter.text = $"{Math.Round(pp, 2)}pp";
        }
    }
}
