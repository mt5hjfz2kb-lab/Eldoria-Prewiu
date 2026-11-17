using System;
using System.Collections;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Runtime-only diagnostics in the *isolated* Valoria V6 Web performance trial.
    /// This component never uses ICommandGateway, saves, rewards, or construction
    /// commands. The optional built-quarry toggle is a labelled visual QA fixture.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class City2DWebPerfProbe : MonoBehaviour
    {
        private City2DStateBridge bridge;
        private CityBuilding quarry;
        private bool quarryBuiltForQa;
        private float sampleSeconds;
        private float sampleLongestMs;
        private int frames;
        private int framesOver50Ms;
        private int framesOver100Ms;
        private string readout = "Midiendo FPS...";
        private GUIStyle labelStyle;

        private IEnumerator Start()
        {
            bridge = FindFirstObjectByType<City2DStateBridge>();
            if (bridge == null || bridge.IsLiveBound)
                throw new InvalidOperationException("Web Perf trial must be isolated and demo-only");
            foreach (var item in FindObjectsByType<CityBuilding>(FindObjectsSortMode.None))
                if (item.BuildingId == "cantera") quarry = item;
            if (quarry == null)
                throw new InvalidOperationException("QA quarry not present");
            bridge.LevelsChanged += OnDemoStageChanged;
            yield return null;
            bridge.SetDemoBastionLevel(5);
            Debug.Log("CIUDAD2D_WEBPERF_V6_READY engine=Unity trial=isolated stage=5 " +
                      "quarry=available physical_iphone=NOT_YET_VERIFIED");
        }

        private void OnDemoStageChanged()
        {
            // Changing the storyboard Bastion stage must not carry a fake
            // constructed quarry across chapters.
            quarryBuiltForQa = false;
        }

        private void Update()
        {
            float frameTime = Time.unscaledDeltaTime;
            if (frameTime <= 0f || frameTime > 1f) return;
            frames++;
            sampleSeconds += frameTime;
            sampleLongestMs = Mathf.Max(sampleLongestMs, frameTime * 1000f);
            if (frameTime > .05f) framesOver50Ms++;
            if (frameTime > .10f) framesOver100Ms++;
            if (sampleSeconds < 5f) return;
            float fps = frames / sampleSeconds;
            readout = "FPS 5 s: " + fps.ToString("F1") +
                      "   Max frame: " + sampleLongestMs.ToString("F0") + " ms" +
                      "\nTirones >50 ms: " + framesOver50Ms +
                      "   >100 ms: " + framesOver100Ms;
            Debug.Log("CIUDAD2D_WEBPERF_V6_SAMPLE fps_avg5=" + fps.ToString("F1") +
                      " max_frame_ms=" + sampleLongestMs.ToString("F0") +
                      " over50=" + framesOver50Ms + " over100=" + framesOver100Ms +
                      " stage=" + (bridge != null ? bridge.DemoBastionLevel : 0) +
                      " scene=Valoria2D_demo_only");
            sampleSeconds = 0f;
            sampleLongestMs = 0f;
            frames = framesOver50Ms = framesOver100Ms = 0;
        }

        private void OnGUI()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.wordWrap = true;
                labelStyle.normal.textColor = new Color(.98f,.88f,.68f,1f);
            }
            labelStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * .020f),14,26);
            float margin = Mathf.Max(12f, Screen.width * .03f);
            float width = Mathf.Min(255f,Screen.width-margin*2f);
            float safeTop = Screen.height - Screen.safeArea.yMax;
            float top = Mathf.Max(65f, safeTop + 65f);
            GUI.Box(new Rect(Screen.width-width-margin,top,width,103f),"");
            GUI.Label(new Rect(Screen.width-width-margin+8f,top+3f,width-16f,63f),
                      "VALORIA V6 · PRUEBA WEB\n"+readout,labelStyle);
            if (bridge != null && bridge.DemoBastionLevel == 5 && quarry != null)
            {
                string title = quarryBuiltForQa ? "Cantera QA: restaurar" : "Cantera QA: verla construida";
                if (GUI.Button(new Rect(Screen.width-width-margin+8f,top+70f,width-16f,29f),title))
                {
                    quarryBuiltForQa = !quarryBuiltForQa;
                    quarry.SetAppearance(quarryBuiltForQa
                        ? new City2DPlotView(City2DPlotPhase.Built,1,5)
                        : new City2DPlotView(City2DPlotPhase.Available,0,5));
                    foreach (var gate in FindObjectsByType<City2DPremiumShadowGate>(
                        FindObjectsSortMode.None)) gate.Refresh();
                    Debug.Log("CIUDAD2D_WEBPERF_V6_QUARRY_QA fixture_built="+quarryBuiltForQa+
                              " persistent_state_unchanged=true");
                }
            }
        }
        private void OnDestroy()
        {
            if (bridge != null) bridge.LevelsChanged -= OnDemoStageChanged;
        }
    }
}
