using Cubiquity;
using UnityEngine;

public class MeshSyncHandler : MonoBehaviour
{
    Volume volume;

    void OnEnable()
    {
        volume = gameObject.GetComponent<Volume>();

        if (volume != null)
        {
            volume.OnMeshSyncComplete += PrintSyncCompleteMessage;
            volume.OnMeshSyncLost += PrintSyncLostMessage;
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
            volume.OnMeshSyncComplete -= PrintSyncCompleteMessage;
            volume.OnMeshSyncLost -= PrintSyncLostMessage;
        }
    }

    void PrintSyncCompleteMessage()
    {
        Debug.Log("Mesh syncronization complete");
    }

    void PrintSyncLostMessage()
    {
        Debug.Log("Mesh syncronization lost");
    }
}