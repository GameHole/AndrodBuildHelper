using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActivityGeter
{
    private static AndroidJavaObject activity;
    public static AndroidJavaObject GetActivity()
    {
#if !UNITY_EDITOR
        if (activity == null)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unityPlayer != null)
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif
        return activity;
    }
    private static AndroidJavaObject app;
    public static AndroidJavaObject GetApplication()
    {
        if (app == null)
        {
            var act = GetActivity();
            if (act != null)
            {
                app = act.Call<AndroidJavaObject>("getApplicationContext");
            }
        }
        return app;
    }
}
