using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using static PP_Helper.Data.ProfileDataLoader;

namespace PP_Helper
{
    public class ProfileDownloader : MonoBehaviour
    {
        private const string URI_PREFIX = "https://new.scoresaber.com/api/player/";
        private const int MAX_PAGE = 50;
        private const double EPSILON = 0.001; // pp gets rounded to 2 decimal places anyway
        private const float WAIT_TIME_SECONDS = 0.5f;
        private List<SongPage> pages;
        public Action<int> OnPageFinished;
        public Action<List<SongPage>> OnProfileDataFinished;

        private void Start()
        {
            pages = new List<SongPage>();
            StartCoroutine(GetProfileData());
        }

        IEnumerator GetProfileData()
        {
            int page = 1;
            ulong id = BS_Utils.Gameplay.GetUserInfo.GetUserID();
            while (page <= MAX_PAGE && !IsFinished())
            {
                yield return StartCoroutine(GetProfilePage(id, page));

                OnPageFinished?.Invoke(page);
                page++;
                yield return new WaitForSeconds(WAIT_TIME_SECONDS);
            }
            OnProfileDataFinished?.Invoke(pages);
        }

        IEnumerator GetProfilePage(ulong id, int page)
        {
            string uri = URI_PREFIX + id + "/scores/top/" + page.ToString();
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
                if (webRequest.isNetworkError)
                {
                    throw new WebException();
                }
                else
                {
                    pages.Add(JsonConvert.DeserializeObject<SongPage>(webRequest.downloadHandler.text));
                }
            }
        }

        private bool IsFinished()
        {
            if (pages.Count == 0)
                return false;
            // Previous page was empty or contained 0pp play
            var page = pages.Last();
            if (page.scores.Count == 0)
                return true;
            // Not sure if JSON deserialization goes in order - just check all 8 songs
            foreach (ProfileData song in page.scores)
            {
                if (song.pp <= EPSILON)
                    return true;
            }

            return false;
        }
    }
}
