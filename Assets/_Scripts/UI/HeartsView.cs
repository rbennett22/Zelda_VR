using UnityEngine;

public class HeartsView : MonoBehaviour
{
    const int MAX_NUM_HEARTS = 16;


    [SerializeField]
    GameObject _heartImagePrefab;


    HeartImage[] _heartImages = new HeartImage[MAX_NUM_HEARTS];


    int _numHeartContainers;
    public int NumHeartContainers {
        get { return _numHeartContainers; }
        set
        {
            if(value == _numHeartContainers)
            {
                return;
            }
            _numHeartContainers = value;

            for (int i = 0; i < _heartImages.Length; i++)
            {
                _heartImages[i].gameObject.SetActive(i < _numHeartContainers);
            }
        }
    }


    void Awake ()
    {
        InstantiateHeartImages();
    }
    void InstantiateHeartImages()
    {
        for (int i = 0; i < MAX_NUM_HEARTS; i++)
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
}