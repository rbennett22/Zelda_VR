using UnityEngine;

//FIXME - Should check the .Net rules regarding how the naming of namespaces corresponds to the naming of .dlls.
namespace CubiquityExamples
{
    public class Moon : MonoBehaviour
    {
        public float moonOrbitSpeed = 3.0f;
        public float moonRotationSpeed = -10.0f;

        void Update()
        {
            GameObject moonOrbitPoint = transform.parent.gameObject;
            moonOrbitPoint.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * moonOrbitSpeed);
            transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * moonRotationSpeed);
        }
    }
}