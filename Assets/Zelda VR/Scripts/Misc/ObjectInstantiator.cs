using UnityEngine;

public class ObjectInstantiator : MonoBehaviour
{
    public GameObject defaultPrefab;

    public bool parentToSelf = true;
    public bool transformRelativeToSelf = true;

    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;


    public void DoInstantiate()
    {
        DoInstantiate(defaultPrefab);
    }
    public void DoInstantiate(GameObject original)
    {
        if (original == null)
        {
            return;
        }

        GameObject g = Instantiate(original) as GameObject;

        Transform t = g.transform;
        t.position = position;
        t.eulerAngles = rotation;
        t.localScale = scale;

        t.SetParent(transform, !transformRelativeToSelf);
        if (!parentToSelf)
        {
            t.SetParent(null, true);
        }
    }
}