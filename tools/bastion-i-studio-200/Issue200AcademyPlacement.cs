using System;
using System.Collections.Generic;
using UnityEngine;
namespace Eldoria.Experimentos.ValoriaModular25D
{
    // Stage V-only offsets measured and certified in the isolated editor experiment.
    // Missing data means original placement. No gameplay or texture changes.
    public sealed class Issue200AcademyPlacement
    {
        [Serializable] public sealed class Move { public string name; public float x,y,dx,dy; }
        [Serializable] public sealed class Layout { public Move[] moves; }
#if UNITY_EDITOR
        public static Layout ProofLayout;
        public static bool ProofMode;
#endif
        readonly List<(SpriteRenderer renderer,Vector3 foot)> props=new List<(SpriteRenderer,Vector3)>();
        Layout saved;
        bool loaded;
        public void Register(SpriteRenderer r){props.Add((r,r.transform.localPosition));}
        public void Apply(bool enabled)
        {
            if(!loaded){
                loaded=true;
                var data=Resources.Load<TextAsset>("Issue200AcademyLayout");
                if(data)saved=JsonUtility.FromJson<Layout>(data.text);
            }
            Layout layout=saved;
#if UNITY_EDITOR
            if(ProofMode)layout=ProofLayout;
#endif
            foreach(var p in props){
                if(!p.renderer)continue;
                var foot=p.foot;
                if(enabled&&layout!=null&&layout.moves!=null){
                    foreach(var m in layout.moves){
                        if(p.renderer.sprite.name==m.name&&Mathf.Abs(p.foot.x-m.x)<.001f&&Mathf.Abs(p.foot.y-m.y)<.001f){
                            foot+=new Vector3(m.dx,m.dy,0);break;
                        }
                    }
                }
                p.renderer.transform.localPosition=foot;
                p.renderer.sortingOrder=Modular25DWorld.FootOrder(foot.y);
            }
        }
    }
}
