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
            public readonly int District;
            public Prop(int district, string asset, Vector2 origin, float x, float y, float scale)
            {
                District=district;
                Asset=asset;
                Foot=origin+new Vector2(x,y);
                Scale=scale;
            }
        }

        // Projected stone landing immediately outside the keep's actual threshold.
        public Vector2 CourtCenter => Keep.Door + new Vector2(0,-.46f);
        public Vector2 CourtRadii => new Vector2(Keep.GroundSize.x*.54f,Keep.GroundSize.y*.39f);

        public Vector2 MillBench => Mill.Foot+new Vector2(1.02f,-.46f);
        public Rect MillSelection => new Rect(Mill.Foot+new Vector2(-1.84f,-.53f),new Vector2(3.76f,3.64f));

        public Vector2[] FreightRoad => new[]{Freight,Vector2.Lerp(Freight,Junction,.5f)+Vector2.left*.6f,Junction};
        public Vector2[] ReturnWalk(Parcel plot)
        {
            var walk=Approach(plot);
            return new[]{walk[0],walk[1],walk[2],walk[3],walk[2],walk[1]};
        }
        public Prop[] Environment => new[]{
            new Prop(0,"MuroDerruido",Keep.Foot,-2.62f,2.95f,.95f),
            new Prop(0,"ArbolAntiguo",Keep.Foot,3.55f,2.85f,.88f),
            new Prop(0,"ArboledaAntigua",Keep.Foot,-3.65f,1.65f,.73f),
            new Prop(0,"MatojosFlores",Keep.Foot,-2.85f,-.23f,.58f),
            new Prop(0,"PozoPiedra",Junction,.70f,.42f,.66f),
            new Prop(0,"CasasDelCamino",Junction,-2.73f,-.08f,1f),
            new Prop(0,"MatojosFlores",Junction,1.22f,1.02f,.39f),
            new Prop(0,"ArboledaAntigua",Mill.Foot,-2.62f,1.72f,.85f),
            new Prop(0,"ArbolAntiguo",Mill.Foot,-3.15f,.46f,.65f),
            new Prop(1,"CarroMadera",Mill.Foot,1.66f,-.68f,.72f),
            new Prop(1,"VallaLenador",Mill.Foot,2.03f,-1.34f,.65f),
            new Prop(2,"ArmeriaDelPatio",Barracks.Foot,1.08f,-2.05f,.90f),
            new Prop(2,"VallaLenador",Barracks.Foot,-2.05f,-.55f,.52f),
            new Prop(3,"HuertosValoria",Granary.Foot,-2.35f,.2f,.78f),
            new Prop(3,"CarroMadera",Granary.Foot,1.86f,-.71f,.62f),
            new Prop(3,"PuestoDeAbastos",Granary.Foot,2.76f,-1.45f,.95f),
            new Prop(4,"MuroDerruido",Quarry.Foot,2.17f,.82f,.58f),
            new Prop(4,"CarroMadera",Quarry.Foot,-1.7f,-.82f,.60f)
        };

        public readonly Parcel Keep, Mill, Barracks, Granary, Quarry;
        public readonly Vector2 Junction, Freight;
        public readonly Rect[] Growth;
        public ValoriaUrbanPlan(bool terraces)
        {
            Keep = new Parcel("bastion", new Vector2(0,3), new Vector2(0,-.25f),new Vector2(5,2.4f));
            Mill = new Parcel("sawmill", terraces ? new Vector2(-4,-1) : new Vector2(5,-1),
                new Vector2(-.45f,-.2f),new Vector2(3.8f,1.7f));
            Barracks = new Parcel("barracks", terraces ? new Vector2(6,1) : new Vector2(-6,0),
                new Vector2(-.35f,-.2f),new Vector2(3.8f,1.9f));
            Granary = new Parcel("granary", terraces ? new Vector2(3,-5) : new Vector2(-3,-6),
                new Vector2(.35f,-.2f),new Vector2(3.8f,2));
            Quarry = new Parcel("stoneworks", terraces ? new Vector2(-7,-7) : new Vector2(8,-6),
                new Vector2(.55f,-.2f),new Vector2(4,2.1f));
            Junction = terraces ? new Vector2(2.2f,-1.7f) : new Vector2(-.8f,-1.15f);
            Freight = terraces ? new Vector2(-1,-6.5f) : new Vector2(.3f,-6.9f);
            Growth = terraces ? new[]{new Rect(-14,-8,5,3),new Rect(8,3,5,3)}
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
