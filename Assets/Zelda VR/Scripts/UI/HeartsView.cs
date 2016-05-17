using UnityEngine;

public class HeartsView : MonoBehaviour
{
    const int MAX_NUM_HEARTS = 16;


    [SerializeField]
    GameObject _heartImagePrefab;


    HeartImage[] _heartImages = { };
    int _numHeartContainers;


    void Awake()
    {
        InstantiateHeartImages();
    }
    void InstantiateHeartImages()
    {
        _heartImages = new HeartImage[MAX_NUM_HEARTS];

        for (int i = 0; i < _heartImages.Length; i++)
        {
            _heartImages[i] = InstantiateHeartImage();
        }
    }
    HeartImage InstantiateHeartImage()
    {
        GameObject g = Instantiate(_heartImagePrefab) as GameObject;

        Transform t = g.transform;
        t.SetParent(transform);
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.one;

        return g.GetComponent<HeartImage>();
    }


    public void UpdateHeartContainerCount(int amount)
    {
        amount = Mathf.Clamp(amount, 0, MAX_NUM_HEARTS);
        if (amount == _numHeartContainers)
        {
            return;
        }
        _numHeartContainers = amount;

        for (int i = 0; i < _heartImages.Length; i++)
        {
            _heartImages[i].gameObject.SetActive(i < _numHeartContainers);
        }
    }
    public void UpdateHeartContainersFillState(int numHalfHearts)
    {
        numHalfHearts = Mathf.Clamp(numHalfHearts, 0, _numHeartContainers * 2);

        int numFullHearts = (int)(numHalfHearts * 0.5f);
        bool plusHalfHeart = numHalfHearts % 2 == 1;

        for (int i = 0; i < _heartImages.Length; i++)
        {
            HeartImage heartImage = _heartImages[i].GetComponent<HeartImage>();

            if (i < numFullHearts)
            {
                heartImage.SetToFull();
            }
            else if (i == numFullHearts && plusHalfHeart)
            {
                heartImage.SetToHalfFull();
            }
            else
            {
                heartImage.SetToEmpty();
            }
        }
    }
}