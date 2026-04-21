using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// HTTP client for BeamFlow API.
    /// All methods are async and return null/empty on failure (offline-first).
    /// </summary>
    public static class BeamFlowApi
    {
        public const string PluginVersion = "1.0.4";
        private const string BaseUrl = "https://beamflow.co/api/v1";
        private const int TimeoutSeconds = 10;
        private const int MaxManagedLines = 200;

        // ads.txt line format: domain, account_id, DIRECT|RESELLER[, cert_authority]
        private static readonly Regex AdsTxtLineRegex = new Regex(
            @"^[a-zA-Z0-9.-]+\s*,\s*[A-Za-z0-9_\-]+\s*,\s*(DIRECT|RESELLER)(\s*,\s*[a-fA-F0-9]+)?\s*$",
            RegexOptions.Compiled);

        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
        };

        static BeamFlowApi()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "BeamFlow-Unity-Plugin/" + PluginVersion);
        }

        /// <summary>
        /// Scan a domain's app-ads.txt health.
        /// Returns null on failure (API unreachable or invalid domain).
        /// </summary>
        public static async Task<ScanResult> ScanDomain(string domain, string apiKey = null)
        {
            var cleanDomain = NormalizeDomain(domain);
            if (!IsValidDomain(cleanDomain)) return null;

            try
            {
                var url = $"{BaseUrl}/plugin/scan?domain={Uri.EscapeDataString(cleanDomain)}&type=app_ads_txt";
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
        /// Only returns lines that pass strict ads.txt grammar validation.
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
                int count = 0;
                foreach (var item in result.data.lines)
                {
                    if (count >= MaxManagedLines) break;
                    if (string.IsNullOrEmpty(item.line)) continue;

                    var line = item.line.Trim();
                    // Strip control chars and anything suspicious
                    if (line.Contains('\n') || line.Contains('\r') || line.Contains('\0')) continue;
                    if (line.StartsWith("#")) continue; // Skip comments
                    if (!AdsTxtLineRegex.IsMatch(line)) continue; // Must match ads.txt grammar

                    lines.Add(line);
                    count++;
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
        /// Uses JsonUtility for safe JSON serialization.
        /// </summary>
        public static async void SendTelemetry(string eventType, string domain = "")
        {
            try
            {
                var payload = new TelemetryPayload
                {
                    eventType = eventType ?? "",
                    domain = NormalizeDomain(domain),
                    plugin_version = PluginVersion,
                    unity_version = Application.unityVersion
                };
                // JsonUtility doesn't support renaming, so we manually rename 'eventType' to 'event'
                var json = JsonUtility.ToJson(payload).Replace("\"eventType\":", "\"event\":");
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var resp = await Client.PostAsync($"{BaseUrl}/plugin/telemetry", content);
                resp?.Dispose();
            }
            catch
            {
                // Silently fail — telemetry should never block the user
            }
        }

        /// <summary>
        /// Normalize a domain string (lowercase, strip protocol/www/trailing slash/path).
        /// </summary>
        public static string NormalizeDomain(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var d = input.Trim().ToLowerInvariant();
            d = Regex.Replace(d, @"^https?://", "");
            d = Regex.Replace(d, @"^www\.", "");
            var slashIdx = d.IndexOf('/');
            if (slashIdx > 0) d = d.Substring(0, slashIdx);
            return d.Trim().TrimEnd('/');
        }

        /// <summary>
        /// Validate that a string is a plausible domain name.
        /// Rejects javascript:, file:, and other non-HTTP schemes.
        /// </summary>
        public static bool IsValidDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return false;
            if (domain.Length > 253) return false;
            return Regex.IsMatch(domain, @"^[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?)+$");
        }

        /// <summary>
        /// Validate AdMob publisher ID format: pub- followed by 16 digits.
        /// </summary>
        public static bool IsValidAdMobPublisherId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return Regex.IsMatch(id.Trim(), @"^pub-[0-9]{16}$");
        }

        // JSON response types (Unity JsonUtility requires Serializable classes)

        [Serializable]
        private class TelemetryPayload
        {
            public string eventType;
            public string domain;
            public string plugin_version;
            public string unity_version;
        }

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
