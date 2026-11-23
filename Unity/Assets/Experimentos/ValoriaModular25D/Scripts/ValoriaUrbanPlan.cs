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
