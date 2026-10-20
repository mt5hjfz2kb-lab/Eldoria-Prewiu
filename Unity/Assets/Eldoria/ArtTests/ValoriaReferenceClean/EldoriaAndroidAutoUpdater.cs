using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Eldoria.ReferenceClean {
    [Serializable]
    public sealed class EldoriaAndroidRelease {
        public int schema, versionCode;
        public bool enabled;
        public long bytes;
        public string channel, packageName, versionName, sourceSha, sha256, certificateSha256, apkUrl, message;
    }

    // One immutable, explicitly promoted Android APK per version. Never follow a development branch.
    public static class EldoriaAndroidUpdatePolicy {
        public const string Package = "com.eldoria.valoria.v5test";
        public const string Certificate = "f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8";
        public const string Manifest = "https://raw.githubusercontent.com/mt5hjfz2kb-lab/Eldoria-Prewiu/main/updates/android/family.json";
        public const string ReleasePrefix = "https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code";
        public const long MaximumBytes = 450L * 1024L * 1024L;

        public static bool Accept(EldoriaAndroidRelease candidate, int installedCode, out string reason) {
            reason = "El canal todavía no tiene una versión nueva y verificada.";
            if (candidate == null || !candidate.enabled) return false;
            if (candidate.schema != 1 || candidate.channel != "family" || candidate.packageName != Package) return false;
            if (installedCode <= 0 || candidate.versionCode <= installedCode || candidate.versionCode > 999999) return false;
            if (candidate.bytes < 1024 * 1024 || candidate.bytes > MaximumBytes) return false;
            if (candidate.certificateSha256 != Certificate) return false;
            if (string.IsNullOrEmpty(candidate.sha256) || !Regex.IsMatch(candidate.sha256, "^[0-9a-f]{64}$")) return false;
            if (string.IsNullOrEmpty(candidate.sourceSha) || !Regex.IsMatch(candidate.sourceSha, "^[0-9a-f]{40}$")) return false;
            if (string.IsNullOrEmpty(candidate.versionName) || !Regex.IsMatch(candidate.versionName, @"^[0-9]+[.][0-9]+([.][0-9]+)?$")) return false;
            string expected = ReleasePrefix + candidate.versionCode + "/Eldoria-Valoria-code" + candidate.versionCode + ".apk";
            if (!string.Equals(candidate.apkUrl, expected, StringComparison.Ordinal)) return false;
            reason = null;
            return true;
        }

        public static bool VerifyApk(string path, EldoriaAndroidRelease manifest) {
            if (manifest == null || !File.Exists(path) || new FileInfo(path).Length != manifest.bytes) return false;
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path)) {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                return string.Equals(actual, manifest.sha256, StringComparison.Ordinal);
            }
        }
    }

    // Present in the integrated APK only; no WebGL/iOS impact and no gameplay/save access.
    public sealed class EldoriaAndroidAutoUpdater : MonoBehaviour {
        EldoriaAndroidRelease candidate;
        string information = "";
        bool downloading, downloaded, awaitingPermission, dismissed, checking, waitingForWifi, downloadFailed;
        long downloadId = -1;
        string apkPath, downloadTicketPath;
        DateTime lastCheck = DateTime.MinValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InstallBootstrap() {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (UnityEngine.Application.identifier != EldoriaAndroidUpdatePolicy.Package) return;
            if (FindFirstObjectByType<EldoriaAndroidAutoUpdater>() != null) return;
            var instance = new GameObject("Eldoria / actualizaciones verificadas de main");
            DontDestroyOnLoad(instance);
            instance.AddComponent<EldoriaAndroidAutoUpdater>();
#endif
        }

        IEnumerator Start() {
#if UNITY_ANDROID && !UNITY_EDITOR
            yield return new WaitForSecondsRealtime(3f);
            yield return Check();
            // An in-game Wi-Fi connection may appear after startup; resume without user action.
            while (true) {
                yield return new WaitForSecondsRealtime(30f);
                if (waitingForWifi && candidate != null && !downloading && !downloaded &&
                    HasUnmeteredNetwork())
                    StartCoroutine(DownloadUpdate(true));
            }
#else
            yield break;
#endif
        }

        void OnApplicationFocus(bool focused) {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!focused) return;
            if (waitingForWifi && !downloading && !downloaded && HasUnmeteredNetwork())
                StartCoroutine(DownloadUpdate(true));
            if (!checking && !downloading && !downloaded &&
                (DateTime.UtcNow - lastCheck).TotalMinutes >= 30)
                StartCoroutine(Check());
#endif
        }

        IEnumerator Check() {
            if (checking || downloading) yield break;
            checking = true;
            lastCheck = DateTime.UtcNow;
            string address = EldoriaAndroidUpdatePolicy.Manifest + "?v=" + DateTime.UtcNow.Ticks;
            using (var request = UnityWebRequest.Get(address)) {
                request.timeout = 10;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success &&
                    request.downloadedBytes > 1 && request.downloadedBytes <= 8192) {
                    try {
                        int installed = InstalledVersionCode();
                        var next = JsonUtility.FromJson<EldoriaAndroidRelease>(request.downloadHandler.text);
                        if (EldoriaAndroidUpdatePolicy.Accept(next, installed, out _)) {
                            if (candidate == null || next.versionCode > candidate.versionCode) {
                                candidate = next;
                                dismissed = false;
                                downloaded = false;
                                awaitingPermission = false;
                                downloadId = -1;
                                waitingForWifi = false;
                                downloadFailed = false;
                                information = next.message ?? "";
                            }
                        }
                    } catch (Exception error) {
                        Debug.LogWarning("ELDORIA_UPDATE_MANIFEST: " + error.Message);
                    }
                }
            }
            checking = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Download the approved APK quietly on unmetered Wi-Fi; never launch installation here.
            if (candidate != null && !downloaded && !downloading)
                StartCoroutine(DownloadUpdate(true));
#endif
        }

        int InstalledVersionCode() {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
            using (var package = pm.Call<AndroidJavaObject>("getPackageInfo", EldoriaAndroidUpdatePolicy.Package, 0)) {
                return (int)package.Call<long>("getLongVersionCode");
            }
#else
            return 0;
#endif
        }

        // Respect mobile data plans. A user can explicitly override this gate using the button.
        bool HasUnmeteredNetwork() {
#if UNITY_ANDROID && !UNITY_EDITOR
            try {
                if (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable) return false;
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var connectivity = activity.Call<AndroidJavaObject>("getSystemService", "connectivity"))
                    return connectivity != null && !connectivity.Call<bool>("isActiveNetworkMetered");
            } catch (Exception error) {
                Debug.LogWarning("ELDORIA_UPDATE_NETWORK: " + error.Message);
                return false;
            }
#else
            return false;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Reattach an Android download after an app restart. Status 1/2/4 is pending, 8 completed.
        int DownloadStatus(AndroidJavaObject manager, long id, out long received) {
            received = 0;
            if (id < 0) return 0;
            using (var query = new AndroidJavaObject("android.app.DownloadManager$Query")) {
                using (var filter = query.Call<AndroidJavaObject>("setFilterById", new long[] { id })) { }
                using (var cursor = manager.Call<AndroidJavaObject>("query", query)) {
                    if (cursor == null || !cursor.Call<bool>("moveToFirst")) return 0;
                    int status = cursor.Call<int>("getInt", cursor.Call<int>("getColumnIndexOrThrow", "status"));
                    received = cursor.Call<long>("getLong", cursor.Call<int>("getColumnIndexOrThrow", "bytes_so_far"));
                    return status;
                }
            }
        }

        void DropBadDownload(AndroidJavaObject manager) {
            if (downloadId >= 0) {
                try { manager.Call<int>("remove", new long[] { downloadId }); }
                catch (Exception e) { Debug.LogWarning("ELDORIA_UPDATE_REMOVE: " + e.Message); }
            }
            try {
                if (!string.IsNullOrEmpty(downloadTicketPath) && File.Exists(downloadTicketPath)) File.Delete(downloadTicketPath);
                if (!string.IsNullOrEmpty(apkPath) && File.Exists(apkPath)) File.Delete(apkPath);
            } catch (Exception e) { Debug.LogWarning("ELDORIA_UPDATE_CLEANUP: " + e.Message); }
            downloadId = -1;
        }

#endif
        void OnGUI() {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Keep normal gameplay unobstructed while an APK downloads in Android's background service.
            if (candidate == null || dismissed || downloading ||
                (!downloaded && !waitingForWifi && !downloadFailed)) return;
            var old = GUI.matrix;
            float scale = Mathf.Max(0.75f, Mathf.Min(Screen.width / 720f, Screen.height / 1280f));
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale, height = Screen.height / scale;
            float boxWidth = Mathf.Min(640f, width - 32f), x = (width - boxWidth) / 2f;
            var rect = new Rect(x, Mathf.Max(12, height * .12f), boxWidth, 208);
            GUI.depth = -1000;
            GUI.Box(rect, "");
            var heading = new GUIStyle(GUI.skin.label) { fontSize = 27, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            var body = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUI.Label(new Rect(x + 14, rect.y + 12, boxWidth - 28, 36),
                downloaded ? "Eldoria " + candidate.versionName + " lista para instalar" :
                (waitingForWifi ? "Eldoria " + candidate.versionName + " · esperando Wi-Fi" :
                                  "Eldoria " + candidate.versionName + " · reintentar"), heading);
            GUI.Label(new Rect(x + 18, rect.y + 50, boxWidth - 36, 70),
                downloaded ? "Descarga verificada. Instálala cuando termines de jugar." :
                (waitingForWifi ? "Se descargará sola con Wi-Fi, o puedes autorizar datos móviles." :
                                  "Descarga interrumpida. No se ha instalado nada."), body);
            GUI.enabled = !downloading;
            if (GUI.Button(new Rect(x + 22, rect.y + 138, boxWidth * .58f - 26f, 51),
                downloaded ? "Instalar" : (waitingForWifi ? "Usar datos móviles" : "Reintentar"))) {
                if (downloaded) LaunchInstaller();
                else StartCoroutine(DownloadUpdate(false));
            }
            if (GUI.Button(new Rect(x + boxWidth * .58f + 8, rect.y + 138, boxWidth * .42f - 30f, 51), "Ahora no"))
                dismissed = true;
            GUI.enabled = true;
            GUI.matrix = old;
#endif
        }

        IEnumerator DownloadUpdate(bool automatic) {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (downloading || candidate == null || downloaded) yield break;
            // Download on Wi-Fi automatically. Metered/mobile data is used only after a tap.
            if (automatic && !HasUnmeteredNetwork()) {
                waitingForWifi = true;
                information = "Esperando Wi-Fi para descargar la nueva versión.";
                yield break;
            }
            downloading = true;
            waitingForWifi = false;
            downloadFailed = false;
            information = "Preparando descarga segura…";
            using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject manager = null;
                try {
                    manager = activity.Call<AndroidJavaObject>("getSystemService", "download");
                    if (manager == null) throw new Exception("Servicio de descargas no disponible");
                    using (var dir = activity.Call<AndroidJavaObject>("getExternalFilesDir", "Download")) {
                        if (dir == null) throw new Exception("No hay almacenamiento de descargas");
                        string folder = dir.Call<string>("getAbsolutePath");
                        Directory.CreateDirectory(folder);
                        string filename = "Eldoria-code" + candidate.versionCode + "-" + candidate.sha256.Substring(0, 12) + ".apk";
                        apkPath = Path.Combine(folder, filename);
                        // Reattach an existing DownloadManager job rather than repeating the download.
                        // Never trust a partial or orphaned APK as installable.
                        downloadTicketPath = apkPath + ".download-id";
                        if (File.Exists(downloadTicketPath) &&
                            long.TryParse(File.ReadAllText(downloadTicketPath), out long knownId) && knownId > 0)
                            downloadId = knownId;
                        long oldBytes;
                        int oldStatus = DownloadStatus(manager, downloadId, out oldBytes);
                        if (oldStatus == 0 || oldStatus == 16) {
                            DropBadDownload(manager);
                            oldStatus = 0;
                        }
                        if (oldStatus == 0) {
                            if (File.Exists(apkPath)) File.Delete(apkPath);
                            using (var androidUri = new AndroidJavaClass("android.net.Uri"))
                            using (var uri = androidUri.CallStatic<AndroidJavaObject>("parse", candidate.apkUrl))
                            using (var request = new AndroidJavaObject("android.app.DownloadManager$Request", uri)) {
                                request.Call<AndroidJavaObject>("setDestinationInExternalFilesDir", activity, "Download", filename);
                                request.Call<AndroidJavaObject>("setMimeType", "application/vnd.android.package-archive");
                                request.Call<AndroidJavaObject>("setTitle", "Eldoria " + candidate.versionName);
                                request.Call<AndroidJavaObject>("setDescription", "Actualización oficial de prueba");
                                request.Call<AndroidJavaObject>("setNotificationVisibility", 1);
                                using (var network = request.Call<AndroidJavaObject>("setAllowedOverMetered", !automatic)) { }
                                downloadId = manager.Call<long>("enqueue", request);
                                File.WriteAllText(downloadTicketPath, downloadId.ToString());
                            }
                        }
                    }
                } catch (Exception error) {
                    information = "No se ha podido iniciar la descarga: " + error.Message;
                    downloadFailed = true;
                    Debug.LogWarning("ELDORIA_UPDATE_DOWNLOAD: " + error);
                    downloading = false;
                    if (manager != null) manager.Dispose();
                    yield break;
                }

                if (!downloaded) {
                    float started = Time.realtimeSinceStartup;
                    bool finished = false, failed = false;
                    while (!finished && !failed && Time.realtimeSinceStartup - started < 1800f) {
                        yield return new WaitForSecondsRealtime(1f);
                        try {
                            long received;
                            int status = DownloadStatus(manager, downloadId, out received);
                            if (status == 8) { finished = true; break; }
                            if (status == 0 || status == 16) { failed = true; break; }
                            information = "Descargando actualización: " +
                                Mathf.Clamp(Mathf.RoundToInt(100f * received / candidate.bytes), 0, 99) + " %";
                        } catch (Exception error) {
                            Debug.LogWarning("ELDORIA_UPDATE_POLL: " + error.Message);
                            failed = true;
                        }
                    }
                    if (!finished || failed) {
                        information = "Descarga interrumpida. Puedes reintentarlo.";
                        // A paused Wi-Fi-only DownloadManager job is preserved for the next opening.
                        if (failed) DropBadDownload(manager);
                        else waitingForWifi = automatic;
                        downloadFailed = failed || !automatic;
                        downloading = false;
                        manager.Dispose();
                        yield break;
                    }
                    information = "Verificando integridad de la actualización…";
                    // Stream SHA-256 on a worker: never freeze the Unity render loop on a 200MB APK.
                    var verification = System.Threading.Tasks.Task.Run(
                        () => EldoriaAndroidUpdatePolicy.VerifyApk(apkPath, candidate));
                    while (!verification.IsCompleted) yield return null;
                    downloaded = verification.Status == System.Threading.Tasks.TaskStatus.RanToCompletion &&
                                 verification.Result;
                    if (!downloaded) DropBadDownload(manager);
                }
                manager.Dispose();
            }
            downloading = false;
            if (!downloaded) {
                if (!waitingForWifi) downloadFailed = true;
                information = "La descarga no superó la verificación. No se instalará.";
                yield break;
            }
            // Show a user-controlled Install button; NEVER force an installer during startup.
            dismissed = false;
            waitingForWifi = false;
            downloadFailed = false;
            information = "APK descargada y verificada. Se instalará solo cuando lo confirmes.";
            Debug.Log("ELDORIA_UPDATE_READY versionCode=" + candidate.versionCode);
#else
            yield break;
#endif
        }

        void LaunchInstaller() {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!downloaded || candidate == null) return;
            try {
                // Verify again after any round-trip to Android settings or app backgrounding.
                if (!EldoriaAndroidUpdatePolicy.VerifyApk(apkPath, candidate)) {
                    downloaded = false;
                    information = "El archivo cambió. Vuelve a descargarlo.";
                    return;
                }
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var pm = activity.Call<AndroidJavaObject>("getPackageManager")) {
                    if (!pm.Call<bool>("canRequestPackageInstalls")) {
                        information = "Permite instalar Eldoria desde esta aplicación y vuelve aquí.";
                        using (var androidUri = new AndroidJavaClass("android.net.Uri"))
                        using (var packageUri = androidUri.CallStatic<AndroidJavaObject>("parse", "package:" + EldoriaAndroidUpdatePolicy.Package))
                        using (var settings = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_UNKNOWN_APP_SOURCES", packageUri)) {
                            activity.Call("startActivity", settings);
                        }
                        awaitingPermission = true;
                        return;
                    }
                    if (downloadId < 0) {
                        // Verified cached download from previous session: ask DownloadManager to reacquire it.
                        // Never expose a file:// URI to Android. Restart the download on the next tap.
                        downloaded = false;
                        information = "Pulsa Actualizar para preparar el instalador de Android.";
                        return;
                    }
                    using (var manager = activity.Call<AndroidJavaObject>("getSystemService", "download"))
                    using (var uri = manager.Call<AndroidJavaObject>("getUriForDownloadedFile", downloadId)) {
                        if (uri == null) throw new Exception("No se recibió un enlace seguro para el APK");
                        using (var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW")) {
                            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
                            intent.Call<AndroidJavaObject>("addFlags", 0x10000000 | 1); // NEW_TASK | GRANT_READ_URI_PERMISSION
                            activity.Call("startActivity", intent);
                            awaitingPermission = false;
                            information = "Confirma la instalación en Android. No desinstales Eldoria.";
                        }
                    }
                }
            } catch (Exception error) {
                Debug.LogWarning("ELDORIA_UPDATE_INSTALL: " + error);
                information = "Android no pudo abrir la instalación. Puedes reintentarlo.";
            }
#endif
        }
    }
}
