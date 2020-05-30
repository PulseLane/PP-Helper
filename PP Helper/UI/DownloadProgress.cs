﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PP_Helper.Utils
{
    // Credit to SongCore for most of this code - https://github.com/Kylemc1413/SongCore/blob/5e2d9da3761d3ecf3763dc984d8baaca6de76d91/ProgressBar.cs
    public class DownloadProgress : MonoBehaviour
    {
        private static readonly Vector3 Position = new Vector3(-2.5f, 2.5f, 2.5f);
        private static readonly Vector3 Rotation = new Vector3(0, -60f, 0);
        private static readonly Vector3 Scale = new Vector3(0.01f, 0.01f, 0.01f);

        private static readonly Vector2 CanvasSize = new Vector2(100, 50);
        private Canvas _canvas;

        private static readonly Vector2 HeaderPosition = new Vector2(10, 15);
        private static readonly Vector2 HeaderSize = new Vector2(100, 20);
        private const string DownloadStartedText = "Fetching profile...";
        private const string ProgressText = "Fetched Page";
        private const string DownloadFinishedText = "Processed profile";
        private const float WaitTime = 2f;
        private const float ProgressFontSize = 15f;
        private TMP_Text _progressText;
        public static DownloadProgress instance { get; private set; }

        private void Awake()
        {
            gameObject.transform.position = Position;
            gameObject.transform.eulerAngles = Rotation;
            gameObject.transform.localScale = Scale;

            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.enabled = false;
            var rectTransform = _canvas.transform as RectTransform;
            rectTransform.sizeDelta = CanvasSize;
            instance = this;

            _progressText = CreateText(_canvas.transform as RectTransform, ProgressText, HeaderPosition);
            rectTransform = _progressText.transform as RectTransform;
            rectTransform.SetParent(_canvas.transform, false);
            rectTransform.anchoredPosition = HeaderPosition;
            rectTransform.sizeDelta = HeaderSize;
            _progressText.text = ProgressText;
            _progressText.fontSize = ProgressFontSize;
        }

        public void DownloadStart()
        {
            _canvas.enabled = true;
            _progressText.text = DownloadStartedText;
        }

        public void PageDownloaded(int pageNumber)
        {
            _progressText.text = ProgressText + $" {pageNumber}";
        }

        public void DownloadFinished()
        {
            _progressText.text = DownloadFinishedText;
            StartCoroutine(RemoveDownloadText());
        }

        IEnumerator RemoveDownloadText()
        {
            yield return new WaitForSeconds(WaitTime);
            _canvas.enabled = false;
        }

        public static TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition)
        {
            return CreateText(parent, text, anchoredPosition, new Vector2(60f, 10f));
        }

        public static TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject gameObj = new GameObject("CustomUIText");
            gameObj.SetActive(false);

            TextMeshProUGUI textMesh = gameObj.AddComponent<TextMeshProUGUI>();
            textMesh.font = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(t => t.name == "Teko-Medium SDF No Glow"));
            textMesh.rectTransform.SetParent(parent, false);
            textMesh.text = text;
            textMesh.fontSize = 4;
            textMesh.color = Color.white;

            textMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.sizeDelta = sizeDelta;
            textMesh.rectTransform.anchoredPosition = anchoredPosition;

            gameObj.SetActive(true);
            return textMesh;
        }
    }
}