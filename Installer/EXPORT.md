# MonetizationScripts.unitypackage Export Guide

## Bootstrap package (ship to empty projects)

Include only:

- `Assets/Monetization/Installer/Editor/MonetizationInstallerWindow.cs`
- `Assets/Monetization/Installer/Editor/FrameworkInstallModule.cs`
- `Assets/Monetization/Installer/Editor/BootstrapManifestUtility.cs`
- `Assets/Monetization/Installer/Editor/BootstrapInstallerConfig.cs`
- `Assets/Monetization/Installer/Editor/InstallerReflectionBridge.cs`
- `Assets/Monetization/Installer/Editor/MiniJSON.cs`
- `Assets/Monetization/Installer/Editor/THEBADDEST.Monetization.Bootstrap.Installer.Editor.asmdef`
- `Assets/Monetization/Installer/MonetizationScripts.unitypackage`
- `Assets/Monetization/Logo/`

Do **not** require `installer_config.json` in the bootstrap drop (it arrives with the unitypackage).

## MonetizationScripts.unitypackage (export from this project)

**Include:**

- `Runtime/` (all asmdefs, Core, Abstractions, providers)
- `JsonDataUtility/`
- `Editor/` (`PackageManagerModule`, `MonetizationInstallerBridge`, `MonetizationInstallerEditorWindow`, profile editor, validators, build processor)
- `Resources/`, `Content/`, `Demo/` (optional)
- `Installer/installer_config.json`
- `Installer/Dependencies/*.tgz`

**Exclude:**

- `Dev/` (maintainer-only tools; not for end users)
- `Installer/MonetizationScripts.unitypackage` (no self-import)
- `Installer/Editor/MonetizationInstallerWindow.cs`
- `Installer/Editor/FrameworkInstallModule.cs`
- `Installer/Editor/BootstrapManifestUtility.cs`
- `Installer/Editor/BootstrapInstallerConfig.cs`
- `Installer/Editor/InstallerReflectionBridge.cs`
- `Installer/Editor/MiniJSON.cs` (bootstrap copy; Configuration has runtime MiniJSON)
- `Installer/Editor/THEBADDEST.Monetization.Bootstrap.Installer.Editor.asmdef`
- `Logo/` (shipped with bootstrap only)

## Empty project test flow

1. Import bootstrap (`Installer/` + `Logo/`) only — must compile with zero errors.
2. `Tools → Monetization → Installer` — click **Install Monetization**.
3. Bootstrap imports unitypackage, waits for `installer_config.json`, installs `corePackages` + `registries`, then opens Package Manager in the same window title after compile.
4. Install SDK providers per row (reads `providers` + copies `Dependencies/*.tgz` from config).

## Config as single source of truth

Both install phases read `Installer/installer_config.json`:

| Module | Assembly | Config keys used |
|--------|----------|------------------|
| Framework install | Bootstrap | `corePackages`, `registries` |
| Package Manager | Editor | `providers[].packages`, `providers[].tgzPackages`, `registries` |
