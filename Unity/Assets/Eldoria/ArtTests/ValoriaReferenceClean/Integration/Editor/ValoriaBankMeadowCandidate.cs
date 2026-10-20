#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
  // Isolated same-camera A/B for the OWNER's 2026-10-20 compact open Valoria.
  // No scene save, no gameplay/collider/POI/plot/bridge modification, no promotion.
  public static class ValoriaBankMeadowCandidate
  {
    const string Scene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
    const string RockPath = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/rock_3/rock_3_diff_1k.png";
    const string RefHash = "86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d";

    [Serializable]
    public sealed class Proof
    {
      public string source_commit, reference_sha256, scene, donor_asset, method, camera_before, camera_after;
      public string before_main_sha256, after_main_sha256, before_portrait_sha256, after_portrait_sha256;
      public int modified_alpha_cells, total_alpha_cells, protected_cells, added_rock_layer;
      public float changed_pixels_main_fraction;
      public bool source_scene_saved, heightmap_changed, gameplay_modified, same_camera, same_light;
      public bool art_approved, published;
      public string status;
    }

    static string Workspace => Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
    static string Evidence => Path.Combine(Workspace,"docs/evidence/valoria-reference-clean");
    static void Need(bool ok, string why) { if (!ok) throw new InvalidOperationException(why); }
    static string SHA(string file)
    {
      using (var h = SHA256.Create())
        return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(file))).Replace("-","").ToLowerInvariant();
    }

    static string CameraStamp(Camera cam)
    {
      var p=cam.transform.position;var q=cam.transform.rotation;
      return p.x.ToString("R")+","+p.y.ToString("R")+","+p.z.ToString("R")+"/"+
        q.x.ToString("R")+","+q.y.ToString("R")+","+q.z.ToString("R")+","+q.w.ToString("R")+"/"+
        cam.orthographicSize.ToString("R")+"/"+cam.fieldOfView.ToString("R");
    }

    static float Ease(float left,float right,float value)
    {
      float t=Mathf.Clamp01((value-left)/(right-left));
      return t*t*(3f-2f*t);
    }

    static float RiverX(float x)
    {
      return -23f+1.5f*Mathf.Sin(x*.065f)+.85f*Mathf.Sin(x*.19f+.4f)*Ease(6,17,Mathf.Abs(x));
    }
    static float RiverWidth(float x)
    {
      return 7.1f+.75f*Mathf.Sin(x*.095f+.7f)+.48f*Mathf.Sin(x*.23f);
    }
    static float RoadX(float z)
    {
      return z<-10f?-1.5f+Mathf.Tan(14f*Mathf.Deg2Rad)*(z+10f):
          -1.5f*Mathf.Clamp01((25.6f-z)/35.6f);
    }
    static bool Protected(float x,float z)
    {
      if (z>-78&&z<27&&Mathf.Abs(x-RoadX(z))<7.2f) return true;
      if (Mathf.Abs(x)<13.5f&&z>23f&&z<47f) return true;
      if (Mathf.Abs(x+1.5f)<16f&&Mathf.Abs(z+10f)<7f) return true;
      var plots=new[]{new Vector2(-23,28),new Vector2(-19,9),new Vector2(29,32),new Vector2(28,12)};
      foreach(var p in plots)
        if(Mathf.Abs(x-p.x)<11.5f&&Mathf.Abs(z-p.y)<9f)return true;
      return false;
    }

    // PBR scan uses the previously downloaded CC0 Poly Haven rock_3 texture.
    // This is a large-scale material/contact experiment on the real terrain,
    // not a new rock wall, town, camp or extra 3D object.
    static TerrainLayer RockLayer(Texture2D diffuse)
    {
      var l=new TerrainLayer { name="CC0 bank-ground continuity / isolated A-B",
        diffuseTexture=diffuse, tileSize=new Vector2(4.4f,4.4f), metallic=0f, smoothness=.02f};
      return l;
    }

    static float PixelFraction(string left,string right)
    {
      var a=new Texture2D(2,2,TextureFormat.RGB24,false);var b=new Texture2D(2,2,TextureFormat.RGB24,false);
      try
      {
        Need(a.LoadImage(File.ReadAllBytes(left)) && b.LoadImage(File.ReadAllBytes(right)),"Captured Unity image failed to decode");
        Need(a.width==b.width&&a.height==b.height,"A/B resolution mismatch");
        var aa=a.GetPixels32();var bb=b.GetPixels32();int changed=0;
        for(int i=0;i<aa.Length;i++)
          if(Mathf.Abs(aa[i].r-bb[i].r)>5 || Mathf.Abs(aa[i].g-bb[i].g)>5 || Mathf.Abs(aa[i].b-bb[i].b)>5)
            changed++;
        return changed/(float)aa.Length;
      }
      finally {Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
    }

    public static void Run()
    {
      try {Execute(); Debug.Log("ELDORIA_BANK_MEADOW_OWNER_AB_CAPTURED_ART_REVIEW_REQUIRED");EditorApplication.Exit(0);}
      catch(Exception error) {Debug.LogException(error);EditorApplication.Exit(1);}
    }

    static void Execute()
    {
      string jpeg=Path.Combine(Workspace,"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg");
      Need(File.Exists(jpeg)&&SHA(jpeg)==RefHash,"2026-10-20 owner image missing or replaced");
      Directory.CreateDirectory(Evidence);
      EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
      var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
      Need(city!=null&&city.PlayerCamera!=null,"Current integrated Valoria and official camera required");
      var terrains=Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None)
        .Where(t=>t.gameObject.activeInHierarchy).ToArray();
      Need(terrains.Length>=1,"Integrated city terrain missing");
      var terrain=terrains.OrderBy(t=>Vector3.Distance(t.transform.position,Vector3.zero)).First();
      var collider=terrain.GetComponent<TerrainCollider>();
      Need(collider!=null,"Original terrain collider is protected");
      var cam=city.PlayerCamera;
      var sun=RenderSettings.sun;
      string camStamp=CameraStamp(cam);
      string lightStamp=sun ? sun.transform.rotation.ToString("R") + sun.color.ToString("R") + sun.intensity.ToString("R") : "no-sun";
      var original=terrain.terrainData;
      Need(original!=null&&original.terrainLayers.Length>=4,"Expected production meadow material families absent");
      var rock=AssetDatabase.LoadAssetAtPath<Texture2D>(RockPath);
      Need(rock!=null,"Existing licensed PBR rock donor missing");

      string before=Path.Combine(Evidence,"owner178-bank-meadow-before-main.png");
      string after=Path.Combine(Evidence,"owner178-bank-meadow-after-main.png");
      string beforeMobile=Path.Combine(Evidence,"owner178-bank-meadow-before-portrait.png");
      string afterMobile=Path.Combine(Evidence,"owner178-bank-meadow-after-portrait.png");
      ValoriaReferenceCleanBuilder.Capture(cam,before,1448,1086);
      ValoriaReferenceCleanBuilder.Capture(cam,beforeMobile,900,1600);
      Need(camStamp==CameraStamp(cam),"Before capture unexpectedly moved the official camera");

      TerrainData trial=null;TerrainLayer added=null;
      int touched=0,protectedCells=0,full=0;
      try
      {
        trial=Object.Instantiate(original);
        trial.name="Non-persistent owner 178 meadow and natural bank A-B";
        added=RockLayer(rock);
        var family=original.terrainLayers;
        trial.terrainLayers=family.Concat(new[]{added}).ToArray();
        int n=original.alphamapResolution;
        var baseline=original.GetAlphamaps(0,0,n,n);
        int count=baseline.GetLength(2);
        Need(count==family.Length,"Alpha channels / material families mismatch");
        float[,,] alphas=new float[n,n,count+1];
        float totalSizeX=original.size.x,totalSizeZ=original.size.z;
        full=n*n;
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
          float wx=terrain.transform.position.x+(x+.5f)/n*totalSizeX;
          float wz=terrain.transform.position.z+(z+.5f)/n*totalSizeZ;
          for(int k=0;k<count;k++)alphas[z,x,k]=baseline[z,x,k];
          if(Protected(wx,wz)){protectedCells++;continue;}

          float d=Mathf.Abs(wz-RiverX(wx))-RiverWidth(wx);
          // Bank shelves stay low; no cliffs or river widening. Their scanned
          // stone ground visually joins the old authored rock meshes to the soil.
          float wetEdge=Ease(-.25f,.9f,d)*(1f-Ease(3.2f,7.6f,d));
          float bankRock=.69f*wetEdge;
          // Wide, gently varied grass/sun-dried transitions in existing fields.
          // Unlike rejected micro-repaints, the change is spatially legible at
          // the actual full-frame city camera. No new terrain objects are made.
          float lowFrequency=Mathf.PerlinNoise(wx*.024f+12.4f,wz*.029f+22.1f);
          float midFrequency=Mathf.PerlinNoise(wx*.055f+31.4f,wz*.051f+2.7f);
          float meadows=Ease(.32f,.72f,.72f*lowFrequency+.28f*midFrequency);
          float dryness=.08f+.22f*meadows;
          float inland=Ease(5f,11f,d);
          dryness*=inland;
          float sum=0;
          for(int k=0;k<count;k++)sum+=baseline[z,x,k];
          if(sum<=.0001f)continue;
          // Retain existing wear and functional ground identity; shift only
          // meadow-family weights and then insert a geological edge family.
          for(int k=0;k<count;k++)alphas[z,x,k]=(1f-bankRock)*baseline[z,x,k]/sum;
          float shift=Mathf.Min((1f-bankRock)*dryness,alphas[z,x,0]*.40f);
          alphas[z,x,0]-=shift;
          alphas[z,x,3]+=shift;
          alphas[z,x,count]=bankRock;
          touched++;
        }
        Need(touched>full/4 && protectedCells>0,"Candidate changed no meaningful unprotected terrain footprint");
        trial.SetAlphamaps(0,0,alphas);
        terrain.terrainData=trial;
        collider.terrainData=trial;
        terrain.Flush();
        Need(camStamp==CameraStamp(cam),"Candidate changed the camera");
        ValoriaReferenceCleanBuilder.Capture(cam,after,1448,1086);
        ValoriaReferenceCleanBuilder.Capture(cam,afterMobile,900,1600);
        var proof=new Proof {
          source_commit=Environment.GetEnvironmentVariable("GITHUB_SHA"),
          reference_sha256=RefHash,scene=Scene,donor_asset=RockPath,
          method="CC0 donor PBR rock ground + measured real river center/width, protected gameplay plots and path, Unity terrain alpha authoring. Test-only; no permanent meshes.",
          camera_before=camStamp,camera_after=CameraStamp(cam),
          before_main_sha256=SHA(before),after_main_sha256=SHA(after),
          before_portrait_sha256=SHA(beforeMobile),after_portrait_sha256=SHA(afterMobile),
          modified_alpha_cells=touched,total_alpha_cells=full,protected_cells=protectedCells,
          added_rock_layer=1,
          changed_pixels_main_fraction=PixelFraction(before,after),
          source_scene_saved=false,heightmap_changed=false,gameplay_modified=false,
          same_camera=camStamp==CameraStamp(cam),
          same_light=lightStamp==(sun ? sun.transform.rotation.ToString("R")+sun.color.ToString("R")+sun.intensity.ToString("R") : "no-sun"),
          art_approved=false,published=false,status="REAL_UNITY_AB_ART_REVIEW_REQUIRED"
        };
        Need(proof.same_camera&&proof.same_light,"A/B camera or lighting not identical");
        File.WriteAllText(Path.Combine(Evidence,"owner178-bank-meadow-review.json"),JsonUtility.ToJson(proof,true));
        Debug.Log("OWNER178_VISIBILITY "+proof.changed_pixels_main_fraction.ToString("P2")+" ART_REVIEW_REQUIRED");
      }
      finally
      {
        terrain.terrainData=original;
        collider.terrainData=original;
        terrain.Flush();
        if(trial!=null)Object.DestroyImmediate(trial);
        if(added!=null)Object.DestroyImmediate(added);
      }
      Need(camStamp==CameraStamp(cam),"Camera changed despite same-camera trial");
      // Captures are diagnostic; this tool never saves the source scene.
    }
  }
}
#endif
