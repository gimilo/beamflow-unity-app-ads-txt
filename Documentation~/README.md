# BeamFlow App-Ads.txt Manager for Unity

Auto-detect your ad SDKs and generate a verified app-ads.txt file. The only Unity tool that checks your entries against 2,000+ real ad partner databases.

## Why you need this

Google made app-ads.txt **mandatory for all AdMob apps** (January 2025). Without a valid app-ads.txt file, your ad requests may be dropped or limited, directly impacting your revenue.

## What it does

1. **Auto-detects** which ad SDKs are in your Unity project (AdMob, AppLovin MAX, Unity Ads, ironSource, and 8 more)
2. **Generates** the correct app-ads.txt with all required lines for your networks
3. **Verifies** entries against BeamFlow's database of 2,000+ ad networks (optional)
4. **Outputs** the file ready to upload to your developer website

## Installation

### From Git URL (recommended)
1. In Unity, go to **Window > Package Manager**
2. Click **+** > **Add package from git URL**
3. Enter: `https://github.com/beamflow/unity-app-ads-txt.git`

### From Unity Asset Store
Search for "App-Ads.txt Manager" in the Asset Store.

## Usage

1. Open **Window > BeamFlow > App-Ads.txt Manager**
2. Your installed ad SDKs are automatically detected
3. Enter your AdMob Publisher ID (pub-XXXXXXXXXXXXXXXX)
4. Click **Copy to Clipboard** or **Save to File**
5. Upload the file to your developer website root (yourdomain.com/app-ads.txt)

## Verification (optional)

Enter your BeamFlow API key (free at beamflow.co/developers) to verify your app-ads.txt entries against live sellers.json data from 2,000+ ad networks.

## Supported Networks

- Google AdMob
- AppLovin MAX
- Unity Ads / LevelPlay
- ironSource
- Meta Audience Network
- Vungle / Liftoff
- Chartboost
- InMobi
- Mintegral
- Pangle (TikTok)
- Digital Turbine / Fyber
- Amazon Publisher Services

## Requirements

- Unity 2021.3 LTS or later
- No runtime dependencies (editor-only tool)

## Support

- Website: https://beamflow.co
- Contact: hello@beamflow.co
