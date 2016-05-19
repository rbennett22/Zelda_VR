using UnityEngine;

public class ObjectSpawner : ObjectInstantiator
{
    const string INSTANTIATE_METHOD_NAME = "DoInstantiate";


    public float delay;
    public float repeatRate;


    public void Begin()
    {
        InvokeRepeating(INSTANTIATE_METHOD_NAME, delay, repeatRate);
    }
    public void Stop()
    {
        CancelInvoke(INSTANTIATE_METHOD_NAME);
    }
}