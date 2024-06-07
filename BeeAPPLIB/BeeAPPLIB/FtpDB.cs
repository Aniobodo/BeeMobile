using FluentFTP;
using FluentFTP.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace BeeAPPLIB
{
    public class FtpDB
    {
        public FtpClient FtpConn = new FtpClient();
        private const string FtpUsername = "xx";
        private const string FtpPassword = "yy";
        public bool IsConnected { get { return FtpConn.IsConnected; } }
        public FtpDB()
        {
            FtpConn.Host = "www.beetec.com"; FtpConn.Credentials = new NetworkCredential(FtpUsername, FtpPassword);
            //07.06.2024 vorübergehend aus FtpConn.Connect();
        }
        public void Connect() { /*07.06.2024 vorübergehend aus FtpConn.Connect();*/ }
        public void DisConnect() { FtpConn.Disconnect(); }
        //[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_stream")] public static extern ref FtpSocketStream Straem(this FtpClient client);
        //FtpSocketStream _barVariable = typeof(Foo).GetField("_bar", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(objectForFooClass);
        public byte[] DownloadBytes(string address)
        {
            try
            {
                FtpConn.DownloadBytes(out byte[] buffer, address);
                return buffer;
            }
            catch { return Array.Empty<byte>(); }
        }
        public string DownloadString(string address)
        {
            try
            {
                FtpConn.DownloadBytes(out byte[] buffer, address);
                return Encoding.UTF8.GetString(buffer);
            }
            catch { return string.Empty; }
        }
        public Stream DownloadStream(string address)
        {
            try
            {
                Stream outstream = new MemoryStream();
                FtpConn.DownloadStream(outstream, address);
                return outstream;
            }
            catch { return new MemoryStream(); }
        }
        public bool UploadData(string address, byte[] buffer)
        {
            try 
            { 
                var ftpstatus = FtpConn.UploadBytes(buffer, address);
                if (ftpstatus.IsFailure()) return false;                
                return true;
            }
            catch { return false; }
        }
        public void Rename(string oldpath, string newpath)
        {
            try { FtpConn.Rename(oldpath, newpath); }
            catch { return; }
        }
        public void DeleteFile(string address)
        {
            try { FtpConn.DeleteFile(address); }
            catch { return; }
        }
        public void DeleteDir(string address)
        {
            try { FtpConn.DeleteDirectory(address); }
            catch { return; }
        }
        public bool CreateDir(string address)
        {
            try { return FtpConn.CreateDirectory(address); }
            catch { return false; }
        }
        public List<string> GetList(string address)
        {
            List<string> ftplist = new List<string>();
            try
            {
                var addrlist = FtpConn.GetNameListing(address);
                foreach (var addr in addrlist)
                {
                    if (address + "." == addr || address + ".." == addr) continue;
                    ftplist.Add(addr);
                }
                return ftplist;
            }
            catch { return ftplist; }
        }
    }
}
