using UnityEngine;

public class DisableOnAwake : MonoBehaviour
{
    [SerializeField]
    bool _override;

    void Awake()
    {
        if (!_override)
        {
            gameObject.SetActive(false);
        }
    }
}