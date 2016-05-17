using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    [InitializeOnLoad]
    public class Autorun
    {
        static Autorun()
        {
            // We don't want to annoy the user with these messages every time they go in and out of
            // play mode. This is a crude way of only doing it the first time they launch the editor.
            bool editorJustLaunched = (EditorApplication.timeSinceStartup < 5);

            if (editorJustLaunched)
            {
#if CUBIQUITY_USE_UNSAFE
                Debug.Log("Cubiquity is currently configured to use 'unsafe' code (as recommended) for communicating with the native code library.");
#else
                Debug.Log("Cubiquity is currently configured to use managed code (the default) for communicating with the native code library.\n" +
                    "Please see the installation section of the user manual for information on enabling 'unsafe' code for maximum performance.");
#endif
            }
        }
    }
}