// Type-check facade only. This is NOT Unity's real runtime or editor implementation.
using System;
using System.Collections.Generic;
namespace UnityEngine {
  public enum FindObjectsInactive { Exclude,Include }
  public enum FindObjectsSortMode { None }
  public partial class Object {
    public static T[] FindObjectsByType<T>(FindObjectsInactive i,FindObjectsSortMode s) where T:Object => Array.Empty<T>();
    public static void DestroyImmediate(Object o){}
  }
  public partial class Component {
    public T[] GetComponents<T>() where T:Component=>Array.Empty<T>();
  }
  public partial class GameObject { public bool activeSelf=>true; }
  public partial class RectTransform { public void GetWorldCorners(Vector3[] corners){} }
  public struct Vector3 {
    public float x,y,z;
    public static Vector3 operator+(Vector3 a,Vector3 b)=>a;
    public static Vector3 operator*(Vector3 a,float k)=>a;
  }
  public partial struct Vector2 {
    public static float Distance(Vector2 a,Vector2 b)=>0;
  }
  public class Camera:Component {}
  public class Canvas:Behaviour { public static void ForceUpdateCanvases(){} }
  public class Application {public static string dataPath=>"";}
  public static class JsonUtility {public static string ToJson(object v,bool pretty)=>"";}
  public class Texture2D:Object {
    public Texture2D(int w,int h,TextureFormat f,bool mip){}
    public int width=>0;public int height=>0;
    public bool LoadImage(byte[] data,bool markNonReadable)=>false;
    public Color32[] GetPixels32()=>Array.Empty<Color32>();
  }
  public struct Color32 {public byte r,g,b,a;}
  public enum TextureFormat { RGB24 }
  public static partial class Mathf {
    public static float Min(float a,float b)=>Math.Min(a,b);
    public static int FloorToInt(float x)=>(int)Math.Floor(x);
    public static int CeilToInt(float x)=>(int)Math.Ceiling(x);
  }
  public static partial class Debug {
    public static void LogError(string s){}
    public static void LogException(Exception e){}
  }
  public static class RectTransformUtility {
    public static Vector2 WorldToScreenPoint(Camera camera,Vector3 pos)=>default;
  }
}
namespace UnityEngine.EventSystems {
  using System.Collections.Generic;using UnityEngine;
  public class EventSystem:Behaviour {
    public static EventSystem current=>new EventSystem();
    public void RaycastAll(PointerEventData p,List<RaycastResult> results){}
  }
  public class PointerEventData {
    public PointerEventData(EventSystem system){}
    public int pointerId;
    public Vector2 position;
    public InputButton button;
    public enum InputButton { Left,Right,Middle }
  }
  public struct RaycastResult {public GameObject gameObject;}
  public interface IPointerClickHandler {}
  public static class ExecuteEvents {
    public static GameObject GetEventHandler<T>(GameObject o) where T:class => o;
  }
}
namespace UnityEditor {
  using System;
  [AttributeUsage(AttributeTargets.Class)] public sealed class InitializeOnLoadAttribute:Attribute {}
  [AttributeUsage(AttributeTargets.Method)] public sealed class MenuItem:Attribute {public MenuItem(string path){} }
  public static class EditorApplication {
    public static event Action update {add{} remove{}}
    public static bool isPlaying=>false;
    public static double timeSinceStartup=>0;
    public static void EnterPlaymode(){}
    public static void Exit(int code){}
  }
  public static class EditorPrefs {
    public static void SetBool(string key,bool value){}
  }
  public static class SessionState {
    public static void SetBool(string key,bool value){}
    public static bool GetBool(string key,bool fallback)=>false;
  }
}
namespace UnityEditor.SceneManagement {
  public enum OpenSceneMode { Single }
  public static class EditorSceneManager {
    public static void OpenScene(string path,OpenSceneMode mode){}
  }
}
namespace Eldoria.UI.RoyalGold {
  using UnityEngine;
  public class RoyalMission:Component {public RoyalText Objective;}
  public class RoyalModal:Component {public RoyalButton Extra;}
  public partial class RoyalHUD {
    public string SelectedBuildingId=>"";public bool NarrativeOpen=>false;
    public RoyalMission Mission;public RoyalModal Modal;
    public void Refresh(){}
  }
}
namespace Eldoria.UI.RoyalGold.V2 {
  public partial class RoyalGoldV3ContextHud {public void SynchronizeNow(){}}
}
