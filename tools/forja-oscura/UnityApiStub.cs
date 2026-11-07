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
    public Transform transform {get;} = new Transform();
    public T GetComponent<T>() where T : Component => default;
    public T AddComponent<T>() where T : Component => default;
  }
  public partial class Transform : Component, IEnumerable<Transform> {
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
  public struct Vector4 { public float sqrMagnitude=>0f; }
  public class Sprite : Object { public Vector4 border; }
  public class CanvasRenderer : Component {}
  public static class Time { public static float unscaledTime=>0; }
  public static class Resources { public static T Load<T>(string path) where T:Object=>default; }
  public static partial class Mathf { public static float Max(float x,float y)=>Math.Max(x,y); }
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
  public struct ColorBlock {}
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
  public class RoyalText : Graphic { public string text {get;set;} }
  public class RoyalIconGraphic:Graphic {}
  public class RoyalButton:Button {}
  public class RoyalHudLayout:Behaviour {}
  public class RoyalHeader:Behaviour { public RoyalText Realm; }
  public class RoyalResourceBar:Behaviour { public RoyalText Wood,Stone,Food; }
  public class RoyalBuildingPanel:Behaviour { public RoyalButton Primary; }
  public class RoyalNav:Behaviour {
    public class Item { public RoyalSection Section; public RoyalButton Button; public RoyalText Label; }
    public Item[] Items;
  }
  public partial class RoyalHUD:MonoBehaviour {
    public RoyalHeader Realm;
    public RoyalResourceBar Resources;
    public RoyalNav Navigation;
    public RoyalBuildingPanel Building;
  }
}
namespace Eldoria.UI.RoyalGold.V2 {
  using UnityEngine;using Eldoria.UI.RoyalGold;
  public class RoyalGoldV2Hud:MonoBehaviour {
    public RectTransform ChronicleObjectiveRect,ChronicleBuildingRect;
  }
  public partial class RoyalGoldV3ContextHud:MonoBehaviour {
    public RectTransform ContextRect;
    public bool IsConfigured,ContextVisible;
    public RoyalButton VisibleActionButton,VisibleCloseButton;
  }
}
