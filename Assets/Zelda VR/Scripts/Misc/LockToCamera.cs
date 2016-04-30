using UnityEngine;

namespace Eyefluence.Utility
{
    public class LockToCamera : MonoBehaviour
    {
        [SerializeField]
        Transform _uiPlane = null;       // uiPlane should be a child of the active Camera, posed as you want your UI Canvas to be posed
        public Transform UiPlane
        {
            get
            {
                if (_uiPlane == null)
                {
                    _uiPlane = FindUiPlane();
                }
                return _uiPlane;
            }
        }
        //public Transform UiPlane { get { return _uiPlane ?? (_uiPlane = FindUiPlane()); } }       // I don't know why but this doesn't work
        Transform FindUiPlane()
        {
            GameObject g = GameObject.Find("UI Plane");
            if (g == null)
            {
                g = GameObject.Find("CenterEyeAnchor");
                if (g == null)
                {
                    g = GameObject.FindGameObjectWithTag("MainCamera");
                }
            }

            return (g == null) ? null : g.transform;
        }

        public float zOffset = 1;

        // If applyScaling is true, then for a given zOffset the uiPlane will be scaled such that it takes up the
        //  same amount of screen space as it does when (uiPlane.position.z == baseZOffset && uiPlane.scale == baseScale)
        public bool applyScaling;
        public float baseZOffset = 1;
        public Vector3 baseScale = Vector3.one;


        void LateUpdate()
        {
            UpdateTransform();
        }

        public void UpdateTransform()
        {
            if (UiPlane != null)
            {
                transform.position = UiPlane.position + zOffset * UiPlane.forward;
                transform.rotation = UiPlane.rotation;

                if (applyScaling)
                {
                    transform.localScale = baseScale * (zOffset / baseZOffset);
                }
            }
        }
    }
}