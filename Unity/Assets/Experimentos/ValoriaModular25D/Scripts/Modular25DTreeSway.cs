using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    public sealed class Modular25DTreeSway : MonoBehaviour
    {
        public float Seed;
        void Update(){transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*.54f+Seed)*.65f);}
    }


    /// <summary>
    /// One gesture owner. A drag or pinch can NEVER also select a building.
    /// World interaction is blocked by actual uGUI raycasts, not estimated
    /// screen bands that fail with aspect ratio or open construction panels.
    /// </summary>
}
