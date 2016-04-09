using UnityEngine;
using System;

public static class iTweenExtensions
{
    public static void SafeStopTweenByName(string name)
    {
        if (iTween.tweens.Contains(name))
            iTween.StopByName(name);
    }
    public static void SafeStopTweenByName(GameObject target, string name)
    {
        if (iTween.tweens.Contains(name))
            iTween.StopByName(target, name);
    }
    public static void SafeStopTweenByName(GameObject target, string name, bool includeChildren)
    {
        if (iTween.tweens.Contains(name))
            iTween.StopByName(target, name, includeChildren);
    }
}
