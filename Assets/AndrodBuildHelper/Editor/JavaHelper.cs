using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace MiniGameSDK
{
    public class JavaHelper
    {
        static string filePath = AssetDatabase.GUIDToAssetPath("e7cc477c5595c7046ad97114d5969668");//floader Editor/interfaceTemplete;
        //[MenuItem("java/test")]
        //static void javaTest()
        //{
        //    //string gradlePath = "Assets/Plugins/Android/UnityPlayerActivity.java";
        //    //File.Delete(gradlePath);
        //    //OpenUnityActivity();
        //    string m = "Assets/Demo_Test/TestInter.java";
        //    RegistJavaInterface(m);
        //    AssetDatabase.Refresh();
        //}
        class InterfaceInfo
        {
            public string className;
            public List<string> interfaces=new List<string>();
        }
        static GradleHelper.Gradle OpenUnityActivity()
        {
            string tmpFile = EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/Source/com/unity3d/player/UnityPlayerActivity.java";
            string dir = "Assets/Plugins/Android";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string[] paths = Directory.GetFiles(dir, "UnityPlayerActivity.java", SearchOption.TopDirectoryOnly);
            string gradlePath = "Assets/Plugins/Android/UnityPlayerActivity.java";
            if (paths.Length == 0)
            {
                File.Copy(tmpFile, gradlePath);
            }
            else
            {
                gradlePath = paths[0];
            }
            var ret = new GradleHelper.Gradle(gradlePath);
            if (paths.Length == 0)
            {
                TryBuildFrameCode(ret);
                CopyInterfaces(filePath);
            }
            var pkg = ret.Root.GetValues()[0] as GradleHelper.Value;
            pkg.str = $"package {PlayerSettings.applicationIdentifier};";
            ret.Save();
            return ret;
        }
        public static void RegistJavaInterface(string path)
        {
            var javaFile = OpenUnityActivity();
            var unityFile = javaFile.Root;
            var inteTree = new GradleHelper.Gradle(path).Root;
            string pkg = GetPackage(inteTree);
            if (string.IsNullOrEmpty(pkg))
            {
                throw new ArgumentException($"file ::{path} not contain a 'package' line");
            }
            var info = GetInterfaceInfo(inteTree);
            unityFile.InsertValue(1, $"import {pkg}.{info.className};");
            var constr = FindFunc(unityFile.GetNodes()[0], "UnityPlayerActivity");
            var instName = getInterfaceInstName($"{pkg.Replace('.','_')}_{info.className}");
            constr.AddValue($"{info.className} {instName} = new {info.className}();");
            foreach (var item in info.interfaces)
            {
                constr.AddValue($"{getInterfaceInstName(item)}.add({instName});");
            }
            javaFile.Save();
        }
        static Dictionary<string, string> nameToFuncStr;
        static void LoadInterfaces()
        {
            if (nameToFuncStr != null) return;
            nameToFuncStr = new Dictionary<string, string>();
            foreach (var item in Directory.GetFiles(filePath))
            {
                if (item.Contains(".meta")) continue;
                var gd = new GradleHelper.Gradle(item);
                var nd = gd.Root.GetNodes()[0];
                string name = nd.name.Split(' ')[2];
                string func = nd.GetValues()[0].str;
                func = func.Substring(5, func.Length - 5);
                int idxLeft = func.IndexOf('(');
                int idxRight = func.IndexOf(')');
                if (idxLeft + 1 != idxRight )
                {
                    var ss = func.Split('(', ' ', ')');
                    StringBuilder builder = new StringBuilder();
                    builder.Append(ss[0]);
                    builder.Append('(');
                    builder.Append(ss[2]);
                    builder.Append(");");
                    func = builder.ToString();
                }
                nameToFuncStr.Add(name, func);
            }
        }
        static void CopyInterfaces(string foader)
        {
            string dir = "Assets/Plugins/Android";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            foreach (var item in Directory.GetFiles(foader))
            {
                if (item.Contains(".meta")) continue;
                var name = Path.GetFileNameWithoutExtension(item);
                File.Copy(item, Path.Combine(dir, $"{name}.java"));
            }
        }
        static void TryBuildFrameCode(GradleHelper.Gradle unityActivity)
        {
            var unityRoot = unityActivity.Root;
            unityRoot.InsertValue(1, "import com.unity3d.player.UnityPlayer;");
            unityRoot.InsertValue(1, "import java.util.ArrayList;");
            LoadInterfaces();
            var unityClass = FindUnityActivity(unityRoot);
            if (unityClass == null)
            {
                throw new ArgumentException("'UnityPlayerActivity' node not found,please check unity version");
            }
            foreach (var item in nameToFuncStr)
            {
                string name = item.Key;
                unityRoot.InsertValue(1, $"import com.api.unityactivityinterface.{name};");
                string instName = getInterfaceInstName(name);
                int idx = item.Value.IndexOf("(") - 1;
                var f = item.Value.Substring(0, idx);
                var fn = FindFunc(unityClass, f);
                if (fn != null)
                {
                    var forNode = new GradleHelper.Node($"for({name} item:{instName})");
                    forNode.AddValue($"item.{item.Value}");
                    fn.Add(forNode);
                }
            }
            foreach (var item in nameToFuncStr)
            {
                string name = item.Key;
                string instName = getInterfaceInstName(name);
                var listValue = new GradleHelper.Value($"ArrayList<{name}> {instName} = new ArrayList<{name}>();");
                unityClass.Insert(0, listValue);
            }
            var constr = new GradleHelper.Node("public UnityPlayerActivity()");
            unityClass.Insert(0, constr);
            unityActivity.Save();
        }
        static GradleHelper.Node FindFunc(GradleHelper.Node node,string funcStr)
        {
            foreach (var item in node.GetNodes())
            {
                //Debug.Log($"of::{item.name} ag ::{funcStr}");
                if (item.name.Contains(funcStr))
                    return item;
            }
            return null;
        }
        static string getInterfaceInstName(string name)
        {
            return $"_{ name.ToLower()}";
        }
        static GradleHelper.Node FindUnityActivity(GradleHelper.Node unityRoot)
        {
            foreach (var item in unityRoot.GetNodes())
            {
                if (item.name.Contains("UnityPlayerActivity"))
                {
                    return item;
                }
            }
            return null;
        }
        static string GetPackage(GradleHelper.Node root)
        {
            string pkg = "package";
            foreach (var item in root.GetValues())
            {
                if (item.str.StartsWith(pkg))
                {
                    int idx = pkg.Length + 1;
                    int len = item.str.Length - idx-1;
                    return item.str.Substring(idx, len);
                }
            }
            return null;
        }
        static InterfaceInfo GetInterfaceInfo(GradleHelper.Node root)
        {
            InterfaceInfo info = new InterfaceInfo();
            foreach (var item in root.GetNodes())
            {
                if (item.name.Contains("class"))
                {
                    var strs = item.name.Split(' ', ',');
                    for (int i = 0; i < strs.Length; i++)
                    {
                        var str = strs[i];
                        if (str == "class")
                        {
                            info.className = strs[++i];
                            continue;
                        }
                        if(str == "implements")
                        {
                            for (int j = i+1; j < strs.Length; j++)
                            {
                                info.interfaces.Add(strs[j]);
                            }
                            break;
                        }
                    }
                    return info;
                }
            }
            return null;
        }
    }
}

