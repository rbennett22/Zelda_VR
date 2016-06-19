using Immersio.Utility;
using System;
using UnityEngine;

public class OverlayShuttersViewController : Singleton<OverlayShuttersViewController>, IOverlayShutterViewDelegate
{
    [SerializeField]
    OverlayShuttersView _view;


    public Predicate<OverlayShuttersViewController> _isReadyToOpen_Predicate;
    bool IsReadyToOpen() { return (_isReadyToOpen_Predicate == null) ? true : _isReadyToOpen_Predicate(this); }

    Action _onCloseCompleteCallback, _onOpenCompleteCallback;

    float _realDeltaTime;
    float _prevRealTime;
    float _intermissionTimer;
    float _intermissionDuration;


    public bool CloseAndOpenSequenceIsPlaying { get; private set; }
    public bool AnySequenceIsPlaying { get { return CloseAndOpenSequenceIsPlaying || _view.IsOpening || _view.IsClosing; } }


    override protected void Awake()
    {
        base.Awake();

        _view.viewDelegate = this;
        _view.gameObject.SetActive(true);

        _prevRealTime = Time.realtimeSinceStartup;
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
        if (_intermissionTimer <= 0)
        {
            return;
        }

        _intermissionTimer -= _realDeltaTime;
        if (_intermissionTimer <= 0)
        {
            IntermissionTimesUp();
        }
    }
    void IntermissionTimesUp()
    {
        if (IsReadyToOpen())
        {
            Open(_onOpenCompleteCallback);

            _isReadyToOpen_Predicate = null;
        }
        else
        {
            RestartTimer();
        }
    }
    void RestartTimer()
    {
        _intermissionTimer = _intermissionDuration;
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

    #endregion IOverlayShutterViewDelegate Methods
}