using UnityEngine;

namespace Uniblocks
{
    public class ChunkExtension : MonoBehaviour
    {
        void Awake()
        {
            if (GetComponent<MeshRenderer>() == null)
            {
                gameObject.layer = Engine.NO_COLLIDE_LAYER;
            }
        }
    }
}