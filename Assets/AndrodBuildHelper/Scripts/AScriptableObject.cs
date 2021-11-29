using System.Collections.Generic;
using UnityEngine;
public abstract class AScriptableObject : ScriptableObject
{
    public abstract string filePath { get; }
    public static T Get<T>()where T : AScriptableObject
    {
        var tmp = CreateInstance<T>();
        return Resources.Load<T>(tmp.filePath);
    }
}