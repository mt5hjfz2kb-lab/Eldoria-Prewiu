using System;
using System.Collections.Generic;
using UnityEngine;
namespace Eldoria.Experimentos.ValoriaModular25D
{
    // Isolated competition preview. Same original sprites/foot/scale except
    // three authored candidates, only at V. No promotion or gameplay changes.
    public sealed class Issue200CompetitionArt
    {
        public static readonly string[] Names={"AceraZapateros","ArmeriaDelPatio","CorteCaliza"};
#if UNITY_EDITOR
        public static int PreviewMask=7;
#else
        public const int PreviewMask=7;
#endif
        readonly List<(SpriteRenderer r,Sprite original,int index)> entries=new List<(SpriteRenderer,Sprite,int)>();
        readonly Sprite[] candidates=new Sprite[3];
        public void Register(SpriteRenderer r){
            int i=Array.IndexOf(Names,r.sprite.name);
            if(i>=0)entries.Add((r,r.sprite,i));
        }
        public void Apply(int level){
            foreach(var e in entries){
                bool use=level==5&&(PreviewMask&(1<<e.index))!=0;
                if(use&&candidates[e.index]==null){
                    candidates[e.index]=Resources.Load<Sprite>("Issue200Competition/"+Names[e.index]);
                    if(!candidates[e.index])throw new InvalidOperationException("Missing competition asset "+Names[e.index]);
                }
                e.r.sprite=use?candidates[e.index]:e.original;
                if(e.r.gameObject.activeInHierarchy)e.r.GetComponent<Modular25DArchitecturalShadow>()?.Refresh();
            }
        }
    }
}
