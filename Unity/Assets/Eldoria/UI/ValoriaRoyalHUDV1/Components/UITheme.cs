using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 [CreateAssetMenu(menuName="Eldoria/Royal Gold/Theme")]
 public sealed class UITheme:ScriptableObject {
  public Color Ink=new Color(.032f,.049f,.08f,1),RoyalBlue=new Color(.067f,.13f,.23f,1),Gold=new Color(.73f,.60f,.35f,1),Text=new Color(.95f,.90f,.80f,1);
  [Range(.8f,1.35f)] public float TextScale=1f; public bool ReduceMotion;
  public const float TouchSize=64f;
 }
}
