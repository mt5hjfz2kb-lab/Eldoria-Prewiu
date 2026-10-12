using System;
using System.IO;
using System.Threading.Tasks;
using Eldoria.Presentation;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    // Builds the actual Valoria scene from the same assembly that was rendered
    // for art review. Progression and touch input remain owned by SlicePresenter.
    public static class ValoriaBastion1MeshPlayerScene
    {
        const string Input="Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedSource";
        const string PlayerScene="Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";

        public static async Task Build(Scene scene,GameObject bastion,Camera camera)
        {
            var oldControls=camera.GetComponent<Eldoria.Art.Bastion1AssemblyCamera>();
            if(oldControls!=null)UnityEngine.Object.DestroyImmediate(oldControls);
            camera.tag="MainCamera";
            camera.gameObject.name="Valoria strategic player camera";
            camera.orthographicSize=38.5f;

            var visual=new GameObject("Valoria authored mesh presentation");
            var presentation=visual.AddComponent<ValoriaParcelPresentation>();
            presentation.MeshMode=true;
            presentation.ProductionCamera=camera;
            presentation.MeshHomePosition=camera.transform.position;
            presentation.MeshHomeRotation=camera.transform.rotation;
            presentation.MeshHomeOrthographicSize=camera.orthographicSize;
            presentation.MeshPanHalfExtents=new Vector2(12f,12f);

            // The bridge/gate and axial stairs remain empty transit space. The
            // buildable sawmill is placed to the left of the open middle court.
            var sawmill0=await Import("SawmillState0V1",new Vector3(-16f,0f,3f),8.5f);
            var sawmill1=await Import("SawmillState1V1",new Vector3(-16f,0f,3f),8.5f);
            var sawmill2=await Import("SawmillState2V1",new Vector3(-16f,0f,3f),8.5f);
            sawmill0.transform.SetParent(visual.transform,true);
            sawmill1.transform.SetParent(visual.transform,true);
            sawmill2.transform.SetParent(visual.transform,true);
            sawmill1.SetActive(false);
            sawmill2.SetActive(false);

            // The barracks parcel stays empty in Bastion I. Its existing model
            // appears only after the Bastion II unlock and construction completes.
            var barracks=await Import("Valoria_Cuartel_AP2_v1",new Vector3(16f,0f,3f),8.5f);
            barracks.transform.SetParent(visual.transform,true);
            barracks.SetActive(false);
            var sawmillTarget=Target("InteractiveProxy_LeftCabinParcel","sawmill",
                new Vector3(-16f,1.1f,3f),new Vector3(10f,2.2f,9f),visual.transform);
            var barracksTarget=Target("InteractiveProxy_RightCampParcel","barracks",
                new Vector3(16f,1.1f,3f),new Vector3(10f,2.2f,9f),visual.transform);
            Target("InteractiveProxy_Bastion","bastion",
                new Vector3(0f,5f,-21f),new Vector3(19f,10f,9f),visual.transform);

            presentation.Bindings=new[]
            {
                new ValoriaParcelPresentation.Binding
                {
                    ParcelId="sawmill",BuildingId="sawmill",VariantBit=1,
                    Ground=sawmill0,Construction=sawmill1,Built=sawmill2,Target=sawmillTarget
                },
                new ValoriaParcelPresentation.Binding
                {
                    ParcelId="barracks",BuildingId="barracks",VariantBit=2,
                    Built=barracks,Target=barracksTarget
                }
            };
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Path.GetDirectoryName(PlayerScene));
            if(!EditorSceneManager.SaveScene(scene,PlayerScene))
                throw new Exception("Unable to save canonical player scene");

            // Capture the initial state without pretending the mature buildings
            // are part of Bastion I. The gameplay state is applied at scene load.
            var directory=Path.GetFullPath("Bastion1ReferenceCaptures");
            Bastion1ReferenceCapture.Render(camera,Path.Combine(directory,"bastion1-player-initial.png"),1440,1000);
            Bastion1ReferenceCapture.Render(camera,Path.Combine(directory,"bastion1-player-mobile.png"),780,520);
            Debug.Log("BASTION1_PLAYER_SCENE_COMPLETE "+PlayerScene+
                " sawmill=authored-state0/1/2 barracks=locked-until-bastion2 camera=orthographic");
        }

        static BoxCollider Target(string name,string id,Vector3 center,Vector3 size,Transform parent)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.position=center;
            go.AddComponent<WorldHotspot>().Id=id;
            var collider=go.AddComponent<BoxCollider>();
            collider.size=size;
            collider.isTrigger=true;
            return collider;
        }

        static async Task<GameObject> Import(string name,Vector3 floorCentre,float width)
        {
            var file=Path.GetFullPath(Input+"/"+name+".glb");
            if(!File.Exists(file))throw new FileNotFoundException("Authored state missing",file);
            var importer=new GltfImport(null,new UninterruptedDeferAgent());
            if(!await importer.Load(new Uri(file).AbsoluteUri))
                throw new Exception("glTFast could not load "+name);
            var root=new GameObject(name);
            if(!await importer.InstantiateMainSceneAsync(root.transform))
                throw new Exception("glTFast could not instantiate "+name);
            Bastion1MaterialBindings.Apply(root,importer,file);
            var bounds=Bounds(root);
            if(bounds.size.x<.001f||bounds.size.z<.001f)
                throw new Exception("Authored model has no world footprint: "+name);
            var scale=width/Mathf.Max(bounds.size.x,bounds.size.z);
            root.transform.localScale=Vector3.one*scale;
            bounds=Bounds(root);
            root.transform.position=floorCentre-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                Bastion1ReferenceCapture.Persist(filter.sharedMesh);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
                foreach(var material in renderer.sharedMaterials)
                {
                    if(material==null)continue;
                    foreach(var property in material.GetTexturePropertyNames())
                        Bastion1ReferenceCapture.Persist(material.GetTexture(property));
                    Bastion1ReferenceCapture.Persist(material);
                }
            Debug.Log("BASTION1_AUTHORED_MODEL "+name+" bounds="+Bounds(root));
            return root;
        }

        static Bounds Bounds(GameObject root)
        {
            var renderers=root.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new Exception("No visible renderer in "+root.name);
            var result=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)result.Encapsulate(renderers[i].bounds);
            return result;
        }
    }
}
