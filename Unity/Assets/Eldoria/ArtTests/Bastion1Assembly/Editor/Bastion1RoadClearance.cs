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
                    // Only the existing lower paving corridor; staircase starts
                    // beyond z=-5.5. An extra grid cell of margin prevents terrain
                    // interpolation covering the side of the 4m-wide road.
                    if(Mathf.Abs(world.x)>3.5f||world.z< -5.5f||world.z>14.5f||world.y<=.12f)continue;
                    world.y=.12f;vertices[i]=filter.transform.InverseTransformPoint(world);localChanged++;
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
