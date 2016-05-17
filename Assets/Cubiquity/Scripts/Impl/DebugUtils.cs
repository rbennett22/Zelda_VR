using UnityEngine;

namespace Cubiquity
{
    namespace Impl
    {
        public class DebugUtils
        {
            public static bool assertsEnabled = false;

            // Use of 'Conditional' means that this function only exists if the condition is met.
            // More info here: http://answers.unity3d.com/questions/19122/assert-function.html
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void Assert(bool condition, string message)
            {
                if (assertsEnabled && (!condition))
                {
                    Debug.LogWarning("Cubiquity Assert() failed: " + message);
                }
            }
        }
    }
}