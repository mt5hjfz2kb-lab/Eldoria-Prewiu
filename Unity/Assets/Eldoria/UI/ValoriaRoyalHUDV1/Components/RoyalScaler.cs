using UnityEngine;using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold {public sealed class RoyalScaler:CanvasScaler {
 protected override void OnEnable(){uiScaleMode=ScaleMode.ScaleWithScreenSize;referenceResolution=new Vector2(1600,900);screenMatchMode=ScreenMatchMode.MatchWidthOrHeight;matchWidthOrHeight=1;referencePixelsPerUnit=100;base.OnEnable();}
}}
