using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MiniGameSDK;
public class TestEditor : MonoBehaviour
{
    [MenuItem("java/test")]
    static void javaTest()
    {
        //string gradlePath = "Assets/Plugins/Android/UnityPlayerActivity.java";
        //File.Delete(gradlePath);
        //OpenUnityActivity();
        string m = "Assets/Demo_Test/testAppInit.java";
        JavaHelper.RegistJavaInterface(m);
        AssetDatabase.Refresh();
    }
}
