using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Mobile touch drags must reach Unity Input System rather than being consumed as
    /// browser page navigation. Runs on the REAL WebGL export, not on static mock pages.
    /// Browser reproduction: D09 real candidate QA run 37912348373, canvas touch-action:auto,
    /// Unity mouse pan telemetry present while CDP touch pan telemetry was absent.
    /// </summary>
    public static class EldoriaWebGLTouchActionPostprocess
    {
        public const string Marker = "eldoria-webgl-touch-action-v1";

        [PostProcessBuild(1000)]
        public static void InjectTouchAction(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.WebGL) return;
            if (string.IsNullOrEmpty(pathToBuiltProject))
                throw new InvalidOperationException("Missing WebGL build destination");

            var file = Path.Combine(pathToBuiltProject, "index.html");
            if (!File.Exists(file))
                throw new FileNotFoundException("Cannot certify WebGL touch input without index.html", file);

            var html = File.ReadAllText(file);
            if (html.Contains(Marker)) return; // safe for repeated postprocess callbacks
            var headEnd = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            if (headEnd < 0) throw new InvalidDataException("WebGL template is missing </head>; refusing partial input patch");

            const string style = "<style id=\"eldoria-webgl-touch-action-v1\">"
                + "html,body{overscroll-behavior:none;}"
                + "#unity-canvas, #unity-container, canvas{touch-action:none!important;-ms-touch-action:none;overscroll-behavior:contain;}"
                + "</style>\n";
            html = html.Insert(headEnd, style);
            File.WriteAllText(file, html);
            if (!File.ReadAllText(file).Contains(Marker))
                throw new IOException("WebGL touch-action post-build verification failed");
        }
    }
}
