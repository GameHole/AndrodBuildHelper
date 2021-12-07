using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace MiniGameSDK
{
	public class IOHelper
	{
        //[MenuItem("Assets/Test/Copy")]
        //static void Test()
        //{
        //    var guid = Selection.assetGUIDs;
        //    if (guid.Length > 0)
        //    {
        //        CopyDirectory(AssetDatabase.GUIDToAssetPath(guid[0]), "Assets/Z_test/TestCopy");
        //    }
        //}
        //[MenuItem("Assets/Test/Delete")]
        //static void Test1()
        //{
        //    var guid = Selection.assetGUIDs;
        //    if (guid.Length > 0)
        //    {
        //        DeleteDirectory(AssetDatabase.GUIDToAssetPath(guid[0]));
        //    }
        //}
        public static string GetRarExePath()
        {
            using (var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe"))
            {
                if (reg == null)
                    return null;
                return reg.GetValue("").ToString();
            }
        }
        public static void ComprassDirToRar(string srcDir, string destRarFile)
        {
            var rarExe = GetRarExePath();
            if (!string.IsNullOrEmpty(rarExe))
            {
                var dir = Path.GetDirectoryName(destRarFile);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = rarExe,
                        Arguments = $"a -afzip -m0 -ep1 -r \"{destRarFile}\" \"{srcDir}\"",
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = dir,
                        CreateNoWindow = false,
                    }
                })
                {
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }
            }
        }
        public static void CopyFileWithReplease(string src,string dest,params KeyValuePair<string,string>[] repleases)
        {
            var txt = File.ReadAllText(src);
            foreach (var item in repleases)
            {
                txt = txt.Replace(item.Key, item.Value);
            }
            var dir = Path.GetDirectoryName(dest);
            //Debug.Log(dir);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(dest, txt);
        }
        public static void DeleteDirectory(string src)
        {
            if (Directory.Exists(src))
            {
                DeleteDirectoryFiles(src);
                Directory.Delete(src,true);
            }
        }
        public static void DeleteDirectoryFiles(string src)
        {
            if (Directory.Exists(src))
            {
                foreach (var item in Directory.GetFiles(src))
                {
                    File.Delete(item);
                }
                foreach (var item in Directory.GetDirectories(src))
                {
                    DeleteDirectoryFiles(item);
                }
            }
        }
        public static void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        public static void CopyDirectoryWithClear(string src,string dest)
        {
            DeleteDirectory(dest);
            CopyDirectory(src, dest, false);
        }
        public static void CopyDirectory(string src, string dest, bool isReplease=false,Func<string,string> dealFilePath=null)
        {
            if (Directory.Exists(src))
            {
                if (!Directory.Exists(dest))
                    Directory.CreateDirectory(dest);
                foreach (var item in Directory.GetFiles(src))
                {
                    if (item.EndsWith(".meta")) continue;
                    string srcFile = item;
                    string destFile = item.Replace(src, dest);
                    if (dealFilePath != null)
                        destFile = dealFilePath(destFile);
                    //Debug.Log($"file from {srcFile} to {destFile}");
                    if (File.Exists(destFile))
                    {
                        if (isReplease)
                            File.Delete(destFile);
                        else
                            continue;
                    }
                    File.Copy(srcFile, destFile);
                }
                foreach (var item in Directory.GetDirectories(src))
                {
                    string srcFL = item;
                    string destFL = item.Replace(src, dest);
                    //Debug.Log($"dir from {srcFL} to {destFL}");
                    CopyDirectory(srcFL, destFL, isReplease);
                }
            }
        }
    }
}
