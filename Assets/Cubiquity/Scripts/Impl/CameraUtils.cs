using UnityEngine;

namespace Cubiquity.Impl
{
    public class CameraUtils
    {
        private static Vector3 currentCameraPosition = new Vector3(0.0f, 0.0f, 0.0f);

        public static Vector3 getCurrentCameraPosition()
        {
            if (Camera.current != null)
            {
                currentCameraPosition = Camera.current.transform.position;
            }

            return currentCameraPosition;
        }
    }
}