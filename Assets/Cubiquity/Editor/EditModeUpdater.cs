using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    [InitializeOnLoad]
    public class EditModeUpdater
    {
        static EditModeUpdater()
        {
            EditorApplication.update -= updateAllVolumes; // Make sure it's not already added.
            EditorApplication.update += updateAllVolumes; // Make sure it is now added.
        }

        static void updateAllVolumes()
        {
            Object[] volumes = Object.FindObjectsOfType(typeof(Volume));
            foreach (Object volume in volumes)
            {
                ((Volume)volume).EditModeUpdateHandler();
            }
        }
    }
}