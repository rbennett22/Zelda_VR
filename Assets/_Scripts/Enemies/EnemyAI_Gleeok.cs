using UnityEngine;

public class EnemyAI_Gleeok : MonoBehaviour 
{

    public GameObject headPrefab;
    public GameObject angryHeadPrefab;
    public Transform headsContainer;
    public Transform neckMarker;
    public int numHeads = 2;


    void Start()
    {
        transform.forward = new Vector3(0, 0, -1);

        for (int i = 0; i < numHeads; i++)
        {
            float phaseOffset = 2 * Mathf.PI * i / numHeads;
            SpawnHead(phaseOffset);
        }
    }

    void SpawnHead(float phaseOffset)
    {
        GameObject head = Instantiate(headPrefab) as GameObject;
        head.name = headPrefab.name;
        head.transform.parent = headsContainer;
        head.transform.localPosition = Vector3.zero;

        EnemyAI_GleeokHead gleeokHead = head.GetComponent<EnemyAI_GleeokHead>();
        gleeokHead.phaseOffset = phaseOffset;
        gleeokHead.neckMarker = neckMarker;
        gleeokHead.gleeok = this;
    }


    void OnHeadDied(EnemyAI_GleeokHead head)
    {
        if (--numHeads > 0)
        {
            SpawnAngryHead(head.transform.position);
        }
        else
        {
            HealthController hc = GetComponent<HealthController>();
            hc.isIndestructible = false;
            hc.Kill(null, true);
        }
    }

    public void SpawnAngryHead(Vector3 position)
    {
        GameObject angryHead = Instantiate(angryHeadPrefab, position, Quaternion.identity) as GameObject;
        angryHead.transform.parent = headsContainer;
    }

}