using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour 
{
    //[SerializeField]
    //TimerUI _timerUI;

    public bool autoStart;


    float _time_totalSecs;
    public float Time_TotalSecs
    {
        get { return _time_totalSecs; }
        private set
        {
            _time_totalSecs = value;
            //RefreshUI();
        }
    }

    public float Time_SecsPart
    {
        get { return Mathf.Floor(_time_totalSecs % 60); }
    }
    public float Time_MinsPart
    {
        get { return Mathf.Floor(_time_totalSecs / 60); }
    }

    
    bool _isRunning;


    void Start()
    {
        if (autoStart)
        {
            Begin();
        }
    }

	
	public void Begin () 
	{
        _isRunning = true;
	}
    public void Stop()
    {
        _isRunning = false;
    }
    public void Reset()
    {
        Stop();
        Time_TotalSecs = 0;
    }


	void Update () 
	{
        if (_isRunning)
        {
            Time_TotalSecs += Time.deltaTime;
        }
	}

    /*void RefreshUI()
    {
        if (_timerUI != null)
        {
            _timerUI.SetTime_TotalSecs(_time_totalSecs);
        }
    }*/


    public static string GetTimeAsFormattedString(float time_secs)
    {
        string minutes = Mathf.Floor(time_secs / 60).ToString("00");
        string seconds = Mathf.Floor(time_secs % 60).ToString("00");

        return minutes + ":" + seconds;
    }
}
