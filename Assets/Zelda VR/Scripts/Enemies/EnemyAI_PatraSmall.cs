using UnityEngine;
using System.Collections;


public class EnemyAI_PatraSmall : MonoBehaviour 
{
    public float phaseOffset;
    public float rotationSpeed = 3.0f;
    

    float _startTime;


    public EnemyAI_Patra ParentPatra { get; set; }
    public float Radius { get; set; }       // Radius of circle around ParentPatra


    void Start()
    {
        _startTime = Time.time;
    }


	void Update () 
    {
        float time = Time.time - _startTime;

        float x = Radius * Mathf.Cos(time * rotationSpeed + phaseOffset);
        float y = transform.localPosition.y;
        float z = Radius * Mathf.Sin(time * rotationSpeed + phaseOffset);

        transform.localPosition = new Vector3(x, y, z);
	}

}