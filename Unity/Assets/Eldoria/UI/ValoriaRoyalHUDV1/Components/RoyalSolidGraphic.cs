using UnityEngine;using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold {public sealed class RoyalSolidGraphic:MaskableGraphic {
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;v.AddVert(new Vector3(r.xMin,r.yMin),color,Vector2.zero);v.AddVert(new Vector3(r.xMin,r.yMax),color,Vector2.zero);v.AddVert(new Vector3(r.xMax,r.yMax),color,Vector2.zero);v.AddVert(new Vector3(r.xMax,r.yMin),color,Vector2.zero);v.AddTriangle(0,1,2);v.AddTriangle(2,3,0);}
}}
