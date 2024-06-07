using BeeAPPLIB;
using System.Globalization;
using System.Xml;

namespace BeeMobileApp.Classes
{
    public static class BamRollOutPlan  //This class will hold all the information about rollout plan
    {
        public static List<(List<BeeCarpet> bamcarpets, List<Line2D> lappings, List<Line2D> tepzutep)> RollOutPlan { get; set; }
        public static void Evaluate(XmlNodeList layers) //Call this method to fill in the info of rollout plan
        {
            RollOutPlan = new List<(List<BeeCarpet> bamcarpet, List<Line2D> lappings, List<Line2D> tepzutep)>();
            try
            {
                foreach (XmlNode layernode in layers)
                {
                    List<BeeCarpet> carpets = new List<BeeCarpet>(); List<Line2D> laps = new List<Line2D>(); List<Line2D> tepDis = new List<Line2D>();
                    var carpetnodes = layernode.SelectNodes("Carpet"); var lapnodes = layernode.SelectNodes("Lapping"); var tepzutepnodes = layernode.SelectNodes("TepZuTep");
                    foreach (XmlNode carpetnode in carpetnodes)
                    {
                        //P1, P2, P3 and P4 are the four corners of the carpet with x and y coordinates.
                        //Roll1 and Roll2 are the two points of the rollout (usually from the center of carpet) with x and y coordinates.
                        var p1 = new Point2D(float.Parse(carpetnode.GetAtrrib("P1x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(carpetnode.GetAtrrib("P1y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var p2 = new Point2D(float.Parse(carpetnode.GetAtrrib("P2x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(carpetnode.GetAtrrib("P2y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var p3 = new Point2D(float.Parse(carpetnode.GetAtrrib("P3x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(carpetnode.GetAtrrib("P3y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var p4 = new Point2D(float.Parse(carpetnode.GetAtrrib("P4x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(carpetnode.GetAtrrib("P4y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var rollp1 = new Point2D(float.Parse(carpetnode.GetAtrrib("Roll1x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(carpetnode.GetAtrrib("Roll1y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var rollp2 = new Point2D(float.Parse(carpetnode.GetAtrrib("Roll2x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(carpetnode.GetAtrrib("Roll2y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var rollout = new Line2D(rollp1, rollp2);
                        var rollvec = new Vector2D(rollp1, rollp2);
                        var a1 = new Point2D(rollp1.X + 0.1f * rollvec.X + 0.1f * rollvec.Y, rollp1.Y - 0.1f * rollvec.X + 0.1f * rollvec.Y);
                        var a2 = new Point2D(rollp1.X + 0.1f * rollvec.X - 0.1f * rollvec.Y, rollp1.Y + 0.1f * rollvec.X + 0.1f * rollvec.Y);
                        Point2D[] arrow = new Point2D[] { rollp1, a1, a2 };
                        BeeCarpet carpet = new BeeCarpet(carpetnode.GetAtrrib("Name"), carpetnode.GetAtrrib("RollText"), p1, p2, p3, p4, rollout, rollvec, arrow);
                        carpets.Add(carpet);
                    }
                    foreach (XmlNode lapnode in lapnodes)
                    {
                        //P1 and P2 are the two points of the lapping with x and y coordinates.
                        var p1 = new Point2D(float.Parse(lapnode.GetAtrrib("P1x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(lapnode.GetAtrrib("P1y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var p2 = new Point2D(float.Parse(lapnode.GetAtrrib("P2x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(lapnode.GetAtrrib("P2y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        Line2D lapline = new Line2D(p1, p2);
                        laps.Add(lapline);
                    }
                    foreach (XmlNode tepzutepnode in tepzutepnodes)
                    {
                        //P1 and P2 are the two points of the dimension between one carpet end to other carpet start with x and y coordinates.
                        var p1 = new Point2D(float.Parse(tepzutepnode.GetAtrrib("P1x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(tepzutepnode.GetAtrrib("P1y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        var p2 = new Point2D(float.Parse(tepzutepnode.GetAtrrib("P2x"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")), float.Parse(tepzutepnode.GetAtrrib("P2y"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                        Line2D tepzutepline = new Line2D(p1, p2);
                        tepDis.Add(tepzutepline);
                    }
                    RollOutPlan.Add((carpets, laps, tepDis));
                }
            }
            catch { return; }
        }
    }
    public static class BamGeom     //This class will hold all the information about geometry of a structural member
    {
        public static List<(int Closed, int IsArc, int PolyArc, Point2D Center, int Radius, List<(Point2D point, float bulge)> Geom_Coords)> Geom_Points { get; set; }
        public static void Evaluate(XmlNode GeomNode)   //Call this method to fill in the info of geometry
        {
            Geom_Points = new List<(int Closed, int IsArc, int PolyArc, Point2D Center, int Radius, List<(Point2D point, float bulge)> Geom_Coords)>();
            try
            {
                foreach (XmlNode geomnode in GeomNode.SelectNodes("Geom"))
                {
                    //Check the documentation for Structural member geometry or BAMPOLY file
                    var clsd = int.TryParse(geomnode.GetAtrrib("Closed"), out int clsdres) ? clsdres : 0;
                    var isarc = int.TryParse(geomnode.GetAtrrib("IsArc"), out int isarcres) ? isarcres : 0;
                    var plyarc = int.TryParse(geomnode.GetAtrrib("PolyArc"), out int plyarcres) ? plyarcres : 0;
                    var rad = 0; var cntr = new Point2D(0, 0);
                    var geomcoor = new List<(Point2D point, float bulge)>();
                    var ptsnode = geomnode.SelectSingleNode("Pts");
                    if (isarc == 1 && plyarc == 0)
                    {
                        rad = float.TryParse(ptsnode.GetAtrrib("radius"), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out float radres) ? (int)radres : 0;
                        var cntrstr = ptsnode.GetAtrrib("center").Split(',');
                        var cx = float.TryParse(cntrstr[0], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out float cxres) ? (int)cxres : 0;
                        var cy = float.TryParse(cntrstr[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out float cyres) ? (int)cyres : 0;
                        cntr = new Point2D(cx, cy);
                    }
                    else
                    {
                        foreach (XmlAttribute att in ptsnode.Attributes)
                        {
                            var coors = att.Value.Split(',');
                            var coorx = float.TryParse(coors[0], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out float cxres) ? (int)cxres : 0;
                            var coory = float.TryParse(coors[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out float cyres) ? (int)cyres : 0;
                            var bulge = float.TryParse(coors[3], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out float bres) ? (int)bres : 0;
                            geomcoor.Add((new Point2D(coorx, coory), bulge));
                        }
                    }
                    Geom_Points.Add((clsd, isarc, plyarc, cntr, rad, geomcoor));
                }
            }
            catch { return; }
        }
    }

    public static class BarsBands       //This class will hold the information for all the bands and the bars points in an individual carpet
    {
        public static List<Bar2D> Bars { get; set; }
        public static List<Line2D> Bands { get; set; }
        public static void Evaluate(XmlDocument doc)    //Call this method to fill in the info of bars and bands for a carpet
        {
            Bars = new List<Bar2D>(); Bands = new List<Line2D>();
            foreach (XmlNode bar in doc.SelectNodes("/XML/Steel/Bar"))
            {
                var b1 = new Line2D(new Point2D(int.Parse(bar.GetAtrrib("X1")), int.Parse(bar.GetAtrrib("Y1"))), new Point2D(int.Parse(bar.GetAtrrib("X2")), int.Parse(bar.GetAtrrib("Y2"))));
                Bars.Add(new Bar2D(b1, int.Parse(bar.GetAtrrib("Dia"))));
            }
            foreach (XmlNode band in doc.SelectNodes("/XML/Slab/Band"))
            {
                var l1 = new Line2D(new Point2D(int.Parse(band.GetAtrrib("X1")), int.Parse(band.GetAtrrib("Y1"))), new Point2D(int.Parse(band.GetAtrrib("X2")), int.Parse(band.GetAtrrib("Y2"))));
                Bands.Add(l1);
            }
        }
    }

    public static class BamStorageCarpets       //This class will hold the information regarding the storage data
    {
        public static bool HasCarpets { get; set; }             // If true then this storage class instance has some carpets
        public static int StorageRotateDeg { get; set; }        // Rotation of the storage image (not used currently)
        public static List<string> CarpetNames { get; set; }    // Names of the carpets in the evaluated storage.
        public static List<int> CarpetPosition { get; set; }    // Positions of the carpets in the evaluated storage.
        public static void Evaluate(XmlNode storagenode)    //Call this method to fill the storage info
        {
            HasCarpets = false; StorageRotateDeg = 0;
            try
            {
                CarpetNames = new List<string>();
                CarpetPosition = new List<int>();
                switch (storagenode.GetAtrrib("Orientation"))
                {
                    case "Horizontal":
                        StorageInfo.Orientation = EnumOrientation.Horizontal; break;
                    case "Vertical":
                        StorageInfo.Orientation = EnumOrientation.Vertical; break;
                }
                StorageInfo.GridSize = int.Parse(storagenode.GetAtrrib("Size"));
                InfoGraphics.StorageName = storagenode.GetAtrrib("Name");
                StorageRotateDeg = int.Parse(storagenode.GetAtrrib("Navigation"));

                var names = storagenode.GetAtrrib("Carpets").Split('|', StringSplitOptions.RemoveEmptyEntries);
                var positions = storagenode.GetAtrrib("Position").Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 0) HasCarpets = true;
                for (int i = 0; i < names.Length; i++)
                {
                    CarpetNames.Add(names[i]); CarpetPosition.Add(int.Parse(positions[i]));
                }
            }
            catch { HasCarpets = false; StorageRotateDeg = 0; }
        }
        public static string StorageCarpetString()  //This method with return all the carpets available in this storage class instance as a string
        {
            try
            {
                string storageCarpets = string.Empty;
                foreach (var carp in CarpetNames)
                {
                    storageCarpets += carp + "|";
                }
                return storageCarpets;
            }
            catch { return "null"; }
        }
    }

    public static class StorageInfo     //This class is used for orientation of the storage and size of the storage
    {
        public static EnumOrientation Orientation { get; set; } //Orientation represents if the carpets are lying horizontally or vertically in the storage image
        public static int GridSize { get; set; }                //GridSize is the size of the grid assigned for this storage. integer value
    }
    public enum EnumOrientation
    {
        None = 0, Horizontal = 1, Vertical = 2
    }
}
