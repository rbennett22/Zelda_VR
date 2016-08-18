using UnityEngine;
using System.Collections;

public class Sequence_Base : MonoBehaviour
{
    protected bool _isPlaying;


    public void Play()
    {
        if (_isPlaying) { return; }
        _isPlaying = true;

        StartCoroutine(DoPlay());
    }
    virtual protected IEnumerator DoPlay() { yield break; }
}