using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MiniGameSDK
{
    public static class DynamicPermission
    {
#if UNITY_ANDROID
        static AndroidJavaClass permission;
        public static void GetPermission(params string[] permissions)
        {
            if (permission == null)
            {
                permission = new AndroidJavaClass("com.unity.dynamicpermissiongetter.DynamicPermission");
            }
            if (permission != null)
            {
                permission.CallStatic("Get", "mark", ActivityGeter.GetActivity(), permissions);
            }
        }
#endif
    }
}

