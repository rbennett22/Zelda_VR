using UnityEngine;


public class Candle : MonoBehaviour
{
    public GameObject flamePrefab;
    public AudioClip flameDropSound;
    public float flameDropDistance = 2.0f;
    public float cooldown = 1.0f;

    float FlameRadius { get { return flamePrefab.GetComponent<SphereCollider>().radius; } }


    GameObject _spawnedFlame = null;
    Transform _projectilesContainer;
    float _lastFlameDropTime = float.NegativeInfinity;


    public bool CanUse { 
        get {
            if (_spawnedFlame != null) { return false; }
            return Time.time - _lastFlameDropTime > cooldown;
        }
    }


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;
    }

    public void DropFlame()
    {
        if (!CanUse) { return; }

        _spawnedFlame = Instantiate(flamePrefab) as GameObject;
        DetermineFlameDropPosition(_spawnedFlame);
        _spawnedFlame.transform.parent = _projectilesContainer;

        SoundFx.Instance.PlayOneShot(flameDropSound);

        DungeonRoom dr = CommonObjects.Player_C.OccupiedDungeonRoom();
        if (dr != null && !dr.IsNpcRoom)
        {
            dr.ActivateTorchLights();
        }

        NotifyOnDestroy n = _spawnedFlame.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnFlameDestroyed";

        _lastFlameDropTime = Time.time;
    }

    void DetermineFlameDropPosition(GameObject flame)
    {
        Vector3 flamePos;

        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Ray ray = new Ray(pos, forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, flameDropDistance))
        {
            flamePos = pos + forward * (hitInfo.distance - FlameRadius);
        }
        else
        {
            flamePos = pos + forward * flameDropDistance;
        }

        flame.transform.position = flamePos;
    }

    void OnFlameDestroyed()
    {
        //print("OnFlameDestroyed");
        _spawnedFlame = null;
    }

}
