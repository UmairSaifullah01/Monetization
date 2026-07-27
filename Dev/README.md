# Monetization Dev Tools

Maintainer-only editor utilities for this repository. **Not** included in:

- Bootstrap installer drop (`Installer/` + `Logo/`)
- `MonetizationScripts.unitypackage` export

## Tools

| Menu | Purpose |
|------|---------|
| `Tools → Monetization Dev → Generate Installer Config From Manifest` | Sync `Installer/installer_config.json` tgz mappings from `manifest.json` and `Installer/Dependencies/` |
| `Tools → Monetization Dev → Icon Downloader` | Browse/search Unity editor icons and export selected PNGs to `Editor/Icons/` for the profile inspector |
| `Tools → Monetization Dev → Export MonetizationScripts.unitypackage` | Export the framework content package to `Installer/MonetizationScripts.unitypackage` (Runtime, JsonDataUtility, Editor, Resources, Content, Demo, `installer_config.json`, `Dependencies/*.tgz`) |
| `Tools → Monetization Dev → Export Installer Bootstrap Package` | Export the bootstrap installer package (installer Editor scripts + Logo + embedded `MonetizationScripts.unitypackage`) to a chosen path |
| `Tools → Monetization Dev → Export All Packages` | Export the content package, then the bootstrap package |

Include/exclude lists follow [`../Installer/EXPORT.md`](../Installer/EXPORT.md). Export the content package before the bootstrap package, since the bootstrap embeds `MonetizationScripts.unitypackage`.

Use these while developing the framework in this project. End users install via `Tools → Monetization → Installer` only.
