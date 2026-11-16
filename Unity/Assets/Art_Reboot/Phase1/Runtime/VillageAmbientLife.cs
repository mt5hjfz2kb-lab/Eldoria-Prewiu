using UnityEngine;
namespace Eldoria.ArtReboot
{
    public sealed class VillageAmbientLife : MonoBehaviour
    {
        public Material warmWindows;
        public float baseGlow=1.65f;
        void Update(){Evaluate(Time.time);}
        public void Evaluate(float time)
        {
            if(warmWindows!=null)warmWindows.SetFloat("_Glow",baseGlow*(1f+.018f*Mathf.Sin(time*1.7f)+.009f*Mathf.Sin(time*3.1f)));
        }
    }
}
