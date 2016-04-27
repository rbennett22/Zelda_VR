using UnityEngine;

public interface IOverlayShutterViewDelegate
{
    void OnCloseFinished(OverlayShuttersView sender);
    void OnOpenFinished(OverlayShuttersView sender);
}

public class OverlayShuttersView : MonoBehaviour
{
    public IOverlayShutterViewDelegate viewDelegate;

    public float shutterSpeed = 500.0f;


    float _leftShutterX, _rightShutterX;
    float _shutterWidth = Screen.width * 0.6f;
    float _shutterHeight = Screen.height * 2;
    float _screenCenterX = Screen.width * 0.5f;

    float _realDeltaTime;
    float _prevRealTime;


    public bool IsClosing { get; private set; }
    public bool IsOpening { get; private set; }

    public void Close()
    {
        if (IsClosing) { return; }

        _leftShutterX = _screenCenterX - _shutterWidth * 2;
        _rightShutterX = _screenCenterX + _shutterWidth;

        IsClosing = true;
    }

    public void Open()
    {
        if (IsOpening) { return; }

        _leftShutterX = _screenCenterX - _shutterWidth;
        _rightShutterX = _screenCenterX;

        IsOpening = true;
    }


    void Update()
    {
        float realTime = Time.realtimeSinceStartup;
        _realDeltaTime = realTime - _prevRealTime;

        float deltaX = _realDeltaTime * shutterSpeed;

        if (IsClosing)
        {
            _leftShutterX += deltaX;
            _rightShutterX -= deltaX;

            if (_leftShutterX > _screenCenterX - _shutterWidth + 25)
            {
                OnCloseFinished();
            }
        }
        else if (IsOpening)
        {
            _leftShutterX -= deltaX;
            _rightShutterX += deltaX;

            if (_leftShutterX < _screenCenterX - 2 * _shutterWidth)
            {
                OnOpenFinished();
            }
        }

        _prevRealTime = realTime;
    }

    void OnCloseFinished()
    {
        IsClosing = false;

        if (viewDelegate != null)
        {
            viewDelegate.OnCloseFinished(this);
        }
    }
    void OnOpenFinished()
    {
        IsOpening = false;

        if (viewDelegate != null)
        {
            viewDelegate.OnOpenFinished(this);
        }
    }
}