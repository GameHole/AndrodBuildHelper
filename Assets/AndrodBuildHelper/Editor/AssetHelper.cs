using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public static class AssetHelper
{
    static string _basicPath = "Assets/Resources";
    public static T CreateOrGetAsset<T>(string basicPath=null) where T : AScriptableObject
    {
        var res = ScriptableObject.CreateInstance<T>();
        if (string.IsNullOrEmpty(basicPath))
            basicPath = _basicPath;
        string path = $"{basicPath}/{res.filePath}.asset";
        if (File.Exists(path))
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        AssetDatabase.CreateAsset(res, path);
        return res;
    }
    //public static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
    //{
    //    var res = ScriptableObject.CreateInstance<T>();
    //    //string path = "Assets/Resources/" + res.filePath + ".asset";
    //    if (File.Exists(path))
    //    {
    //        return Resources.Load<T>(path);
    //    }
    //    string dir = Path.GetDirectoryName(path);
    //    if (!Directory.Exists(dir))
    //        Directory.CreateDirectory(dir);
    //    AssetDatabase.CreateAsset(res, path);
    //    return res;
    //}
    public static bool Exists<T>() where T : AScriptableObject
    {
        var res = ScriptableObject.CreateInstance<T>();
        string path = "Assets/Resources/" + res.filePath + ".asset";
        return File.Exists(path);
    }
    public static void Save(this ScriptableObject sc)
    {
        EditorUtility.SetDirty(sc);
        AssetDatabase.SaveAssets();
    }
}
