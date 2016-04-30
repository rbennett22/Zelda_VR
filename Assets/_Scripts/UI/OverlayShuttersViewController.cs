using UnityEngine;
using System;
using Immersio.Utility;

public class OverlayShuttersViewController : Singleton<OverlayShuttersViewController>, IOverlayShutterViewDelegate
{
    [SerializeField]
    OverlayShuttersView _view;


    float _realDeltaTime;
    float _prevRealTime;
    float _intermissionTimer;
    float _intermissionDuration;
    
    Action _onCloseCompleteCallback, _onOpenCompleteCallback;


    public bool CloseAndOpenSequenceIsPlaying { get; private set; }
    public bool AnySequenceIsPlaying { get { return CloseAndOpenSequenceIsPlaying || _view.IsOpening || _view.IsClosing; } }


    void Awake()
    {
        _view.viewDelegate = this;

        _view.gameObject.SetActive(true);
    }


    public void Close(Action onCompleteCallback)
    {
        if (_view.IsClosing) { return; }

        _onCloseCompleteCallback = onCompleteCallback;

        _view.Close();
    }

    public void Open(Action onCompleteCallback, float delay = 0)
    {
        if (_view.IsOpening) { return; }

        _onOpenCompleteCallback = onCompleteCallback;

        if (delay > 0)
        {
            BeginIntermission(delay);
            return;
        }

        _view.Open();
    }

    public void PlayCloseAndOpenSequence(Action onCloseCompleteCallback, Action onOpenCompleteCallback, float intermissionDuration = 0.0f, bool closeInstantly = false)
    {
        if (AnySequenceIsPlaying) { return; }

        CloseAndOpenSequenceIsPlaying = true;

        _onCloseCompleteCallback = onCloseCompleteCallback;
        _onOpenCompleteCallback = onOpenCompleteCallback;
        _intermissionDuration = intermissionDuration;

        if (closeInstantly)
        {
            OnCloseFinished();
        }
        else
        {
            Close(onCloseCompleteCallback);
        }
    }


    void Update()
    {
        float realTime = Time.realtimeSinceStartup;
        _realDeltaTime = realTime - _prevRealTime;

        UpdateIntermissionTimer();

        _prevRealTime = realTime;
    }

    void UpdateIntermissionTimer()
    {
        if (_intermissionTimer > 0)
        {
            _intermissionTimer -= _realDeltaTime;
            if (_intermissionTimer <= 0)
            {
                Open(_onOpenCompleteCallback);
            }
        }
    }


    #region IOverlayShutterViewDelegate Methods

    void IOverlayShutterViewDelegate.OnCloseFinished(OverlayShuttersView sender)
    {
        OnCloseFinished();
    }
    void OnCloseFinished()
    {
        if (_onCloseCompleteCallback != null)
        {
            _onCloseCompleteCallback();
        }

        if (CloseAndOpenSequenceIsPlaying)
        {
            BeginIntermission(_intermissionDuration);
        }
    }
    void BeginIntermission(float duration)
    {
        _intermissionDuration = duration;
        _intermissionTimer = _intermissionDuration;
    }

    void IOverlayShutterViewDelegate.OnOpenFinished(OverlayShuttersView sender)
    {
        CloseAndOpenSequenceIsPlaying = false;

        if (_onOpenCompleteCallback != null)
        {
            _onOpenCompleteCallback();
        }
    }

    #endregion
}