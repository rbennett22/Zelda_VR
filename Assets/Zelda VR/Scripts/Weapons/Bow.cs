using UnityEngine;


public class Bow : MonoBehaviour
{
    public GameObject arrowPrefab;
    public AudioClip arrowFireSound;
    public Vector3 bowPositionOffset = new Vector3(0, 0.15f, 0);

    //public Texture CrosshairImage = null;
    //OVRCrosshair Crosshair = new OVRCrosshair();


    GameObject _spawnedArrow = null;
    Transform _projectilesContainer;


    public bool CanUse { get { return (_spawnedArrow == null); } }


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;

        //InitCrossHair();
    }

    /*void InitCrossHair()
    {
        Crosshair.Init();
        Crosshair.SetCrosshairTexture(ref CrosshairImage);
        OVRCameraController cc = GameObject.FindGameObjectWithTag("CameraController").GetComponent<OVRCameraController>();
        OVRPlayerController pc = CommonObjects.PlayerController_C;
        Crosshair.SetOVRCameraController(ref cc);
        Crosshair.SetOVRPlayerController(ref pc);
    }*/

    void Start()
    {
        transform.localPosition += bowPositionOffset;
    }


    /*void Update()
    {
        Crosshair.UpdateCrosshair();
    }

    void OnGUI()
    {
        Crosshair.OnGUICrosshair();
    }*/

    public void Fire()
    {
        _spawnedArrow = Instantiate(arrowPrefab) as GameObject;
        _spawnedArrow.name = arrowPrefab.name;

        float arrowLength = 0;
        CapsuleCollider cc = _spawnedArrow.GetComponent<CapsuleCollider>();
        if (cc != null) { arrowLength = cc.height; }
        else
        {
            SphereCollider sc = _spawnedArrow.GetComponent<SphereCollider>();
            if (sc != null) { arrowLength = sc.radius * 2; }
        }

        Vector3 offset = arrowLength * 0.7f * transform.forward;

        SimpleProjectile p = _spawnedArrow.GetComponent<SimpleProjectile>();
        p.transform.parent = _projectilesContainer;
        p.transform.position = transform.position + offset;

        p.transform.up = transform.forward;
        /*Vector3 euler = p.transform.rotation.eulerAngles;
        euler.x = 90;
        p.transform.rotation = Quaternion.Euler(euler);*/

        p.direction = transform.forward;

        SoundFx.Instance.PlayOneShot(arrowFireSound);

        NotifyOnDestroy n = _spawnedArrow.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnArrowDestroyed";
    }

    void OnArrowDestroyed()
    {
        //print("OnArrowDestroyed");
        _spawnedArrow = null;
    }
}