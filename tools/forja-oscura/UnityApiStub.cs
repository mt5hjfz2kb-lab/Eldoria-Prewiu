// A bounded C# semantic compilation of the REAL Forja runtime source.
// These API skeletons are type-check helpers, NOT Unity binaries. It proves
// consistency of C# members in this model only; Unity itself remains untested.
using System;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine {
  public partial class Object {
    public static implicit operator bool(Object o) => !ReferenceEquals(o,null);
    public static void Destroy(Object o) {}
  }
  public partial class Component : Object {
    public GameObject gameObject {get;} = new GameObject();
    public Transform transform {get;} = new Transform();
    public T GetComponent<T>() where T : Component => default;
    public T GetComponentInChildren<T>(bool includeInactive = false) where T : Component => default;
    public T[] GetComponentsInChildren<T>(bool includeInactive = false) where T : Component => Array.Empty<T>();
  }
  public class Behaviour : Component { public bool enabled {get;set;} = true; public bool isActiveAndEnabled => enabled; }
  public class MonoBehaviour : Behaviour { }
  public partial class GameObject : Object {
    public GameObject() {}
    public GameObject(string name, params Type[] components) {}
    public int layer {get;set;}
    public bool activeInHierarchy => true;
    public bool activeSelf => true;
    public Transform transform {get;} = new Transform();
    public T GetComponent<T>() where T : Component => default;
    public T AddComponent<T>() where T : Component => default;
  }
  public partial class Transform : Component, IEnumerable<Transform> {
    public Transform parent {get;set;}
    public string name {get;set;}
    public int childCount {get;set;}
    public Transform GetChild(int i) => default;
    public void SetParent(Transform parent,bool worldPositionStays) {}
    public void SetAsFirstSibling() {}
    public void SetAsLastSibling() {}
    public IEnumerator<Transform> GetEnumerator() => ((IEnumerable<Transform>)Array.Empty<Transform>()).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
  public partial class RectTransform : Transform {
    public Vector2 anchorMin,anchorMax,pivot,offsetMin,offsetMax,anchoredPosition,sizeDelta;
    public Rect rect;
  }
  public struct Rect { public float width,height; }
  public struct Color {
    public float r,g,b,a;
    public Color(float r,float g,float b,float a=1f){this.r=r;this.g=g;this.b=b;this.a=a;}
    public static Color white => new Color(1,1,1,1);
  }
  public partial struct Vector2 {
    public float x,y; public Vector2(float x,float y){this.x=x;this.y=y;}
    public static Vector2 zero=>new Vector2();public static Vector2 one=>new Vector2(1,1);
  }
  public partial struct Vector4 { public float sqrMagnitude=>0f; }
  public class Sprite : Object { public Vector4 border; }
  public class CanvasRenderer : Component {}
  public class Canvas : Behaviour { public float scaleFactor {get;set;} = 1f; public static void ForceUpdateCanvases(){} }
  public static class Time { public static float unscaledTime=>0; }
  public static class Resources { public static T Load<T>(string path) where T:Object=>default; }
  public static partial class Mathf { public static float Clamp(float value,float min,float max)=>Math.Min(max,Math.Max(min,value)); public static float Max(float x,float y)=>Math.Max(x,y); public static float Min(float x,float y)=>Math.Min(x,y); public static int CeilToInt(float x)=>(int)Math.Ceiling(x); }
  public static partial class Debug { public static void Log(string s){} public static void LogWarning(string s){} }
  [AttributeUsage(AttributeTargets.Class)]public sealed class DisallowMultipleComponent:Attribute {}
  [AttributeUsage(AttributeTargets.Class)]public sealed class DefaultExecutionOrder:Attribute {public DefaultExecutionOrder(int n){} }
}
namespace UnityEngine.Events {
  public delegate void UnityAction();
  public class UnityEvent {
    public void AddListener(UnityAction f){} public void RemoveListener(UnityAction f){}
  }
}
namespace UnityEngine.UI {
  using UnityEngine; using UnityEngine.Events;
  public class Graphic : Behaviour { public Color color;public bool raycastTarget; }
  public class Image : Graphic {
    public Sprite sprite;
    public Type type;
    public enum Type { Simple, Sliced }
  }
  public struct SpriteState { public Sprite highlightedSprite,pressedSprite,selectedSprite,disabledSprite; }
  public struct ColorBlock {
    public Color normalColor,highlightedColor,pressedColor,selectedColor,disabledColor;
    public static ColorBlock defaultColorBlock => new ColorBlock();
  }
  public class Selectable : Behaviour {
    public enum Transition { None,ColorTint,SpriteSwap }
    public Transition transition; public ColorBlock colors; public SpriteState spriteState; public bool interactable;
    public Graphic targetGraphic;
  }
  public class Button:Selectable { public UnityEvent onClick {get;}=new UnityEvent(); }
}
namespace Eldoria.UI.RoyalGold {
  using UnityEngine;using UnityEngine.UI;
  public enum RoyalSection { Bastion,Inventory,Army,World,Missions }
  // Mirrors ONLY the public static ordering contract in the real BottomNavigation.
  // Runtime uses its original Unity implementation; this is a typecheck stub.
  public static class BottomNavigation {
    public static int ChronicleOrder(RoyalSection s) {
      switch(s) {
        case RoyalSection.Bastion: return 0;
        case RoyalSection.Inventory: return 1;
        case RoyalSection.Army: return 2;
        case RoyalSection.World: return 3;
        case RoyalSection.Missions: return 4;
        default: return 99;
      }
    }
  }
  public class RoyalText : Graphic { public string text {get;set;} public int fontSize {get;set;} public float preferredWidth=>0f; public float preferredHeight=>0f; public RectTransform rectTransform=>new RectTransform(); }
  public class RoyalIconGraphic:Graphic {}
  public class RoyalButton:Button {}
  public class RoyalHudLayout:Behaviour {}
  public class BuildingLabel:MonoBehaviour { public RoyalText Label; public RoyalButton Select; }
  public class RoyalHeader:Behaviour { public RoyalText Realm,Power; }
  public class RoyalResourceBar:Behaviour { public RoyalText Wood,Stone,Food; }
  public class RoyalPrimary:Component {public RoyalButton Button;public RoyalText Label;}
  public class RoyalBuildingPanel:Behaviour { public RoyalPrimary Primary; public RoyalText Title; public object Offer; }
  public class RoyalNav:Behaviour {
    public class Item { public RoyalSection Section; public RoyalButton Button; public RoyalText Label; }
    public Item[] Items;
    public event System.Action<RoyalSection> Requested;
  }
  public partial class RoyalHUD:MonoBehaviour {
    public RoyalButton Settings;
    public RoyalHeader Realm;
    public RoyalResourceBar Resources;
    public RoyalNav Navigation;
    public RoyalBuildingPanel Building;
  }
}
namespace Eldoria.UI.RoyalGold.V2 {
  using UnityEngine;using Eldoria.UI.RoyalGold;
  public class RoyalGoldV3BuildingPreview:MonoBehaviour { public Color BackdropColor => default; public bool TightFraming => false; public void SetTightFraming(bool enabled){} public void SetBackdrop(Color color){} }
  public class RoyalGoldV2Hud:MonoBehaviour {
    public RectTransform ChronicleObjectiveRect,ChronicleBuildingRect;
    public bool ChronicleIllustratedBuildingVisible => false;
    public void SynchronizeNow(){}
    public void InvalidatePresentationLayout(){}
  }
  public partial class RoyalGoldV3ContextHud:MonoBehaviour {
    public RectTransform ContextRect;
    public bool IsConfigured,ContextVisible;
    public RoyalButton VisibleActionButton,VisibleCloseButton;
  }
}
