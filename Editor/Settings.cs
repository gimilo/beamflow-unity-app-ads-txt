using System;
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
        /// Get cached managed lines from BeamFlow (survives offline).
        /// Stored as newline-separated string in EditorPrefs.
        /// </summary>
        public static List<string> GetCachedManagedLines()
        {
            var raw = EditorPrefs.GetString(KeyPrefix + "ManagedLinesCache", "");
            if (string.IsNullOrEmpty(raw)) return new List<string>();
            return new List<string>(raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Cache managed lines from the BeamFlow API.
        /// </summary>
        public static void SetCachedManagedLines(List<string> lines)
        {
            EditorPrefs.SetString(KeyPrefix + "ManagedLinesCache", string.Join("\n", lines));
            EditorPrefs.SetInt(KeyPrefix + "ManagedLinesCacheTs", (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        /// <summary>
        /// Get the set of managed lines the user has explicitly excluded.
        /// </summary>
        public static HashSet<string> GetExcludedManagedLines()
        {
            var raw = EditorPrefs.GetString(KeyPrefix + "ExcludedManagedLines", "");
            if (string.IsNullOrEmpty(raw)) return new HashSet<string>();
            return new HashSet<string>(raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Save the set of excluded managed lines.
        /// </summary>
        public static void SetExcludedManagedLines(IEnumerable<string> lines)
        {
            EditorPrefs.SetString(KeyPrefix + "ExcludedManagedLines", string.Join("\n", lines));
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
