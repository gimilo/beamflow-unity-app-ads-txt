using UnityEditor;
using UnityEngine;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// Adds BeamFlow settings under Edit > Preferences > BeamFlow.
    /// </summary>
    public class BeamFlowSettingsProvider : UnityEditor.SettingsProvider
    {
        public BeamFlowSettingsProvider(string path, SettingsScope scopes)
            : base(path, scopes) { }

        [SettingsProvider]
        public static UnityEditor.SettingsProvider CreateProvider()
        {
            return new BeamFlowSettingsProvider("Preferences/BeamFlow", SettingsScope.User)
            {
                label = "BeamFlow",
                keywords = new[] { "beamflow", "ads.txt", "app-ads.txt", "admob", "monetization" }
            };
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("BeamFlow App-Ads.txt Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // AdMob Publisher ID
            EditorGUILayout.LabelField("AdMob Publisher ID", EditorStyles.miniBoldLabel);
            var pubId = EditorGUILayout.TextField(Settings.PublisherId);
            if (pubId != Settings.PublisherId)
                Settings.PublisherId = pubId;
            EditorGUILayout.HelpBox(
                "Your AdMob Publisher ID (format: pub-XXXXXXXXXXXXXXXX). Find it in AdMob > Account > Publisher ID.",
                MessageType.Info);
            EditorGUILayout.Space(10);

            // Developer Website
            EditorGUILayout.LabelField("Developer Website", EditorStyles.miniBoldLabel);
            var website = EditorGUILayout.TextField(Settings.DeveloperWebsite);
            if (website != Settings.DeveloperWebsite)
                Settings.DeveloperWebsite = BeamFlowApi.NormalizeDomain(website);
            EditorGUILayout.HelpBox(
                "The domain where your app-ads.txt will be hosted. Must match the website URL in your App Store / Play Store listing.",
                MessageType.Info);
            EditorGUILayout.Space(10);

            // BeamFlow API Key
            EditorGUILayout.LabelField("BeamFlow API Key (optional)", EditorStyles.miniBoldLabel);
            var apiKey = EditorGUILayout.PasswordField(Settings.ApiKey);
            if (apiKey != Settings.ApiKey)
                Settings.ApiKey = apiKey;
            EditorGUILayout.HelpBox(
                "Optional. Connect your BeamFlow account for sellers.json verification. Get a free API key at beamflow.co/developers.\n\n" +
                "Note: Stored in Unity EditorPrefs (per-machine, unencrypted, shared across projects). Do not share your editor preferences.",
                MessageType.Info);

            EditorGUILayout.Space(15);
            if (GUILayout.Button("Open App-Ads.txt Manager Window"))
            {
                AppAdsTxtWindow.ShowWindow();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("beamflow.co", EditorStyles.miniLabel);
        }
    }
}
