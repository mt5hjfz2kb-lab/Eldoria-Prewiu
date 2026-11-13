using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    // One presentation vocabulary, without owning any gameplay or save state.
    public static class BreachWatchControlKit
    {
        public enum Role { Primary, Secondary, TabActive, TabInactive, Icon }
        public static readonly Color Gold = new Color(.94f,.75f,.45f,1f);
        public static readonly Color Ivory = new Color(.97f,.93f,.84f,1f);
        static Sprite action, stone, slicedStone, icon;
        static Sprite Skin(Role role)
        {
            if(role==Role.Icon)
                return icon ? icon : (icon=Resources.Load<Sprite>(
                    "VigiliaArt/OwnerControls/close_medallion"));
            if(role==Role.Primary || role==Role.TabActive)
                return action ? action : (action=Resources.Load<Sprite>(
                    "VigiliaArt/PremiumSurfaces/burgundy_action"));
            if(!stone)stone=Resources.Load<Sprite>(
                "VigiliaArt/PremiumSurfaces/panel_fill");
            if(!stone)return null;
            if(stone.border.sqrMagnitude>.1f)return stone;
            if(!slicedStone)
            {
                var r=stone.rect;
                float edge=Mathf.Min(44f,Mathf.Min(r.width,r.height)*.12f);
                slicedStone=Sprite.Create(stone.texture,r,new Vector2(.5f,.5f),
                    stone.pixelsPerUnit,0,SpriteMeshType.FullRect,
                    new Vector4(edge,edge,edge,edge));
                slicedStone.name="Vigilia / canonical secondary nine-slice";
            }
            return slicedStone;
        }
        public static bool Paint(Image image,Role role,bool raycast=true)
        {
            if(!image)return false;
            var sprite=Skin(role);
            if(!sprite)return false;
            if(image.sprite!=sprite)image.sprite=sprite;
            var kind=role==Role.Icon ? Image.Type.Simple : Image.Type.Sliced;
            if(image.type!=kind)image.type=kind;
            bool aspect=role==Role.Icon;
            if(image.preserveAspect!=aspect)image.preserveAspect=aspect;
            if(image.color!=Color.white)image.color=Color.white;
            if(image.raycastTarget!=raycast)image.raycastTarget=raycast;
            return true;
        }
        public static bool IsMailboxTab(string name)
        {
            return name=="Buzón informes" || name=="Buzón recompensas" ||
                name=="Buzón sistema" || name=="Buzón narrativa";
        }
    }
}
