using UnityEngine;


public interface IWorld
{
    void DoUpdate(bool ignoreProxThreshMin = false);
}


public class World : MonoBehaviour, IWorld
{
    [SerializeField]
    GameObject[] _spawnManagers;


	void IWorld.DoUpdate(bool ignoreProxThreshMin = false) 
	{
        foreach (GameObject g in _spawnManagers)
        {
            g.GetComponent<ISpawnManager>().DoUpdate(ignoreProxThreshMin);
        }
	}
}
