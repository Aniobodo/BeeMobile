using System.Net.Sockets;
using System.Reflection;

namespace BeeAPPLIB
{
    public static class Constants
    {
        public static readonly string BamAPPKey = "[BEEAG]";
        public static readonly string DataSep = "<DATASEP>";
        public static readonly string PxmlExt = ".PXML";
        public static readonly string GeomExt = ".poly";
        public static readonly string ImageExt = ".png";
        public static readonly string TextExt = ".txt";
        public static readonly string StorageImages = "StorageImages";
        public static readonly string ImgSep = "<ImageSep>";
        public static readonly string pxmlSeprator = "<PxmlFile>";
        public static readonly string GameStr = "Game";
        public static readonly string SiteStr = "Site";
        public static readonly string GetValue = "get";
        public static readonly string SetValue = "set";
        public static readonly string Production = "Production";
        public static readonly string Logistic = "Logistic";
        public static readonly string CraneStr = "Crane";
        public static readonly string InstallStr = "Installation";
        public static readonly string CheckCarpet = "CheckCarpet";
        public static readonly string GeneralInfo = "General";
        public static readonly string FlyOut = "Flyout";
        public static readonly string TeamInfo = "TeamInfo";
        public static readonly string GetInfo = "GetInfo";
        public static readonly string NewTeam = "NewTeam";
        public static readonly string SameTeam = "SameTeam";
        public static readonly string ImportItem = "Import";
        public static readonly string SaveItem = "Save";
        public static readonly string DeleteItem = "Delete";
        public static readonly string Register = "Register";
        public static readonly string Settings = "Settings";
        public static readonly string Password = "Password";
        public static readonly string Resend = "Resend";
        public static readonly string NullStr = "null";
        public static readonly string NoCarpet = "NoCarpet";
        public static readonly string GamePartners = "GamePartners";
        public static readonly string GetPartners = "GetPartners";
        public static readonly string GetUsers = "GetUsers";
        public static readonly string NBB = "LooseBars";
        public static readonly string Beetec = "Beetec";
        public static readonly string Element = "Element";
        public static readonly string Layer = "Layer";
        public static readonly string ManualBars = "ManualBars";
        public static readonly string Ubars = "Ubars";
        public static readonly string PXML_Document = "PXML_Document";
        public static readonly string Geom_Document = "BAMPOLY";
        public static readonly string GID = "[GiiiID]";
        public static readonly string ZID = "[ZiiiID]";

        public static readonly string IPfromOutside = "8.8.8.8";
        public static readonly int PortFromOutside = 80;
        public static readonly string IPfromInside = "8.8.8.8";
        public static readonly int PortFromInside = 443;

    }

    public static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
    }
}
