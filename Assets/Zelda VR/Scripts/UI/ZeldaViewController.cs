using UnityEngine;

public static class ZeldaViewController 
{
    public static GameObject InstantiateView(GameObject prefab, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Failed to instantiate the view because the provided prefab is null");
            return null;
        }

        GameObject g = Object.Instantiate(prefab);
        g.name = prefab.name;

        Transform t = g.transform;
        t.SetParent(parent);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        return g;
    }
}
