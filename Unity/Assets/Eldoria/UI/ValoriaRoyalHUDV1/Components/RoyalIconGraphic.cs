using UnityEngine;
using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold {
 /// <summary>Original icon source geometry. Names and numbers remain real Text, never baked into art.</summary>
 public sealed class RoyalIconGraphic:MaskableGraphic {
  public int Icon; // 0 crest,1 wood,2 stone,3 food,4 hero,5 army,6 mission,7 bag,8 world,9 bastion,10 settings,11 close,12 arrow,13 collapse,14 expand
  VertexHelper v;
  void Shape(params Vector2[] p){if(p.Length<3)return;var r=rectTransform.rect;int k=v.currentVertCount;foreach(var t in p)v.AddVert(new Vector2(r.xMin+r.width*t.x/100,r.yMin+r.height*t.y/100),color,Vector2.zero);for(int i=1;i<p.Length-1;i++)v.AddTriangle(k,k+i,k+i+1);}
  Vector2 P(float x,float y)=>new Vector2(x,y);
  void Box(float x,float y,float w,float h)=>Shape(P(x,y),P(x+w,y),P(x+w,y+h),P(x,y+h));
  void Stroke(float x,float y,float a,float b,float width=3){var d=(P(a,b)-P(x,y)).normalized;var n=P(-d.y,d.x)*width/2;Shape(P(x,y)-n,P(a,b)-n,P(a,b)+n,P(x,y)+n);}
  void Ellipse(float x,float y,float w,float h){var p=new Vector2[20];for(int i=0;i<p.Length;i++){float a=i*Mathf.PI*2/p.Length;p[i]=P(x+Mathf.Cos(a)*w,y+Mathf.Sin(a)*h);}Shape(p);}
  void Castle(){Box(23,23,15,47);Box(62,23,15,47);Box(38,23,24,31);for(int i=0;i<3;i++){Box(21+7*i,70,5,8);Box(60+7*i,70,5,8);}Box(20,18,60,5);Box(47,23,6,17);Stroke(50,57,50,87,2);Shape(P(51,86),P(68,81),P(51,76));}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();v=vh;
   switch(Icon){
    case 0:Shape(P(16,86),P(84,86),P(80,43),P(67,24),P(50,12),P(33,24),P(20,43));break;
    case 1:Stroke(28,23,73,75,14);Stroke(22,37,63,82,13);Stroke(39,20,81,66,13);Ellipse(25,29,7,6);Ellipse(43,24,7,6);break;
    case 2:Shape(P(12,28),P(35,58),P(59,53),P(80,20),P(29,15));Shape(P(38,66),P(55,88),P(78,75),P(89,42),P(74,34));break;
    case 3:Stroke(39,15,57,84,3);for(int i=0;i<5;i++){float y=35+9*i;Ellipse(39+i*2.5f,y,8,4);Ellipse(57+i*2.5f,y+2,8,4);}break;
    case 4:Shape(P(28,62),P(35,80),P(60,84),P(77,62),P(70,32),P(50,22),P(28,39));Box(25,51,55,5);Stroke(52,79,52,28,4);Box(30,22,42,5);break;
    case 5:Stroke(24,20,76,80,8);Shape(P(67,77),P(83,88),P(80,68));Stroke(17,34,36,17,6);Stroke(25,80,75,20,8);Shape(P(17,88),P(32,77),P(20,68));Stroke(65,17,83,34,6);break;
    case 6:Box(28,21,47,60);Ellipse(28,80,12,6);Ellipse(66,21,12,6);Box(23,23,7,51);break;
    case 7:Shape(P(24,24),P(20,41),P(33,71),P(66,71),P(80,41),P(74,24));Box(36,74,28,5);Shape(P(37,80),P(29,91),P(70,91),P(62,80));break;
    case 8:for(int i=0;i<40;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;Stroke(50+34*Mathf.Cos(a),50+34*Mathf.Sin(a),50+34*Mathf.Cos(b),50+34*Mathf.Sin(b),3);}Stroke(16,50,84,50);Stroke(23,70,77,70);Stroke(23,30,77,30);Stroke(50,16,50,84);break;
    case 9:Castle();break;
    case 10:for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Stroke(50+19*Mathf.Cos(a),50+19*Mathf.Sin(a),50+35*Mathf.Cos(a),50+35*Mathf.Sin(a),9);}Ellipse(50,50,22,22);break;
    case 11:Stroke(29,29,71,71,5);Stroke(29,71,71,29,5);break;
    case 13:Stroke(28,50,72,50,6);break;
    case 14:Stroke(28,50,72,50,6);Stroke(50,28,50,72,6);break;
    default:Stroke(34,24,62,50,5);Stroke(62,50,34,76,5);break;
   }
  }
 }
}
