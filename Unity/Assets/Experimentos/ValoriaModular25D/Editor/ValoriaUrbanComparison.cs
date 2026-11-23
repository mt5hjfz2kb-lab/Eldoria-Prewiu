#if UNITY_EDITOR
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    // Editor-only maquettes with the same imported actors, ground material,
    // doors and camera. Never reads or writes a player save.
    public static class ValoriaUrbanComparison
    {
        public static void Capture()
        {
            var output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../Artifacts/ValoriaModular25D/Native"));
            Directory.CreateDirectory(output);
            foreach (bool terraces in new[]{false,true})
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var plan=new ValoriaUrbanPlan(terraces);
                var root=new GameObject("Urban design study");
                root.AddComponent<Modular25DGround>().Build();
                var roads=root.AddComponent<Modular25DLandscape>();
                var shader=Shader.Find("Sprites/Default");
                var stone=new Material(shader){mainTexture=Resources.Load<Texture2D>("ValoriaModular25D/OrganicCobbleRoad")};
                var earth=new Material(shader){mainTexture=Resources.Load<Texture2D>("ValoriaModular25D/HaulersEarth")};
                var plots=new[]{plan.Keep,plan.Mill,plan.Barracks,plan.Granary,plan.Quarry};
                foreach(var plot in plots)
                {
                    var renderer=new GameObject(plot.Id).AddComponent<SpriteRenderer>();
                    renderer.sprite=plot==plan.Keep ? Modular25DArt.BastionForLevel(5) :
                        plot==plan.Mill ? Modular25DArt.Sawmill() : Modular25DArt.ExpansionBuilding(plot.Id);
                    renderer.transform.position=plot.Foot;
                    renderer.sortingOrder=Modular25DWorld.FootOrder(plot.Foot.y);
                    bool paved=plot==plan.Keep || plot==plan.Barracks;
                    roads.AddRoad(plot.Id,plan.Approach(plot),paved?1.32f:.92f,paved?stone:earth,paved);
                }
                roads.AddRoad("Freight",new[]{plan.Freight,Vector2.Lerp(plan.Freight,plan.Junction,.5f)+Vector2.left*.6f,plan.Junction},1.15f,stone,true);
                var cam=new GameObject("Portrait camera").AddComponent<Camera>();
                cam.orthographic=true;
                var prefix="urban-"+(terraces?"B":"A")+"-";
                var report=new Modular25DNativeCapture.Evidence();
                void Frame(string name,Vector2 point,float size)
                {
                    Modular25DNativeCapture.Render(cam,output,report,prefix+name,390,844,
                        new Vector3(point.x,point.y,-30),size);
                }
                Frame("home",new Vector2(0,3.8f),6.6f);
                Frame("junction",plan.Junction,6.6f);
                for(int i=1;i<plots.Length;i++)Frame(plots[i].Id,plots[i].Foot+Vector2.up,6.6f);
                Frame("overview",Vector2.zero,24f);
                report.sourceSha=System.Environment.GetEnvironmentVariable("GITHUB_SHA");
                File.WriteAllText(Path.Combine(output,prefix+"frames.json"),JsonUtility.ToJson(report,true));
                Object.DestroyImmediate(stone);
                Object.DestroyImmediate(earth);
            }
        }
    }
}
#endif
