#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    // Real imported sprite alpha, projected through actual object transforms.
    // For this identity-rotation orthographic camera, XY gaps are screen gaps
    // multiplied by pixels/world unit. Convex hulls are conservative, not exact occlusion.
    public static class Issue200AcademySolver
    {
        sealed class Shape {
            public SpriteRenderer r; public Vector2[] hull; public Vector2 offset;
            public Vector2 Foot => (Vector2)r.transform.position+offset;
        }
        [Serializable] public sealed class Settings { public float wellLimit,marketCartLimit,quarryCartLimit,targetMobilePx; }
        [Serializable] public sealed class Metric {
            public string target, obstacle, result;
            public float beforeWorld, afterWorld, beforeMobilePx, afterMobilePx, dx,dy, routeBefore,routeAfter;
        }
        [Serializable] public sealed class Report {
            public string method="Imported alpha >128 convex hull SAT; actual transforms, camera fixed; conservative separation not occlusion percentage.";
            public float pixelsPerWorldUnit;
            public Metric[] metrics;
            public int moved;
        }
        static float Cross(Vector2 o,Vector2 a,Vector2 b)=>(a.x-o.x)*(b.y-o.y)-(a.y-o.y)*(b.x-o.x);
        static Vector2[] Hull(List<Vector2> p){
            var s=p.Distinct().OrderBy(q=>q.x).ThenBy(q=>q.y).ToArray();
            if(s.Length<3)throw new Exception("Empty alpha silhouette");
            var l=new List<Vector2>();var u=new List<Vector2>();
            foreach(var q in s){while(l.Count>=2&&Cross(l[l.Count-2],l[l.Count-1],q)<=0)l.RemoveAt(l.Count-1);l.Add(q);}
            for(int i=s.Length-1;i>=0;i--){var q=s[i];while(u.Count>=2&&Cross(u[u.Count-2],u[u.Count-1],q)<=0)u.RemoveAt(u.Count-1);u.Add(q);}
            l.RemoveAt(l.Count-1);u.RemoveAt(u.Count-1);l.AddRange(u);return l.ToArray();
        }
        static Color32[] ReadAlpha(Texture2D src){
            var temp=RenderTexture.GetTemporary(src.width,src.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            var old=RenderTexture.active;Texture2D copy=null;
            try{
                Graphics.Blit(src,temp);RenderTexture.active=temp;
                copy=new Texture2D(src.width,src.height,TextureFormat.RGBA32,false,true);
                copy.ReadPixels(new Rect(0,0,src.width,src.height),0,0);copy.Apply(false,false);
                return copy.GetPixels32();
            }finally{RenderTexture.active=old;if(copy)UnityEngine.Object.DestroyImmediate(copy);RenderTexture.ReleaseTemporary(temp);}
        }
        static Shape Build(SpriteRenderer r){
            var s=r.sprite;var tex=s.texture;
            // StreetElevation warps Portales; use its expanded bounds conservatively.
            if(s.name=="PortalesDeOficios"||s.packed){
                var b=r.bounds;return new Shape{r=r,hull=new[]{new Vector2(b.min.x,b.min.y),new Vector2(b.max.x,b.min.y),new Vector2(b.max.x,b.max.y),new Vector2(b.min.x,b.max.y)}};
            }
            Rect tr;
            try{tr=s.textureRect;}catch(UnityException){
                var b=r.bounds;return new Shape{r=r,hull=new[]{new Vector2(b.min.x,b.min.y),new Vector2(b.max.x,b.min.y),new Vector2(b.max.x,b.max.y),new Vector2(b.min.x,b.max.y)}};
            }
            var data=ReadAlpha(tex);int left=Mathf.RoundToInt(tr.xMin),bottom=Mathf.RoundToInt(tr.yMin),w=Mathf.RoundToInt(tr.width),h=Mathf.RoundToInt(tr.height);
            var pts=new List<Vector2>();
            for(int y=0;y<h;y++){
                int min=w,max=-1;
                for(int x=0;x<w;x++)if(data[(bottom+y)*tex.width+left+x].a>128){min=Math.Min(min,x);max=Math.Max(max,x);}
                if(max<0)continue;
                foreach(int x in new[]{min,max+1}){
                    var local=new Vector3((x+s.textureRectOffset.x-s.pivot.x)/s.pixelsPerUnit,(y+.5f+s.textureRectOffset.y-s.pivot.y)/s.pixelsPerUnit,0);
                    if(r.flipX)local.x=-local.x;if(r.flipY)local.y=-local.y;
                    pts.Add(r.transform.TransformPoint(local));
                }
            }
            return new Shape{r=r,hull=Hull(pts)};
        }
        static float Gap(Shape a,Vector2 ao,Shape b){
            float best=float.NegativeInfinity;
            foreach(var h in new[]{a.hull,b.hull})for(int i=0;i<h.Length;i++){
                var e=h[(i+1)%h.Length]-h[i];if(e.sqrMagnitude<1e-10f)continue;
                var axis=new Vector2(-e.y,e.x).normalized;
                float al=float.PositiveInfinity,ah=float.NegativeInfinity,bl=al,bh=ah;
                foreach(var q in a.hull){float v=Vector2.Dot(q+ao,axis);al=Mathf.Min(al,v);ah=Mathf.Max(ah,v);}
                foreach(var q in b.hull){float v=Vector2.Dot(q+b.offset,axis);bl=Mathf.Min(bl,v);bh=Mathf.Max(bh,v);}
                best=Mathf.Max(best,Mathf.Max(bl-ah,al-bh));
            }
            return best;
        }
        static float RouteGap(Vector2 p,List<Vector2[]> routes){
            float best=float.PositiveInfinity;
            foreach(var route in routes)for(int i=1;i<route.Length;i++){
                var a=route[i-1];var b=route[i];var v=b-a;
                float t=v.sqrMagnitude<1e-8f?0:Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude);
                best=Mathf.Min(best,Vector2.Distance(p,a+t*v));
            }
            return best;
        }
        static bool Named(Shape q,string n)=>q.r.sprite.name==n;
        public static Issue200AcademyPlacement.Layout Solve(Modular25DWorld world,string path){
            var settings=JsonUtility.FromJson<Settings>(File.ReadAllText(Path.Combine(path,"academy-settings.json")));
            if(settings==null||settings.wellLimit<.25f||settings.wellLimit>.80f||settings.marketCartLimit<.30f||settings.marketCartLimit>1f||settings.quarryCartLimit<.25f||settings.quarryCartLimit>.85f||settings.targetMobilePx<3f||settings.targetMobilePx>6f)throw new Exception("Invalid director limits");
            var cam=world.ViewCamera;var plan=ValoriaUrbanPlan.City;
            if(!cam.orthographic||Quaternion.Angle(cam.transform.rotation,Quaternion.identity)>.01f)throw new Exception("Unexpected projection");
            float px=844f/(2*Modular25DCameraInput.HomeSizeForAspect(390f/844f));
            var core=new[]{world.BastionRenderer,world.SawmillRenderer,world.BarracksRenderer,world.GranaryRenderer,world.StoneworksRenderer};
            var shapes=world.GetComponentsInChildren<SpriteRenderer>(false).Where(r=>r.sprite&&(core.Contains(r)||r.name.StartsWith("Original Eldoria art | "))).Select(Build).ToList();
            var routes=new List<Vector2[]>{plan.FreightRoad,plan.Approach(plan.Keep),plan.Approach(plan.Mill),plan.Approach(plan.Barracks),plan.Approach(plan.Granary),plan.Approach(plan.Quarry)};
            routes.AddRange(plan.BoroughStreets);
            routes.Add(plan.PosternWalk());routes.Add(plan.ReturnWalk(plan.Granary));routes.Add(plan.ReturnWalk(plan.Quarry));
            var moves=new List<Issue200AcademyPlacement.Move>();var reports=new List<Metric>();
            var well=shapes.Single(q=>Named(q,"PozoPiedra"));
            var peso=shapes.Single(q=>Named(q,"CasasDelPeso"));
            var granary=shapes.Single(q=>q.r==world.GranaryRenderer);
            var quarry=shapes.Single(q=>q.r==world.StoneworksRenderer);
            var marketCart=shapes.Single(q=>Named(q,"CarroMadera")&&Vector2.Distance(q.Foot,plan.Granary.Foot+new Vector2(1.86f,-.71f))<.01f);
            var quarryCart=shapes.Single(q=>Named(q,"CarroMadera")&&Vector2.Distance(q.Foot,plan.Quarry.Foot+new Vector2(-1.7f,-.82f))<.01f);
            // Fixed historic architecture, paths and approved mill. Only move small props.
            foreach(var pair in new[]{(target:well,obstacle:peso,limit:settings.wellLimit),(target:marketCart,obstacle:granary,limit:settings.marketCartLimit),(target:quarryCart,obstacle:quarry,limit:settings.quarryCartLimit)}){
                var a=pair.target;var b=pair.obstacle;
                float before=Gap(a,Vector2.zero,b);float route=RouteGap(a.Foot,routes);
                var neighbors=shapes.Where(q=>q!=a&&q!=b&&Vector2.Distance(q.Foot,a.Foot)<6).ToArray();
                var oldGaps=neighbors.Select(q=>Gap(a,Vector2.zero,q)).ToArray();
                float required=settings.targetMobilePx/px;Vector2 best=Vector2.zero;float bestGap=before;float bestDist=0;
                if(before<required){
                    int steps=Mathf.FloorToInt(pair.limit/.05f);
                    for(int ix=-steps;ix<=steps;ix++)for(int iy=-steps;iy<=steps;iy++){
                        var off=new Vector2(ix*.05f,iy*.05f);float dist=off.magnitude;if(dist>pair.limit||dist<.001f)continue;
                        float g=Gap(a,off,b);if(g<before+.10f)continue;
                        if(RouteGap(a.Foot+off,routes)<Mathf.Max(0,Mathf.Min(route,.34f)-.025f))continue;
                        bool valid=true;
                        for(int i=0;i<neighbors.Length;i++){
                            float ng=Gap(a,off,neighbors[i]);
                            // Preserve existing authored overlap or clearance; don't move the problem to a new landmark.
                            if(ng<Mathf.Min(oldGaps[i],.05f)-.035f){valid=false;break;}
                        }
                        if(!valid)continue;
                        bool meets=g>=required,oldMeets=bestGap>=required;
                        if((meets&&!oldMeets)||(meets&&oldMeets&&dist<bestDist-.00001f)||(!meets&&!oldMeets&&g>bestGap+.00001f)){
                            best=off;bestGap=g;bestDist=dist;
                        }
                    }
                }
                if(best.sqrMagnitude>0){
                    moves.Add(new Issue200AcademyPlacement.Move{name=a.r.sprite.name,x=a.r.transform.localPosition.x,y=a.r.transform.localPosition.y,dx=best.x,dy=best.y});
                    a.offset=best;
                }
                reports.Add(new Metric{target=a.r.sprite.name+" @ "+a.r.transform.localPosition,obstacle=b.r.sprite.name,result=best.sqrMagnitude>0?"measured_candidate":"retain_original",beforeWorld=before,afterWorld=bestGap,beforeMobilePx=before*px,afterMobilePx=bestGap*px,dx=best.x,dy=best.y,routeBefore=route,routeAfter=RouteGap(a.Foot,routes)});
            }
            var layout=new Issue200AcademyPlacement.Layout{moves=moves.ToArray()};
            File.WriteAllText(Path.Combine(path,"Issue200AcademyLayout.json"),JsonUtility.ToJson(layout,true));
            File.WriteAllText(Path.Combine(path,"placement-metrics.json"),JsonUtility.ToJson(new Report{pixelsPerWorldUnit=px,metrics=reports.ToArray(),moved=moves.Count},true));
            Debug.Log("ACADEMY_LAYOUT_MEASURED moves="+moves.Count);
            return layout;
        }
    }
}
#endif
