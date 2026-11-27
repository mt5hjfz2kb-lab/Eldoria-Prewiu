using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    // All coordinates refer to the projected ground plane, before camera projection.
    public sealed class ValoriaUrbanPlan
    {
        public sealed class Parcel
        {
            public readonly string Id;
            public readonly Vector2 Foot, Door, GroundSize;
            public Parcel(string id, Vector2 foot, Vector2 doorOffset, Vector2 size)
            {
                Id = id;
                Foot = foot;
                Door = foot + doorOffset;
                GroundSize = size;
            }
        }

        public static readonly ValoriaUrbanPlan City = new ValoriaUrbanPlan(true);
        public readonly Rect CameraTravel = Rect.MinMaxRect(-13,-10,13,9);
        public sealed class Prop
        {
            public readonly string Asset;
            public readonly Vector2 Foot;
            public readonly float Scale;
            public readonly int District, FirstLevel, LastLevel;
            public Prop(int district, string asset, Vector2 origin, float x, float y, float scale, int firstLevel=1,int lastLevel=5)
            {
                District=district;
                FirstLevel=firstLevel;
                LastLevel=lastLevel;
                Asset=asset;
                Foot=origin+new Vector2(x,y);
                Scale=scale;
            }
        }

        // Projected stone landing immediately outside the keep's actual threshold.
        public Vector2 CourtCenter => Keep.Door + new Vector2(0,-.46f);
        public Vector2 CourtRadii => new Vector2(Keep.GroundSize.x*.54f,Keep.GroundSize.y*.39f);

        public Vector2 MillBench => Mill.Foot+new Vector2(1.34f,-.86f);
        public Rect MillSelection => new Rect(Mill.Foot+new Vector2(-1.84f,-.53f),new Vector2(3.76f,3.64f));

        public Vector2[] FreightRoad => new[]{Freight,Vector2.Lerp(Freight,Junction,.5f)+Vector2.left*.6f,Junction};
        // These future entrances and centre lines reserve access without drawing unfinished roads.
        public Vector2[][] GrowthApproaches => new[]{
            new[]{Freight,new Vector2(-3.3f,-5.08f),new Vector2(-6.1f,-4.05f),new Vector2(Growth[0].xMax,Growth[0].center.y)},
            new[]{Approach(Barracks)[1],new Vector2(6.85f,-2.22f),new Vector2(8.77f,-1.92f),new Vector2(9.55f,Growth[1].yMin)}
        };
        public Vector2[] ReturnWalk(Parcel plot)
        {
            var walk=Approach(plot);
            return new[]{walk[0],walk[1],walk[2],walk[3],walk[2],walk[1]};
        }
        // The existing castle courier can now use the mature postern passage.
        public Vector2[] PosternWalk()
        {
            var walk=Approach(Keep);
            var lane=PostigoLane(2.24f);
            var entry=PostigoLane(0f);
            var tunnel=UpperRow+new Vector2(-.10f,.88f);
            var exit=BoroughStreets[5][3];
            return new[]{walk[0],walk[1],walk[2],walk[3],CourtCenter,lane,entry,
                UpperRow,tunnel,exit,tunnel,UpperRow,entry,lane,CourtCenter,walk[2],walk[1]};
        }
        public Vector2 UpperRow => Keep.Foot + new Vector2(-4.38f,.76f);
        public Vector2 UpperCourt => Keep.Foot + new Vector2(-2.87f,3.98f);
        public Vector2 OrchardBoundary => new Vector2((UpperCourt.x+Growth[1].center.x)*.5f, Growth[1].yMax+1.93f);
        public Vector2 WestFront => Keep.Door + new Vector2(-1.39f,-3.02f);
        public Vector2 EastFront => Keep.Door + new Vector2(2.12f,-2.61f);
        public Vector2 WorkshopRow => Junction + new Vector2(-3.50f,-1.34f);
        public Vector2 MarketRow => Freight + new Vector2(4.02f,2.38f);

        // A unit along the painted frontage maps to a shorter, oblique ground axis.
        // Door stations use the same basis as the elevation and its ground shadow.
        public static readonly Vector2 PostigoAxis = new Vector2(.88f,-.21f);
        public static readonly Vector2 PortalesAxis = new Vector2(.92f,-.15f);
        public Vector2 PostigoLane(float station) => UpperRow + PostigoAxis*station + Vector2.down*.32f;
        public Vector2 PortalesLane(float station) => WorkshopRow + PortalesAxis*station + Vector2.down*.22f;

        public Vector2[][] BoroughStreets => new[]{
            // The postern lane returns to the mill, enclosing a built block.
            new[]{CourtCenter,PostigoLane(2.24f),PostigoLane(0),PostigoLane(-2.10f),
                UpperRow+new Vector2(-2.78f,-.96f),Mill.Door+new Vector2(-1.28f,.47f),Mill.Door},
            // Opposite the mill yard, the workshop arcade opens onto the well.
            new[]{Mill.Door,PortalesLane(-2.20f),PortalesLane(-.30f),
                PortalesLane(2.05f),Junction},
            // The weighing house fronts the supply lane between well and granary.
            new[]{Junction,MarketRow+new Vector2(-2.93f,-.21f),MarketRow+new Vector2(-1.45f,-.55f),
                MarketRow+new Vector2(1.62f,-.61f),Granary.Door+new Vector2(1.86f,-.68f),Granary.Door},
            new[]{CourtCenter,Keep.Foot+new Vector2(2.42f,-.69f),Keep.Foot+new Vector2(3.83f,.22f),
                Keep.Foot+new Vector2(5.78f,.24f),Barracks.Door+new Vector2(1.82f,.16f),Barracks.Door},
            // Rise from the postern, follow the stepped thresholds and turn down
            // the east flank. The short northern return stays behind the keep.
            new[]{UpperRow+new Vector2(-2.02f,-.5f),UpperRow+new Vector2(-3.03f,.42f),
                UpperCourt+new Vector2(-2.03f,.03f),UpperCourt+new Vector2(.48f,-.48f),
                Keep.Foot+new Vector2(2.42f,3.05f),Keep.Foot+new Vector2(5.42f,1.91f),
                Keep.Foot+new Vector2(5.78f,.24f)},
            // Passage below the central arch, connecting the street to the upper return.
            new[]{PostigoLane(0),UpperRow,UpperRow+new Vector2(-.10f,.88f),
                UpperCourt+new Vector2(-2.03f,.03f)}
        };

        public Prop[] Environment => new[]{
            new Prop(0,"MuroDerruido",Keep.Foot,-2.88f,.40f,.86f,1,4),
            new Prop(0,"ArbolAntiguo",Keep.Foot,3.55f,2.85f,.88f,1,4),
            new Prop(0,"ArboledaAntigua",Keep.Foot,-3.65f,1.65f,.73f,1,4),
            new Prop(0,"MatojosFlores",Keep.Foot,-2.85f,-.23f,.58f),
            new Prop(0,"PozoPiedra",Junction,.94f,-.20f,.75f),
            new Prop(0,"CasasDelCamino",Keep.Door,-1.62f,-2.55f,.86f,1,4),
            new Prop(0,"AceraZapateros",WestFront,0,0,1f,5),
            new Prop(2,"CasasDeOficio",Keep.Door,2.38f,-2.72f,.87f,2,4),
            new Prop(2,"CasaDelGremio",EastFront,0,0,1f,5),
            new Prop(0,"LindeDelVergel",OrchardBoundary,0,0,1f,5),
            new Prop(3,"FrenteTejedores",Junction,-2.15f,-1.57f,.96f,4,4),
            new Prop(0,"ManzanaDelAdarve",UpperCourt,0f,0f,.88f,5),
            new Prop(2,"CasasDeOficio",Keep.Foot,3.62f,2.44f,.87f,5),
            new Prop(2,"FrenteTejedores",Keep.Foot,4.03f,.79f,.84f,5),
            new Prop(0,"ViviendaDelPostigo",UpperRow,-1.49f,.31f,1f,5),
            new Prop(0,"CalleDelPostigo",UpperRow,0,0,1f,5),
            new Prop(0,"ToneleriaDelPostigo",UpperRow,1.79f,-.27f,1f,5),
            new Prop(3,"PortalesDeOficios",WorkshopRow,0,0,1f,5),
            new Prop(3,"CasasDelPeso",MarketRow,0,0,1f,5),
            new Prop(3,"LonjaGrano",Freight,1.76f,.63f,.66f,3),
            new Prop(0,"MatojosFlores",Junction,1.22f,1.02f,.39f),
            new Prop(0,"ArboledaAntigua",Mill.Foot,-2.62f,1.72f,.85f),
            new Prop(0,"ArbolAntiguo",Mill.Foot,-3.15f,.46f,.65f),
            new Prop(1,"CarroMadera",Mill.Foot,1.66f,-.68f,.72f),
            new Prop(1,"VallaLenador",Mill.Foot,2.03f,-1.34f,.65f),
            new Prop(2,"ArmeriaDelPatio",Barracks.Foot,1.12f,-2.12f,.88f),
            new Prop(2,"VallaLenador",Barracks.Foot,-2.05f,-.55f,.52f),
            new Prop(3,"HuertosValoria",Granary.Foot,-2.35f,.2f,.78f),
            new Prop(3,"CarroMadera",Granary.Foot,1.86f,-.71f,.62f),
            new Prop(3,"PuestoDeAbastos",Granary.Foot,.73f,-2.12f,.88f,4),
            new Prop(4,"CorteCaliza",Quarry.Foot,-1.85f,1.30f,1f),
            new Prop(4,"CarroMadera",Quarry.Foot,-1.7f,-.82f,.60f)
        };

        public readonly Parcel Keep, Mill, Barracks, Granary, Quarry;
        public readonly Vector2 Junction, Freight;
        public readonly Rect[] Growth;
        public ValoriaUrbanPlan(bool terraces)
        {
            Keep = new Parcel("bastion", new Vector2(0,3), new Vector2(0,-.25f),new Vector2(5,2.4f));
            Mill = new Parcel("sawmill", terraces ? new Vector2(-4.65f,.05f) : new Vector2(5,-1),
                new Vector2(-.45f,-.2f),new Vector2(3.8f,1.7f));
            Barracks = new Parcel("barracks", terraces ? new Vector2(5.85f,1.28f) : new Vector2(-6,0),
                new Vector2(-.35f,-.2f),new Vector2(3.8f,1.9f));
            Granary = new Parcel("granary", terraces ? new Vector2(3.95f,-5.65f) : new Vector2(-3,-6),
                new Vector2(.35f,-.2f),new Vector2(3.8f,2));
            Quarry = new Parcel("stoneworks", terraces ? new Vector2(-7.35f,-7.75f) : new Vector2(8,-6),
                new Vector2(.55f,-.2f),new Vector2(4,2.1f));
            Junction = terraces ? new Vector2(.64f,-1.72f) : new Vector2(-.8f,-1.15f);
            Freight = terraces ? new Vector2(-.65f,-5.9f) : new Vector2(.3f,-6.9f);
            Growth = terraces ? new[]{new Rect(-14,-5.55f,5,3),new Rect(8,3,5,3)}
                              : new[]{new Rect(-12,-7,5,3),new Rect(5,5,5,3)};
        }

        public Vector2[] Approach(Parcel plot)
        {
            Vector2 start = plot == Granary || plot == Quarry ? Freight : Junction;
            Vector2 apron = plot.Door + new Vector2(0,-.85f);
            return new[]{start,new Vector2(Mathf.Lerp(start.x,apron.x,.60f),
                Mathf.Lerp(start.y,apron.y,.40f)),apron,plot.Door};
        }
    }
}
