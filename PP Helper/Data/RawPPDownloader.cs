using Newtonsoft.Json;
using PP_Helper.Data;
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
    public class RawPPDownloader : MonoBehaviour
    {
        private const string URI_PREFIX = "https://cdn.pulselane.dev/";
        private const string FILE_NAME = "raw_pp.json";
        public Action<Dictionary<string, RawPPData>> OnDataDownloaded;

        public void StartDownloading()
        {
            StartCoroutine(GetRawPP());
        }

        IEnumerator GetRawPP()
        {
            string uri = URI_PREFIX + FILE_NAME;
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
                    OnDataDownloaded?.Invoke(JsonConvert.DeserializeObject<Dictionary<string, RawPPData>>(webRequest.downloadHandler.text));
                }
            }
        }
    }
}
