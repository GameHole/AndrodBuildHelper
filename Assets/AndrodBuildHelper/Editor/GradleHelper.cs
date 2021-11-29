using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
public static class GradleHelper 
{
    public interface INode
    {
       string GetString();
    }
    public class Value: INode, IEquatable<Value>
    {
        public string str;
        public Value(string s)
        {
            str = s;
        }
        public string GetString()
        {
            return str + "\n";
        }
        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Value);
        }

        public bool Equals(Value other)
        {
            return other != null &&
                   str == other.str;
        }
        public override string ToString()
        {
            return str;
        }
    }
    public class Node: INode, IEquatable<Node>
    {
        bool isRoot;
        static readonly string tab = "     ";
        public string name;
        public string tabs = "";
        public Node()
        {
            isRoot = true;
        }
        public Node(string n)
        {
            name = n;
            isRoot = false;
        }
        internal List<INode> values = new List<INode>();
        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }

        public bool Equals(Node other)
        {
            if (other != null)
            {
                if(name == other.name)
                {
                    if (values.Count == other.values.Count)
                    {
                        for (int i = 0; i < values.Count; i++)
                        {
                            if (!values[i].Equals(other.values[i]))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public void Add(INode value)
        {
            if (values.Contains(value)) return;
            values.Add(value);
        }
        public void AddValue(string value)
        {
            var v = new Value(value);
            if (values.Contains(v)) return;
            values.Add(v);
        }
        public void Remove(INode value)
        {
            values.Remove(value);
        }
        public bool Contain(INode value)
        {
            return values.Contains(value);
        }

        public string GetString()
        {
            StringBuilder builder = new StringBuilder();
            if (!isRoot)
            {
                builder.Append(tabs);
                builder.Append(name);
                builder.Append('{');
                builder.Append('\n');
            }
            foreach (var item in values)
            {
                if(item is Node n)
                {
                    n.tabs = tabs + (isRoot ? "" : tab);
                    //builder.Append(item.GetString());
                }
                else
                {
                    if (!isRoot)
                    {
                        builder.Append(tabs);
                        builder.Append(tab);
                    }
                }
                builder.Append(item.GetString());
            }
            if (!isRoot)
            {
                //builder.Remove(builder.Length - 1, 1);
                builder.Append(tabs);
                builder.Append('}');
                builder.Append('\n');
            }
            return builder.ToString();
        }
        public bool TryGetNode(string name,out Node node)
        {
            node = default;
            foreach (var item in values)
            {
                if(item is Node n)
                {
                    if (n.name == name)
                    {
                        node = n;
                        return true;
                    }
                }
            }
            return false;
        }
        public Node GetNode(string name)
        {
            foreach (var item in values)
            {
                if (item is Node node)
                {
                    if (node.name == name)
                        return node;
                }
            }
            return null;
        }
        public Value FindValue(string v)
        {
            foreach (var item in values)
            {
                if (item is Value n)
                {
                    if (n.str == v)
                    {
                        return n;
                    }
                }
            }
            return null;
        }
        public List<Value> GetValues()
        {
            List<Value> ret = new List<Value>();
            foreach (var item in values)
            {
                if (item is Value n)
                {
                    ret.Add(n);
                }
            }
            return ret;
        }
        public List<Node> GetNodes()
        {
            List<Node> ret = new List<Node>();
            foreach (var item in values)
            {
                if (item is Node n)
                {
                    ret.Add(n);
                }
            }
            return ret;
        }
        public Node FindNode(string path)
        {
            string[] splits = path.Split('/');
            if (splits.Length > 0)
            {
                var parent = GetNode(splits[0]);
                if (parent != null)
                {
                    for (int i = 1; i < splits.Length; i++)
                    {
                        if (!parent.TryGetNode(splits[i], out var node))
                            return null;
                        parent = node;
                    }
                }
                return parent;
            }
            return null;
        }
        public int IndexOfNode(INode node)
        {
            return values.IndexOf(node);
        }
        public int IndexOfValue(string v)
        {
            var vv = new Value(v);
            return IndexOfNode(vv);
        }
        public void Insert(int idx,INode node)
        {
            if (values.Contains(node)) return;
            values.Insert(idx, node);
        }
        public void InsertValue(int idx, string value)
        {
            Insert(idx, new Value(value));
        }
        public bool InsertValueAt(string target, string value)
        {
            int idx = IndexOfValue(target);
            if (idx >= 0)
            {
                InsertValue(idx, value);
                return true;
            }
            return false;
        }
        public bool InsertValueAfter(string target, string value)
        {
            int idx = IndexOfValue(target);
            if (idx >= 0)
            {
                InsertValue(idx + 1, value);
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 1240601617;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<INode>>.Default.GetHashCode(values);
            return hashCode;
        }
    }
    public class Gradle
    {
        string filePath;
        Node root;
        public Node Root => root;
        //List<INode> nodes = new List<INode>();
        public Gradle(string file)
        {
            filePath = file;
            root = new Node();
            Read(File.ReadAllText(filePath));
        }
        void Read(string file)
        {
            StringBuilder builder = new StringBuilder();
            Stack<Node> stack = new Stack<Node>();
            for (int i = 0; i < file.Length; i++)
            {
                if (file[i] == '{')
                {
                    var name = builder.ToString().Trim();
                    if(string.IsNullOrEmpty(name))
                    {
                        Node fn = root;
                        if (stack.Count > 0)
                        {
                            fn = stack.Peek();
                        }
                        var vs = fn.values;
                        Value n = vs[vs.Count - 1] as Value;
                        vs.RemoveAt(vs.Count - 1);
                        name = n.str;
                    }
                    var node = new Node(name);
                    stack.Push(node);
                    builder.Clear();
                }
                else if (file[i] == '}')
                {
                    BuildValue(builder, stack);
                    var n = stack.Pop();
                    if (stack.Count > 0)
                    {
                        stack.Peek().Add(n);
                    }
                    else
                    {
                        root.Add(n);
                    }
                }
                else
                {
                    builder.Append(file[i]);
                    if (file[i] == '\n')
                    {
                        BuildValue(builder, stack);
                    }
                } 
            }
        }

        private void BuildValue(StringBuilder builder, Stack<Node> stack)
        {
            if (builder.Length > 1)
            {
                var str = builder.ToString().Trim();
                if (!string.IsNullOrEmpty(str))
                {
                    var v = new Value(str);
                    if (stack.Count > 0)
                    {
                        stack.Peek().Add(v);
                    }
                    else
                    {
                        root.Add(v);
                    }
                }
            }
            builder.Clear();
        }

        public void Save()
        {
            File.WriteAllText(filePath, GetString());
        }
        public string GetString()
        {
            //StringBuilder builder = new StringBuilder();
            //foreach (var item in nodes)
            //{
            //    builder.Append(item.GetString());
            //}
            return root.GetString();
        }
        public void SetImplementation(string packet)
        {
            var node =Root.FindNode("dependencies");
            if (node != null)
            {
                node.Add(new Value(packet));
            }
        }
        //public Node GetNode(string name)
        //{
        //    foreach (var item in nodes)
        //    {
        //        if(item is Node node)
        //        {
        //            if (node.name == name)
        //                return node;
        //        }
        //    }
        //    return null;
        //}
        
        //public Node FindNode(string path)
        //{
        //    string[] splits = path.Split('/');
        //    if (splits.Length > 0)
        //    {
        //        var parent = GetNode(splits[0]);
        //        if (parent != null)
        //        {
        //            for (int i = 1; i < splits.Length; i++)
        //            {
        //                if (!parent.TryGetNode(splits[i], out var node))
        //                    return null;
        //                parent = node;
        //            }
        //        }
        //        return parent;
        //    }
        //    return null;
        //}
    }
    
    //[MenuItem("Test/AA")]
    //static void AA()
    //{
    //    SetImplementation("aa");
    //    AssetDatabase.Refresh();
    //}
    public static Gradle Open()
    {
        string tmpFile = EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/mainTemplate.gradle";
        string dir = "Assets/Plugins/Android";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string[] paths = Directory.GetFiles(dir, "*.gradle", SearchOption.TopDirectoryOnly);
        string gradlePath = "Assets/Plugins/Android/mainTemplate.gradle";
        if (paths.Length == 0)
        {
            File.Copy(tmpFile, gradlePath);
        }
        else
        {
            gradlePath = paths[0];
        }
        return new Gradle(gradlePath);
    }
    public static Gradle Open(string path)
    {
        return new Gradle(path);
    }
    public static void SetImplementation(string packet)
    {
        string tmpFile = EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/mainTemplate.gradle";
        string dir = "Assets/Plugins/Android";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string[] paths = Directory.GetFiles(dir, "*.gradle", SearchOption.TopDirectoryOnly);
        string gradlePath = "Assets/Plugins/Android/mainTemplate.gradle";
        if (paths.Length == 0)
        {
            File.Copy(tmpFile, gradlePath);
        }
        else
        {
            gradlePath = paths[0];
        }
       
        string inst = $"implementation '{packet}'\n";
        string file = File.ReadAllText(gradlePath);
        if (!file.Contains(inst))
        {
            int idx = file.LastIndexOf("**DEPS**}");
            if (idx > 0)
            {
                file = file.Insert(idx, inst);
                File.WriteAllText(gradlePath, file);
            }
        }
    }
    public static void CombineProguard(string txtPath, string mark,string outFloader)
    {
        string floader = outFloader;
        if (!Directory.Exists(floader))
            Directory.CreateDirectory(floader);
        string prgFile = $"{floader}/proguard-user.txt";
        if (!File.Exists(prgFile))
            File.Create(prgFile).Close();
        string f = File.ReadAllText(prgFile);
        string startMark = $"# COMBINE_MARK_{mark}_START\n";
        string endMark = $"# COMBINE_MARK_{mark}_END\n";
        string txtStr = $"{startMark}{File.ReadAllText(txtPath)}{endMark}";
        string combined = null;
        int sidx = f.IndexOf(startMark);
        if (sidx >= 0)
        {
            int eid = f.IndexOf(endMark);
            string removed = f.Remove(sidx, eid + endMark.Length - sidx);
            combined = removed.Insert(sidx, txtStr);
        }
        else
        {
            combined = f + "\n" + txtStr;
        }
        File.WriteAllText(prgFile, combined);
    }
    public static void CombineProguard(string txtPath,string mark)
    {
        CombineProguard(txtPath, mark, "Assets/Plugins/Android/");
    }
}
