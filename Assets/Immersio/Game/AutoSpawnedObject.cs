using UnityEngine;

public interface IAutoSpawner {
	void OnSpawnedObjectDestroy (AutoSpawnedObject spawnedObject);
}

public class AutoSpawnedObject : MonoBehaviour {
	public IAutoSpawner autoSpawner;

	void OnDestroy () {
		autoSpawner.OnSpawnedObjectDestroy(this);
	}
}
