using UnityEngine;

namespace Cubiquity
{
    /// Causes the volume to have a collision mesh and allows it to participate in collisions.
    /**
	 * The role of the VolumeCollider component for volumes is conceptually similar to the role of Unity's MeshCollider class for meshes.
	 * Specifically, it can be attached to a GameObject which also has a Volume component to cause that Volume component to be able to
	 * collide with other colliders.
	 *
	 * \sa VolumeRenderer
	 */

    public abstract class VolumeCollider : MonoBehaviour
    {
        void OnEnable()
        {
            hasChanged = true;
        }

        void OnDisable()
        {
            hasChanged = true;
        }

        /// \cond
        [System.NonSerialized]
        public bool hasChanged = true;

        /// \endcond

        public bool useInEditMode
        {
            get
            {
                return mUseInEditMode;
            }
            set
            {
                if (mUseInEditMode != value) // Important, as this setter get call often by inspector.
                {
                    mUseInEditMode = value;

                    // No need to set hasChanged as that is for syncing properties. In this
                    // case we need to add/remove components, which means rebuilding the volume.
                    Volume volume = gameObject.GetComponent<Volume>();
                    volume.RequestFlushInternalData();
                }
            }
        }
        [SerializeField]
        private bool mUseInEditMode = false;

        // Dummy start method rqured for the 'enabled' checkbox to show up in the inspector.
        void Start() { }
    }
}