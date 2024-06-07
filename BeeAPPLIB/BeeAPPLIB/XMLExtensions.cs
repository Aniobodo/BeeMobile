using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace BeeAPPLIB
{
    public static class MiscMethods
    {
        public static string GameStorageName(string userID, string gameID, bool WithExt = false)
        {
            if(WithExt) return userID + "_" + gameID + Constants.ImageExt;
            else return userID + "_" + gameID;
        }
    }
    public static class XMLExtensions
    {
        public static XmlNode DocRoot(this XmlDocument doc) => doc.DocumentElement;
        internal static XmlNode? GetChildNodeByName(this XmlNode parentnode, string childname)
        {
            if (parentnode == null) return null;
            return parentnode.ChildNodes.First(childname);
        }
        internal static List<XmlNode> GetChildNodesByName(this XmlNode parentnode, string childname)
        {
            List<XmlNode> selected = new List<XmlNode>();
            if (parentnode == null) return selected;
            foreach (XmlNode child in parentnode.ChildNodes) { if (child.Name == childname) selected.Add(child); }
            return selected;
        }
        public static void AppendAttribut(this XmlDocument doc, XmlNode node, string attname, string attvalue)
        {
            XmlAttribute attrib = doc.CreateAttribute(attname);
            attrib.Value = attvalue;
            node.Attributes.Append(attrib);
        }
        internal static XmlNode? First(this XmlNodeList nodes, string childNodename)
        {
            if (nodes == null) return null;
            for (int i = 0; i < nodes.Count; i++) if (nodes[i].Name == childNodename) return nodes[i];
            return null;
        }
        public static void AppendAttribut(this XmlDocument doc, XmlNode node, List<string> attnames, List<string> attvalues)
        {
            for (int i = 0; i < attnames.Count; i++)
            {
                doc.AppendAttribut(node, attnames[i], attvalues[i]);
            }
        }
        public static XmlNode CreateRoot(this XmlDocument doc)
        {
            var node = doc.CreateElement("XML");
            doc.AppendChild(node);
            return node;
        }
        public static string? GetAtrrib(this XmlNode node, string attname, string? defaultval = null)
        {
            try 
            { 
                return node == null || node.Attributes[attname] == null ? defaultval : node.Attributes[attname].Value;                
            }
            catch { return defaultval; }
        }
        public static XmlNode? GetNodeAt(this XmlNodeList nodes, string attname, string attval)
        {
            try
            {
                if (nodes == null || nodes.Count == 0) return null;
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes[attname].Value == attval) return node;
                }
                return null;
            }
            catch { return null; }
        }
        public static XmlNode? GetNodeAt(this List<XmlNode> nodes, string attname, string attval)
        {
            try
            {
                if (nodes == null || nodes.Count == 0) return null;
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes[attname].Value == attval) return node;
                }
                return null;
            }
            catch { return null; }
        }
        public static byte[] IndentXml(this XmlDocument doc)
        {
            try
            {
                using var ms = new MemoryStream();
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new UTF8Encoding(true)
                };
                using var xmlTextWriter = XmlWriter.Create(ms, settings);
                doc.Save(xmlTextWriter);
                return ms.ToArray();
            }
            catch
            {
                return new byte[0];
            }
        }
    }
}
