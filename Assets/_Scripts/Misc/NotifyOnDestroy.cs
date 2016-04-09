using UnityEngine;


public class NotifyOnDestroy : MonoBehaviour
{

    public GameObject receiver;     // who to notify
    public string methodName = "OnGameObjectDestroyed";


	void OnDestroy () 
    {
        if (receiver != null)
        {
            receiver.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
	}

}
