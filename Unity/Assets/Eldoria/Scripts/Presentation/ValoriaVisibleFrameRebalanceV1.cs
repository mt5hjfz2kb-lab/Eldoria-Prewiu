using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Audit-driven rebalance of the renderers that actually dominate the current official frame.
    // Preserves textures and geometry; only presentation renderers/material multipliers are changed.
    public static class ValoriaVisibleFrameRebalanceV1
    {
        public static bool Enabled=false;
        public static int ForegroundSuppressed{get;private set;}
        public static int RenderersRetinted{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            ForegroundSuppressed=0;
            RenderersRetinted=0;

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);

                // The visible-renderer audit shows this legacy edge family as large disconnected pale boulders.
                // The accepted cliff substrate now provides the actual foreground mass, so remove the redundant family.
                if(chain.Contains("valoria · full frame foreground edge v1"))
                {
                    r.enabled=false;
                    ForegroundSuppressed++;
                    continue;
                }

                // Never disturb the Hero Bastion, backplate or heraldry.
                if(chain.Contains("certified hero bastion")||
                   chain.Contains("valoria · bastion hero")||
                   chain.Contains("backplate")||
                   chain.Contains("banner")||
                   chain.Contains("flag"))continue;

                Color tint;
                bool apply=false;

                // Largest audited white surfaces.
                if(chain.Contains("valoria · cliff island"))
                {
                    tint=new Color(.66f,.62f,.54f,1f); apply=true;
                }
                else if(chain.Contains("vpd · upper terrace earth")||
                        (chain.Contains("groundkit")&&chain.Contains("terrace")))
                {
                    tint=new Color(.63f,.55f,.43f,1f); apply=true;
                }
                else if(chain.Contains("groundkit l1 landing"))
                {
                    tint=new Color(.62f,.58f,.50f,1f); apply=true;
                }
                else if(chain.Contains("assetlibrary reprocessing")&&chain.Contains("hero")&&chain.Contains("terrace"))
                {
                    tint=new Color(.72f,.68f,.60f,1f); apply=true;
                }
                else if(chain.Contains("valoria · full frame architecture"))
                {
                    if(chain.Contains("roof"))tint=new Color(.38f,.39f,.37f,1f);
                    else if(chain.Contains("work wing"))tint=new Color(.55f,.46f,.34f,1f);
                    else tint=new Color(.64f,.59f,.50f,1f);
                    apply=true;
                }
                else if(chain.Contains("aserradero · dedicated sawmill"))
                {
                    tint=new Color(.72f,.62f,.48f,1f); apply=true;
                }
                else if(chain.Contains("valoria · cliff cleanup · military hall"))
                {
                    tint=new Color(.64f,.58f,.49f,1f); apply=true;
                }
                else if(chain.Contains("rescued upper civil residence")||
                        chain.Contains("rescued seam residential"))
                {
                    tint=new Color(.66f,.61f,.52f,1f); apply=true;
                }
                else if(chain.Contains("terrainterrace")&&
                        (chain.Contains("support")||chain.Contains("training edge")))
                {
                    tint=new Color(.64f,.61f,.54f,1f); apply=true;
                }
                else continue;

                if(apply)
                {
                    ApplyTint(r,tint);
                    RenderersRetinted++;
                }
            }
        }

        static void ApplyTint(Renderer r,Color tint)
        {
            var block=new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            bool hasBase=false,hasColor=false,hasFactor=false;
            foreach(var m in r.sharedMaterials)
            {
                if(m==null)continue;
                hasBase|=m.HasProperty("_BaseColor");
                hasColor|=m.HasProperty("_Color");
                hasFactor|=m.HasProperty("_BaseColorFactor");
            }
            if(hasBase)block.SetColor("_BaseColor",tint);
            if(hasColor)block.SetColor("_Color",tint);
            if(hasFactor)block.SetColor("_BaseColorFactor",tint);
            r.SetPropertyBlock(block);
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
