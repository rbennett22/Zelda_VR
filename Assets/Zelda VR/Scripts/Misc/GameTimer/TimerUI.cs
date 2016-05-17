using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public Text gameTimerMinsTensText, gameTimerMinsOnesText, gameTimerSecsTensText, gameTimerSecsOnesText;


    public void SetTime_TotalSecs(float totalSecs)
    {
        int minutesPart = (int)Mathf.Floor(totalSecs / 60);
        int secondsPart = (int)Mathf.Floor(totalSecs % 60);

        int minutesTensPart = minutesPart / 10;
        int minutesOnesPart = minutesPart % 10;
        int secondsTensPart = secondsPart / 10;
        int secondsOnesPart = secondsPart % 10;

        gameTimerMinsTensText.text = minutesTensPart.ToString();
        gameTimerMinsOnesText.text = minutesOnesPart.ToString();
        gameTimerSecsTensText.text = secondsTensPart.ToString();
        gameTimerSecsOnesText.text = secondsOnesPart.ToString();
    }
}