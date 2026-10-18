using UnityEngine;
using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold {
 /// <summary>Editable, resolution-independent royal frame. Vertex-colour mesh; no runtime textures or shaders.</summary>
 public sealed class RoyalFrameGraphic:MaskableGraphic {
  public bool Round; public int Variant;
  static readonly Color Gold=new Color(.66f,.51f,.27f,1), Light=new Color(.90f,.78f,.51f,1);
  Color Tint(Color c)=>c*color;
  void Poly(VertexHelper v,Vector2[] points,Color bottom,Color top){
   int start=v.currentVertCount;float lo=float.MaxValue,hi=float.MinValue;var center=Vector2.zero;
   foreach(var p in points){center+=p;lo=Mathf.Min(lo,p.y);hi=Mathf.Max(hi,p.y);}center/=points.Length;
   v.AddVert(center,Tint(Color.Lerp(bottom,top,.5f)),Vector2.zero);
   foreach(var p in points)v.AddVert(p,Tint(Color.Lerp(bottom,top,Mathf.InverseLerp(lo,hi,p.y))),Vector2.zero);
   for(int i=0;i<points.Length;i++)v.AddTriangle(start,start+1+i,start+1+(i+1)%points.Length);
  }
  void Plate(VertexHelper v,Rect r,float inset,Color low,Color high){
   r=new Rect(r.x+inset,r.y+inset,Mathf.Max(0,r.width-2*inset),Mathf.Max(0,r.height-2*inset));
   if(Round){var p=new Vector2[64];for(int i=0;i<p.Length;i++){float a=i*Mathf.PI*2/p.Length;p[i]=r.center+new Vector2(Mathf.Cos(a)*r.width/2,Mathf.Sin(a)*r.height/2);}Poly(v,p,low,high);}
   else{float c=Mathf.Min(12,Mathf.Min(r.width,r.height)/5);Poly(v,new[]{new Vector2(r.xMin+c,r.yMin),new Vector2(r.xMax-c,r.yMin),new Vector2(r.xMax,r.yMin+c),new Vector2(r.xMax,r.yMax-c),new Vector2(r.xMax-c,r.yMax),new Vector2(r.xMin+c,r.yMax),new Vector2(r.xMin,r.yMax-c),new Vector2(r.xMin,r.yMin+c)},low,high);}
  }
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var r=rectTransform.rect;if(r.width<8||r.height<8)return;
   Plate(vh,r,0,new Color(0,0,0,.6f),new Color(0,0,0,.6f));
   Plate(vh,r,2,new Color(.25f,.18f,.09f,1),Light);
   Plate(vh,r,3.5f,new Color(.05f,.065f,.09f,1),new Color(.05f,.065f,.09f,1));
   // One restrained gold edge leaves more room for readable content.
   Color low=Variant==1?new Color(.08f,.15f,.23f,1):new Color(.023f,.034f,.055f,.98f);
   Color high=Variant==1?new Color(.17f,.29f,.43f,1):new Color(.068f,.105f,.17f,.99f);
   Plate(vh,r,4.5f,low,high);
   if(!Round&&r.width>110){float y=r.yMax-5;float x=r.center.x;Poly(vh,new[]{new Vector2(x-6,y),new Vector2(x,y+3),new Vector2(x+6,y),new Vector2(x,y-3)},Gold,Light);}
  }
 }
}
