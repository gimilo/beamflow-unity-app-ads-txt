# Changelog

## [1.0.1] - 2026-04-21

### Added
- BeamFlow managed lines: admin-curated ad network lines are automatically
  appended to the generated app-ads.txt (no user action needed)
- Managed lines cached in EditorPrefs for offline use
- Visual indicator when managed lines are active

### Fixed
- Added .meta files for Unity package (required when installed via Git URL)

## [1.0.0] - 2026-04-20

### Added
- Auto-detection of 12 ad network SDKs (AdMob, AppLovin MAX, Unity Ads, ironSource, Meta, Vungle, Chartboost, InMobi, Mintegral, Pangle, Digital Turbine, Amazon APS)
- Built-in app-ads.txt line templates for all supported networks
- Line generator with deduplication and per-network publisher ID support
- Copy to clipboard and save to file
- BeamFlow API integration for sellers.json verification (optional)
- Settings under Edit > Preferences > BeamFlow
- Telemetry (tool opened, file copied/saved)
- Offline-first: works without BeamFlow account or internet connection
