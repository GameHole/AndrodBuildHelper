using System.IO;
using System.Xml;
using UnityEditor;

public static class XmlHelper 
{
    public static readonly string plgpath= "Assets/Plugins/Android/AndroidManifest.xml";
    public static XmlDocument GetAndroidManifest()
    {
        string tmpFile = EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer/Apk/AndroidManifest.xml";
        if (!Directory.Exists("Assets/Plugins/Android"))
            Directory.CreateDirectory("Assets/Plugins/Android");
        if (!File.Exists(plgpath))
        {
            File.Copy(tmpFile, plgpath);
        }
        XmlDocument xml = new XmlDocument();
        xml.Load(plgpath);
        return xml;
    }
    public static XmlDocument Open(string path)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(path);
        return xml;
    }
    public static void Save(this XmlDocument xml)
    {
        xml.Save(plgpath);
    }
    public static void SetPermission(this XmlDocument xdoc, string value)
    {
        var node = FindNode(xdoc, "/manifest/uses-permission", "android:name", value);
        if (node == null)
        {
            var manifestNode = xdoc.SelectSingleNode("/manifest");
            var ele = xdoc.CreateElement("uses-permission");
            ele.Attributes.Append(ele.CreateAttribute("name", value));
            manifestNode.AppendChild(ele);
        }
    }
    public static void RemovePermission(this XmlDocument xdoc, string value)
    {
        var node = FindNode(xdoc, "/manifest/uses-permission", "android:name", value);
        //UnityEngine.Debug.Log(node);
        if (node != null)
        {
            var manifestNode = xdoc.SelectSingleNode("/manifest");
           
            manifestNode.RemoveChild(node);
            //var ele = xdoc.CreateElement("uses-permission");
            //ele.Attributes.Append(ele.CreateAttribute("name", value));
            //manifestNode.AppendChild(ele);
        }
    }
    public static XmlElement AppendAttribute(this XmlElement element, string attributeName, string value, string attrNamespace = "apk/res/android")
    {
        element.Attributes.Append(element.CreateAttribute(attributeName, value, attrNamespace));
        return element;
    }
    public static XmlAttribute CreateAttribute(this XmlNode node, string attributeName, string value,string attrNamespace = "apk/res/android")
    {
        try
        {
            XmlDocument doc = node.OwnerDocument;
            XmlAttribute attr = null;

            attr = doc.CreateAttribute(attributeName, $"http://schemas.android.com/{attrNamespace}");
            attr.Value = value;
            node.Attributes.SetNamedItem(attr);
            return attr;
        }
        catch (System.Exception err)
        {
            string desc = err.Message;
            return null;
        }
    }
    public static XmlNode FindNode(this XmlDocument xmlDoc, string xpath, string attributeName, string attributeValue)
    {
        XmlNodeList nodes = xmlDoc.SelectNodes(xpath);
        //Debug.Log(nodes.Count);
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes.Item(i);
            var att = node.Attributes[attributeName];
            if (att != null)
            {
                if (att.Value == attributeValue)
                {
                    return node;
                }
            }
        }
        return null;
    }
}
