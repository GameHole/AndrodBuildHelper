using System.Collections.Generic;
using UnityEngine;
namespace MiniGameSDK
{
    public class PlatfotmHelper
    {
        public static bool isEditor()
        {
            return Application.platform == RuntimePlatform.WindowsEditor
                 || Application.platform == RuntimePlatform.LinuxEditor
                 || Application.platform == RuntimePlatform.OSXEditor;
        }
    }
}
