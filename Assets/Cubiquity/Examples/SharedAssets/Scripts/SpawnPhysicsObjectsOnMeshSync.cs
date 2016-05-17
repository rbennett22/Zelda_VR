using Cubiquity;
using UnityEngine;

public class SpawnPhysicsObjectsOnMeshSync : MonoBehaviour
{
    Volume volume;

    void OnEnable()
    {
        volume = gameObject.GetComponent<Volume>();

        // After pressing play, it can take some time for our volume to load and for the mesh to syncronize.
        // If we spawn the physics objects too early they will just fall through the floor. Therefore we use
        // this event to wait until the mesh is loaded, and then we spawn the objects.
        if (volume != null)
        {
            volume.OnMeshSyncComplete += SpawnPhysicsObjects;
        }
        else
        {
            Debug.LogError("This example script should be attached to a game object with a TerrainVolume or ColoredCubesVolume attached");
        }
    }

    void OnDisable()
    {
        if (volume != null)
        {
            volume.OnMeshSyncComplete -= SpawnPhysicsObjects;
        }
    }

    void SpawnPhysicsObjects()
    {
        // Add a bunch of physics objects to the scene.
        int objectSpacing = 32;
        for (int z = 32; z <= 96; z += objectSpacing)
        {
            for (int x = 32; x <= 96; x += objectSpacing)
            {
                GameObject physicsObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                physicsObject.transform.localPosition = new Vector3(x, 64.0f, z);
                physicsObject.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);

                physicsObject.AddComponent<Rigidbody>();
            }
        }

        // Mesh sync events continue to occur each time the user edits the volume.
        // We don't want to spawn objects every time so we unsubscribe from this event.
        volume.OnMeshSyncComplete -= SpawnPhysicsObjects;
    }
}