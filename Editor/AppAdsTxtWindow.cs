using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// Main editor window for BeamFlow App-Ads.txt Manager.
    /// Accessible via Window > BeamFlow > App-Ads.txt Manager.
    /// </summary>
    public class AppAdsTxtWindow : EditorWindow
    {
        private List<SdkDetector.DetectedSdk> _detectedSdks;
        private Dictionary<string, bool> _enabledNetworks = new Dictionary<string, bool>();
        private Dictionary<string, string> _networkPubIds = new Dictionary<string, string>();
        private string _generatedContent = "";
        private int _lineCount;
        private Vector2 _scrollPos;
        private Vector2 _outputScrollPos;
        private bool _isVerifying;
        private BeamFlowApi.ScanResult _verificationResult;
        private string _statusMessage = "";
        private bool _showAdvanced;

        [MenuItem("Window/BeamFlow/App-Ads.txt Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<AppAdsTxtWindow>();
            window.titleContent = new GUIContent("App-Ads.txt Manager", EditorGUIUtility.IconContent("d_TextAsset Icon").image);
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnEnable()
        {
            DetectSdks();
            BeamFlowApi.SendTelemetry("tool_opened", Settings.DeveloperWebsite);
        }

        private void DetectSdks()
        {
            _detectedSdks = SdkDetector.DetectAll();

            // Initialize enabled state (auto-enable detected SDKs)
            _enabledNetworks.Clear();
            _networkPubIds.Clear();

            foreach (var sdk in _detectedSdks)
            {
                _enabledNetworks[sdk.Id] = sdk.IsDetected;
                _networkPubIds[sdk.Id] = Settings.GetNetworkPublisherId(sdk.Id);
            }

            // Load main publisher ID for AdMob
            if (string.IsNullOrEmpty(_networkPubIds.GetValueOrDefault("admob")))
                _networkPubIds["admob"] = Settings.PublisherId;

            RegenerateContent();
        }

        private void RegenerateContent()
        {
            var enabled = _enabledNetworks
                .Where(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();

            _generatedContent = LineGenerator.Generate(enabled, _networkPubIds);
            _lineCount = LineGenerator.CountDataLines(_generatedContent);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawHeader();
            EditorGUILayout.Space(10);
            DrawNetworkSection();
            EditorGUILayout.Space(10);
            DrawPublisherIdSection();
            EditorGUILayout.Space(10);
            DrawOutputSection();
            EditorGUILayout.Space(10);
            DrawActionButtons();
            EditorGUILayout.Space(10);
            DrawVerificationSection();
            EditorGUILayout.Space(15);
            DrawFooter();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BeamFlow App-Ads.txt Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Detection", GUILayout.Width(130)))
            {
                DetectSdks();
                _statusMessage = "SDKs re-scanned.";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                "Auto-detect your ad SDKs and generate a verified app-ads.txt file.",
                EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawNetworkSection()
        {
            EditorGUILayout.LabelField("Detected Ad Networks", EditorStyles.boldLabel);

            var detectedCount = _detectedSdks.Count(s => s.IsDetected);
            EditorGUILayout.HelpBox(
                $"{detectedCount} ad network(s) detected in your project. Check/uncheck to include in your app-ads.txt.",
                MessageType.Info);

            // Detected SDKs first, then undetected
            var sorted = _detectedSdks.OrderByDescending(s => s.IsDetected).ToList();

            foreach (var sdk in sorted)
            {
                EditorGUILayout.BeginHorizontal();

                var wasEnabled = _enabledNetworks.GetValueOrDefault(sdk.Id, false);
                var isEnabled = EditorGUILayout.ToggleLeft(
                    sdk.DisplayName,
                    wasEnabled,
                    sdk.IsDetected ? EditorStyles.boldLabel : EditorStyles.label,
                    GUILayout.Width(200));

                if (isEnabled != wasEnabled)
                {
                    _enabledNetworks[sdk.Id] = isEnabled;
                    RegenerateContent();
                }

                // Status badge
                if (sdk.IsDetected)
                {
                    var prevColor = GUI.color;
                    GUI.color = new Color(0.2f, 0.8f, 0.2f);
                    EditorGUILayout.LabelField($"Detected ({sdk.DetectionMethod})", EditorStyles.miniLabel, GUILayout.Width(120));
                    GUI.color = prevColor;
                }
                else
                {
                    EditorGUILayout.LabelField("Not found", EditorStyles.miniLabel, GUILayout.Width(120));
                }

                // Publisher ID input (inline, for networks that need it)
                if (isEnabled && NetworkTemplates.RequiresPublisherId(sdk.Id))
                {
                    // Handled in the publisher ID section below
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPublisherIdSection()
        {
            EditorGUILayout.LabelField("Publisher IDs", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Enter your publisher ID for each enabled network. For AdMob, this is your pub-XXXXXXXXXXXXXXXX from the AdMob console.",
                MessageType.Info);

            // AdMob publisher ID (always shown first if enabled)
            if (_enabledNetworks.GetValueOrDefault("admob", false))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Google AdMob:", GUILayout.Width(150));
                var admobId = EditorGUILayout.TextField(_networkPubIds.GetValueOrDefault("admob", ""));
                if (admobId != _networkPubIds.GetValueOrDefault("admob", ""))
                {
                    _networkPubIds["admob"] = admobId;
                    Settings.PublisherId = admobId;
                    Settings.SetNetworkPublisherId("admob", admobId);
                    RegenerateContent();
                }
                EditorGUILayout.EndHorizontal();
            }

            // Show other networks that need publisher IDs in advanced section
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Other Network Publisher IDs (optional)");
            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;
                foreach (var sdk in _detectedSdks)
                {
                    if (sdk.Id == "admob") continue; // Already shown above
                    if (!_enabledNetworks.GetValueOrDefault(sdk.Id, false)) continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{sdk.DisplayName}:", GUILayout.Width(150));
                    var netId = EditorGUILayout.TextField(_networkPubIds.GetValueOrDefault(sdk.Id, ""));
                    if (netId != _networkPubIds.GetValueOrDefault(sdk.Id, ""))
                    {
                        _networkPubIds[sdk.Id] = netId;
                        Settings.SetNetworkPublisherId(sdk.Id, netId);
                        RegenerateContent();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawOutputSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Generated app-ads.txt ({_lineCount} lines)", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            _outputScrollPos = EditorGUILayout.BeginScrollView(
                _outputScrollPos,
                EditorStyles.helpBox,
                GUILayout.Height(200));

            EditorGUILayout.TextArea(_generatedContent, EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndScrollView();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(30)))
            {
                EditorGUIUtility.systemCopyBuffer = _generatedContent;
                _statusMessage = "Copied to clipboard! Paste this into your app-ads.txt file on your website.";
                BeamFlowApi.SendTelemetry("file_copied", Settings.DeveloperWebsite);
            }

            if (GUILayout.Button("Save to File", GUILayout.Height(30)))
            {
                var defaultName = "app-ads.txt";
                var lastPath = Settings.LastOutputPath;
                var dir = string.IsNullOrEmpty(lastPath) ? Application.dataPath : Path.GetDirectoryName(lastPath);

                var path = EditorUtility.SaveFilePanel("Save app-ads.txt", dir, defaultName, "txt");
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, _generatedContent);
                    Settings.LastOutputPath = path;
                    _statusMessage = $"Saved to {path}";
                    BeamFlowApi.SendTelemetry("file_saved", Settings.DeveloperWebsite);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        private void DrawVerificationSection()
        {
            EditorGUILayout.LabelField("Verification (optional)", EditorStyles.boldLabel);

            var website = Settings.DeveloperWebsite;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Developer Website:", GUILayout.Width(130));
            var newWebsite = EditorGUILayout.TextField(website);
            if (newWebsite != website)
                Settings.DeveloperWebsite = newWebsite;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BeamFlow API Key:", GUILayout.Width(130));
            var apiKey = EditorGUILayout.PasswordField(Settings.ApiKey);
            if (apiKey != Settings.ApiKey)
                Settings.ApiKey = apiKey;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !_isVerifying && !string.IsNullOrEmpty(Settings.DeveloperWebsite);
            if (GUILayout.Button(_isVerifying ? "Verifying..." : "Verify on BeamFlow", GUILayout.Height(25)))
            {
                RunVerification();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Full Report on BeamFlow", GUILayout.Height(25)))
            {
                var domain = Settings.DeveloperWebsite;
                if (!string.IsNullOrEmpty(domain))
                    Application.OpenURL($"https://beamflow.co/scan?domain={domain}&type=app_ads_txt");
                else
                    Application.OpenURL("https://beamflow.co/scan");
            }

            EditorGUILayout.EndHorizontal();

            if (_verificationResult != null)
            {
                EditorGUILayout.Space(5);
                var grade = _verificationResult.healthGrade ?? "?";
                var score = _verificationResult.healthScore;
                var verified = _verificationResult.verifiedRecords;
                var total = _verificationResult.totalRecords;
                var issues = _verificationResult.errorCount + _verificationResult.mismatchCount;

                var msgType = score >= 70 ? MessageType.Info : (score >= 50 ? MessageType.Warning : MessageType.Error);
                EditorGUILayout.HelpBox(
                    $"Health Score: {score}/100 (Grade {grade})\n" +
                    $"Total: {total} lines | Verified: {verified} | Issues: {issues} | Unverified: {_verificationResult.unverifiedIdCount}",
                    msgType);
            }
        }

        private async void RunVerification()
        {
            _isVerifying = true;
            _verificationResult = null;
            Repaint();

            _verificationResult = await BeamFlowApi.ScanDomain(
                Settings.DeveloperWebsite,
                Settings.ApiKey);

            _isVerifying = false;

            if (_verificationResult == null)
                _statusMessage = "Could not reach BeamFlow API. Check your internet connection or try again later.";

            Repaint();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Powered by BeamFlow", EditorStyles.miniLabel);
            if (GUILayout.Button("beamflow.co", EditorStyles.linkLabel, GUILayout.Width(80)))
            {
                Application.OpenURL("https://beamflow.co");
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
