using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// HTTP client for BeamFlow API.
    /// All methods are async and return null on failure (offline-first).
    /// </summary>
    public static class BeamFlowApi
    {
        private const string BaseUrl = "https://beamflow.co/api/v1";
        private const int TimeoutSeconds = 10;

        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
        };

        static BeamFlowApi()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "BeamFlow-Unity-Plugin/1.0.3");
        }

        /// <summary>
        /// Scan a domain's app-ads.txt health.
        /// Returns null on failure (API unreachable).
        /// </summary>
        public static async Task<ScanResult> ScanDomain(string domain, string apiKey = null)
        {
            try
            {
                var url = $"{BaseUrl}/plugin/scan?domain={Uri.EscapeDataString(domain)}&type=app_ads_txt";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(apiKey))
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await Client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonUtility.FromJson<ScanResponse>(json)?.data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BeamFlow] API scan failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Fetch managed app-ads.txt lines from BeamFlow.
        /// Returns empty list on failure.
        /// </summary>
        public static async Task<List<string>> GetManagedLines(string apiKey = null)
        {
            try
            {
                var url = $"{BaseUrl}/plugin/managed-lines?type=app_ads_txt";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(apiKey))
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await Client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return new List<string>();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonUtility.FromJson<ManagedLinesResponse>(json);
                if (result?.data?.lines == null) return new List<string>();

                var lines = new List<string>();
                foreach (var item in result.data.lines)
                {
                    if (!string.IsNullOrEmpty(item.line))
                        lines.Add(item.line);
                }
                return lines;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BeamFlow] Failed to fetch managed lines: {e.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Send telemetry event (non-blocking, fire-and-forget).
        /// </summary>
        public static async void SendTelemetry(string eventType, string domain = "")
        {
            try
            {
                var body = $"{{\"event\":\"{eventType}\",\"domain\":\"{domain}\",\"plugin_version\":\"1.0.3\",\"wp_version\":\"Unity {Application.unityVersion}\"}}";
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                await Client.PostAsync($"{BaseUrl}/plugin/telemetry", content);
            }
            catch
            {
                // Silently fail — telemetry should never block the user
            }
        }

        // JSON response types (Unity JsonUtility requires Serializable classes)
        [Serializable]
        public class ScanResponse
        {
            public bool success;
            public ScanResult data;
        }

        [Serializable]
        public class ScanResult
        {
            public string domain;
            public int healthScore;
            public string healthGrade;
            public int totalRecords;
            public int verifiedRecords;
            public int errorCount;
            public int mismatchCount;
            public int unverifiedIdCount;
        }

        [Serializable]
        public class ManagedLinesResponse
        {
            public bool success;
            public ManagedLinesData data;
        }

        [Serializable]
        public class ManagedLinesData
        {
            public string file_type;
            public ManagedLine[] lines;
            public int count;
        }

        [Serializable]
        public class ManagedLine
        {
            public string line;
            public string ssp_domain;
            public string label;
        }
    }
}
