using Newtonsoft.Json;
using PP_Helper.Utils;
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
        private const double EPSILON = 0.001; // pp gets rounded to 2 decimal places anyway
        private List<SongPage> pages;
        public Action<int> OnPageFinished;
        public Action<List<SongPage>> OnProfileDataFinished;
        public Action OnErrorDownloading;

        private void Start()
        {
            pages = new List<SongPage>();
            StartCoroutine(GetProfileData());
        }

        IEnumerator GetProfileData()
        {
            int page = 1;
            ulong id = BS_Utils.Gameplay.GetUserInfo.GetUserID();
            while (!IsFinished())
            {
                yield return StartCoroutine(GetProfilePage(id, page));

                OnPageFinished?.Invoke(page);
                page++;
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
                    OnErrorDownloading?.Invoke();
                    throw new WebException();
                }
                else
                {
                    // TODO: modifiers
                    pages.Add(JsonConvert.DeserializeObject<SongPage>(webRequest.downloadHandler.text));
                    var responseHeaders = webRequest.GetResponseHeaders();
                    var remaining = responseHeaders["x-ratelimit-remaining"];
                    Logger.log.Debug($"Rate limit remaining: {remaining}");
                    if (responseHeaders["x-ratelimit-remaining"].Equals("0"))
                    {
                        var resetTime = UnixTimeStampToDateTime(Convert.ToInt64(responseHeaders["x-ratelimit-reset"]) + 1);
                        Logger.log.Debug($"Hit rate limit, waiting until {resetTime}");
                        DownloadProgress.instance.WaitUntil(resetTime.ToLocalTime());
                        yield return new WaitUntil(() => DateTime.UtcNow > resetTime);
                    }
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

        private static DateTime UnixTimeStampToDateTime(long timeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timeStamp).DateTime;
        }
    }
}
