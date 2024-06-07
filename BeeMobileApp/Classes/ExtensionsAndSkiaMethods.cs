using SkiaScene;
using SkiaSharp;
using Preferences = Microsoft.Maui.Storage.Preferences;

namespace BeeMobileApp.Classes
{
    internal static class Extensions
    {
        public static void EditControl(this Label control, double height = 0, double width = 0, double padding = 0)     //Edit the control Xamrin.Forms.Label
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
            control.Padding = padding;
        }
        public static void EditControl(this Button control, double height = 0, double width = 0, int cornerradius = 0, double padding = 0)  //Edit the control Xamrin.Forms.Button
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
            control.Padding = padding; control.CornerRadius = cornerradius;
        }
        public static void EditControl(this Image control, double height = 0, double width = 0)     //Edit the control Xamrin.Forms.Image
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
        }
        public static void EditControl(this ImageButton control, double height = 0, double width = 0, int cornerradius = 0, double padding = 0)     //Edit the control Xamrin.Forms.ImageButton
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
            control.Padding = padding; control.CornerRadius = cornerradius;
        }
        public static void EditControl(this Frame control, double height = 0, double width = 0, float cornerradius = 0, double padding = 0)     //Edit the control Xamrin.Forms.Frame
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
            control.Padding = padding; control.CornerRadius = cornerradius;
        }
        public static void EditControl(this StackLayout control, double height = 0, double width = 0, double padding = 0)       //Edit the control Xamrin.Forms.StackLayout
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
            control.Padding = padding;
        }
        public static void EditControl(this Grid control, double height = 0, double width = 0, double padding = 0)      //Edit the control Xamrin.Forms.Grid
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
            control.Padding = padding;
        }
        public static void EditControl(this BoxView control, double height = 0, double width = 0)       //Edit the control Xamrin.Forms.BoxView
        {
            if (height != 0) control.HeightRequest = height;
            if (width != 0) control.WidthRequest = width;
        }
        #region Point2D Extensions
        public static float Length(this Point2D p1, Point2D p2) => (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        public static Point2D Midpoint(this Point2D p1, Point2D p2) => new Point2D((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        #endregion

        #region Line2D Extensions
        public static float Length(this Line2D l) => (float)Math.Sqrt(Math.Pow(l.P2.X - l.P1.X, 2) + Math.Pow(l.P2.Y - l.P1.Y, 2));
        public static Point2D Midpoint(this Line2D l) => new Point2D((l.P1.X + l.P2.X) / 2, (l.P1.Y + l.P2.Y) / 2);
        public static Vector2D ToVector(this Line2D l) => new Vector2D(l.P1, l.P2);
        public static float Slope(this Line2D l) => (l.P2.Y - l.P1.Y) / (l.P2.X - l.P1.X);
        public static bool IsParallelTo(this Line2D l1, Line2D l2, float abstolerance = 0.01f)
        {
            if (l1.IsVertical() && l2.IsVertical()) return true;
            if (l1.IsVertical() || l2.IsVertical()) return false;
            return Math.Abs(l1.Slope() - l2.Slope()) < Math.Abs(abstolerance);
        }
        public static float ToParallelLineDistance(this Line2D l1, Line2D l2)
        {
            if (l1.IsVertical() && l2.IsVertical()) return l2.P1.X - l1.P1.X;
            return Math.Abs(-l1.Slope() * l1.P1.X + l1.P1.Y - (-l2.Slope() * l2.P1.X + l2.P1.Y));
        }
        public static bool IsVertical(this Line2D l) => Math.Abs(l.P2.X - l.P1.X) / l.Length() < 0.001;

        public static bool IntersectsWith(this Line2D l1, Line2D l2)
        {
            if (l1.IsParallelTo(l2)) return false;
            float x, y;
            if (l1.IsVertical())
            {
                var s = l2.Slope(); var yint = -s * l2.P1.X + l2.P1.Y;
                x = l1.P1.X; y = s * x + yint;
            }
            else if (l2.IsVertical())
            {
                var s = l1.Slope(); var yint = -s * l1.P1.X + l1.P1.Y;
                x = l2.P1.X; y = s * x + yint;
            }
            else
            {
                var s1 = l1.Slope(); var s2 = l2.Slope(); var yint1 = s1 * l1.P1.X + l1.P1.Y; var yint2 = -s2 * l2.P1.X + l2.P1.Y;
                x = (yint2 - yint1) / (s1 - s2); y = s1 * x + yint1;
            }
            return x >= Math.Min(l1.P1.X, l1.P2.X) && x <= Math.Max(l1.P1.X, l1.P2.X) && x >= Math.Min(l2.P1.X, l2.P2.X) && x <= Math.Max(l2.P1.X, l2.P2.X) &&
                   y >= Math.Min(l1.P1.Y, l1.P2.Y) && y <= Math.Max(l1.P1.Y, l1.P2.Y) && y >= Math.Min(l2.P1.Y, l2.P2.Y) && y <= Math.Max(l2.P1.Y, l2.P2.Y);
        }
        #endregion

        #region Vector2D Extensions
        public static float Length(this Vector2D v) => (float)Math.Sqrt(Math.Pow(v.X, 2) + Math.Pow(v.Y, 2));
        public static Vector2D ToUnitVector(this Vector2D v)
        {
            var magnitude = v.Length();
            var temp = new Vector2D(v.X / magnitude, v.Y / magnitude);
            if (temp.X == 0 || temp.X == 1 || temp.X == -1 || temp.Y == 0 || temp.Y == 1 || temp.Y == -1) return new Vector2D((float)Math.Round(temp.X), (float)Math.Round(temp.Y));
            return temp;
        }
        public static Vector2D Clock90Rot(this Vector2D v) => new Vector2D(v.Y, -v.X);
        public static Vector2D CounterClock90Rot(this Vector2D v) => new Vector2D(-v.Y, v.X);
        public static Vector2D CounterClock45Rot(this Vector2D v)
        {
            float rot45 = 0.5f * (float)Math.Sqrt(2);
            return new Vector2D(v.X * rot45 - v.Y * rot45, v.X * rot45 + v.Y * rot45);
        }
        public static Vector2D Rot180(this Vector2D v) => new Vector2D(-v.X, -v.Y);
        #endregion        
    }
    internal static class SkiaMethods   //This class contains all methods which are related to skia sharp. Skia sharp is the 2D renderer used in this app
    {
        public static byte[] GeomImage(bool ForAugmentedReality, float zoom = 1)   //Creates the geometry image from the geometry points. ForAugmentedReality is true if the image is for Augmented reality. Zoom in or out the image
        {
            try
            {
                InfoGraphics.InitialCenter = new SKPoint(App.WidthPixels / 2, App.HeightPixels / 2);
                RatioSkia(out float Xmin, out float Xmax, out float Ymax, out float Ymin, out float ratioX, out float ratioY);
                var imgWidth = (Xmax - Xmin) / ratioX; var imgHeight = (Ymax - Ymin) / ratioY;
                var info = new SKImageInfo((int)imgWidth, (int)imgHeight);
                ISKScene newScene;
                newScene = new SKScene(new RenderGeometry(ForAugmentedReality));
                var centerPoint = new SKPoint(imgWidth / 2, imgHeight);
                newScene.ScreenCenter = centerPoint;
                using (var surface = SKSurface.Create(info))
                {
                    SKCanvas canvas = surface.Canvas;
                    newScene.Zoom(centerPoint, zoom);
                    newScene.Render(canvas);
                    var image = surface.Snapshot(); SKData encoded = image.Encode();
                    return encoded.ToArray();
                }
            }
            catch { return null; }
        }
        public static void RatioSkia(out float Xmin, out float Xmax, out float Ymax, out float Ymin, out float ratioX, out float ratioY) // Calculate the ratio required to scale the drawing
        {

            Xmin = 0; Xmax = 0; Ymax = 0; Ymin = 0; ratioX = 0; ratioY = 0;
            try
            {
                int count = 0;
                foreach (var (Closed, IsArc, PolyArc, Center, Radius, Geom_Coords) in BamGeom.Geom_Points)
                {
                    if (IsArc == 1 && PolyArc == 0) continue;
                    else
                    {
                        foreach (var coor in Geom_Coords)
                        {
                            if (count == 0)
                            {
                                Xmin = coor.point.X; Xmax = coor.point.X;
                                Ymin = coor.point.Y; Ymax = coor.point.Y;
                            }
                            else
                            {
                                if (coor.point.X < Xmin) Xmin = coor.point.X;
                                if (coor.point.X > Xmax) Xmax = coor.point.X;
                                if (coor.point.Y < Ymin) Ymin = coor.point.Y;
                                if (coor.point.Y > Ymax) Ymax = coor.point.Y;
                            }
                            count++;
                        }
                    }

                }

                //Ymax = Ymax + 0.1f * Ymax;                
                if ((Ymax - Ymin) / (Xmax - Xmin) <= InfoGraphics.InitialCenter.Y / InfoGraphics.InitialCenter.X)
                {
                    ratioX = (Xmax - Xmin) / (2 * InfoGraphics.InitialCenter.X);
                    ratioY = (Ymax - Ymin) / (2 * InfoGraphics.InitialCenter.X) / ((Ymax - Ymin) / (Xmax - Xmin));
                }
                else
                {
                    ratioY = (Ymax - Ymin) / (2 * InfoGraphics.InitialCenter.Y);
                    ratioX = (Xmax - Xmin) / (2 * InfoGraphics.InitialCenter.Y) / ((Xmax - Xmin) / (Ymax - Ymin));
                }
            }
            catch { return; }
        }

        //This method will draw the carpet on skia sharp canvas
        public static void DrawRollOut(this SKCanvas canvas, BeeCarpet bamcarpet, float Xmin, float Ymax, float ratioX, float ratioY, SKPath ArrowHead, Point2D RollDirSplit1,
            Point2D RollDirSplit2, SKPath RollArrowHead1, SKPath RollArrowHead2, SKPoint RollSym, SKRect RollSymRect, float RollSymRot, float DirMagSkia, float TextRot, SKPoint TextMidPoint, float scale)
        {
            try
            {
                #region SKpaint
                SKPaint PaintFill = new SKPaint
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = SKColors.Red,
                    StrokeWidth = 2 / scale
                };

                float[] pix = { 20, 5 };
                SKPaint startEle = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Red,
                    PathEffect = SKPathEffect.CreateDash(pix, 25),
                    StrokeWidth = 4 / scale
                };

                SKPaint dirEle = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Red,
                    StrokeWidth = 2 / scale
                };

                SKPaint textEle = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Red,
                    IsAntialias = true
                };
                #endregion
                canvas.DrawPath(ArrowHead, PaintFill);
                canvas.DrawLine((bamcarpet.Rollout.P1.X - Xmin) / ratioX, (Ymax - bamcarpet.Rollout.P1.Y) / ratioY, (RollDirSplit1.X - Xmin) / ratioX, (Ymax - RollDirSplit1.Y) / ratioY, dirEle);
                canvas.DrawLine((RollDirSplit2.X - Xmin) / ratioX, (Ymax - RollDirSplit2.Y) / ratioY, (bamcarpet.Rollout.P2.X - Xmin) / ratioX, (Ymax - bamcarpet.Rollout.P2.Y) / ratioY, dirEle);
                canvas.DrawLine((bamcarpet.P2.X - Xmin) / ratioX, (Ymax - bamcarpet.P2.Y) / ratioY, (bamcarpet.P3.X - Xmin) / ratioX, (Ymax - bamcarpet.P3.Y) / ratioY, startEle);
                canvas.DrawCircle(RollSym.X, RollSym.Y, 0.075f * DirMagSkia, dirEle);
                canvas.DrawCircle(RollSym.X, RollSym.Y, 0.005f * DirMagSkia, PaintFill);
                canvas.Save(); canvas.RotateDegrees(RollSymRot, RollSym.X, RollSym.Y);
                canvas.DrawArc(RollSymRect, 0, 90, false, dirEle); canvas.DrawArc(RollSymRect, 180, 90, false, dirEle);
                canvas.DrawPath(RollArrowHead1, PaintFill); canvas.DrawPath(RollArrowHead2, PaintFill);
                canvas.Restore();
                canvas.Save(); canvas.RotateRadians(TextRot, TextMidPoint.X, TextMidPoint.Y);
                textEle.TextSize = 0.4f * DirMagSkia * textEle.TextSize / textEle.MeasureText(bamcarpet.Text);
                canvas.DrawText(bamcarpet.Text, TextMidPoint.X - textEle.MeasureText(bamcarpet.Text) / 2, TextMidPoint.Y + textEle.TextSize, textEle);
                textEle.TextSize = 0.25f * DirMagSkia * textEle.TextSize / textEle.MeasureText(bamcarpet.Lfdnr);
                canvas.DrawText(bamcarpet.Lfdnr, TextMidPoint.X - textEle.MeasureText(bamcarpet.Lfdnr) / 2, TextMidPoint.Y - textEle.TextSize / 5, textEle);
                float arcradii = 0.6f * textEle.TextSize;
                canvas.DrawLine(TextMidPoint.X - 0.2f * DirMagSkia + arcradii, TextMidPoint.Y - 0.1f * textEle.TextSize, TextMidPoint.X + 0.2f * DirMagSkia - arcradii, TextMidPoint.Y - 0.1f * textEle.TextSize, dirEle);
                canvas.DrawLine(TextMidPoint.X - 0.2f * DirMagSkia + arcradii, TextMidPoint.Y - 1.3f * textEle.TextSize, TextMidPoint.X + 0.2f * DirMagSkia - arcradii, TextMidPoint.Y - 1.3f * textEle.TextSize, dirEle);
                SKRect rect = new SKRect(TextMidPoint.X - 0.2f * DirMagSkia, TextMidPoint.Y - 1.3f * textEle.TextSize, TextMidPoint.X - 0.2f * DirMagSkia + 2 * arcradii, TextMidPoint.Y - 0.1f * textEle.TextSize);
                canvas.DrawArc(rect, 90, 180, false, dirEle);
                rect = new SKRect(TextMidPoint.X + 0.2f * DirMagSkia - 2 * arcradii, TextMidPoint.Y - 1.3f * textEle.TextSize, TextMidPoint.X + 0.2f * DirMagSkia, TextMidPoint.Y - 0.1f * textEle.TextSize);
                canvas.DrawArc(rect, 270, 180, false, dirEle);
                canvas.Restore();
            }
            catch { return; }
        }

        //This method will draw all the dimension (lapping, carpet to carpet, carpet to formwork) on skia sharp canvas
        public static void DrawDimension(this SKCanvas canvas, Line2D line, float Xmin, float Ymax, float ratioX, float ratioY, float dimptlinewidth, SKPaint lineDim, SKPaint textDim)
        {
            var VectorUnit = line.ToVector().ToUnitVector(); var length = Math.Round(line.Length()).ToString(); float TextRot = 0;
            if (VectorUnit.X >= 0 && VectorUnit.Y != -1) TextRot = (float)Math.Asin(-VectorUnit.Y);
            if (VectorUnit.X <= 0 && VectorUnit.Y != 1) TextRot = (float)Math.Asin(VectorUnit.Y);

            var CounterVec = VectorUnit.CounterClock90Rot(); var ClockVec = VectorUnit.Clock90Rot(); var CcVec45 = VectorUnit.CounterClock45Rot(); var Clock45 = CcVec45.Rot180();
            var MidPoint = line.Midpoint(); SKPoint RotPoint = new SKPoint((MidPoint.X - Xmin) / ratioX, (Ymax - MidPoint.Y) / ratioY);
            canvas.DrawLine((line.P1.X - Xmin) / ratioX, (Ymax - line.P1.Y) / ratioY, (line.P2.X - Xmin) / ratioX, (Ymax - line.P2.Y) / ratioY, lineDim);
            SKPoint b1 = new SKPoint((line.P1.X + CounterVec.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P1.Y + CounterVec.Y * dimptlinewidth)) / ratioY);
            SKPoint b2 = new SKPoint((line.P1.X + ClockVec.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P1.Y + ClockVec.Y * dimptlinewidth)) / ratioY);
            SKPoint t1 = new SKPoint((line.P2.X + CounterVec.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P2.Y + CounterVec.Y * dimptlinewidth)) / ratioY);
            SKPoint t2 = new SKPoint((line.P2.X + ClockVec.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P2.Y + ClockVec.Y * dimptlinewidth)) / ratioY);
            canvas.DrawLine(b1, b2, lineDim); canvas.DrawLine(t1, t2, lineDim);
            b1 = new SKPoint((line.P1.X + CcVec45.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P1.Y + CcVec45.Y * dimptlinewidth)) / ratioY);
            b2 = new SKPoint((line.P1.X + Clock45.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P1.Y + Clock45.Y * dimptlinewidth)) / ratioY);
            t1 = new SKPoint((line.P2.X + CcVec45.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P2.Y + CcVec45.Y * dimptlinewidth)) / ratioY);
            t2 = new SKPoint((line.P2.X + Clock45.X * dimptlinewidth - Xmin) / ratioX, (Ymax - (line.P2.Y + Clock45.Y * dimptlinewidth)) / ratioY);
            canvas.DrawLine(b1, b2, lineDim); canvas.DrawLine(t1, t2, lineDim);
            canvas.Save(); canvas.RotateRadians(TextRot, RotPoint.X, RotPoint.Y);
            canvas.DrawText(length, RotPoint.X - textDim.MeasureText(length) / 2, RotPoint.Y - textDim.TextSize / 3, textDim);
            canvas.Restore();
        }
        public static Line2D CarpetToEdgeDim(BeeCarpet bamcarpet) //Calculates the dimension line from the start of the installation carpet to the nearest egde of formwork
        {
            try
            {
                List<Line2D> alldedge = new List<Line2D>();
                for (int i = 0; i < BamGeom.Geom_Points[0].Geom_Coords.Count - 1; i++)
                {
                    alldedge.Add(new Line2D(BamGeom.Geom_Points[0].Geom_Coords[i].point, BamGeom.Geom_Points[0].Geom_Coords[i + 1].point));
                }
                float magnitude = 0; Vector2D Vec = new Vector2D(0, 0);
                Point2D P1 = bamcarpet.Rollout.P2; var rollline = new Line2D(bamcarpet.P2, bamcarpet.P3);
                var UnitVec = bamcarpet.RollVec.ToUnitVector();
                foreach (var edge in alldedge)
                {
                    if (edge.IsParallelTo(rollline))
                    {
                        var dis = edge.ToParallelLineDistance(rollline);
                        if (dis > magnitude)
                        {
                            Point2D temp = new Point2D(P1.X + UnitVec.X * 1000000, P1.Y + UnitVec.Y * 1000000);
                            Line2D intersection = new Line2D(P1, temp);
                            if (intersection.IntersectsWith(edge)) { magnitude = dis; Vec = UnitVec; }
                        }
                    }
                }
                Point2D P2 = new Point2D(P1.X + Vec.X * magnitude, P1.Y + Vec.Y * magnitude);
                return new Line2D(P1, P2);
            }
            catch { return new Line2D(); }
        }
        public static void SkiaGeometryPoints(out List<SKPath> geomPath, out List<List<float>> circlePoints) //This method will convert all the geometry points BamGeom.Geom_Points into Skia sharp points
        {
            try
            {
                geomPath = new List<SKPath>(); circlePoints = new List<List<float>>();
                RatioSkia(out float Xmin, out float Xmax, out float Ymax, out float Ymin, out float ratioX, out float ratioY);

                foreach (var (Closed, IsArc, PolyArc, Center, Radius, Geom_Coords) in BamGeom.Geom_Points)
                {
                    SKPath Gpath = new SKPath(); SKRect rect; int count; bool bulge = false; double sweepAngle, startAngle;
                    if (IsArc == 1)
                    {
                        if (PolyArc == 1)
                        {
                            var centerPoint = CenterAndRadius(Geom_Coords[0].point, Geom_Coords[1].point, Geom_Coords[2].point, out float radius);
                            circlePoints.Add(new List<float> { (centerPoint.X - Xmin) / ratioX, (Ymax - centerPoint.Y) / ratioY, radius / ratioX });
                        }
                        else
                        {
                            circlePoints.Add(new List<float> { (Center.X - Xmin) / ratioX, (Ymax - Center.Y) / ratioY, Radius / ratioX });
                        }
                    }
                    else
                    {
                        count = 0;
                        foreach (var coor in Geom_Coords)
                        {
                            if (bulge)
                            {
                                float Norm = Geom_Coords[count - 1].point.Length(coor.point);
                                float radius = Norm / (2 * (float)Math.Sin(2 * Math.Atan(Math.Abs(Geom_Coords[count - 1].bulge))));
                                float sArc = Norm / 2;
                                float dArc = sArc * (1 - Geom_Coords[count - 1].bulge * Geom_Coords[count - 1].bulge) / (2 * Geom_Coords[count - 1].bulge);
                                float vN = (coor.point.Y - Geom_Coords[count - 1].point.Y) / Norm;
                                float uN = (coor.point.X - Geom_Coords[count - 1].point.X) / Norm;
                                float cX = -vN * dArc + (coor.point.X + Geom_Coords[count - 1].point.X) / 2;
                                float cY = uN * dArc + (coor.point.Y + Geom_Coords[count - 1].point.Y) / 2;


                                sweepAngle = -180 * 4 * Math.Atan(Geom_Coords[count - 1].bulge) / Math.PI;

                                if (Geom_Coords[count - 1].point.X > cX && Geom_Coords[count - 1].point.Y > cY) { startAngle = 1.5 * Math.PI + Math.Atan((Geom_Coords[count - 1].point.X - cX) / (Geom_Coords[count - 1].point.Y - cY)); }
                                else if (Geom_Coords[count - 1].point.X < cX && Geom_Coords[count - 1].point.Y > cY) { startAngle = Math.PI + (float)Math.Atan((Geom_Coords[count - 1].point.Y - cY) / (cX - Geom_Coords[count - 1].point.X)); }
                                else if (Geom_Coords[count - 1].point.X < cX && Geom_Coords[count - 1].point.Y < cY) { startAngle = 0.5 * Math.PI + (float)Math.Atan((cX - Geom_Coords[count - 1].point.X) / (cY - Geom_Coords[count - 1].point.Y)); }
                                else { startAngle = (float)Math.Atan((cY - Geom_Coords[count - 1].point.Y) / (Geom_Coords[count - 1].point.X - cX)); }
                                startAngle = startAngle * 180 / Math.PI;
                                float Rcx = cX - radius; float Rcy = cY + radius;

                                rect = new SKRect((Rcx - Xmin) / ratioX, (Ymax - Rcy) / ratioY, (2 * radius + Rcx - Xmin) / ratioX, (Ymax - Rcy + 2 * radius) / ratioY);
                                Gpath.AddArc(rect, (float)startAngle, (float)sweepAngle);
                                bulge = false;
                            }
                            else
                            {
                                if (count == 0) { Gpath.MoveTo((coor.point.X - Xmin) / ratioX, (Ymax - coor.point.Y) / ratioY); }
                                else Gpath.LineTo((coor.point.X - Xmin) / ratioX, (Ymax - coor.point.Y) / ratioY);
                            }
                            if (Geom_Coords[count].bulge != 0) bulge = true;
                            count++;

                        }

                        if (Closed == 1)
                        {
                            var index = count - 1;
                            if (Geom_Coords[index].bulge != 0)
                            {
                                float Norm = Geom_Coords[index].point.Length(Geom_Coords[0].point);
                                float radius = Norm / (2 * (float)Math.Sin(2 * Math.Atan(Math.Abs(Geom_Coords[index].bulge))));
                                float sArc = Norm / 2;
                                float dArc = sArc * (1 - (float)Math.Pow(Geom_Coords[index].bulge, 2)) / (2 * Geom_Coords[index].bulge);
                                float uN = (Geom_Coords[0].point.X - Geom_Coords[index].point.X) / Norm;
                                float vN = (Geom_Coords[0].point.Y - Geom_Coords[index].point.Y) / Norm;
                                float cX = -vN * dArc + (Geom_Coords[0].point.X + Geom_Coords[index].point.X) / 2;
                                float cY = uN * dArc + (Geom_Coords[0].point.Y + Geom_Coords[index].point.Y) / 2;

                                sweepAngle = -180 * 4 * Math.Atan(Geom_Coords[index].bulge) / Math.PI;

                                if (Geom_Coords[index].point.X > cX && Geom_Coords[index].point.Y > cY) { startAngle = 1.5 * Math.PI + Math.Atan((Geom_Coords[index].point.X - cX) / (Geom_Coords[index].point.Y - cY)); }
                                else if (Geom_Coords[index].point.X < cX && Geom_Coords[index].point.Y > cY) { startAngle = Math.PI + (float)Math.Atan((Geom_Coords[index].point.Y - cY) / (cX - Geom_Coords[index].point.X)); }
                                else if (Geom_Coords[index].point.X < cX && Geom_Coords[index].point.Y < cY) { startAngle = 0.5 * Math.PI + (float)Math.Atan((cX - Geom_Coords[index].point.X) / (cY - Geom_Coords[index].point.Y)); }
                                else { startAngle = (float)Math.Atan((cY - Geom_Coords[index].point.Y) / (Geom_Coords[index].point.X - cX)); }
                                startAngle = startAngle * 180 / Math.PI;
                                float Rcx = cX - radius; float Rcy = cY + radius;
                                rect = new SKRect((Rcx - Xmin) / ratioX, (Ymax - Rcy) / ratioY, (2 * radius + Rcx - Xmin) / ratioX, (Ymax - Rcy + 2 * radius) / ratioY);

                                Gpath.AddArc(rect, (float)startAngle, (float)sweepAngle);

                            }
                            else Gpath.LineTo(Gpath[0].X, Gpath[0].Y);
                        }
                        geomPath.Add(Gpath);
                    }
                }
            }
            catch { geomPath = new List<SKPath>(); circlePoints = new List<List<float>>(); }
        }
        public static Point2D CenterAndRadius(Point2D p0, Point2D p1, Point2D p2, out float radius) // Calculate the center of circle and its radius using three points on the circumference
        {
            try
            {
                Point2D center;
                float b1, c1, b2, c2, m1x, m1y, m2x, m2y;
                b1 = (p1.X - p0.X) / (p0.Y - p1.Y); b2 = (p2.X - p1.X) / (p1.Y - p2.Y);
                m1x = (p0.X + p1.X) / 2; m1y = (p0.Y + p1.Y) / 2; m2x = (p1.X + p2.X) / 2; m2y = (p1.Y + p2.Y) / 2;

                if (b1 == 0 || b2 == 0)
                {
                    if (b1 == 0) { c2 = m2y - b2 * m2x; center = new Point2D((m1y - c2) / b2, m1y); }
                    else { c1 = m1y - b1 * m1x; center = new Point2D((m2y - c1) / b1, m2y); }
                }
                else if (float.IsInfinity(b1) || float.IsInfinity(b2))
                {
                    if (float.IsInfinity(b1)) { c2 = m2y - b2 * m2x; center = new Point2D(m1x, b2 * m1x + c2); }
                    else { c1 = m1y - b1 * m1x; center = new Point2D(m2x, b1 * m2x + c1); }
                }
                else { c1 = m1y - b1 * m1x; c2 = m2y - b2 * m2x; var tempx = (c2 - c1) / (b1 - b2); center = new Point2D(tempx, b1 * tempx + c1); }
                radius = (float)Math.Sqrt(Math.Pow(p0.X - center.X, 2) + Math.Pow(p0.Y - center.Y, 2));
                return center;
            }
            catch { radius = 0; return new Point2D(); }
        }
        public static int AngleBwLines(SKPoint p1, SKPoint p2, SKPoint p3, SKPoint p4)
        {
            double angle;
            float m1 = (p2.Y - p1.Y) / (p2.X - p1.X);
            float m2 = (p4.Y - p3.Y) / (p4.X - p3.X);
            if (float.IsInfinity(m1)) angle = Math.PI / 2 - Math.Abs(Math.Atan(m2));
            else if (float.IsInfinity(m2)) angle = Math.PI / 2 - Math.Abs(Math.Atan(m1));
            else angle = Math.Atan((m1 - m2) / (1 + m1 * m2));
            return (int)(180 * angle / Math.PI);
        }

    }
    public class Generic : Page
    {
        public static new async Task<bool> DisplayAlert(string title, string message, string accept) => await DisplayAlert(title, message, accept);


        public static string GetID()        //Gets the user ID which is automatically saved during login
        {
            if (Preferences.ContainsKey("ID")) return "ID";
            return string.Empty;
        }
        public static string GetPassword()  //Gets the user Password which is automatically saved during login
        {
            if (Preferences.ContainsKey("Pass")) return "Pass";
            return string.Empty;
        }


        public static async Task<bool> CameraPermission()   //Prompts the user to give permission to use the device camera
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Camera access denied", "BeeAPP does not have access to your camera. To enable access, go to Settings and turn on camera permission", "Ok");
            }
            return status == PermissionStatus.Granted;
        }
        public static async Task<bool> MediaPermission()    //Prompts the user to give permission to use the device media
        {
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Media access denied", "BeeAPP does not have access to your media. To enable access, go to Settings and turn on media permission", "Ok");
            }
            return status == PermissionStatus.Granted;
        }
        public static async void ShowFTPAlert()
        {
            await DisplayAlert("Try again", "Could not upload the image to the FTP server.\nCheck your internet connection.\nIf the problem persists, then contact support \"support@bee.com\".", "Ok");
        }
    }

}
