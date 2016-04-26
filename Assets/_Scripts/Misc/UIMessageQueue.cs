using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIMessageQueue : MonoBehaviour 
{
    const float FadeOutDuration = 3.0f;


    public List<Text> textEntries;
    public bool hideEntries = true;
    public float showForTime = 10;
    public bool autoClearOnHide = true;
    public bool reverseDirection;


    Queue<string> _queue = new Queue<string>();


	void Awake () 
	{
        Clear();
	}


    public void AddEntry(string message)
    {
        _queue.Enqueue(message);
        if(_queue.Count > textEntries.Count)
        {
            _queue.Dequeue();
        }

        RefreshUI();

        ShowEntries();
        
        if(hideEntries)
        {
            Invoke(HideEntries_MethodName, showForTime);
        }
    }

    public void Clear()
    {
        _queue.Clear();
        for (int i = 0; i < textEntries.Count; i++)
        {
            _queue.Enqueue(string.Empty);
        }

        RefreshUI();
    }


    void RefreshUI()
    {
        string[] strArr = _queue.ToArray();
        int n = textEntries.Count;
        for (int i = 0; i < n; i++)
        {
            int idx = reverseDirection ? i : n - 1 - i;
            textEntries[idx].text = strArr[i];
        }
    }

    void ShowEntries()
    {
        CancelInvoke(HideEntries_MethodName);
        iTween.StopByName(gameObject, FadeOutTweenName);

        SetAlphaForEntries(1.0f);
    }

    const string HideEntries_MethodName = "HideEntries";
    void HideEntries()
    {
        FadeOut();
    }

    const string FadeOutTweenName = "FadeOutTween";
    void FadeOut()
    {
        iTween.ValueTo(gameObject, iTween.Hash(
            "name", FadeOutTweenName,
            "from", 1.0f,
            "to", 0.0f,
            "time", FadeOutDuration,
            "ease", iTween.EaseType.easeInOutCubic,
            "onupdate", SetAlphaForEntries_MethodName,
            "onupdatetarget", gameObject,
            "oncomplete", FadeOutOnComplete_MethodName,
            "oncompletetarget", gameObject
            ));
    }

    const string FadeOutOnComplete_MethodName = "FadeOutOnComplete";
    void FadeOutOnComplete()
    {
        if(autoClearOnHide)
        {
            Clear();
        }
    }

    const string SetAlphaForEntries_MethodName = "SetAlphaForEntries";
    void SetAlphaForEntries(float alpha)
    {
        foreach (var entry in textEntries)
        {
            Color c = entry.color;
            c.a = alpha;
            entry.color = c;
        }
    }
    

    /*void Update()
    {
        if(Input.anyKeyDown)
        {
            string name = "Fred";
            string msg = name + " has left the match\nAI will inherit " + name + "'s planets";
            AddEntry(msg);
        }
    }*/

}
