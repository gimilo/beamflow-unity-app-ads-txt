using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// Detects which ad network SDKs are installed in the Unity project.
    /// Uses file path checks, type reflection, and UPM package manifest parsing.
    /// </summary>
    public static class SdkDetector
    {
        public class DetectedSdk
        {
            public string Id;
            public string DisplayName;
            public bool IsDetected;
            public string DetectionMethod; // "file", "type", "upm"
        }

        private static readonly SdkSignature[] Signatures = new[]
        {
            new SdkSignature("admob", "Google AdMob",
                directories: new[] { "Assets/GoogleMobileAds", "Assets/Plugins/Android/GoogleMobileAdsPlugin" },
                types: new[] { "GoogleMobileAds.Api.MobileAds" }),

            new SdkSignature("applovin", "AppLovin MAX",
                directories: new[] { "Assets/MaxSdk", "Assets/AppLovin" },
                upmPackages: new[] { "com.applovin.applovin-sdk" }),

            new SdkSignature("unity_ads", "Unity Ads / LevelPlay",
                directories: new[] { "Assets/LevelPlay" },
                upmPackages: new[] { "com.unity.ads", "com.unity.services.levelplay", "com.unity.ads.ios-support" }),

            new SdkSignature("ironsource", "ironSource",
                directories: new[] { "Assets/IronSource" },
                types: new[] { "IronSource" }),

            new SdkSignature("meta", "Meta Audience Network",
                directories: new[] { "Assets/AudienceNetwork", "Assets/Plugins/Android/AudienceNetwork" },
                types: new[] { "AudienceNetwork.AdView" }),

            new SdkSignature("vungle", "Vungle / Liftoff",
                directories: new[] { "Assets/Vungle", "Assets/Plugins/Vungle" },
                types: new[] { "Vungle.VungleSDK" }),

            new SdkSignature("chartboost", "Chartboost",
                directories: new[] { "Assets/Chartboost" },
                types: new[] { "Chartboost.Chartboost" }),

            new SdkSignature("inmobi", "InMobi",
                directories: new[] { "Assets/InMobi", "Assets/Plugins/Android/InMobi" }),

            new SdkSignature("mintegral", "Mintegral",
                directories: new[] { "Assets/Mintegral", "Assets/Plugins/Android/Mintegral" }),

            new SdkSignature("pangle", "Pangle (TikTok)",
                directories: new[] { "Assets/Pangle", "Assets/CSJ" },
                types: new[] { "Pangle.PangleSDK" }),

            new SdkSignature("dt_exchange", "Digital Turbine / Fyber",
                directories: new[] { "Assets/Fyber", "Assets/DT" }),

            new SdkSignature("amazon_aps", "Amazon Publisher Services",
                directories: new[] { "Assets/AmazonAds", "Assets/Plugins/Android/AmazonAds" }),
        };

        /// <summary>
        /// Scan the project and return detection results for all known ad networks.
        /// </summary>
        public static List<DetectedSdk> DetectAll()
        {
            var upmPackages = GetUpmPackages();
            var results = new List<DetectedSdk>();

            foreach (var sig in Signatures)
            {
                var sdk = new DetectedSdk
                {
                    Id = sig.Id,
                    DisplayName = sig.DisplayName,
                    IsDetected = false,
                    DetectionMethod = ""
                };

                // Check directories
                if (sig.Directories != null)
                {
                    foreach (var dir in sig.Directories)
                    {
                        if (Directory.Exists(dir))
                        {
                            sdk.IsDetected = true;
                            sdk.DetectionMethod = "file";
                            break;
                        }
                    }
                }

                // Check types (if not already found)
                if (!sdk.IsDetected && sig.Types != null)
                {
                    foreach (var typeName in sig.Types)
                    {
                        var type = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a =>
                            {
                                try { return a.GetTypes(); }
                                catch { return Array.Empty<Type>(); }
                            })
                            .FirstOrDefault(t => t.FullName == typeName);

                        if (type != null)
                        {
                            sdk.IsDetected = true;
                            sdk.DetectionMethod = "type";
                            break;
                        }
                    }
                }

                // Check UPM packages (if not already found)
                if (!sdk.IsDetected && sig.UpmPackages != null)
                {
                    foreach (var pkg in sig.UpmPackages)
                    {
                        if (upmPackages.Contains(pkg))
                        {
                            sdk.IsDetected = true;
                            sdk.DetectionMethod = "upm";
                            break;
                        }
                    }
                }

                results.Add(sdk);
            }

            return results;
        }

        /// <summary>
        /// Get the list of installed UPM packages using the official Unity API.
        /// Falls back to parsing manifest.json if the API is slow/unavailable.
        /// </summary>
        private static HashSet<string> GetUpmPackages()
        {
            var packages = new HashSet<string>();

            try
            {
                var listRequest = Client.List(offlineMode: true, includeIndirectDependencies: false);
                // Block briefly for the result (we're on the main thread in editor)
                var timeout = DateTime.UtcNow.AddSeconds(3);
                while (!listRequest.IsCompleted && DateTime.UtcNow < timeout)
                {
                    System.Threading.Thread.Sleep(50);
                }

                if (listRequest.IsCompleted && listRequest.Status == StatusCode.Success && listRequest.Result != null)
                {
                    foreach (var pkg in listRequest.Result)
                    {
                        if (!string.IsNullOrEmpty(pkg.name))
                            packages.Add(pkg.name);
                    }
                    return packages;
                }
            }
            catch
            {
                // Fall through to manifest parse
            }

            // Fallback: parse manifest.json directly
            try
            {
                var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");
                if (!File.Exists(manifestPath)) return packages;

                var json = File.ReadAllText(manifestPath);
                var depsStart = json.IndexOf("\"dependencies\"", StringComparison.Ordinal);
                if (depsStart < 0) return packages;

                var braceStart = json.IndexOf('{', depsStart);
                if (braceStart < 0) return packages;

                var braceEnd = FindMatchingBrace(json, braceStart);
                if (braceEnd < 0) return packages;

                var depsBlock = json.Substring(braceStart + 1, braceEnd - braceStart - 1);
                // Match "com.xxx.yyy" keys (package names have dots)
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    depsBlock, @"""([a-z][a-z0-9_\-]*(\.[a-z0-9_\-]+)+)""\s*:");
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    packages.Add(m.Groups[1].Value);
                }
            }
            catch
            {
                // Silently fail — manifest parse error shouldn't break the tool
            }

            return packages;
        }

        /// <summary>
        /// Find the matching closing brace for an opening brace, handling nested braces.
        /// </summary>
        private static int FindMatchingBrace(string json, int openIdx)
        {
            int depth = 0;
            bool inString = false;
            bool escape = false;
            for (int i = openIdx; i < json.Length; i++)
            {
                char c = json[i];
                if (escape) { escape = false; continue; }
                if (c == '\\' && inString) { escape = true; continue; }
                if (c == '"') { inString = !inString; continue; }
                if (inString) continue;
                if (c == '{') depth++;
                else if (c == '}') { depth--; if (depth == 0) return i; }
            }
            return -1;
        }

        private class SdkSignature
        {
            public string Id;
            public string DisplayName;
            public string[] Directories;
            public string[] Types;
            public string[] UpmPackages;

            public SdkSignature(string id, string displayName,
                string[] directories = null, string[] types = null, string[] upmPackages = null)
            {
                Id = id;
                DisplayName = displayName;
                Directories = directories;
                Types = types;
                UpmPackages = upmPackages;
            }
        }
    }
}
