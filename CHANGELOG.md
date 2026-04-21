# Changelog

## [1.0.5] - 2026-04-21

### Added
- Install tracking: one-time telemetry event fired on first load (per machine)
  via `[InitializeOnLoad]`, tracked in EditorPrefs to avoid duplicate events
- Error telemetry: unhandled exceptions in refresh/verify/save flows are
  reported to BeamFlow for debugging (non-blocking)
- Platform field in telemetry events (`platform: "unity"`) to distinguish
  from WordPress plugin in analytics

## [1.0.4] - 2026-04-21

### Security fixes
- Telemetry JSON now uses JsonUtility (prevents string injection)
- Managed lines from API are validated against strict ads.txt grammar
  regex and capped at 200 (prevents compromised API / MITM poisoning)
- Developer Website input normalized and validated before use in URLs
  (prevents javascript:/file: URL attacks via Application.OpenURL)
- AdMob Publisher ID format validation (pub-XXXXXXXXXXXXXXXX)

### Stability fixes
- `if (this == null) return;` after every async await (prevents
  MissingReferenceException when window is closed mid-request)
- async void methods wrapped in try/catch (prevents editor crashes)
- Replaced Dictionary.GetValueOrDefault with extension method
  (Unity 2021.3 uses .NET Standard 2.0 which lacks that API)
- SdkDetector now uses official UnityEditor.PackageManager.Client API
  with manifest.json fallback (more robust than hand-rolled JSON parser)

### UX fixes
- Publisher ID section auto-expands when enabled networks are missing
  IDs, with warning: direct monetization lines are silently skipped
  without an ID (was misleadingly labeled "optional")
- Save to File now defaults to project root, not Assets/ (prevents
  accidental commit into game binary) + try/catch on file write
- Security note added to Preferences panel about EditorPrefs storage

## [1.0.3] - 2026-04-21

### Added
- "BeamFlow Recommended Lines" section with checkboxes (checked by default)
  - Curated by BeamFlow's ad operations team to maximize revenue
  - Each line has an individual checkbox to include/exclude
  - Include All / Exclude All bulk toggles
  - Exclusions saved per-developer in EditorPrefs
  - Dimmed display for excluded lines
- Live counter: "BeamFlow Recommended Lines (N/M included)"

## [1.0.2] - 2026-04-21

### Added
- "Where to put your app-ads.txt" section with two clear options:
  - Option A: Redirect from developer's own website to BeamFlow
  - Option B: Use BeamFlow subdomain as the store-listed website
- "Host on BeamFlow" button — deep-links to /host wizard with mobile app pre-selected
- Copy example redirect rules (Apache, Nginx, Cloudflare)
- Store listing update instructions (Google Play + App Store)

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
