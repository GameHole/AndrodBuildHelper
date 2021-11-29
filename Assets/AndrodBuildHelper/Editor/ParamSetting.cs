using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace MiniGameSDK
{
    [InitializeOnLoad]
    public class ParamSetting
    {
        static List<IParamSettng> catches;
        static ParamSetting()
        {
            Setting();
        }
        [MenuItem("SDK/设置参数")]
        static void Setting()
        {
            if (catches == null)
            {
                catches = new List<IParamSettng>();
                var type = typeof(IParamSettng);
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var item in assembly.GetTypes())
                    {
                        if (item.IsAbstract || item.IsInterface || item.IsGenericType) continue;
                        if (type.IsAssignableFrom(item))
                        {
                            var m = Activator.CreateInstance(item) as IParamSettng;
                            catches.Add(m);
                        }
                    }
                }
            }
            foreach (var item in catches)
            {
                item.SetParam();
            }
        }
    }
}

