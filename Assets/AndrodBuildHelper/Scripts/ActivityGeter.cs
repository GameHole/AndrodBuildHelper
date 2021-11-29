using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActivityGeter
{
#if UNITY_ANDROID
    private static AndroidJavaObject activity;
    public static AndroidJavaObject GetActivity()
    {
        if (activity == null)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unityPlayer != null)
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
        return activity;
    }
#endif
}
