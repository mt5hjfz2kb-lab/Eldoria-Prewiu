using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace Eldoria.EditorTools
{
    internal static class Bastion1ExistingRoadIntegration
    {
        internal static async Task Apply(GameObject assembly)
        {
            var file=Path.GetFullPath("Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedSource/RoadFamilyV1.glb");
            using(var hash=SHA256.Create())using(var stream=File.OpenRead(file))
                if(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant()!=
                    "683310d65fd83de40115b310b35ceae8b35b935baaf8305a068d8e1203709f67")
                    throw new Exception("Frozen authored RoadFamilyV1 changed");
            var importer=new GltfImport(null,new UninterruptedDeferAgent());
            if(!await importer.Load(new Uri(file).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}))
                throw new Exception("Existing road import failed");
            var road=new GameObject("Bastion I existing authored continuous paving");
            road.transform.SetParent(assembly.transform,false);
            if(!await importer.InstantiateMainSceneAsync(road.transform))throw new Exception("Road instantiate failed");
            Bastion1MaterialBindings.Apply(road,importer,file);
            int selected=0;
            foreach(var renderer in road.GetComponentsInChildren<Renderer>(true))
            {
                if((renderer.name=="MainRoad"||renderer.name.StartsWith("MainRoad_",StringComparison.Ordinal))){selected++;continue;}
                renderer.gameObject.SetActive(false);
            }
            if(selected!=1)throw new Exception("Exactly one joined frozen MainRoad model required, got "+selected);
            var bounds=new Bounds();bool first=true;
            foreach(var renderer in road.GetComponentsInChildren<Renderer>())
            {if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}
            if(first||bounds.size.x<1||bounds.size.z<10)throw new Exception("Existing road bounds invalid");
            var scale=new Vector3(4.1f/bounds.size.x,1,24f/bounds.size.z);
            road.transform.localScale=scale;
            road.transform.position=new Vector3(-bounds.center.x*scale.x,.245f-bounds.min.y,6.5f-bounds.center.z*scale.z);
            foreach(var t in assembly.GetComponentsInChildren<Transform>(true))
                if(t.name.StartsWith("CentralRoad_",StringComparison.Ordinal))t.gameObject.SetActive(false);
            Debug.Log("BASTION1_EXISTING_CONTINUOUS_ROAD donor_sha256=683310d65fd83de40115b310b35ceae8b35b935baaf8305a068d8e1203709f67 selected_source_meshes="+selected+
                " source_mesh_vertices_modified=false route_z_min=-5.5 route_z_max=18.5 width=4.1 author=existing_closed_family_instance_integration");
        }
    }
}
