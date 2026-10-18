# Existing optimized Android procedure recovered

Read-only recovery 2026-10-18; old workstream remains paused.

Source: experiment/valoria-v5-android-apk-20261012, tree 2efd346034bdb89ae063406558f0343d753e489a; `.github/workflows/valoria-v5-android-apk.yml` and `Unity/Assets/Eldoria/Scripts/Editor/ValoriaV5AndroidCandidate.cs`.

Existing solution temporarily stashes only legacy Resources/Valoria/*.glb + metas and StreamingAssets/ValoriaFamilyV3, compiles one generated-mesh scene, restores originals in finally. ARM64/IL2CPP, minSdk29, medium stripping. ApplicationId com.eldoria.valoria.v5test; useCustomKeystore=false (debug signing). Version inherited from project. Exact successful APK, versionCode and signing certificate still need recovery/verification before compatible update can be asserted. Do not change app id, overwrite keys, uninstall, or reset saved data. User reports the optimized app installed and working on physical Android; native55's 1.21GB APK is not assumed to be that artifact. Art production remains primary.
