using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TestGradle 
{
    [MenuItem("Test/A")]
    static void Test()
    {
        var a = new GradleHelper.Gradle("Assets/Plugins/Android/mainTemplate.gradle");
        var nn = a.Root.FindNode("dependencies");
        nn.Add(new GradleHelper.Value("implementation 'com.lovedise:permissiongen:0.1.6'"));
        nn.Add(new GradleHelper.Value("implementation 'com.lovedise:permissiongen:0.1.6'"));
        //Debug.Log();
        string outPath = "Assets/Demo_Test/1.gradle";
        if (File.Exists(outPath))
            File.Delete(outPath);
        //File.Create(outPath).Close();
        File.WriteAllText(outPath, a.GetString());
        AssetDatabase.Refresh();
    }
}
