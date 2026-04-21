using UnityEditor;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// Tracks first-time install of the plugin.
    /// Fires once per machine (per EditorPrefs key) to avoid spam on every editor restart.
    /// </summary>
    [InitializeOnLoad]
    public static class InstallTracker
    {
        private const string InstallFlagKey = "BeamFlow_AppAdsTxt_Installed_v1";

        static InstallTracker()
        {
            // Defer to next editor tick to avoid blocking startup
            EditorApplication.delayCall += CheckInstallation;
        }

        private static void CheckInstallation()
        {
            try
            {
                if (EditorPrefs.GetBool(InstallFlagKey, false))
                {
                    // Already tracked this install
                    return;
                }

                EditorPrefs.SetBool(InstallFlagKey, true);
                BeamFlowApi.SendTelemetry("install", "");
            }
            catch
            {
                // Silently fail — telemetry should never block editor startup
            }
        }
    }
}
