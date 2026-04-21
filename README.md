# BeamFlow App-Ads.txt Manager for Unity

> Auto-detect your ad SDKs. Generate a verified app-ads.txt file in 30 seconds.

[![Version](https://img.shields.io/github/v/release/gimilo/beamflow-unity-app-ads-txt)](https://github.com/gimilo/beamflow-unity-app-ads-txt/releases)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black)](https://unity.com)

The only Unity Editor tool that scans your project, detects your ad SDKs, and generates a complete app-ads.txt file verified against 2,000+ real ad network sellers.json files.

**Free forever. No account needed. Works offline.**

## Why you need this

Google made app-ads.txt **mandatory for all AdMob apps (January 2025)**. Without a valid file, your ad requests may be dropped or limited, directly costing you revenue.

Only 24.5% of Play Store apps have app-ads.txt set up correctly. BeamFlow makes it trivial.

## What it does

- **Auto-detects** installed ad SDKs: AdMob, AppLovin MAX, Unity Ads, ironSource, Meta, Vungle, Chartboost, InMobi, Mintegral, Pangle, Digital Turbine, Amazon
- **Generates** the complete app-ads.txt with correct DIRECT and RESELLER lines for every detected network
- **Verifies** (optional) every entry against BeamFlow's database of 2,000+ live sellers.json files
- **Includes** curated recommended lines from BeamFlow's ad ops team to maximize your demand
- **Hosts** (optional) your app-ads.txt free on a BeamFlow subdomain, IAB-compliant

## Installation

### Via Unity Package Manager (Git URL)

1. Open **Window > Package Manager** in Unity
2. Click **+** > **Add package from git URL**
3. Enter: `https://github.com/gimilo/beamflow-unity-app-ads-txt.git`

### Via Unity Asset Store

Coming soon. [Learn more](https://beamflow.co/unity-plugin).

## Quick start

1. **Open the window:** `Window > BeamFlow > App-Ads.txt Manager`
2. **SDKs auto-detected:** checked boxes show what's installed in your project
3. **Enter your AdMob Publisher ID** (`pub-XXXXXXXXXXXXXXXX`)
4. **Copy to clipboard** or **Save to file**
5. **Upload** to your developer website root (`yourdomain.com/app-ads.txt`)

## Where to host the file

Your app-ads.txt must be hosted on the website listed in your app's store listing. Two options:

### Option A: I have a website
Set up a 301 redirect from `yourdomain.com/app-ads.txt` to your BeamFlow hosted URL. The plugin shows you the exact redirect rule (Apache, Nginx, Cloudflare).

### Option B: I don't have a website
Get a free BeamFlow subdomain (`mygame.beamflow.co`) and update your Play Store "Website" or App Store "Marketing URL" to that URL. Same pattern Google recommends with Firebase Hosting.

Both are IAB-spec compliant.

## Supported ad networks

| Network | Auto-detected | Lines included |
|---------|:-:|:-:|
| Google AdMob | ✓ | ✓ |
| AppLovin MAX | ✓ | ✓ |
| Unity Ads / LevelPlay | ✓ | ✓ |
| ironSource | ✓ | ✓ |
| Meta Audience Network | ✓ | ✓ |
| Vungle / Liftoff | ✓ | ✓ |
| Chartboost | ✓ | ✓ |
| InMobi | ✓ | ✓ |
| Mintegral | ✓ | ✓ |
| Pangle (TikTok) | ✓ | ✓ |
| Digital Turbine / Fyber | ✓ | ✓ |
| Amazon Publisher Services | ✓ | ✓ |

## Verification (optional)

Get a free BeamFlow API key at [beamflow.co/developers](https://beamflow.co/developers) and enter it in the plugin settings. The plugin will verify every line in your app-ads.txt against live sellers.json data from 2,000+ ad networks.

## Privacy & telemetry

The plugin sends anonymous telemetry (install events, errors) to help us improve it. No personal data or project contents are sent. See [PRIVACY](https://beamflow.co/privacy).

## Requirements

- Unity 2021.3 LTS or later
- .NET Standard 2.0 or 2.1
- No runtime dependencies (editor-only tool)

## Contributing

Issues and pull requests welcome at [github.com/gimilo/beamflow-unity-app-ads-txt](https://github.com/gimilo/beamflow-unity-app-ads-txt).

## License

[Apache 2.0](LICENSE)

## Links

- **Plugin page:** [beamflow.co/unity-plugin](https://beamflow.co/unity-plugin)
- **Library articles:** [beamflow.co/library](https://beamflow.co/library)
- **Contact:** hello@beamflow.co

---

Built by [BeamFlow](https://beamflow.co). A former Unity employee and 15-year ad monetization veteran who saw developers lose revenue to broken app-ads.txt files daily.
