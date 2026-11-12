using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    // Presentation only. Disabled by default; no gameplay, scene or save commands.
    [DefaultExecutionOrder(31000), DisallowMultipleComponent]
    public sealed class BreachWatchTypography : MonoBehaviour
    {
        public enum Role { Inscription, Narrative, Instrument }
        public const string QueryFlag = "eldoria_typography=source-sans3";
        const string Fonts = "VigiliaArt/Fonts/";
        sealed class Original
        {
            public Font Font, AppliedFont;
            public FontStyle Style, AppliedStyle;
            public int Size, Min, Max;
            public bool BestFit;
            public TextAnchor Alignment;
            public Vector3 Scale;
            public float LineSpacing;
            public bool OwnRoom;
            public Vector2 OriginalRoom,AppliedRoom;
        }
        readonly Dictionary<Text, Original> originals = new Dictionary<Text, Original>();
        readonly List<Text> stale = new List<Text>();
        Font inscription, inscriptionBold, narrative, narrativeBold, instrument, instrumentBold;
        public bool CandidateEnabled { get; private set; }
        public int AppliedCount => originals.Count;

        public static bool IsOptedIn(string url)
        {
            if(string.IsNullOrEmpty(url))return false;
            int q=url.IndexOf('?');if(q<0)return false;
            int fragment=url.IndexOf('#');if(fragment>=0&&fragment<q)return false;
            int end=url.IndexOf('#',q);
            string query=end<0?url.Substring(q+1):url.Substring(q+1,end-q-1);
            foreach(string item in query.Split('&'))if(item==QueryFlag)return true;
            return false;
        }

        public bool SetCandidate(bool enabled)
        {
            if(!enabled){Restore();return true;}
            inscription=Resources.Load<Font>(Fonts+"CormorantGaramond-Regular");
            inscriptionBold=Resources.Load<Font>(Fonts+"CormorantGaramond-SemiBold");
            narrative=Resources.Load<Font>(Fonts+"LibreBaskerville-Regular");
            narrativeBold=Resources.Load<Font>(Fonts+"LibreBaskerville-Bold");
            instrument=Resources.Load<Font>(Fonts+"SourceSans3-Regular");
            instrumentBold=Resources.Load<Font>(Fonts+"SourceSans3-Semibold");
            if(!inscription||!inscriptionBold||!narrative||!narrativeBold||!instrument||!instrumentBold)
            {
                Restore();Debug.LogError("VIGILIA_TYPOGRAPHY_IMPORTED_FONT_MISSING");return false;
            }
            CandidateEnabled=true;SynchronizeNow();return true;
        }

        // Roles are based on control identity/semantics, never on font size.
        // All actual Selectables are instruments, including navigation/actions.
        public static Role ResolveRole(Text text)
        {
            string name=text.name.ToLowerInvariant();
            // The quest tablet itself is a Button, but its inscription is
            // still a title. Do not turn every descendant of it into data.
            if(name=="player name"||name=="real building name"||
               name=="actual chapter"||name=="actual objective"||
               name=="compact engraved cost heading"||name=="reserved talent availability"||
               name=="hero card name"||name.Contains("eldoria heraldry"))return Role.Inscription;
            if(name.Contains("category label")||name=="training or expedition breakdown")
                return Role.Instrument;
            if(name=="unit profile and composition")
                foreach(char character in text.text??"")
                    if(char.IsDigit(character))return Role.Instrument;
            // A mailbox row is clickable, but its preview remains prose.
            if(name.StartsWith("mensaje resumen"))return Role.Narrative;
            for(Transform p=text.transform;p!=null;p=p.parent)
            {
                if(p.GetComponent<Selectable>())return Role.Instrument;
                if(p.GetComponent<Canvas>())break;
            }
            if(name.Contains("quantity")||name.Contains("amount")||name.Contains("cost")||
               name.Contains("metric")||name.Contains("pending troops")||name.Contains("event date")||
               name.Contains("historical composition")||name=="recursos actuales"||name=="página"||
               name=="vida"||name=="ataque"||name=="defensa"||name=="poder"||
               name.Contains("progress")||name.Contains("power")||name.Contains("timer")||
               name.Contains("stat")||name.Contains("level")||name.Contains("count")||
               name.Contains("value")||name.Contains("capacity")||name.Contains("duration"))
                return Role.Instrument;
            if(name.Contains("title")||name.Contains("heading")||name.Contains("chapter")||
               name=="player name"||name=="real building name"||name=="actual objective"||name.Contains("hero name"))
                return Role.Inscription;
            return Role.Narrative;
        }

        static void RememberLayout(Text text, Original state)
        {
            // Other owners retain responsive reflow. We never change these
            // properties; remember their current values, not an obsolete orientation.
            state.Size=text.fontSize;state.Min=text.resizeTextMinSize;
            state.Max=text.resizeTextMaxSize;state.BestFit=text.resizeTextForBestFit;
            state.Alignment=text.alignment;state.Scale=text.rectTransform.localScale;
            state.LineSpacing=text.lineSpacing;
        }

        public void SynchronizeNow()
        {
            if(!CandidateEnabled)return;
            stale.Clear();
            foreach(var pair in originals)
                if(!pair.Key||!pair.Key.isActiveAndEnabled||!pair.Key.gameObject.activeInHierarchy||
                   !pair.Key.transform.IsChildOf(transform))
                    stale.Add(pair.Key);
            foreach(var text in stale)
            {if(text)RestoreOne(text,originals[text]);originals.Remove(text);}
            foreach(var text in GetComponentsInChildren<Text>(false))
            {
                if(!text||!text.isActiveAndEnabled)continue;
                if(!originals.TryGetValue(text,out var state))
                {
                    state=new Original {Font=text.font,Style=text.fontStyle};
                    originals.Add(text,state);
                }
                else
                {
                    // A canonical screen may legitimately refresh its font.
                    if(text.font!=state.AppliedFont)state.Font=text.font;
                    if(text.fontStyle!=state.AppliedStyle)state.Style=text.fontStyle;
                }
                RememberLayout(text,state);
                if(state.OwnRoom&&text.rectTransform.sizeDelta!=state.AppliedRoom)
                    state.OwnRoom=false; // canonical reflow supplies the new baseline
                bool heavy=state.Style==FontStyle.Bold||state.Style==FontStyle.BoldAndItalic||
                    (state.Font&&state.Font.name.IndexOf("bold",StringComparison.OrdinalIgnoreCase)>=0);
                var role=ResolveRole(text);
                state.AppliedFont=role==Role.Inscription?(heavy?inscriptionBold:inscription):
                    role==Role.Narrative?(heavy?narrativeBold:narrative):
                    (heavy?instrumentBold:instrument);
                bool italic=state.Style==FontStyle.Italic||state.Style==FontStyle.BoldAndItalic;
                state.AppliedStyle=italic?FontStyle.Italic:FontStyle.Normal;
                text.font=state.AppliedFont;text.fontStyle=state.AppliedStyle;
                if(text.name=="Actual objective")
                {
                    var rect=text.rectTransform;
                    float needed=text.preferredHeight+2f;
                    if(needed>rect.rect.height)
                    {
                        if(!state.OwnRoom)state.OriginalRoom=rect.sizeDelta;
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,needed);
                        state.AppliedRoom=rect.sizeDelta;state.OwnRoom=true;
                    }
                }
            }
        }

        static void RestoreOne(Text text, Original state)
        {
            // Do not overwrite a canonical owner's subsequent restoration.
            if(text.font==state.AppliedFont)text.font=state.Font;
            if(text.fontStyle==state.AppliedStyle)text.fontStyle=state.Style;
            if(state.OwnRoom&&text.rectTransform.sizeDelta==state.AppliedRoom)
                text.rectTransform.sizeDelta=state.OriginalRoom;
            // Size, best-fit, alignment, spacing and scale were never mutated.
        }
        public void Restore()
        {
            foreach(var pair in originals)if(pair.Key)RestoreOne(pair.Key,pair.Value);
            originals.Clear();CandidateEnabled=false;
        }
        void LateUpdate(){SynchronizeNow();}
        void OnDisable(){Restore();}
        void OnDestroy(){Restore();}
    }
}
