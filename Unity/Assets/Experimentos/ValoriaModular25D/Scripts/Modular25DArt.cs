using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// Original, reproducible raster source. Every prop uses one palette, one isometric
    /// projection, one light direction (top-left), common foot pivots and 2x antialiasing.
    /// This is a first functional art candidate; never mistake it for approved final art.
    /// </summary>
    internal static class Modular25DArt
    {
        static readonly Color32 Ink = C(36, 42, 48);
        static readonly Color32 Deep = C(59, 69, 77);
        static readonly Color32 Stone = C(130, 139, 133);
        static readonly Color32 StoneLight = C(184, 182, 160);
        static readonly Color32 StoneDark = C(90, 103, 107);
        static readonly Color32 Slate = C(58, 78, 90);
        static readonly Color32 SlateLight = C(93, 112, 120);
        static readonly Color32 Timber = C(119, 77, 54);
        static readonly Color32 TimberLight = C(170, 120, 74);
        static readonly Color32 Grass = C(93, 123, 87);
        static readonly Color32 GrassLight = C(119, 143, 94);
        static readonly Color32 Earth = C(117, 102, 81);
        static readonly Color32 EarthLight = C(151, 133, 97);
        static readonly Color32 Glow = C(241, 173, 88);
        static readonly Color32 Mystic = C(115, 93, 145);
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        static Color32 C(byte r, byte g, byte b, byte a = 255) { return new Color32(r,g,b,a); }
        static Vector2 P(float x,float y) { return new Vector2(x,y); }

        static Sprite Make(string key, int w, int h, float ppu, Vector2 pivot, Action<Paint> draw)
        {
            if (Cache.TryGetValue(key, out var s)) return s;

            // Optional production artwork: each original Blender bake is an
            // independent Sprite in this experiment's own Resources folder.
            // The procedural fallback lets the sandbox compile and test even
            // before artistic QA accepts a new family. Do not use image backplates.
            var baked = Resources.Load<Sprite>("ValoriaModular25D/" + key);
            if (baked != null)
            {
                Cache.Add(key, baked);
                return baked;
            }
            var paint = new Paint(w,h);
            draw(paint);
            var pixels = paint.Resolve();
            var texture = new Texture2D(w,h,TextureFormat.RGBA32,false,false);
            texture.name = "ELDORIA_modular_" + key;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(pixels);
            texture.Apply(false,true);
            s = Sprite.Create(texture,new Rect(0,0,w,h),pivot,ppu,0,SpriteMeshType.FullRect);
            s.name = key;
            Cache.Add(key,s);
            return s;
        }

        public static Sprite Tile(int kind, int variation)
        {
            string key = "ground_" + kind + "_" + variation;
            return Make(key,132,68,64f,P(.5f,.5f),a =>
            {
                // The tile has no outline or base: no elevated platform effect.
                a.Poly(Grass,P(1,34),P(66,67),P(131,34),P(66,1));
                int seed = 7 + kind * 43 + variation * 31;
                var random = new System.Random(seed);
                // Only interior noise: all tile boundaries retain the same grass value.
                for (int i=0;i<60;i++)
                {
                    float x=15 + (float)random.NextDouble()*103;
                    float y=12 + (float)random.NextDouble()*44;
                    if (Mathf.Abs(x-66)/66f + Mathf.Abs(y-34)/33f > .78f) continue;
                    var c = i%4==0 ? GrassLight : C(77,109,81,170);
                    a.Ellipse(x,y,2+(float)random.NextDouble()*5,1.1f,c);
                }
                if (kind==1)
                {
                    // A road must actually meet opposite diamond edges.
                    // Organic interior variation, fixed seam endpoints across tiles.
                    float bend=variation*1.7f;
                    a.Poly(Earth,P(14,42),P(29,49),P(66,46+bend),
                        P(107,17),P(120,25),P(108,31),P(70,58+bend),P(22,52));
                    a.Poly(C(151,133,97,135),P(26,45),P(66,43+bend),
                        P(108,22),P(111,28),P(68,53+bend),P(27,49));
                    for(int i=0;i<8;i++)
                    {
                        float x=29+(float)random.NextDouble()*72;
                        float y=53f-(x-28f)*.36f+(float)random.NextDouble()*9f;
                        a.Ellipse(x,y,1.5f,1.1f,StoneLight);
                    }
                }
                else if (kind==2)
                {
                    // Soil is only a ground-level construction parcel.
                    a.Poly(EarthLight,P(28,31),P(61,48),P(108,34),P(69,17),P(31,24));
                    a.Poly(C(145,129,100,135),P(42,31),P(62,42),P(94,32),P(67,23));
                    for(int i=0;i<8;i++)
                    {
                        float x=44+(float)random.NextDouble()*49;
                        float y=25+(float)random.NextDouble()*15;
                        a.Ellipse(x,y,1.5f,1f,StoneLight);
                    }
                }
                else if(kind==3)
                {
                    a.Ellipse(62,34,40,17,C(82,108,83,180));
                    a.Ellipse(69,34,24,10,C(131,126,99,150));
                }
            });
        }

        public static Sprite Bastion()
        {
            return Make("bastion_i",480,416,96f,P(.5f,.095f),a =>
            {
                // Abandoned imperial masonry with a modest, asymmetrically repaired keep.
                a.Poly(Ink,P(62,88),P(78,237),P(126,269),P(160,250),P(174,310),P(239,354),
                    P(300,305),P(316,239),P(374,245),P(424,194),P(416,82),P(277,34),P(131,44));
                // Main wall: left/west face darker than illuminated north plane.
                a.Poly(StoneDark,P(105,90),P(111,225),P(237,274),P(239,106),P(179,68));
                a.Poly(Stone,P(238,274),P(371,239),P(369,95),P(239,106));
                a.Poly(StoneLight,P(117,223),P(236,263),P(363,232),P(238,192));
                a.Poly(Stone,P(118,221),P(238,183),P(238,101),P(116,137));
                a.Poly(StoneDark,P(239,182),P(360,226),P(360,135),P(239,100));
                // Uneven warm-lit upper keep, roof with weighted slate eaves.
                a.Poly(Ink,P(159,245),P(181,318),P(232,352),P(309,320),P(331,257),P(255,230));
                a.Poly(Slate,P(171,247),P(224,333),P(243,340),P(244,249));
                a.Poly(SlateLight,P(244,249),P(242,341),P(300,311),P(321,256));
                a.Line(P(225,331),P(300,307),4,StoneLight);
                a.Poly(StoneDark,P(186,240),P(248,257),P(309,239),P(310,185),P(249,165),P(184,195));
                a.Poly(StoneLight,P(185,238),P(245,255),P(245,168),P(183,195));
                a.Poly(Stone,P(246,253),P(311,235),P(310,185),P(246,167));
                // Broken east remnant preserves Valoria's post-catastrophe identity.
                a.Poly(Ink,P(348,233),P(370,263),P(399,249),P(409,207),P(420,195),P(413,93),P(364,67),P(332,92));
                a.Poly(StoneDark,P(346,226),P(368,251),P(390,240),P(400,209),P(397,113),P(349,96));
                a.Poly(Stone,P(391,235),P(403,217),P(407,104),P(391,113));
                a.Poly(StoneLight,P(349,226),P(369,247),P(386,237),P(366,218));
                a.Poly(Deep,P(349,225),P(378,206),P(370,194),P(344,201));
                // Cut stone joints, kept within faces.
                for (int y=112;y<203;y+=20)
                {
                    a.Line(P(121,y),P(221,y-28),2,C(82,93,92,190));
                    a.Line(P(256,y-5),P(350,y+22),2,C(83,96,94,160));
                }
                // Reinforced front gate with arch and imperfect timber doors.
                a.Poly(Ink,P(194,89),P(195,135),P(205,157),P(238,168),P(267,155),P(279,136),P(279,76),P(241,55));
                a.Poly(Timber,P(207,85),P(206,133),P(214,147),P(241,156),P(242,58));
                a.Poly(C(93,58,47),P(245,59),P(245,154),P(260,147),P(267,132),P(266,79));
                for (int k=0;k<4;k++)a.Line(P(209+k*10,75),P(209+k*10,138),2,TimberLight);
                a.Line(P(210,97),P(264,96),5,Deep);
                a.Ellipse(235,100,4,5,Glow);
                // Timber repairs and diagonal bracing: no fake raised rectangular base.
                a.Line(P(95,66),P(95,198),10,Timber);
                a.Line(P(96,119),P(158,82),6,TimberLight);
                a.Line(P(319,100),P(372,166),7,Timber);
                a.Line(P(372,166),P(375,85),8,Timber);
                a.Line(P(329,108),P(399,105),9,TimberLight);
                // Small, consistent amber windows.
                a.Poly(Ink,P(203,213),P(203,231),P(215,239),P(226,232),P(226,207));
                a.Poly(Glow,P(208,216),P(208,228),P(216,233),P(221,230),P(221,215));
                a.Line(P(214,214),P(215,232),3,Timber);
                a.Poly(Ink,P(271,204),P(273,224),P(285,230),P(296,219),P(296,197));
                a.Poly(Glow,P(278,206),P(278,219),P(286,224),P(290,216),P(290,202));
                // Valoria imperial crest with one restrained fissure-violet accent.
                a.Poly(C(61,78,104),P(137,183),P(166,171),P(167,149),P(152,139),P(136,155));
                a.Line(P(150,179),P(151,149),3,Glow);
                a.Line(P(143,161),P(159,159),3,Glow);
                a.Line(P(394,192),P(388,178),3,Mystic);
                a.Line(P(388,178),P(397,166),2,Mystic);
                // Steps are isolated stones, not a baked plinth.
                a.Poly(StoneDark,P(200,54),P(241,42),P(278,54),P(256,66),P(221,70));
                a.Poly(StoneLight,P(213,48),P(239,38),P(261,45),P(243,53));
            });
        }

        public static Sprite Sawmill()
        {
            return Make("sawmill_i",362,299,96f,P(.5f,.085f),a =>
            {
                // Distinct low workshop: open saw frame, stacked logs and slate lean-to.
                a.Poly(Ink,P(43,84),P(68,184),P(162,261),P(281,211),P(320,171),P(310,73),P(192,28),P(90,44));
                a.Poly(StoneDark,P(89,88),P(108,164),P(193,195),P(197,71),P(149,54));
                a.Poly(EarthLight,P(198,193),P(299,158),P(299,83),P(198,69));
                // Heavy asymmetrical roof and a purposeful overhang.
                a.Poly(Ink,P(64,174),P(153,270),P(191,276),P(304,227),P(330,174),P(199,215));
                a.Poly(Slate,P(76,175),P(156,260),P(188,266),P(195,214));
                a.Poly(SlateLight,P(197,213),P(188,266),P(296,219),P(314,178));
                a.Line(P(155,263),P(296,217),4,StoneLight);
                // Timber posts, internal shadow, functional production silhouette.
                a.Poly(Deep,P(113,115),P(189,137),P(271,110),P(266,70),P(187,46),P(112,77));
                for (int x=0;x<3;x++)
                {
                    int xx=113+x*73;
                    a.Line(P(xx,73),P(xx+1,157-x*9),9,Timber);
                    a.Line(P(xx-1,149-x*9),P(xx+39,95),5,TimberLight);
                }
                a.Line(P(110,116),P(268,84),7,TimberLight);
                // Authentic saw bench and mechanical tooth/handle, not random crates.
                a.Poly(Timber,P(127,91),P(185,114),P(246,95),P(190,73));
                a.Line(P(174,115),P(172,142),5,Deep);
                a.Line(P(172,142),P(218,132),5,StoneLight);
                a.Line(P(218,132),P(235,112),3,StoneLight);
                for(int x=175;x<215;x+=8)a.Line(P(x,127),P(x+3,121),2,StoneDark);
                a.Ellipse(236,104,16,9,TimberLight);
                // Low, discrete stack of timber at the actual ground contact.
                for(int j=0;j<3;j++)
                {
                    int yy=51+j*13;
                    a.Poly(Timber,P(31,yy+11),P(102,yy+23),P(158,yy+12),P(92,yy));
                    a.Line(P(41,yy+10),P(96,yy+19),3,TimberLight);
                    a.Ellipse(157,yy+12,7,8,TimberLight);
                    a.Ellipse(158,yy+12,3,4,Earth);
                }
                a.Poly(StoneLight,P(278,83),P(304,94),P(321,78),P(294,65));
                a.Line(P(92,72),P(110,82),5,StoneDark);
            });
        }

        public static Sprite SawmillSite()
        {
            return Make("sawmill_available",362,299,96f,P(.5f,.085f),a =>
            {
                // Ground-level site, not an elevated rectangle or a ghost building.
                a.Poly(C(97,88,72,110),P(47,61),P(107,102),P(184,115),
                    P(299,77),P(324,51),P(233,29),P(99,35));
                a.Poly(C(143,131,103,125),P(64,66),P(124,98),P(186,106),P(282,76),
                    P(222,48),P(121,46));
                for(int j=0;j<3;j++)
                {
                    int x=77+j*92;
                    a.Line(P(x,51),P(x+3,81),8,Timber);
                    a.Line(P(x-7,81),P(x+11,81),4,TimberLight);
                }
                a.Poly(StoneDark,P(109,49),P(133,62),P(154,53),P(132,41));
                a.Poly(StoneLight,P(194,67),P(223,80),P(240,64),P(216,52));
                a.Line(P(135,72),P(215,86),7,Timber);
                a.Line(P(145,75),P(210,89),3,TimberLight);
            });
        }

        public static Sprite SawmillWorksite()
        {
            return Make("sawmill_under_construction",362,299,96f,P(.5f,.085f),a =>
            {
                // Different silhouette from the finished workshop. Only partial
                // timberwork and scaffolding are present while the real UTC job runs.
                a.Poly(C(113,99,79,135),P(49,70),P(126,112),P(224,112),P(320,71),P(197,40));
                a.Line(P(109,64),P(108,184),13,Timber);
                a.Line(P(276,67),P(272,179),13,Timber);
                a.Line(P(108,181),P(272,176),14,TimberLight);
                a.Line(P(106,102),P(277,96),11,Timber);
                a.Line(P(107,170),P(264,88),7,TimberLight);
                a.Line(P(267,164),P(123,86),8,Timber);
                a.Line(P(151,86),P(168,232),7,TimberLight);
                a.Line(P(168,229),P(214,213),7,TimberLight);
                a.Line(P(213,211),P(237,87),7,TimberLight);
                a.Poly(Ink,P(136,174),P(165,241),P(181,247),P(213,232),P(215,217),P(173,233));
                a.Poly(Slate,P(147,174),P(166,234),P(181,237),P(181,204));
                a.Poly(SlateLight,P(181,204),P(181,236),P(209,225),P(211,216));
                for(int j=0;j<3;j++)
                {
                    float y=58+j*13;
                    a.Poly(Timber,P(27,y),P(90,y+18),P(142,y+12),P(84,y-6));
                    a.Line(P(35,y+2),P(88,y+16),3,TimberLight);
                }
                a.Poly(StoneDark,P(252,54),P(282,67),P(303,58),P(284,42));
            });
        }

        /// <summary>Independent original deciduous game sprite baked from
        /// the same fixed projection and palette as the buildings.</summary>
        public static Sprite Oak(int variant)
        {
            variant=Mathf.Clamp(variant,0,1);
            // Explicitly no fake 3D geometry fallback: the forest sprite is
            // part of the present original-art package, not a city backplate.
            var oak=Resources.Load<Sprite>("ValoriaModular25D/oak_"+variant);
            if(oak==null)Debug.LogWarning("[Modular25D] Missing oak_"+variant+
                " — fallback pine until original forest assets are imported.");
            return oak ?? Tree(variant);
        }

        public static Sprite Tree(int variant)
        {
            return Make("pine_"+variant,170,245,96f,P(.5f,.06f),a =>
            {
                var canopy = variant==0?C(62,97,83):variant==1?C(64,101,91):C(76,110,89);
                var lighter = variant==0?C(116,145,113):C(113,151,122);
                a.Line(P(86,18),P(85,176),14,Timber);
                a.Line(P(87,25),P(92,167),5,TimberLight);
                a.Poly(Ink,P(32,71),P(83,207),P(133,83),P(119,72),P(147,50),P(92,69),P(37,51));
                a.Poly(canopy,P(35,80),P(84,202),P(125,82),P(110,74),P(139,54),P(85,79),P(39,57));
                a.Poly(lighter,P(82,185),P(81,89),P(50,67),P(40,78));
                a.Poly(canopy,P(51,130),P(85,231),P(118,133),P(101,121),P(130,104),P(88,122),P(40,105));
                a.Poly(lighter,P(85,218),P(85,143),P(49,112),P(56,133));
                a.Poly(C(48,80,75),P(82,139),P(114,104),P(105,88),P(89,99));
                if(variant==2)a.Poly(C(104,117,109),P(84,220),P(100,184),P(85,194));
            });
        }

        /// <summary>Small independently authored painted meadow-tuft sprite.</summary>
        public static Sprite Tuft(int variant)
        {
            return Resources.Load<Sprite>("ValoriaModular25D/tuft_"+Mathf.Clamp(variant,0,1))
                ?? Bush(Mathf.Clamp(variant,0,1));
        }

        public static Sprite Bush(int variant)
        {
            return Make("bush_"+variant,114,79,96f,P(.5f,.15f),a =>
            {
                a.Ellipse(55,28,47,17,Ink);
                a.Ellipse(33,37,27,21,C(76,107,85));
                a.Ellipse(60,48,32,29,C(66,102,83));
                a.Ellipse(85,35,24,19,C(84,118,87));
                a.Ellipse(47,52,13,10,GrassLight);
                a.Ellipse(76,48,9,8,GrassLight);
            });
        }

        public static Sprite Rock()
        {
            return Make("stone",105,66,96f,P(.5f,.15f),a =>
            {
                a.Poly(Ink,P(11,19),P(26,48),P(65,58),P(94,35),P(93,17),P(56,9));
                a.Poly(StoneDark,P(16,21),P(28,45),P(45,37),P(57,11));
                a.Poly(StoneLight,P(28,45),P(64,55),P(88,35),P(47,36));
                a.Poly(Stone,P(47,36),P(88,35),P(89,19),P(57,12));
                a.Line(P(57,51),P(53,37),3,C(80,91,92));
            });
        }

        public static Sprite Villager(int frame)
        {
            return Make("villager_walk_"+frame,78,120,96f,P(.5f,.07f),a =>
            {
                bool step=frame%2==0;
                a.Line(P(34,23),P(step?26:39,7),9,Ink);
                a.Line(P(46,23),P(step?52:41,8),9,Ink);
                a.Line(P(36,24),P(step?28:40,12),5,StoneDark);
                a.Line(P(44,24),P(step?51:42,13),5,StoneDark);
                a.Poly(Ink,P(23,78),P(35,91),P(53,90),P(62,70),P(58,26),P(29,26));
                a.Poly(C(110,86,82),P(28,77),P(39,86),P(50,83),P(56,28),P(33,28));
                a.Line(P(30,69),P(22+(step?4:0),43),7,Ink);
                a.Line(P(54,69),P(58-(step?4:0),42),7,Ink);
                a.Line(P(31,66),P(27,43),5,TimberLight);
                a.Line(P(52,66),P(55,44),5,TimberLight);
                a.Ellipse(42,92,18,21,Ink);
                a.Ellipse(42,92,15,18,C(191,153,118));
                a.Poly(Slate,P(21,97),P(30,111),P(50,114),P(61,100),P(47,105),P(33,105));
                a.Poly(Deep,P(26,101),P(41,119),P(55,102));
                a.Line(P(31,56),P(52,53),4,TimberLight);
            });
        }

        /// <summary>Soft shadow pigment, the same material family as the olive
        /// meadow. Reusable under every building, actor and tree.</summary>
        public static Sprite Shadow(int width,int height)
        {
            string key="contact_"+width+"_"+height;
            if(Cache.TryGetValue(key,out var sprite))return sprite;
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false,false){
                name="Eldoria soft ground shadow "+width+"x"+height,
                filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color32[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                float nx=2f*(x+.5f)/(float)width-1f;
                float ny=2f*(y+.5f)/(float)height-1f;
                float radial=nx*nx+ny*ny;
                float fade=Mathf.Pow(Mathf.Clamp01(1f-radial),2.1f);
                pixels[y*width+x]=new Color32(27,39,24,(byte)Mathf.RoundToInt(142f*fade));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false,true);
            sprite=Sprite.Create(texture,new Rect(0,0,width,height),
                new Vector2(.5f,.5f),96f,0,SpriteMeshType.FullRect);
            sprite.name=key;
            Cache[key]=sprite;
            return sprite;
        }

        sealed class Paint
        {
            readonly int w,h,scale=2;
            readonly Color32[] pixels;
            public Paint(int width,int height){w=width;h=height;pixels=new Color32[w*h*scale*scale];}
            void Pixel(int x,int y,Color32 color)
            {
                int sw=w*scale,sh=h*scale;
                if(x<0||y<0||x>=sw||y>=sh) return;
                int i=y*sw+x;
                float alpha=color.a/255f;
                Color32 old=pixels[i];
                float oa=old.a/255f;
                float dest=alpha+oa*(1f-alpha);
                if(dest<.001f){pixels[i]=new Color32(0,0,0,0);return;}
                pixels[i]=new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt((color.r*alpha+old.r*oa*(1-alpha))/dest),0,255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt((color.g*alpha+old.g*oa*(1-alpha))/dest),0,255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt((color.b*alpha+old.b*oa*(1-alpha))/dest),0,255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(dest*255f),0,255));
            }
            public void Poly(Color32 color,params Vector2[] points)
            {
                float minx=float.MaxValue,maxx=float.MinValue,miny=float.MaxValue,maxy=float.MinValue;
                foreach(var p in points){minx=Mathf.Min(minx,p.x);maxx=Mathf.Max(maxx,p.x);miny=Mathf.Min(miny,p.y);maxy=Mathf.Max(maxy,p.y);}
                int x0=Mathf.Clamp(Mathf.FloorToInt(minx*scale),0,w*scale-1),x1=Mathf.Clamp(Mathf.CeilToInt(maxx*scale),0,w*scale-1);
                int y0=Mathf.Clamp(Mathf.FloorToInt(miny*scale),0,h*scale-1),y1=Mathf.Clamp(Mathf.CeilToInt(maxy*scale),0,h*scale-1);
                for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
                {
                    float px=(x+.5f)/scale,py=(y+.5f)/scale;
                    bool inside=false;
                    for(int i=0,j=points.Length-1;i<points.Length;j=i++)
                        if ((points[i].y>py)!=(points[j].y>py) &&
                           px<(points[j].x-points[i].x)*(py-points[i].y)/(points[j].y-points[i].y)+points[i].x)inside=!inside;
                    if(inside)Pixel(x,y,color);
                }
            }
            public void Ellipse(float cx,float cy,float rx,float ry,Color32 color)
            {
                int x0=Mathf.Max(0,Mathf.FloorToInt((cx-rx)*scale)),x1=Mathf.Min(w*scale-1,Mathf.CeilToInt((cx+rx)*scale));
                int y0=Mathf.Max(0,Mathf.FloorToInt((cy-ry)*scale)),y1=Mathf.Min(h*scale-1,Mathf.CeilToInt((cy+ry)*scale));
                if(rx<=0||ry<=0)return;
                for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
                {float dx=((x+.5f)/scale-cx)/rx,dy=((y+.5f)/scale-cy)/ry;if(dx*dx+dy*dy<=1)Pixel(x,y,color);}
            }
            public void Line(Vector2 a,Vector2 b,float diameter,Color32 color)
            {
                Vector2 v=b-a;
                int steps=Mathf.CeilToInt(v.magnitude*scale*1.5f);
                if(steps<1){Ellipse(a.x,a.y,diameter*.5f,diameter*.5f,color);return;}
                for(int i=0;i<=steps;i++){Vector2 p=Vector2.Lerp(a,b,(float)i/steps);Ellipse(p.x,p.y,diameter*.5f,diameter*.5f,color);}
            }
            public Color32[] Resolve()
            {
                var output=new Color32[w*h];
                int stride=w*scale;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float r=0,g=0,b=0,a=0;
                    for(int sy=0;sy<scale;sy++)for(int sx=0;sx<scale;sx++)
                    {
                        var c=pixels[(y*scale+sy)*stride+x*scale+sx];
                        float alpha=c.a/255f;
                        r+=c.r*alpha;g+=c.g*alpha;b+=c.b*alpha;a+=alpha;
                    }
                    float avgA=a/(scale*scale);
                    output[y*w+x]=avgA<.0001f?new Color32(0,0,0,0):
                        new Color32((byte)Mathf.RoundToInt(r/a),(byte)Mathf.RoundToInt(g/a),
                            (byte)Mathf.RoundToInt(b/a),(byte)Mathf.RoundToInt(avgA*255f));
                }
                return output;
            }
        }
    }
}
