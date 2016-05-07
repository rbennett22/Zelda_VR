#pragma warning disable 0649 // variable is never assigned to

using UnityEngine;

public interface IOverlayShutterViewDelegate
{
    void OnCloseFinished(OverlayShuttersView sender);
    void OnOpenFinished(OverlayShuttersView sender);
}

public class OverlayShuttersView : MonoBehaviour
{
    public IOverlayShutterViewDelegate viewDelegate;

    [SerializeField]
    GameObject _shutterTop, _shutterBottom;


    public bool IsClosing { get; private set; }
    public bool IsOpening { get; private set; }


    public void Close()
    {
        if (IsClosing) { return; }
        IsClosing = true;

        DoTweenShutters("Close");
    }

    public void Open()
    {
        if (IsOpening) { return; }
        IsOpening = true;

        DoTweenShutters("Open");
    }

    void DoTweenShutters(string tweenName)
    {
        iTweenEvent.GetEvent(_shutterTop, tweenName).Play();
        iTweenEvent.GetEvent(_shutterBottom, tweenName).Play();
    }

    // OnCloseFinished will be called by iTween
    void OnCloseFinished()
    {
        IsClosing = false;

        if (viewDelegate != null)
        {
            viewDelegate.OnCloseFinished(this);
        }
    }
    // OnOpenFinished will be called by iTween
    void OnOpenFinished()
    {
        IsOpening = false;

        if (viewDelegate != null)
        {
            viewDelegate.OnOpenFinished(this);
        }
    }
}