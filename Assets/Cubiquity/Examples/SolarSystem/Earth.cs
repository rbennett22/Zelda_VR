using UnityEngine;

//FIXME - Should check the .Net rules regarding how the naming of namespaces corresponds to the naming of .dlls.
namespace CubiquityExamples
{
    public class Earth : MonoBehaviour
    {
        public float earthOrbitSpeed = 1.0f;
        public float earthRotationSpeed = -5.0f;

        void Update()
        {
            GameObject earthOrbitPoint = transform.parent.gameObject;
            earthOrbitPoint.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * earthOrbitSpeed);
            transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * earthRotationSpeed);
        }
    }
}