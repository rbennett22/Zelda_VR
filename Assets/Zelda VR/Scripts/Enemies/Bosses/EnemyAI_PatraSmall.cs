using UnityEngine;

public class EnemyAI_PatraSmall : EnemyAI 
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
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        float time = Time.time - _startTime;
        float theta = time * rotationSpeed + phaseOffset;

        float x = Radius * Mathf.Cos(theta);
        float y = transform.localPosition.y;
        float z = Radius * Mathf.Sin(theta);

        transform.localPosition = new Vector3(x, y, z);
	}
}