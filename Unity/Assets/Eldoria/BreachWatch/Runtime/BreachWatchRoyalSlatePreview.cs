using System;
using System.Collections.Generic;
using UnityEngine;
using Eldoria.ReferenceClean;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Reversible visual A/B, only when Vigilia's opt-in standalone HUD is
    /// installed. Changes the source Bastion I's THREE known real slate slots
    /// (8/9/10), never stone, flags, terrain, scripts, prefabs or player saves.
    ///
    /// The approved image has luminous royal-blue tile work. The genuine
    /// 94541b8 Unity capture has grey-blue roof geometry even though its
    /// flags are royal blue. Material identity and geometry are preserved,
    /// with fresh per-instance materials restored on HUD removal.
    /// This is an art CANDIDATE, never automatic approval.
    /// </summary>
    public sealed class BreachWatchRoyalSlatePreview : MonoBehaviour
    {
        readonly Dictionary<MeshRenderer,Material[]> originals =
            new Dictionary<MeshRenderer,Material[]>();
        readonly List<Material> copies = new List<Material>();

        public int RecoloredSlots { get; private set; }
        public bool Applied => RecoloredSlots==3;

        public void Apply(ValoriaIntegratedWorld host)
        {
            if(!host || !host.City || !host.City.BastionOne || originals.Count!=0)return;
            var renderers=host.City.BastionOne.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var renderer in renderers)
            {
                if(!renderer || renderer.name!="Bastion_I")continue;
                var materials=renderer.sharedMaterials;
                if(materials.Length<=10)continue;
                var next=(Material[])materials.Clone();
                int altered=0;
                for(int slot=8;slot<=10;slot++)
                {
                    var source=materials[slot];
                    if(!source)continue;
                    // Architecture201 established these precise genuine roof
                    // indices. GLB material names can be changed by import;
                    // never infer failure from the display name alone. Log
                    // the exact scene identity and verify by native A/B.
                    Debug.Log("VIGILIA_SLATE_SOURCE renderer="+renderer.name+
                        " slot="+slot+" material="+source.name);
                    var material=new Material(source);
                    material.name="Vigilia A/B royal-blue genuine slate "+slot;
                    // Bright, saturated lapis with visible tile grain.
                    // This is a material tint, not a 2D screenshot or model swap.
                    var blue=slot==8
                        ?new Color(.17f,.43f,.84f,1f)
                        :slot==9
                            ?new Color(.14f,.36f,.73f,1f)
                            :new Color(.20f,.49f,.91f,1f);
                    if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",blue);
                    if(material.HasProperty("baseColorFactor"))material.SetColor("baseColorFactor",blue);
                    if(material.HasProperty("_Color"))material.SetColor("_Color",blue);
                    if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",.015f);
                    if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.27f);
                    // Tiny reflected sky tone keeps recessed tiles blue in
                    // shadow, without an emissive effect on stone or banners.
                    if(material.HasProperty("_EmissionColor"))
                    {
                        material.SetColor("_EmissionColor",new Color(.008f,.021f,.039f,1f));
                        material.EnableKeyword("_EMISSION");
                    }
                    copies.Add(material);
                    next[slot]=material;
                    altered++;
                }
                if(altered!=0)
                {
                    originals.Add(renderer,materials);
                    renderer.sharedMaterials=next;
                    RecoloredSlots+=altered;
                }
            }
            Debug.Log("VIGILIA_WORLD_ROYAL_SLATE_AB realBastionSlateSlots="+
                RecoloredSlots+" prefabUnchanged=true saveUnchanged=true");
        }

        void OnDestroy()
        {
            foreach(var item in originals)
                if(item.Key)item.Key.sharedMaterials=item.Value;
            originals.Clear();
            foreach(var mat in copies)if(mat)Destroy(mat);
            copies.Clear();
            RecoloredSlots=0;
        }
    }
}
