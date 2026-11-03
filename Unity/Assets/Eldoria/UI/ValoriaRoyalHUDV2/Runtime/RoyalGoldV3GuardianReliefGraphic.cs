using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// In-world canonical enemies represented in the royal manuscript as
    /// physically embossed, multi-plane ink-and-metal guardian reliefs.
    /// Unlike the rejected B1-B4 patrol photos this draws NO second landscape
    /// and contains NO photographic sprite, crop edge or circular peephole.
    /// The variant is selected only from the real EarlyRetentionRules.Stage.
    /// Pure static geometry: zero allocations/animation in Update, no touches.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3GuardianReliefGraphic : MaskableGraphic
    {
        [SerializeField] int stageStep;
        [SerializeField] bool paintedFigure;
        [SerializeField] bool breach;
        public int StageStep => stageStep;
        public bool PaintedFigure => paintedFigure;
        public bool Breach => breach;
        public void SetBreach(bool active)
        {
            if(breach==active)return;
            breach=active;
            SetVerticesDirty();
        }
        public void UsePaintedFigure(bool enabled)
        {
            if(paintedFigure==enabled)return;
            paintedFigure=enabled;
            SetVerticesDirty();
        }
        public void SetStage(int actualStep)
        {
            int next=actualStep==5||actualStep==10||actualStep==20||actualStep==25
                ?actualStep:0;
            if(stageStep==next)return;
            stageStep=next;
            SetVerticesDirty();
        }

        static Color C(float r,float g,float b,float a=1f)
            => new Color(r,g,b,a);
        static void Q(VertexHelper v,Vector2 lo,Vector2 hi,
            Color bottom,Color top)
        {
            int k=v.currentVertCount;
            v.AddVert(new Vector2(lo.x,lo.y),bottom,Vector2.zero);
            v.AddVert(new Vector2(hi.x,lo.y),bottom,Vector2.zero);
            v.AddVert(new Vector2(hi.x,hi.y),top,Vector2.zero);
            v.AddVert(new Vector2(lo.x,hi.y),top,Vector2.zero);
            v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);
        }
        static void P(VertexHelper v,Color c,params Vector2[] points)
        {
            int k=v.currentVertCount;
            foreach(var p in points)v.AddVert(p,c,Vector2.zero);
            for(int i=1;i<points.Length-1;i++)
                v.AddTriangle(k,k+i,k+i+1);
        }
        static void S(VertexHelper v,Vector2 a,Vector2 b,float width,Color c)
        {
            var d=b-a;if(d.sqrMagnitude<.001f)return;
            var n=new Vector2(-d.y,d.x).normalized*(width*.5f);
            P(v,c,a-n,a+n,b+n,b-n);
        }
        static void O(VertexHelper v,Vector2 center,float rx,float ry,
            Color middle,int sides=24)
        {
            int k=v.currentVertCount;
            v.AddVert(center,middle,Vector2.zero);
            for(int i=0;i<=sides;i++)
            {
                float a=i*Mathf.PI*2f/sides;
                var c=new Color(middle.r,middle.g,middle.b,0f);
                v.AddVert(center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),c,Vector2.zero);
                if(i>0)v.AddTriangle(k,k+i,k+i+1);
            }
        }

        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width<72f||r.height<72f)return;
            Vector2 A(float x,float y)=>new Vector2(r.xMin+r.width*x,r.yMin+r.height*y);
            float line=Mathf.Max(1f,Mathf.Min(r.width,r.height)*.009f);
            var black=C(.045f,.055f,.075f);
            var deep=C(.095f,.11f,.12f);
            var shadow=C(.15f,.16f,.17f);
            var steel=C(.43f,.43f,.40f);
            var pale=C(.73f,.68f,.56f);
            var gold=C(.93f,.71f,.36f);
            var copper=C(.48f,.31f,.21f);
            var green=C(.13f,.25f,.22f);
            var violet=C(.64f,.35f,.80f);
            bool hunter=stageStep==10;
            bool fortress=stageStep==5;
            bool endurance=stageStep==20;
            bool oath=stageStep==25;
            Color accent=breach?violet:hunter?violet:endurance?C(.63f,.75f,.77f):
                oath?C(.96f,.74f,.37f):gold;

            if(paintedFigure)
            {
                // B6: no inset window and no hard gothic arch behind the image.
                // Real cutout art shares the card material, its warm raking
                // light and the SAME stone footing as the rest of this room.
                O(v,A(.58f,.57f),r.width*.42f,r.height*.50f,
                    C(accent.r,accent.g,accent.b,breach?.24f:.15f));
                O(v,A(.47f,.37f),r.width*.24f,r.height*.42f,
                    C(.99f,.70f,.39f,breach?.065f:.105f));
                O(v,A(.50f,.155f),r.width*.40f,r.height*.115f,
                    C(.013f,.021f,.026f,.85f));
                O(v,A(.51f,.185f),r.width*.29f,r.height*.11f,
                    C(accent.r,accent.g,accent.b,breach?.36f:.21f));
                P(v,C(.13f,.15f,.16f,.74f),
                    A(.09f,.135f),A(.17f,.18f),A(.30f,.17f),
                    A(.47f,.18f),A(.59f,.17f),A(.81f,.175f),
                    A(.91f,.135f),A(.89f,.10f),A(.10f,.10f));
                S(v,A(.16f,.175f),A(.87f,.175f),line*.82f,
                    C(.86f,.64f,.33f,.56f));
                S(v,A(.12f,.102f),A(.90f,.102f),line*.88f,
                    C(.032f,.048f,.056f,.70f));
                return;
            }

            // Sculpted niche and stone plinth are part of the royal card.
            // Open arch instead of a framed square/photograph.
            O(v,A(.5f,.56f),r.width*.42f,r.height*.48f,
                new Color(accent.r,accent.g,accent.b,.15f));
            P(v,C(.015f,.028f,.042f,.80f),
                A(.12f,.12f),A(.12f,.70f),A(.21f,.85f),
                A(.39f,.95f),A(.61f,.95f),A(.79f,.85f),
                A(.88f,.70f),A(.88f,.12f));
            S(v,A(.13f,.18f),A(.13f,.70f),line*2f,C(.62f,.46f,.25f,.80f));
            S(v,A(.87f,.18f),A(.87f,.70f),line*2f,C(.62f,.46f,.25f,.80f));
            S(v,A(.13f,.70f),A(.28f,.90f),line*1.7f,C(.77f,.59f,.31f,.72f));
            S(v,A(.87f,.70f),A(.72f,.90f),line*1.7f,C(.77f,.59f,.31f,.72f));
            S(v,A(.28f,.90f),A(.50f,.98f),line*1.5f,C(.83f,.65f,.33f,.68f));
            S(v,A(.72f,.90f),A(.50f,.98f),line*1.5f,C(.83f,.65f,.33f,.68f));
            O(v,A(.50f,.23f),r.width*.34f,r.height*.075f,C(.00f,.01f,.018f,.65f));

            // Layer I: cloak and steel-backed mass with leg shadows.
            P(v,black,A(.35f,.65f),A(.65f,.66f),A(.81f,.16f),A(.19f,.16f));
            P(v,green,A(.39f,.69f),A(.50f,.64f),A(.38f,.20f),A(.24f,.19f));
            P(v,copper,A(.52f,.65f),A(.64f,.68f),A(.75f,.20f),A(.56f,.20f));
            P(v,shadow,A(.34f,.45f),A(.48f,.47f),A(.46f,.17f),A(.32f,.17f));
            P(v,steel,A(.53f,.46f),A(.66f,.47f),A(.67f,.17f),A(.52f,.17f));
            Q(v,A(.29f,.15f),A(.48f,.195f),deep,steel);
            Q(v,A(.52f,.15f),A(.71f,.195f),deep,steel);

            // Layer II: highly legible armored chest, engraved metal facets.
            P(v,black,A(.31f,.66f),A(.42f,.75f),A(.62f,.75f),
                A(.72f,.64f),A(.64f,.42f),A(.36f,.42f));
            P(v,steel,A(.36f,.68f),A(.47f,.74f),A(.58f,.72f),
                A(.64f,.64f),A(.60f,.49f),A(.40f,.49f));
            P(v,pale,A(.40f,.68f),A(.50f,.74f),A(.59f,.68f),A(.51f,.62f));
            P(v,shadow,A(.39f,.59f),A(.60f,.59f),A(.57f,.50f),A(.42f,.50f));
            S(v,A(.40f,.58f),A(.60f,.58f),line*2f,gold);
            S(v,A(.50f,.72f),A(.50f,.53f),line*.85f,C(.92f,.78f,.56f,.81f));
            P(v,accent,A(.50f,.66f),A(.54f,.60f),A(.50f,.54f),A(.46f,.60f));

            // Layer III: asymmetric pauldrons, hands and raised breastplate.
            P(v,black,A(.30f,.70f),A(.42f,.77f),A(.45f,.65f),
                A(.36f,.56f),A(.25f,.59f));
            P(v,steel,A(.29f,.70f),A(.40f,.76f),A(.42f,.67f),
                A(.35f,.61f),A(.26f,.64f));
            P(v,pale,A(.27f,.68f),A(.39f,.74f),A(.38f,.70f),A(.29f,.66f));
            P(v,black,A(.60f,.76f),A(.73f,.70f),A(.77f,.60f),
                A(.65f,.56f),A(.57f,.66f));
            P(v,steel,A(.62f,.74f),A(.72f,.69f),A(.73f,.62f),
                A(.65f,.61f),A(.59f,.67f));
            P(v,pale,A(.62f,.73f),A(.72f,.68f),A(.71f,.66f),A(.61f,.71f));
            P(v,shadow,A(.28f,.62f),A(.35f,.61f),A(.34f,.42f),A(.25f,.39f));
            P(v,steel,A(.66f,.60f),A(.73f,.62f),A(.79f,.42f),A(.69f,.39f));
            O(v,A(.27f,.41f),r.width*.052f,r.height*.038f,steel);
            O(v,A(.74f,.41f),r.width*.055f,r.height*.035f,steel);

            // Helmet, glowing eyes and crown line; protagonist stays central.
            P(v,black,A(.38f,.78f),A(.39f,.89f),A(.49f,.96f),
                A(.60f,.90f),A(.62f,.79f),A(.57f,.72f),A(.42f,.72f));
            P(v,steel,A(.41f,.82f),A(.42f,.90f),A(.50f,.94f),
                A(.57f,.90f),A(.59f,.82f),A(.54f,.76f),A(.45f,.76f));
            P(v,pale,A(.43f,.90f),A(.50f,.95f),A(.56f,.91f),A(.51f,.88f));
            Q(v,A(.445f,.812f),A(.557f,.828f),black,black);
            S(v,A(.45f,.813f),A(.49f,.813f),line,accent);
            S(v,A(.52f,.813f),A(.56f,.813f),line,accent);
            S(v,A(.50f,.93f),A(.50f,.82f),line*.75f,C(.97f,.84f,.60f,.74f));

            // True illustrated role profile, no fake lore or game statistics.
            if(fortress || oath)
            {
                // Embossed tall shield physically in front of the right arm.
                P(v,black,A(.61f,.71f),A(.84f,.73f),A(.84f,.39f),
                    A(.73f,.28f),A(.60f,.39f));
                P(v,C(.48f,.38f,.28f),A(.63f,.70f),A(.81f,.70f),
                    A(.80f,.41f),A(.73f,.33f),A(.64f,.41f));
                P(v,pale,A(.64f,.69f),A(.80f,.69f),A(.75f,.64f),A(.65f,.63f));
                S(v,A(.73f,.64f),A(.73f,.36f),line*1.9f,gold);
                S(v,A(.64f,.54f),A(.81f,.54f),line*1.6f,gold);
                P(v,accent,A(.73f,.61f),A(.77f,.54f),A(.73f,.46f),A(.69f,.54f));
            }
            if(endurance)
            {
                // Massive endurance hammer and armored gauntlet.
                S(v,A(.78f,.20f),A(.80f,.88f),line*3f,copper);
                Q(v,A(.68f,.78f),A(.92f,.92f),black,steel);
                Q(v,A(.70f,.80f),A(.90f,.91f),steel,pale);
                S(v,A(.72f,.88f),A(.89f,.88f),line*1.4f,gold);
            }
            else if(hunter)
            {
                // Relentless assault, two drawn asymmetric knives.
                S(v,A(.22f,.32f),A(.17f,.73f),line*2f,steel);
                P(v,pale,A(.14f,.74f),A(.19f,.90f),A(.22f,.72f));
                S(v,A(.78f,.32f),A(.88f,.78f),line*2f,steel);
                P(v,pale,A(.84f,.79f),A(.89f,.94f),A(.92f,.78f));
                O(v,A(.50f,.78f),r.width*.33f,r.height*.24f,C(.58f,.24f,.75f,.10f));
            }
            else
            {
                // The non-boss patrol has a spear, the final warden a sword.
                S(v,A(.23f,.22f),A(.20f,.90f),line*2.3f,copper);
                P(v,pale,A(.16f,.86f),A(.20f,.99f),A(.25f,.86f));
                if(oath)
                {
                    S(v,A(.25f,.25f),A(.17f,.74f),line*1.7f,steel);
                    P(v,pale,A(.15f,.72f),A(.17f,.89f),A(.22f,.74f));
                }
            }

            // Warm rim light, grounded stone dais and heraldic edge. No layer
            // intercepts Combatir, XP, Formar marcha or Volver.
            S(v,A(.28f,.19f),A(.75f,.19f),line*1.3f,C(.92f,.68f,.32f,.60f));
            Q(v,A(.13f,.13f),A(.87f,.17f),C(.13f,.12f,.11f),C(.49f,.38f,.22f));
            S(v,A(.14f,.16f),A(.86f,.16f),line*.8f,C(.98f,.80f,.46f,.75f));
            S(v,A(.20f,.12f),A(.80f,.12f),line*1.1f,black);
        }
    }
}
