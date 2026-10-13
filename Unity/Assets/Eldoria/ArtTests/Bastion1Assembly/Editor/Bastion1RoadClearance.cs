using System;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Integration correction on the candidate terrain instance, not a new
    // artistic asset or Qwen-authored operation. Preserve original road meshes,
    // stair elevations, building anchors, terrain topology and UVs. The measured
    // road top is y=.29; old terrain reaches y=1.29 underneath it near z=-5.
    internal static class Bastion1RoadClearance
    {
        internal static void Apply(GameObject root)
        {
            int changed=0;
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if(!filter.name.Contains("playable hilltop")&&!filter.name.Contains("cliff plateau"))continue;
                var source=filter.sharedMesh;if(source==null)continue;
                var vertices=source.vertices;int localChanged=0;
                for(int i=0;i<vertices.Length;i++)
                {
                    var world=filter.transform.TransformPoint(vertices[i]);
                    // Exact existing stair envelope: z=-5.5..-12.5,
                    // height 0..3.5. Terrain previously covered its first steps.
                    // Preserve paving and stair geometry; lower only their support.
                    if(Mathf.Abs(world.x)>3.5f||world.z< -13.5f||world.z>18.5f)continue;
                    float ceiling=world.z>=-5.5f?.12f:
                        Mathf.Max(-.12f,Mathf.Min(3.5f,(-world.z-5.5f)*.5f)-.4f);
                    if(world.y<=ceiling)continue;
                    world.y=ceiling;vertices[i]=filter.transform.InverseTransformPoint(world);localChanged++;
                }
                if(localChanged==0)continue;
                var instance=UnityEngine.Object.Instantiate(source);
                instance.name=source.name+"_existing_road_clearance";
                instance.vertices=vertices;instance.RecalculateNormals();instance.RecalculateBounds();
                filter.sharedMesh=instance;changed+=localChanged;
            }
            Debug.Log("BASTION1_EXISTING_ROAD_CLEARANCE changed_vertices="+changed+
                " road_geometry_changed=false parcel_anchors_changed=false author=integration_bugfix_not_Qwen");
        }
    }
}
