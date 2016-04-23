using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class HeartImage : MonoBehaviour
{
    public Sprite emptySprite, halfFullSprite, fullSprite;


    Image _image;


    public enum FillState
    {
        Empty,
        Half,
        Full
    }
    FillState _state;
    public FillState State
    {
        get { return _state; }
        set
        {
            _state = value;

            switch (_state)
            {
                case FillState.Empty: _image.sprite = emptySprite; break;
                case FillState.Half: _image.sprite = halfFullSprite; break;
                case FillState.Full: _image.sprite = fullSprite; break;
                default: break;
            }
        }
    }


    void Awake()
    {
        _image = GetComponent<Image>();

        State = FillState.Full;
    }
}