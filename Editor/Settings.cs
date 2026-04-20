using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// Project-level settings for BeamFlow App-Ads.txt Manager.
    /// Stored in EditorPrefs (per-machine, not version-controlled).
    /// </summary>
    public static class Settings
    {
        private const string KeyPrefix = "BeamFlow_AppAdsTxt_";

        public static string ApiKey
        {
            get => EditorPrefs.GetString(KeyPrefix + "ApiKey", "");
            set => EditorPrefs.SetString(KeyPrefix + "ApiKey", value);
        }

        public static string PublisherId
        {
            get => EditorPrefs.GetString(KeyPrefix + "PublisherId", "");
            set => EditorPrefs.SetString(KeyPrefix + "PublisherId", value);
        }

        public static string DeveloperWebsite
        {
            get => EditorPrefs.GetString(KeyPrefix + "DeveloperWebsite", "");
            set => EditorPrefs.SetString(KeyPrefix + "DeveloperWebsite", value);
        }

        public static string LastOutputPath
        {
            get => EditorPrefs.GetString(KeyPrefix + "LastOutputPath", "");
            set => EditorPrefs.SetString(KeyPrefix + "LastOutputPath", value);
        }

        /// <summary>
        /// Get publisher IDs for specific networks (stored as JSON-like key-value).
        /// </summary>
        public static string GetNetworkPublisherId(string networkId)
        {
            return EditorPrefs.GetString(KeyPrefix + "NetPubId_" + networkId, "");
        }

        public static void SetNetworkPublisherId(string networkId, string pubId)
        {
            EditorPrefs.SetString(KeyPrefix + "NetPubId_" + networkId, pubId);
        }

        /// <summary>
        /// Get all network publisher IDs as a dictionary.
        /// Falls back to the main PublisherId for AdMob.
        /// </summary>
        public static Dictionary<string, string> GetAllPublisherIds()
        {
            var ids = new Dictionary<string, string>();
            foreach (var networkId in NetworkTemplates.GetAllNetworkIds())
            {
                var id = GetNetworkPublisherId(networkId);
                if (string.IsNullOrEmpty(id) && networkId == "admob")
                    id = PublisherId; // Fallback to main publisher ID
                if (!string.IsNullOrEmpty(id))
                    ids[networkId] = id;
            }
            return ids;
        }
    }
}
