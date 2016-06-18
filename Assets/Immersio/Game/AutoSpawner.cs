using UnityEngine;

namespace Immersio.Utility
{
    public class AutoSpawner : MonoBehaviour, IAutoSpawner
    {
        public delegate bool ShouldObjectSpawn_Del(AutoSpawner autoSpawner, ref Vector3 spawnPosition);
        public ShouldObjectSpawn_Del ShouldObjectSpawn;


        public GameObject spawnObjectPrefab;

        public float cooldown = 0.1f;
        public int maxLiveObjects = 5;
        public Transform spawnedObjectParent;
        public bool doAutoSpawn;


        float _cooldownTimer = 0.1f;
        int _currNumObjects = 0;


        public Vector3 spawnOrigin_Local = Vector3.zero;
        public Vector3 SpawnOrigin_World { get { return transform.TransformPoint(spawnOrigin_Local); } }


        void Start()
        {
            if (!spawnedObjectParent)
            {
                spawnedObjectParent = transform.parent;
            }
        }


        public GameObject RequestToSpawnObject()
        {
            return RequestToSpawnObject(SpawnOrigin_World);
        }
        public GameObject RequestToSpawnObject(Vector3 position)
        {
            if (_cooldownTimer > 0 || _currNumObjects >= maxLiveObjects)
            {
                return null;
            }

            if (!GetShouldObjectSpawn(ref position))
            {
                return null;
            }

            return SpawnObject(position);
        }

        bool GetShouldObjectSpawn(ref Vector3 position)
        {
            if (ShouldObjectSpawn == null)
            {
                return true;
            }

            return ShouldObjectSpawn(this, ref position);
        }

        GameObject SpawnObject(Vector3 position)
        {
            GameObject spawnedObj = Instantiate(spawnObjectPrefab, position, Quaternion.identity) as GameObject;

            if (spawnedObjectParent != null)
            {
                spawnedObj.transform.SetParent(spawnedObjectParent);
            }

            _cooldownTimer = cooldown;
            _currNumObjects++;

            AutoSpawnedObject aso = spawnedObj.AddComponent<AutoSpawnedObject>();
            aso.autoSpawner = this;

            return spawnedObj;
        }


        void Update()
        {
            _cooldownTimer -= Time.deltaTime;

            if (doAutoSpawn)
            {
                RequestToSpawnObject();
            }
        }


        #region IAutoSpawner

        void IAutoSpawner.OnSpawnedObjectDestroy(AutoSpawnedObject spawnedObj)
        {
            _currNumObjects--;
        }

        #endregion IAutoSpawner
    }
}