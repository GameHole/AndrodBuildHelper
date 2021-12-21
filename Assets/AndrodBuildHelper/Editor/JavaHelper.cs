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
        static string actFilePath = AssetDatabase.GUIDToAssetPath("172408e12aa30ba48ae15b79177a6289");//floader Editor/interfaceTemplete/Activity;
        static string appFilePath = AssetDatabase.GUIDToAssetPath("131d279d77a4a484cab2baf1a85ce55b");//floader Editor/interfaceTemplete/Application;
        static readonly string packageName = "com.api.unityactivityinterface";
        static HashSet<string> appInters;
        static void LoadTypes()
        {
            if (appInters != null) return;
            appInters = new HashSet<string>();
            foreach (var item in Directory.GetFiles(appFilePath))
            {
                if (item.EndsWith(".meta")) continue;
                appInters.Add(Path.GetFileNameWithoutExtension(item));
            }
        }
        class InterfaceInfo
        {
            public string className;
            public List<string> interfaces=new List<string>();
            public bool IsApp()
            {
                LoadTypes();
                for (int i = 0; i < interfaces.Count; i++)
                {
                    if (appInters.Contains(interfaces[i]))
                        return true;
                }
                return false;
            }
        }
        static GradleHelper.Gradle OpenCustomUnityApplication()
        {
            var orgPath = AssetDatabase.GUIDToAssetPath("6bcb17f73c169934c908abe76f1d8174");//Editor/interfaceTemplete/Application/CustomUnityApplication.txt
            if(!FindOrCopyFile(orgPath, "CustomUnityApplication.java", out var ret))
            {
                CopyInterfaces(appFilePath);
            }
            SetBuildInfo(ret, "/manifest/application", "CustomUnityApplication", MainManifestType.Luncher);
            return ret;
        }
        static bool FindOrCopyFile(string orgionFilePath,string fileName,out GradleHelper.Gradle gradle)
        {
            string tmpFile = orgionFilePath;// EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/Source/com/unity3d/player/UnityPlayerActivity.java";
            string dir = "Assets/Plugins/Android";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string[] paths = Directory.GetFiles(dir, fileName, SearchOption.TopDirectoryOnly);
            
            if (paths.Length == 0)
            {
                string gradlePath = $"Assets/Plugins/Android/{fileName}";
                File.Copy(tmpFile, gradlePath);
                gradle = new GradleHelper.Gradle(gradlePath);
                return false;
            }
            else
            {
                //gradlePath = ;
                gradle = new GradleHelper.Gradle(paths[0]);
                return true;
            }
        }
        static GradleHelper.Gradle OpenUnityActivity()
        {
            string tmpFile = EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/Source/com/unity3d/player/UnityPlayerActivity.java";
            if (!FindOrCopyFile(tmpFile, "UnityPlayerActivity.java", out var ret))
            {
                TryBuildFrameCode(ret);
                CopyInterfaces(actFilePath);
            }
            SetBuildInfo(ret, "/manifest/application/activity", "UnityPlayerActivity");
            return ret;
        }
        static GradleHelper.Value FindPackage(GradleHelper.Gradle javaFile)
        {
            return javaFile.Root.FindValue((v) =>
            {
                if (!string.IsNullOrEmpty(v.str))
                {
                    if (v.str.StartsWith("package"))
                    {
                        return true;
                    }
                }
                return false;
            });
        }
        static void SetBuildInfo(GradleHelper.Gradle javaFile,string nodePath,string javaClassName,MainManifestType type= MainManifestType.Main)
        {
            var pkg = FindPackage(javaFile);
            if (pkg != null)
            {
                javaFile.Root.Remove(pkg);
                javaFile.Root.InsertValue(0, $"package {packageName};");
                javaFile.Save();
            }
            var xml = XmlHelper.GetAndroidManifest(type);
            var orgNode = xml.SelectSingleNode(nodePath);
            var v = $"{packageName}.{javaClassName}";
            var att = orgNode.Attributes["android:name"];
            if (att == null)
                orgNode.CreateAttribute("name", v);
            else
                att.Value = v;
            xml.Save();
        }
        public static void RegistJavaInterfaceWithReplease(string path,params KeyValuePair<string,string>[] pairs)
        {
            string dir = GetAndroidPath();
            string output = Path.Combine(dir, $"{Path.GetFileNameWithoutExtension(path)}.java");
            IOHelper.CopyFileWithReplease(path, output, pairs);
            RegistJavaInterface(output);
        }
        public static void RegistJavaInterface(string path)
        {
            var inteTree = new GradleHelper.Gradle(path).Root;
            string pkg = GetPackage(inteTree);
            if (string.IsNullOrEmpty(pkg))
            {
                throw new ArgumentException($"file ::{path} not contain a 'package' line");
            }
            var info = GetInterfaceInfo(inteTree);
            GradleHelper.Gradle javaFile;
            string constrStr;
            if (info.IsApp())
            {
                javaFile = OpenCustomUnityApplication();
                constrStr = "CustomUnityApplication";
            }
            else
            {
                javaFile = OpenUnityActivity();
                constrStr = "UnityPlayerActivity";
            }
            var unityFile = javaFile.Root;
            unityFile.InsertValue(1, $"import {pkg}.{info.className};");
            var constr = FindFunc(unityFile.GetNodes()[0], constrStr);
            var instName = getInterfaceInstName($"{pkg.Replace('.', '_')}_{info.className}");
            constr.AddValue($"{info.className} {instName} = new {info.className}();");
            foreach (var item in info.interfaces)
            {
                constr.AddValue($"{getInterfaceInstName(item)}.add({instName});");
            }
            javaFile.Save();
        }
        static Dictionary<string, string> nameToFuncStr;
        static void LoadActivityInterfaces()
        {
            if (nameToFuncStr != null) return;
            nameToFuncStr = new Dictionary<string, string>();
            foreach (var item in Directory.GetFiles(actFilePath))
            {
                if (item.Contains(".meta")) continue;
                var gd = new GradleHelper.Gradle(item);
                var nd = gd.Root.GetNodes()[0];
                string name = nd.name.Split(' ')[2];
                string func = nd.GetValues()[0].str;
                func = func.Substring(5, func.Length - 5);//remove 'void '
                int idxLeft = func.IndexOf('(');
                int idxRight = func.IndexOf(')');
                if (idxLeft + 1 != idxRight )
                {
                    var ss = func.Split('(', ' ', ')');
                    StringBuilder builder = new StringBuilder();
                    builder.Append(ss[0]);//func name
                    builder.Append('(');
                    for (int i = 2; i < ss.Length; i+=2)
                    {
                        builder.Append(ss[i]);
                        
                        //if (i != ss.Length - 1)
                        //    builder.Append(',');
                    }
                    builder.Append(");");
                    func = builder.ToString();
                    func = func.Replace("activity", "this");
                    //Debug.Log($"func::{func}");
                }
                nameToFuncStr.Add(name, func);
            }
        }
        static string GetAndroidPath()
        {
            string dir = "Assets/Plugins/Android";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }
        static void CopyInterfaces(string foader)
        {
            string dir = GetAndroidPath();
            foreach (var item in Directory.GetFiles(foader))
            {
                if (item.Contains(".meta")) continue;
                var name = Path.GetFileNameWithoutExtension(item);
                File.Copy(item, Path.Combine(dir, $"{name}.java"));
            }
        }
        static void ImportUnityClassImplements(GradleHelper.Node unityClass, GradleHelper.Node root)
        {
            string mark = "implements";
            int idx = unityClass.name.IndexOf(mark);
            if (idx > 0)
            {
                int lastIdx = idx + mark.Length + 1;
                var interName = unityClass.name.Substring(lastIdx, unityClass.name.Length- lastIdx);
                root.InsertValue(1, $"import com.unity3d.player.{interName};");
            }
        }
        static void TryBuildFrameCode(GradleHelper.Gradle unityActivity)
        {
            var unityRoot = unityActivity.Root;
            unityRoot.InsertValue(1, "import com.unity3d.player.UnityPlayer;");
            unityRoot.InsertValue(1, "import java.util.ArrayList;");
            LoadActivityInterfaces();
            var unityClass = FindUnityActivity(unityRoot);
            if (unityClass == null)
            {
                throw new ArgumentException("'UnityPlayerActivity' node not found,please check unity version");
            }
            ImportUnityClassImplements(unityClass, unityRoot);
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

