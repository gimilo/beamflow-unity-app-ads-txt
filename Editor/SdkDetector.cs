using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

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
        /// Get the list of installed UPM packages by reading Packages/manifest.json.
        /// </summary>
        private static HashSet<string> GetUpmPackages()
        {
            var packages = new HashSet<string>();
            var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");

            if (!File.Exists(manifestPath))
                return packages;

            try
            {
                var json = File.ReadAllText(manifestPath);
                // Simple parsing: find "dependencies": { "com.xxx": "..." } keys
                var depsStart = json.IndexOf("\"dependencies\"", StringComparison.Ordinal);
                if (depsStart < 0) return packages;

                var braceStart = json.IndexOf('{', depsStart);
                if (braceStart < 0) return packages;

                var braceEnd = json.IndexOf('}', braceStart);
                if (braceEnd < 0) return packages;

                var depsBlock = json.Substring(braceStart, braceEnd - braceStart + 1);
                // Extract quoted keys
                var pos = 0;
                while (pos < depsBlock.Length)
                {
                    var quoteStart = depsBlock.IndexOf('"', pos);
                    if (quoteStart < 0) break;
                    var quoteEnd = depsBlock.IndexOf('"', quoteStart + 1);
                    if (quoteEnd < 0) break;

                    var key = depsBlock.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                    if (key.Contains("."))
                        packages.Add(key);

                    // Skip to after the value
                    pos = quoteEnd + 1;
                    var nextQuote = depsBlock.IndexOf('"', pos);
                    if (nextQuote >= 0)
                        pos = depsBlock.IndexOf('"', nextQuote + 1) + 1;
                    else
                        break;
                }
            }
            catch
            {
                // Silently fail — manifest parse error shouldn't break the tool
            }

            return packages;
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
