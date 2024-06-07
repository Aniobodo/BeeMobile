using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace BeeAPPLIB
{
    public class UploadFiles
    {
        private string Project { get; set; }
        private Socket _socket { get; set; }
        private string Email { get; set; }
        private readonly FtpDB FtpDBConn = new FtpDB();
        List<string> SMs { get; set; }
        List<XmlNode> SMnodes { get; set; }
        List<List<XmlNode>> Pxmlnodes { get; set; }
        List<string> NBBsm { get; set; }
        List<XmlNode> SmNBBnodes { get; set; }
        List<List<XmlNode>> NBBpxmlnodes { get; set; }

        public UploadFiles(string fullprojname) 
        {
            Project = fullprojname; Project = ModifyString(Project);
            FtpDBConn.Connect(); CreateFtpDirs();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Email = string.Empty;
            SMs = new List<string>(); SMnodes = new List<XmlNode>(); Pxmlnodes = new List<List<XmlNode>>();
            NBBsm = new List<string>(); SmNBBnodes = new List<XmlNode>(); NBBpxmlnodes = new List<List<XmlNode>>();
        }
        public UploadFiles(byte[] file)
        {
            Project = GetProject(file);
            FtpDBConn.Connect(); CreateFtpDirs();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Email = string.Empty;
            SMs = new List<string>(); SMnodes = new List<XmlNode>(); Pxmlnodes = new List<List<XmlNode>>();
            NBBsm = new List<string>(); SmNBBnodes = new List<XmlNode>(); NBBpxmlnodes = new List<List<XmlNode>>();
        }
        private void CreateFtpDirs()
        {
            FtpDBConn.CreateDir(FtpLinks.ProjectDirPath + Project); FtpDBConn.CreateDir(FtpLinks.ProjectDirPath + Project + "/PXML_Files"); FtpDBConn.CreateDir(FtpLinks.ProjectDirPath + Project + "/Geom_Files");
            FtpDBConn.CreateDir(FtpLinks.ProjectDirPath + Project + "/NBB_Files"); FtpDBConn.CreateDir(FtpLinks.ProjectDirPath + Project + "/Storage_Images"); FtpDBConn.CreateDir(FtpLinks.ProjectDirPath + Project + "/Site_Images");
        }
        public void AddFile(byte[] data)
        {
            try
            {
                XmlDocument doc = new XmlDocument(); doc.Load(new MemoryStream(data));
                if (doc.DocumentElement.Name == Constants.Geom_Document)
                {
                    string Pname = string.Empty; string Sname = string.Empty;
                    var infos = doc.DocRoot().GetChildNodesByName("Info");
                    foreach (XmlNode info in infos)
                    {
                        if (info.GetAtrrib("ID") == "Project") Pname = info.GetAtrrib("Nr") + "_" + info.GetAtrrib("Name");
                        if (info.GetAtrrib("ID") == "StructuralMember") Sname = info.GetAtrrib("Nr") + "_" + info.GetAtrrib("Name");
                    }
                    Sname = ModifyString(Sname); Pname = ModifyString(Pname);
                    if (Pname != Project) return;
                    FtpDBConn.UploadData(FtpLinks.ProjectDirPath + Project + "/Geom_Files/" + Sname + Constants.GeomExt, data);
                }
                else
                {
                    string Pname = string.Empty; string Sname = string.Empty;
                    var root = doc.DocRoot();
                    Pname = root?.GetChildNodeByName("Order")?.GetChildNodeByName("OrderNo")?.InnerText + "_" + root?.GetChildNodeByName("Order")?.GetChildNodeByName("GenericOrderInfo01")?.InnerText;
                    Pname = ModifyString(Pname);
                    if (Pname != Project) return;
                    Sname = root?.GetChildNodeByName("Order")?.GetChildNodeByName("DrawingNo")?.InnerText + "_" + root?.GetChildNodeByName("Order")?.GetChildNodeByName("Component")?.InnerText;
                    Sname = ModifyString(Sname);

                    var prodnodes = root?.GetChildNodeByName("Order")?.GetChildNodesByName("Product");
                    for (int i = 0; i < prodnodes?.Count; i++)
                    {
                        var IsNBB = prodnodes[i]?.GetChildNodeByName("Slab")?.GetChildNodeByName("Steel")?.GetChildNodeByName("GenericInfo01")?.InnerText == Constants.NBB;
                        if (IsNBB)
                        {
                            if (!NBBsm.Contains(Sname))
                            {
                                NBBpxmlnodes.Add(new List<XmlNode>()); NBBsm.Add(Sname);
                                var smnodeimport = root?.CloneNode(true);
                                var deleteprodnodes = smnodeimport?.GetChildNodeByName("Order")?.GetChildNodesByName("Product");
                                if (deleteprodnodes != null)
                                {
                                    foreach (XmlNode itemnode in deleteprodnodes) itemnode.ParentNode.RemoveChild(itemnode);
                                }
                                if (smnodeimport != null) SmNBBnodes.Add(smnodeimport);
                            }
                            NBBpxmlnodes[NBBsm.IndexOf(Sname)].Add(prodnodes[i]);
                        }
                        else
                        {
                            if (!SMs.Contains(Sname))
                            {
                                Pxmlnodes.Add(new List<XmlNode>()); SMs.Add(Sname);
                                var smnodeimport = root?.CloneNode(true);
                                var deleteprodnodes = smnodeimport?.GetChildNodeByName("Order")?.GetChildNodesByName("Product");
                                if (deleteprodnodes != null)
                                {
                                    foreach (XmlNode itemnode in deleteprodnodes) itemnode.ParentNode.RemoveChild(itemnode);
                                }
                                if (smnodeimport != null) SMnodes.Add(smnodeimport);
                            }
                            Pxmlnodes[SMs.IndexOf(Sname)].Add(prodnodes[i]);
                        }
                    }
                }
            }
            catch { return; }
        }
        public bool Send(out string error)
        {

            for (int i = 0; i < SMs.Count; i++)
            {
                var pxmldoc = new XmlDocument();
                var rootimport = pxmldoc.ImportNode(SMnodes[i], true); pxmldoc.AppendChild(rootimport);
                var root = pxmldoc.DocRoot();
                foreach (XmlNode pxmlnode in Pxmlnodes[i])
                {
                    var productnode = pxmldoc.ImportNode(pxmlnode, true);
                    root?.GetChildNodeByName("Order")?.AppendChild(productnode);
                }
                FtpDBConn.UploadData(FtpLinks.ProjectDirPath + Project + "/PXML_Files/" + SMs[i] + Constants.PxmlExt, pxmldoc.IndentXml());
            }
            for (int i = 0; i < NBBsm.Count; i++)
            {
                var nbbdoc = new XmlDocument();
                var rootimport = nbbdoc.ImportNode(SmNBBnodes[i], true); nbbdoc.AppendChild(rootimport);
                var root = nbbdoc.DocRoot();
                foreach (XmlNode nbbpxmlnode in NBBpxmlnodes[i])
                {
                    var productnode = nbbdoc.ImportNode(nbbpxmlnode, true);
                    root?.GetChildNodeByName("Order")?.AppendChild(productnode);
                }
                FtpDBConn.UploadData(FtpLinks.ProjectDirPath + Project + "/NBB_Files/" + NBBsm[i] + Constants.PxmlExt, nbbdoc.IndentXml());
            }
            string SendData = Constants.DataSep + Project + Constants.DataSep + Email;
            return SocketSend("3", SendData, out error);
        }
        public bool Connect(string email, string password, string ip, int port)
        {
            try
            {
                Email = email;
                _socket.ConnectAsync(ip, port);
                Stopwatch sw = Stopwatch.StartNew();
                while (true) { if (sw.ElapsedMilliseconds > 3000 || _socket.Connected) break; }
                sw.Stop();
                if (!_socket.Connected) { _socket.Close(); return false; }
                string SendData = email + ";" + password;
                return SocketSend("2", SendData, out _);
            }
            catch { return false; }
        }
        public void Disconnect()
        {
            try 
            { 
                FtpDBConn.DisConnect();
                SocketSend("0", string.Empty, out _); _socket.Shutdown(SocketShutdown.Both); _socket.Close();
            } 
            catch { return; }
        }
        private bool SocketSend(string CommandNr, string message, out string returndata)
        {
            returndata = string.Empty;
            try
            {
                int BUFFER_SIZE = 1048;
                if (CommandNr == "0") message = "Exit";
                else message = CommandNr + ";" + message;
                var msg = Encoding.UTF8.GetBytes(Constants.BamAPPKey + message);
                _socket.Send(msg);

                byte[] recieve_bytes = new byte[BUFFER_SIZE]; //socket.ReceiveTimeout = 3000;                
                var rec_count = 0;
                do
                {
                    var buf = new byte[BUFFER_SIZE];
                    var count = _socket.Receive(buf);
                    var temp = new byte[count]; Array.Copy(buf, temp, count);
                    temp.CopyTo(recieve_bytes, rec_count);
                    rec_count += count;
                    Stopwatch sw = Stopwatch.StartNew();
                    while (true) { if (sw.ElapsedMilliseconds > 50) break; }
                    sw.Stop();
                } while (_socket.Available > 0);

                //int rec_count = socket.Receive(recieve_bytes);
                byte[] data = new byte[rec_count];
                Array.Copy(recieve_bytes, data, rec_count);
                var recieve_string = Encoding.UTF8.GetString(data);

                if (recieve_string.Contains(Constants.GID) && recieve_string.Contains(Constants.ZID))
                {
                    recieve_string = recieve_string.Replace(Constants.GID, null); recieve_string = recieve_string.Replace(Constants.ZID, null);
                    var contentMessage = recieve_string.Split("<Valid>", StringSplitOptions.None);
                    returndata = contentMessage[1];
                    return contentMessage[0] == "1";
                }
                else return false;
            }
            catch { return false; }
        }
        private string GetProject(byte[] data)
        {
            try
            {
                string Pname = string.Empty;
                XmlDocument doc = new XmlDocument(); doc.Load(new MemoryStream(data));
                if (doc.DocumentElement.Name == Constants.Geom_Document)
                {
                    XmlNode? info = doc.DocRoot().GetChildNodesByName("Info").GetNodeAt("ID", "Project");
                    Pname = info?.GetAtrrib("Nr") + "_" + info?.GetAtrrib("Name");
                }
                else
                {
                    Pname = doc?.DocRoot()?.GetChildNodeByName("Order")?.GetChildNodeByName("OrderNo")?.InnerText + "_" + doc?.DocRoot()?.GetChildNodeByName("Order")?.GetChildNodeByName("GenericOrderInfo01")?.InnerText;
                }
                Pname = ModifyString(Pname);                
                return Pname;
            }
            catch { return string.Empty; }
        }
        private string ModifyString(string str)
        {
            return Regex.Replace(str, @"(\\|/|:|\*|\?|<|>|\||"")", "").Trim();
        }
    }
}
