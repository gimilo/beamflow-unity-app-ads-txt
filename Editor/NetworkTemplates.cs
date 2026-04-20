using System.Collections.Generic;

namespace BeamFlow.AppAdsTxt
{
    /// <summary>
    /// Built-in app-ads.txt line templates for major ad networks.
    /// These are hardcoded defaults that work offline.
    /// When BeamFlow API is available, dynamic lines supplement these.
    ///
    /// The {PUBLISHER_ID} placeholder is replaced with the user's actual ID.
    /// Lines without placeholders are used as-is.
    /// </summary>
    public static class NetworkTemplates
    {
        /// <summary>
        /// Get the template lines for a specific network.
        /// Returns lines with {PUBLISHER_ID} placeholder where applicable.
        /// </summary>
        public static List<string> GetLines(string networkId)
        {
            if (Templates.TryGetValue(networkId, out var lines))
                return new List<string>(lines);
            return new List<string>();
        }

        /// <summary>
        /// Get all supported network IDs.
        /// </summary>
        public static IEnumerable<string> GetAllNetworkIds() => Templates.Keys;

        /// <summary>
        /// Check if a network requires a publisher ID input.
        /// </summary>
        public static bool RequiresPublisherId(string networkId)
        {
            return networkId == "admob";
        }

        // Built-in templates per network.
        // Sources: Official SDK documentation + CAS.AI aggregated file.
        // {PUBLISHER_ID} is replaced at generation time.
        private static readonly Dictionary<string, string[]> Templates = new Dictionary<string, string[]>
        {
            ["admob"] = new[]
            {
                "google.com, {PUBLISHER_ID}, DIRECT, f08c47fec0942fa0"
            },

            ["applovin"] = new[]
            {
                "applovin.com, {PUBLISHER_ID}, DIRECT, 761f30157406f0d1",
                "google.com, pub-1612785578923498, RESELLER, f08c47fec0942fa0",
                "google.com, pub-8415824362849498, RESELLER, f08c47fec0942fa0",
                "appnexus.com, 14077, RESELLER, f5ab79cb980f11d1",
                "rubiconproject.com, 24168, RESELLER, 0bfd66d529a55807",
                "pubmatic.com, 162223, RESELLER, 5d62403b186f2ace",
                "openx.com, 541177349, RESELLER, 6a698e2ec38604c6",
                "indexexchange.com, 197200, RESELLER, 50b1c356f2c5c8fc"
            },

            ["unity_ads"] = new[]
            {
                "unity.com, {PUBLISHER_ID}, DIRECT",
                "google.com, pub-1612785578923498, RESELLER, f08c47fec0942fa0",
                "google.com, pub-4185460024498456, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 161673, RESELLER, 5d62403b186f2ace",
                "openx.com, 541177349, RESELLER, 6a698e2ec38604c6",
                "appnexus.com, 14077, RESELLER, f5ab79cb980f11d1",
                "rubiconproject.com, 24168, RESELLER, 0bfd66d529a55807"
            },

            ["ironsource"] = new[]
            {
                "is.com, {PUBLISHER_ID}, DIRECT",
                "google.com, pub-1612785578923498, RESELLER, f08c47fec0942fa0",
                "google.com, pub-4185460024498456, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 161673, RESELLER, 5d62403b186f2ace",
                "openx.com, 541177349, RESELLER, 6a698e2ec38604c6",
                "appnexus.com, 14077, RESELLER, f5ab79cb980f11d1",
                "indexexchange.com, 197200, RESELLER, 50b1c356f2c5c8fc"
            },

            ["meta"] = new[]
            {
                "facebook.com, {PUBLISHER_ID}, DIRECT, c3e20eee3f780d68",
                "facebook.com, {PUBLISHER_ID}, RESELLER, c3e20eee3f780d68"
            },

            ["vungle"] = new[]
            {
                "vungle.com, {PUBLISHER_ID}, DIRECT",
                "google.com, pub-0796504668498238, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 162223, RESELLER, 5d62403b186f2ace",
                "openx.com, 541177349, RESELLER, 6a698e2ec38604c6"
            },

            ["chartboost"] = new[]
            {
                "chartboost.com, {PUBLISHER_ID}, DIRECT",
                "google.com, pub-6448830788379498, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 162223, RESELLER, 5d62403b186f2ace"
            },

            ["inmobi"] = new[]
            {
                "inmobi.com, {PUBLISHER_ID}, DIRECT, 83e75a7ae333ca0d",
                "google.com, pub-6611580245498261, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 157866, RESELLER, 5d62403b186f2ace"
            },

            ["mintegral"] = new[]
            {
                "mintegral.com, {PUBLISHER_ID}, DIRECT",
                "google.com, pub-1612785578923498, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 162223, RESELLER, 5d62403b186f2ace",
                "openx.com, 541177349, RESELLER, 6a698e2ec38604c6",
                "appnexus.com, 14077, RESELLER, f5ab79cb980f11d1",
                "indexexchange.com, 197200, RESELLER, 50b1c356f2c5c8fc"
            },

            ["pangle"] = new[]
            {
                "pangleglobal.com, {PUBLISHER_ID}, DIRECT",
                "google.com, pub-7709956668498391, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 162223, RESELLER, 5d62403b186f2ace"
            },

            ["dt_exchange"] = new[]
            {
                "fyber.com, {PUBLISHER_ID}, DIRECT",
                "google.com, pub-2767829498065449, RESELLER, f08c47fec0942fa0",
                "pubmatic.com, 162223, RESELLER, 5d62403b186f2ace"
            },

            ["amazon_aps"] = new[]
            {
                "amazon-adsystem.com, {PUBLISHER_ID}, DIRECT"
            },
        };
    }
}
