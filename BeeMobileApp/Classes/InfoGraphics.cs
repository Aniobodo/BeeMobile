using SkiaScene;
using SkiaSharp;
using System.Reflection;

namespace BeeMobileApp.Classes
{
    internal static class InfoGraphics
    {
        internal static string StorageName { get; set; }        // Variable to store storage name of the current storage
        internal static SKPoint InitialCenter { get; set; }     // Center coordinates of the device screen or skia sharp canvas
        internal static bool CanvasBlink { get; set; }          // Variable used for blinking effects
        internal static byte[] StorageImage { get; set; }       // Variable to store the Bitmap or captured image of the storage
        internal static string ActiveCarpet { get; set; }       // Variable to get the current carpet
        internal static int CompassPointer { get; set; }        // Variable to get degrees of rotation
        internal static int PlanRotateDeg { get; set; }         // Saved position of the formwork
        internal static float PlanScale { get; set; }           // The scale of the roll out plan. Scale = 1 if site and different for game
        internal static int PlanLayer { get; set; }             // The current layer of the rollout plan
    }

    public class RenderRollOutPlan : ISKSceneRenderer    //Skia sharp render class for rollout plan
    {
        private static List<SKPath> GeomPaths { get; set; }
        private static List<List<float>> GeomCirclePoints { get; set; }
        public RenderRollOutPlan()
        {
            GeomPaths = new List<SKPath>(); GeomCirclePoints = new List<List<float>>();
        }
        public void Render(SKCanvas canvas, float angleInRadians, SKPoint center, float scale)
        {
            try
            {
                canvas.Clear(SKColors.White); canvas.Save();
                if (GeomPaths.Count == 0 && GeomCirclePoints.Count == 0)
                {
                    SkiaMethods.SkiaGeometryPoints(out var geompaths, out var circlepoints); GeomPaths = geompaths; GeomCirclePoints = circlepoints;
                }
                SkiaMethods.RatioSkia(out float Xmin, out float Xmax, out float Ymax, out float Ymin, out float ratioX, out float ratioY);
                if (InfoGraphics.PlanRotateDeg != 0) canvas.RotateDegrees(InfoGraphics.CompassPointer, (Xmax - Xmin) / (2 * ratioX), (Ymax - Ymin) / (2 * ratioY));

                #region SkPaint variables
                SKPaint border = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 2 / scale
                };

                SKPaint EleBox = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Red,
                    StrokeWidth = 1 / scale
                };

                SKPaint textDim = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black,
                    IsAntialias = true
                };
                SKPaint lineDim = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 2 / scale
                };
                #endregion

                #region Draw Plan Outline or complete geometry of a structural member
                foreach (var circle in GeomCirclePoints)
                {
                    canvas.DrawCircle(circle[0], circle[1], circle[2], border);
                }
                foreach (var Skiapath in GeomPaths)
                {
                    canvas.DrawPath(Skiapath, border);
                }
                #endregion

                var dimptlinewidth = 75; textDim.TextSize = 200 / ratioX;
                var beelayer = BamRollOutPlan.RollOutPlan[InfoGraphics.PlanLayer];

                //Plotting each carpet and its details
                #region Plot rollout plan
                foreach (var bamcarpet in beelayer.bamcarpets)
                {
                    SKPath BoxPath = new SKPath(); SKPath ArrowHead = new SKPath();

                    BoxPath.MoveTo((bamcarpet.P2.X - Xmin) / ratioX, (Ymax - bamcarpet.P2.Y) / ratioY); BoxPath.LineTo((bamcarpet.P1.X - Xmin) / ratioX, (Ymax - bamcarpet.P1.Y) / ratioY);
                    BoxPath.LineTo((bamcarpet.P4.X - Xmin) / ratioX, (Ymax - bamcarpet.P4.Y) / ratioY); BoxPath.LineTo((bamcarpet.P3.X - Xmin) / ratioX, (Ymax - bamcarpet.P3.Y) / ratioY);

                    ArrowHead.MoveTo((bamcarpet.Arrow[0].X - Xmin) / ratioX, (Ymax - bamcarpet.Arrow[0].Y) / ratioY); ArrowHead.LineTo((bamcarpet.Arrow[1].X - Xmin) / ratioX, (Ymax - bamcarpet.Arrow[1].Y) / ratioY);
                    ArrowHead.LineTo((bamcarpet.Arrow[2].X - Xmin) / ratioX, (Ymax - bamcarpet.Arrow[2].Y) / ratioY);

                    Point2D RollDirSplit1 = new Point2D(bamcarpet.Rollout.P1.X + 0.75f * bamcarpet.RollVec.X, bamcarpet.Rollout.P1.Y + 0.75f * bamcarpet.RollVec.Y);
                    Point2D RollDirSplit2 = new Point2D(bamcarpet.Rollout.P1.X + 0.85f * bamcarpet.RollVec.X, bamcarpet.Rollout.P1.Y + 0.85f * bamcarpet.RollVec.Y);

                    Vector2D RollSymVec = new Vector2D(); float RollSymRot = 15; float TextRot = 0; var RollVecUnit = bamcarpet.RollVec.ToUnitVector();
                    Vector2D rollarrowrot = new Vector2D();
                    if (RollVecUnit.X >= 0 && RollVecUnit.Y != -1)
                    {
                        RollSymVec = new Vector2D(-RollVecUnit.Y, -RollVecUnit.X, bamcarpet.RollVec.Length()); RollSymRot = -15; TextRot = (float)Math.Asin(-RollVecUnit.Y);
                        rollarrowrot = new Vector2D(1, 0);
                    }
                    if (RollVecUnit.X <= 0 && RollVecUnit.Y != 1)
                    {
                        RollSymVec = new Vector2D(RollVecUnit.Y, RollVecUnit.X, bamcarpet.RollVec.Length()); TextRot = (float)Math.Asin(RollVecUnit.Y);
                        rollarrowrot = new Vector2D(0, 1);
                    }

                    SKPoint RollSym = new SKPoint((RollDirSplit2.X + 0.075f * RollSymVec.X - Xmin) / ratioX, (Ymax - RollDirSplit2.Y + 0.075f * RollSymVec.Y) / ratioY);
                    var DirMagSkia = bamcarpet.Rollout.Length() / ratioX;
                    SKRect RollSymRect = new SKRect(RollSym.X - 0.05f * DirMagSkia, RollSym.Y - 0.05f * DirMagSkia, RollSym.X + 0.05f * DirMagSkia, RollSym.Y + 0.05f * DirMagSkia);
                    SKPath RollArrowHead1 = new SKPath();
                    RollArrowHead1.MoveTo(RollSym.X + 0.04f * DirMagSkia * rollarrowrot.X, RollSym.Y + 0.04f * DirMagSkia * rollarrowrot.Y);
                    RollArrowHead1.LineTo(RollSym.X + 0.06f * DirMagSkia * rollarrowrot.X, RollSym.Y + 0.06f * DirMagSkia * rollarrowrot.Y);
                    RollArrowHead1.LineTo(RollSym.X + 0.05f * DirMagSkia * rollarrowrot.X - 0.01f * DirMagSkia * rollarrowrot.Y, RollSym.Y + 0.05f * DirMagSkia * rollarrowrot.Y - 0.01f * DirMagSkia * rollarrowrot.X);
                    RollArrowHead1.Close();
                    SKPath RollArrowHead2 = new SKPath();
                    RollArrowHead2.MoveTo(RollSym.X - 0.06f * DirMagSkia * rollarrowrot.X, RollSym.Y - 0.06f * DirMagSkia * rollarrowrot.Y);
                    RollArrowHead2.LineTo(RollSym.X - 0.04f * DirMagSkia * rollarrowrot.X, RollSym.Y - 0.04f * DirMagSkia * rollarrowrot.Y);
                    RollArrowHead2.LineTo(RollSym.X - 0.05f * DirMagSkia * rollarrowrot.X + 0.01f * DirMagSkia * rollarrowrot.Y, RollSym.Y - 0.05f * DirMagSkia * rollarrowrot.Y + 0.01f * DirMagSkia * rollarrowrot.X);
                    RollArrowHead2.Close();

                    var TempPoint = bamcarpet.Rollout.P1.Midpoint(RollDirSplit1); var TextMidPoint = new SKPoint((TempPoint.X - Xmin) / ratioX, (Ymax - TempPoint.Y) / ratioY);


                    if (bamcarpet.Lfdnr == InfoGraphics.ActiveCarpet)
                    {
                        if (InfoGraphics.CanvasBlink)
                        {
                            canvas.DrawRollOut(bamcarpet, Xmin, Ymax, ratioX, ratioY, ArrowHead, RollDirSplit1, RollDirSplit2, RollArrowHead1, RollArrowHead2, RollSym, RollSymRect, RollSymRot, DirMagSkia, TextRot, TextMidPoint, scale);
                            var BarToEdge = SkiaMethods.CarpetToEdgeDim(bamcarpet);
                            canvas.DrawDimension(BarToEdge, Xmin, Ymax, ratioX, ratioY, dimptlinewidth, lineDim, textDim);
                        }
                    }
                    else
                    {
                        canvas.DrawRollOut(bamcarpet, Xmin, Ymax, ratioX, ratioY, ArrowHead, RollDirSplit1, RollDirSplit2, RollArrowHead1, RollArrowHead2, RollSym, RollSymRect, RollSymRot, DirMagSkia, TextRot, TextMidPoint, scale);
                    }
                    canvas.DrawPath(BoxPath, EleBox);
                }
                #endregion

                #region Plot dimensions on rolout plan                             
                foreach (var lapping in beelayer.lappings)
                {
                    canvas.DrawDimension(lapping, Xmin, Ymax, ratioX, ratioY, dimptlinewidth, lineDim, textDim);
                }
                foreach (var tepzutep in beelayer.tepzutep)
                {
                    canvas.DrawDimension(tepzutep, Xmin, Ymax, ratioX, ratioY, dimptlinewidth, lineDim, textDim);
                }
                #endregion
                canvas.Restore();
            }
            catch { return; }
        }

    }
    public class RenderGeometry : ISKSceneRenderer     //Skia sharp render class for geometry
    {
        private static List<SKPath> PathGeom { get; set; }
        private static int PathInt { get; set; }
        private bool PlotARImg { get; set; }
        private static List<SKPath> GeomPaths { get; set; }
        private static List<List<float>> GeomCirclePoints { get; set; }
        public RenderGeometry(bool isARImg = false)
        {
            PathGeom = new List<SKPath>();
            PlotARImg = isARImg;
            GeomPaths = new List<SKPath>(); GeomCirclePoints = new List<List<float>>();
        }
        public void Render(SKCanvas canvas, float angleInRadians, SKPoint center, float scale)
        {
            try
            {
                canvas.Clear(SKColors.Transparent);

                //if (String.IsNullOrEmpty(InfoGraphics.GeomData)) return;                           
                int BorderCount = 0, maxcount = 0;

                SkiaMethods.RatioSkia(out float Xmin, out _, out float Ymax, out _, out float ratioX, out float ratioY);

                SKPaint border = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 2
                };
                SKPaint fillGeom1 = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.SkyBlue
                };
                SKPaint fillGeom2 = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White
                };
                SKPaint fillPoint = new SKPaint
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = SKColors.Red
                };
                SKPaint Txt = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black,
                    IsAntialias = true
                };

                if (GeomPaths.Count == 0 && GeomCirclePoints.Count == 0)
                {
                    SkiaMethods.SkiaGeometryPoints(out var geompaths, out var circlepoints); GeomPaths = geompaths; GeomCirclePoints = circlepoints;
                }

                #region Draw Plan Outline or complete geometry of a structural member
                foreach (var circle in GeomCirclePoints)
                {
                    canvas.DrawCircle(circle[0], circle[1], circle[2], border);
                }
                foreach (var Skiapath in GeomPaths)
                {
                    canvas.DrawPath(Skiapath, border);
                    if (PathGeom.Count != GeomPaths.Count)
                    {
                        PathGeom.Add(Skiapath);
                        int pixelcount = 0;
                        for (int i = 0; i < App.WidthPixels; i++)
                        {
                            for (int j = 0; j < App.HeightPixels; j++)
                            {
                                if (Skiapath.Contains(i, j)) pixelcount++;
                            }
                        }
                        if (maxcount < pixelcount) { maxcount = pixelcount; PathInt = BorderCount; }
                    }
                    BorderCount++;
                }
                canvas.DrawPath(PathGeom[PathInt], fillGeom1); canvas.DrawPath(PathGeom[PathInt], border);
                for (int i = 0; i < PathGeom.Count; i++)
                {
                    if (i != PathInt) { canvas.DrawPath(PathGeom[i], fillGeom2); canvas.DrawPath(PathGeom[i], border); }
                }
                #endregion

                if (PlotARImg)
                {
                    float mag = 0;
                    Point2D P1 = new Point2D(); Point2D P2 = new Point2D();
                    var Coordinates = BamGeom.Geom_Points[PathInt].Geom_Coords;
                    for (int ind = 0; ind < Coordinates.Count - 2; ind++)
                    {
                        P1 = new Point2D(Coordinates[ind].point); P2 = new Point2D(Coordinates[ind + 1].point);
                        mag = P1.Length(P2);
                        if (mag > 1000) break;
                    }

                    var UnitV = new Vector2D(P1, P2, true);
                    float Epx = P2.X - UnitV.X * 200; float Epy = P2.Y - UnitV.Y * 200;
                    canvas.DrawCircle((P1.X - Xmin) / ratioX, (Ymax - P1.Y) / ratioY, 200 / ratioX, fillPoint);
                    canvas.DrawCircle((P2.X - Xmin) / ratioX, (Ymax - P2.Y) / ratioY, 200 / ratioX, fillPoint);
                    Txt.TextSize = 350 / ratioX; canvas.DrawText("1", (P1.X - Xmin) / ratioX - Txt.MeasureText("1") / 2, (Ymax - P1.Y) / ratioY + Txt.TextSize / 3, Txt);
                    canvas.DrawText("2", (P2.X - Xmin) / ratioX - Txt.MeasureText("2") / 2, (Ymax - P2.Y) / ratioY + Txt.TextSize / 3, Txt);
                    canvas.DrawLine((P1.X + UnitV.X * 200 - Xmin) / ratioX, (Ymax - P1.Y - UnitV.Y * 200) / ratioY, (Epx - Xmin) / ratioX, (Ymax - Epy) / ratioY, new SKPaint { StrokeWidth = 3, Color = SKColors.Red, Style = SKPaintStyle.StrokeAndFill });
                    if (UnitV.X >= 0)
                    {
                        if (UnitV.Y != -1f)
                        {
                            canvas.Save(); canvas.RotateRadians((float)Math.Asin(-UnitV.Y), (Epx - Xmin) / ratioX, (Ymax - Epy) / ratioY);
                            canvas.DrawLine((Epx - 0.2f * mag * UnitV.X - 0.2f * mag * UnitV.Y - Xmin) / ratioX, (Ymax - (Epy - 0.2f * mag * UnitV.Y - 0.2f * mag * UnitV.X)) / ratioY, (Epx - Xmin) / ratioX, (Ymax - Epy) / ratioY, new SKPaint { StrokeWidth = 3, Color = SKColors.Red, Style = SKPaintStyle.StrokeAndFill });
                            canvas.DrawLine((Epx - 0.2f * mag * UnitV.X - 0.2f * mag * UnitV.Y - Xmin) / ratioX, (Ymax - (Epy + 0.2f * mag * UnitV.Y + 0.2f * mag * UnitV.X)) / ratioY, (Epx - Xmin) / ratioX, (Ymax - Epy) / ratioY, new SKPaint { StrokeWidth = 3, Color = SKColors.Red, Style = SKPaintStyle.StrokeAndFill });
                            canvas.Restore();
                        }
                    }
                    if (UnitV.X <= 0)
                    {
                        if (UnitV.Y != 1f)
                        {
                            canvas.Save(); canvas.RotateRadians((float)Math.Asin(UnitV.Y), (Epx - Xmin) / ratioX, (Ymax - Epy) / ratioY);
                            canvas.DrawLine((Epx - 0.2f * mag * UnitV.X - 0.2f * mag * UnitV.Y - Xmin) / ratioX, (Ymax - (Epy - 0.2f * mag * UnitV.Y - 0.2f * mag * UnitV.X)) / ratioY, (Epx - Xmin) / ratioX, (Ymax - Epy) / ratioY, new SKPaint { StrokeWidth = 3, Color = SKColors.Red, Style = SKPaintStyle.StrokeAndFill });
                            canvas.DrawLine((Epx - 0.2f * mag * UnitV.X - 0.2f * mag * UnitV.Y - Xmin) / ratioX, (Ymax - (Epy + 0.2f * mag * UnitV.Y + 0.2f * mag * UnitV.X)) / ratioY, (Epx - Xmin) / ratioX, (Ymax - Epy) / ratioY, new SKPaint { StrokeWidth = 3, Color = SKColors.Red, Style = SKPaintStyle.StrokeAndFill });
                            canvas.Restore();
                        }
                    }

                }
            }
            catch { return; }
        }
    }

    public class RenderBars : ISKSceneRenderer       //Skia sharp class to render loose bars
    {
        private static List<SKPath> GeomPaths { get; set; }
        private static List<List<float>> GeomCirclePoints { get; set; }
        public RenderBars()
        {
            GeomPaths = new List<SKPath>(); GeomCirclePoints = new List<List<float>>();
        }
        public void Render(SKCanvas canvas, float angleInRadians, SKPoint center, float scale)
        {
            canvas.Clear(SKColors.White);
            try
            {
                SkiaMethods.RatioSkia(out float Xmin, out _, out float Ymax, out _, out float ratioX, out float ratioY);

                SKPaint border = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 2 / scale
                };
                SKPaint textDim = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black,
                    IsAntialias = true
                };

                if (GeomPaths.Count == 0 && GeomCirclePoints.Count == 0)
                {
                    SkiaMethods.SkiaGeometryPoints(out var geompaths, out var circlepoints); GeomPaths = geompaths; GeomCirclePoints = circlepoints;
                }

                #region Draw Plan Outline or complete geometry of a structural member
                foreach (var circle in GeomCirclePoints)
                {
                    canvas.DrawCircle(circle[0], circle[1], circle[2], border);
                }
                foreach (var Skiapath in GeomPaths)
                {
                    canvas.DrawPath(Skiapath, border);
                }

                #endregion

                SKPaint BarPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3 / scale
                };

                foreach (var bar in BarsBands.Bars)
                {
                    switch (bar.Diameter)
                    {
                        case int n when n <= 8:
                            BarPaint.Color = SKColors.Blue;
                            break;
                        case 10:
                            BarPaint.Color = SKColors.Magenta;
                            break;
                        case 12:
                            BarPaint.Color = SKColors.Cyan;
                            break;
                        case 14:
                            BarPaint.Color = SKColors.SaddleBrown;
                            break;
                        case 16:
                            BarPaint.Color = SKColors.BlueViolet;
                            break;
                        case 18:
                            BarPaint.Color = SKColors.Chocolate;
                            break;
                        case 20:
                            BarPaint.Color = SKColors.LightSeaGreen;
                            break;
                        case 22:
                            BarPaint.Color = SKColors.Coral;
                            break;
                        case int n when n > 22:
                            BarPaint.Color = SKColors.Olive;
                            break;
                    }
                    textDim.Color = BarPaint.Color;
                    canvas.DrawLine((bar.B.P1.X - Xmin) / ratioX, (Ymax - bar.B.P1.Y) / ratioY, (bar.B.P2.X - Xmin) / ratioX, (Ymax - bar.B.P2.Y) / ratioY, BarPaint);

                    textDim.TextSize = 75 / ratioX;
                    var VectorUnit = bar.B.ToVector().ToUnitVector(); float TextRot = 0;
                    if (VectorUnit.X >= 0 && VectorUnit.Y != -1) TextRot = (float)Math.Asin(-VectorUnit.Y);
                    if (VectorUnit.X <= 0 && VectorUnit.Y != 1) TextRot = (float)Math.Asin(VectorUnit.Y);

                    var CounterVec = VectorUnit.CounterClock90Rot(); var ClockVec = VectorUnit.Clock90Rot(); var CcVec45 = VectorUnit.CounterClock45Rot(); var Clock45 = CcVec45.Rot180();
                    SKPoint RotPoint = new SKPoint((bar.B.P2.X - Xmin) / ratioX, (Ymax - bar.B.P2.Y) / ratioY);

                    canvas.Save(); canvas.RotateRadians(TextRot, RotPoint.X, RotPoint.Y);
                    var text = string.Format("⌀{0} / {1} mm", bar.Diameter, Math.Round(bar.B.Length()));
                    canvas.DrawText(text, RotPoint.X, RotPoint.Y, textDim);
                    canvas.Restore();


                }
            }
            catch { return; }
        }
    }

    public class RenderAnchorage : ISKSceneRenderer      //Skia sharp class to render carpet anchor points
    {
        private float Ewidth { get; set; }
        private float CofGravity { get; set; }
        public RenderAnchorage(float width, float CofG)
        {
            Ewidth = width; CofGravity = CofG;
        }
        public void Render(SKCanvas canvas, float angleInRadians, SKPoint center, float scale)
        {
            canvas.Clear(SKColors.White);
            SKPaint border = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 5
            };
            SKPaint band = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 8
            };
            SKPaint bar = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 2
            };
            SKPaint Hook = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black
            };
            float[] pix = { 15, 10 };
            SKPaint Dim = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                PathEffect = SKPathEffect.CreateDash(pix, 25),
                StrokeWidth = 3
            };
            SKPaint Anchor = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 3
            };
            SKPaint Txt = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black,
                IsAntialias = true
            };

            float cx, cy; cx = InfoGraphics.InitialCenter.X; cy = InfoGraphics.InitialCenter.Y;
            float rad = App.WidthPixels / 12;
            float total = Ewidth, mid = CofGravity, mid1 = (float)Math.Round(CofGravity / 2), mid2 = (float)Math.Round((Ewidth - CofGravity) / 2);

            float anch, anch1, anch2, GapV;

            anch = 0.2f * cx + mid * 1.6f * cx / total;
            anch1 = 0.2f * cx + (mid - mid1) * 1.6f * cx / total;
            anch2 = 0.2f * cx + (mid + mid2) * 1.6f * cx / total;

            float l1 = anch1 - 0.2f * cx, l2 = anch - anch1, l3 = anch2 - anch, l4 = 1.8f * cx - anch2;

            SKPath Spath = new SKPath();
            for (float i = 0.2f; i < (float)Math.PI; i += 0.2f)
            {
                float Orad = rad * rad * (float)Math.Cos(0.5) / (float)Math.Sqrt(Math.Pow(rad, 2) * Math.Pow(Math.Sin(i), 2) + Math.Pow(rad * (float)Math.Cos(0.5), 2) * Math.Pow(Math.Cos(i), 2));
                canvas.DrawLine(0.2f * cx + Orad * (float)Math.Sin(i), cy + Orad * (float)Math.Cos(i), Orad * (float)Math.Sin(i) + 1.8f * cx, cy + Orad * (float)Math.Cos(i), bar);
            }
            for (float i = 0.3f; i < (float)Math.PI; i += 0.2f)
            {
                float Orad = rad * rad * (float)Math.Cos(0.5) / (float)Math.Sqrt(Math.Pow(rad, 2) * Math.Pow(Math.Sin(i), 2) + Math.Pow(rad * (float)Math.Cos(0.5), 2) * Math.Pow(Math.Cos(i), 2));
                canvas.DrawLine(0.2f * cx + Orad * (float)Math.Sin(i), cy + Orad * (float)Math.Cos(i), 0.2f * cx - Orad * (float)Math.Sin(i), cy + Orad * (float)Math.Cos(i), bar);
            }
            canvas.DrawOval(0.2f * cx, cy, rad * (float)Math.Cos(0.5), rad, border);
            SKRect Srect = new SKRect(1.8f * cx - rad * (float)Math.Cos(0.5), cy - rad, 1.8f * cx + rad * (float)Math.Cos(0.5), cy + rad);
            Spath.AddArc(Srect, 90, -180); canvas.DrawPath(Spath, border);
            canvas.DrawLine(0.2f * cx, cy - rad, 1.8f * cx, cy - rad, border);
            canvas.DrawLine(0.2f * cx, cy + rad, 1.8f * cx, cy + rad, border);
            canvas.DrawLine(0.2f * cx - rad * (float)Math.Cos(0.5), cy, 0.2f * cx - rad * (float)Math.Cos(0.5), 1.5f * cy, border);
            canvas.DrawLine(1.8f * cx - rad * (float)Math.Cos(0.5), cy + rad, 1.8f * cx - rad * (float)Math.Cos(0.5), 1.5f * cy, border);
            canvas.DrawLine(0.2f * cx - rad * (float)Math.Cos(0.5), 1.5f * cy, 1.8f * cx - rad * (float)Math.Cos(0.5), 1.5f * cy, border);

            for (float i = 1.02f * cy + rad; i < 1.5f * cy; i += 0.05f * cy)
            {
                canvas.DrawLine(0.2f * cx - rad * (float)Math.Cos(0.5), i, 1.8f * cx - rad * (float)Math.Cos(0.5), i, bar);
            }
            for (float i = 0.4f * cx; i < 1.8f * cx; i += 0.25f * cx)
            {
                Srect = new SKRect(i - rad * (float)Math.Cos(0.5), cy - rad, i + rad * (float)Math.Cos(0.5), cy + rad);
                Spath = new SKPath();
                Spath.AddArc(Srect, 90, -180); canvas.DrawPath(Spath, band);
            }
            for (float i = 0.4f * cx; i < 1.8f * cx; i += 0.25f * cx)
            {
                canvas.DrawLine(i - rad * (float)Math.Cos(0.5), cy + rad, i - rad * (float)Math.Cos(0.5), 1.5f * cy, band);
            }

            canvas.DrawCircle(0.2f * cx, cy, 10, Hook);
            canvas.DrawCircle(1.8f * cx, cy, 10, Hook);
            canvas.DrawLine(0.2f * cx, cy, 0.2f * cx, 0.7f * cy, Dim);
            canvas.DrawLine(1.8f * cx, cy, 1.8f * cx, 0.7f * cy, Dim);
            canvas.DrawLine(anch1, cy - rad, anch1, 0.7f * cy, Anchor);
            canvas.DrawLine(anch2, cy - rad, anch2, 0.7f * cy, Anchor);
            canvas.DrawLine(anch1, 0.7f * cy, anch, 0.35f * cy, Anchor); canvas.DrawLine(anch1, 0.7f * cy, anch1, 0.35f * cy, Dim);
            SKRect degrec = new SKRect(anch1 - 0.1f * cy, 0.7f * cy - 0.1f * cy, anch1 + 0.1f * cy, 0.7f * cy + 0.1f * cy);
            var sweepangle = SkiaMethods.AngleBwLines(new SKPoint(anch1, 0.7f * cy), new SKPoint(anch1, 0.35f * cy), new SKPoint(anch1, 0.7f * cy), new SKPoint(anch, 0.35f * cy)); canvas.DrawArc(degrec, -90, sweepangle, false, Anchor);
            float LenX = (float)Math.Tan(Math.PI * sweepangle / 180) * 0.15f * cy; Txt.TextSize = 50; canvas.DrawText("30°", anch1 + (Math.Abs(LenX) - Txt.MeasureText("30°")) / 2, 0.7f * cy - 0.15f * cy, Txt);
            canvas.DrawLine(anch2, 0.7f * cy, anch, 0.35f * cy, Anchor); canvas.DrawLine(anch2, 0.7f * cy, anch2, 0.35f * cy, Dim);
            degrec = new SKRect(anch2 - 0.1f * cy, 0.7f * cy - 0.1f * cy, anch2 + 0.1f * cy, 0.7f * cy + 0.1f * cy);
            sweepangle = SkiaMethods.AngleBwLines(new SKPoint(anch2, 0.7f * cy), new SKPoint(anch2, 0.35f * cy), new SKPoint(anch2, 0.7f * cy), new SKPoint(anch, 0.35f * cy)); canvas.DrawArc(degrec, -90, -sweepangle, false, Anchor);
            LenX = (float)Math.Tan(Math.PI * sweepangle / 180) * 0.15f * cy; Txt.TextSize = 50; canvas.DrawText("30°", anch2 - Math.Abs(LenX) + (Math.Abs(LenX) - Txt.MeasureText("30°")) / 2, 0.7f * cy - 0.15f * cy, Txt);
            canvas.DrawOval(anch, 0.35f * cy, 10, 20, Hook);
            canvas.DrawCircle(0.2f * cx, 0.7f * cy, 10, Hook);
            canvas.DrawCircle(1.8f * cx, 0.7f * cy, 10, Hook);
            canvas.DrawCircle(anch1, 0.7f * cy, 10, Hook);
            canvas.DrawCircle(anch2, 0.7f * cy, 10, Hook);
            canvas.DrawCircle(anch, 0.7f * cy, 10, Hook);

            GapV = (float)Math.Round((mid - mid1) / InfoGraphics.PlanScale);
            canvas.DrawLine(0.2f * cx, 0.7f * cy, 0.2f * cx + 0.15f * l1, 0.7f * cy, Anchor);
            canvas.DrawLine(0.2f * cx + 0.85f * l1, 0.7f * cy, 0.2f * cx + l1, 0.7f * cy, Anchor);
            canvas.DrawRoundRect(0.2f * cx + 0.15f * l1, 0.7f * cy - 30, 0.7f * l1, 60, 30, 30, Anchor);
            Txt.TextSize = Txt.TextSize * (0.5f * l1) / Txt.MeasureText(GapV.ToString());
            if (Txt.TextSize > 60) Txt.TextSize = 50;
            canvas.DrawText(GapV.ToString(), 0.2f * cx + l1 / 2 - Txt.MeasureText(GapV.ToString()) / 2, 0.7f * cy + Txt.TextSize / 2.5f, Txt);
            GapV = (float)Math.Round(mid1 / InfoGraphics.PlanScale);
            canvas.DrawLine(anch1, 0.7f * cy, anch1 + 0.15f * l2, 0.7f * cy, Anchor);
            canvas.DrawLine(anch1 + 0.85f * l2, 0.7f * cy, anch1 + l2, 0.7f * cy, Anchor);
            canvas.DrawRoundRect(anch1 + 0.15f * l2, 0.7f * cy - 30, 0.7f * l1, 60, 30, 30, Anchor);
            Txt.TextSize = Txt.TextSize * (0.5f * l2) / Txt.MeasureText(GapV.ToString());
            if (Txt.TextSize > 60) Txt.TextSize = 50;
            canvas.DrawText(GapV.ToString(), anch1 + l2 / 2 - Txt.MeasureText(GapV.ToString()) / 2, 0.7f * cy + Txt.TextSize / 2.5f, Txt);
            GapV = (float)Math.Round(mid2 / InfoGraphics.PlanScale);
            canvas.DrawLine(anch, 0.7f * cy, anch + 0.15f * l3, 0.7f * cy, Anchor);
            canvas.DrawLine(anch + 0.85f * l3, 0.7f * cy, anch + l3, 0.7f * cy, Anchor);
            canvas.DrawRoundRect(anch + 0.15f * l3, 0.7f * cy - 30, 0.7f * l3, 60, 30, 30, Anchor);
            Txt.TextSize = Txt.TextSize * (0.5f * l3) / Txt.MeasureText(GapV.ToString());
            if (Txt.TextSize > 60) Txt.TextSize = 50;
            canvas.DrawText(GapV.ToString(), anch + l3 / 2 - Txt.MeasureText(GapV.ToString()) / 2, 0.7f * cy + Txt.TextSize / 2.5f, Txt);
            GapV = (float)Math.Round((total - mid - mid2) / InfoGraphics.PlanScale);
            canvas.DrawLine(anch2, 0.7f * cy, anch2 + 0.15f * l4, 0.7f * cy, Anchor);
            canvas.DrawLine(anch2 + 0.85f * l4, 0.7f * cy, anch2 + l4, 0.7f * cy, Anchor);
            canvas.DrawRoundRect(anch2 + 0.15f * l4, 0.7f * cy - 30, 0.7f * l4, 60, 30, 30, Anchor);
            Txt.TextSize = Txt.TextSize * (0.5f * l4) / Txt.MeasureText(GapV.ToString());
            if (Txt.TextSize > 60) Txt.TextSize = 50;
            canvas.DrawText(GapV.ToString(), anch2 + l4 / 2 - Txt.MeasureText(GapV.ToString()) / 2, 0.7f * cy + Txt.TextSize / 2.5f, Txt);
        }
    }
    public class RenderStorage : ISKSceneRenderer        //Skia sharp class to render storage image and the scanned carpets on top of it
    {
        private int Identifier { get; set; }
        public RenderStorage(int ide)
        {
            Identifier = ide;
        }
        public void Render(SKCanvas canvas, float angleInRadians, SKPoint center, float scale)
        {
            try
            {
                float StartPointX = InfoGraphics.InitialCenter.X - scale * center.X;
                float StartPointY = InfoGraphics.InitialCenter.Y - scale * center.Y;
                canvas.Clear();
                canvas.Save();
                SKPaint border = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 2
                };
                SKPaint fillColor = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White.WithAlpha((byte)(0xFF * 0.75))
                };

                SKPaint Txt = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Red,
                    IsAntialias = true
                };
                SKBitmap bitmap; float bitDim; SKRect rect;
                List<SKPath> CList = new List<SKPath>();
                bitmap = SKBitmap.Decode(InfoGraphics.StorageImage);
                if (bitmap.Height > bitmap.Width) { rect = new SKRect(0, 0, InfoGraphics.InitialCenter.X * 2, InfoGraphics.InitialCenter.Y * 2); bitDim = InfoGraphics.InitialCenter.Y * 2; }
                else { bitDim = bitmap.Height * InfoGraphics.InitialCenter.X * 2 / bitmap.Width; rect = new SKRect(0, InfoGraphics.InitialCenter.Y - bitDim / 2, InfoGraphics.InitialCenter.X * 2, InfoGraphics.InitialCenter.Y + bitDim / 2); }

                //if (BamStorageCarpets.StorageRotateDeg != 0) canvas.RotateDegrees(BamStorageCarpets.StorageRotateDeg - InfoGraphics.CompassPointer, InfoGraphics.InitialCenter.X, InfoGraphics.InitialCenter.Y);

                canvas.DrawBitmap(bitmap, rect);

                float BoxWidth;
                if (StorageInfo.Orientation == EnumOrientation.Vertical) BoxWidth = bitDim / StorageInfo.GridSize;
                else BoxWidth = InfoGraphics.InitialCenter.X * 2 / StorageInfo.GridSize;

                #region Division of the storage area
                if (StorageInfo.Orientation == EnumOrientation.Vertical)
                {
                    int count = 0;
                    for (float i = InfoGraphics.InitialCenter.Y - bitDim / 2; i < InfoGraphics.InitialCenter.Y + bitDim / 2; i += BoxWidth)
                    {
                        if (count == StorageInfo.GridSize) continue;
                        SKPath BoxPath = new SKPath(); //SKPath PathClick = new SKPath();
                        BoxPath.MoveTo(0, i); //PathClick.MoveTo(StartPointX + scale * 0, StartPointY + scale * i);
                        BoxPath.LineTo(InfoGraphics.InitialCenter.X * 2, i); //PathClick.LineTo(StartPointX + scale * (InfoGraphics.InitialCenter.X * 2), StartPointY + scale * i);
                        BoxPath.LineTo(InfoGraphics.InitialCenter.X * 2, i + BoxWidth); //PathClick.LineTo(StartPointX + scale * (InfoGraphics.InitialCenter.X * 2), StartPointY + scale * (i + BoxWidth));
                        BoxPath.LineTo(0, i + BoxWidth); //PathClick.LineTo(StartPointX + scale * 0, StartPointY + scale * (i + BoxWidth));
                        BoxPath.Close(); //PathClick.Close();
                        canvas.DrawPath(BoxPath, border); count++;//CList.Add(PathClick);
                    }
                }

                if (StorageInfo.Orientation == EnumOrientation.Horizontal)
                {
                    int count = 0;
                    for (float i = 0; i < InfoGraphics.InitialCenter.X * 2; i += BoxWidth)
                    {
                        if (count == StorageInfo.GridSize) continue;
                        SKPath BoxPath = new SKPath(); //SKPath PathClick = new SKPath();
                        BoxPath.MoveTo(i, InfoGraphics.InitialCenter.Y - bitDim / 2); //PathClick.MoveTo(StartPointX + scale * i, StartPointY + scale * (InfoGraphics.InitialCenter.Y - bitDim / 2));
                        BoxPath.LineTo(i + BoxWidth, InfoGraphics.InitialCenter.Y - bitDim / 2); //PathClick.LineTo(StartPointX + scale * (i + BoxWidth), StartPointY + scale * (InfoGraphics.InitialCenter.Y - bitDim / 2));
                        BoxPath.LineTo(i + BoxWidth, InfoGraphics.InitialCenter.Y + bitDim / 2); //PathClick.LineTo(StartPointX + scale * (i + BoxWidth), StartPointY + scale * (InfoGraphics.InitialCenter.Y + bitDim / 2));
                        BoxPath.LineTo(i, InfoGraphics.InitialCenter.Y + bitDim / 2); //PathClick.LineTo(StartPointX + scale * i, StartPointY + scale * (InfoGraphics.InitialCenter.Y + bitDim / 2));
                        BoxPath.Close(); //PathClick.Close();
                        canvas.DrawPath(BoxPath, border); count++;//CList.Add(PathClick);
                    }
                }
                #endregion

                int QRIndex = 0; Txt.TextSize = BoxWidth * 0.75f;

                #region Allocation of carpet at the scanned position
                if (BamStorageCarpets.HasCarpets)
                {
                    if (Identifier == 0)
                    {
                        int i = 0;
                        QRIndex = BamStorageCarpets.CarpetNames.Count;
                        if (StorageInfo.Orientation == EnumOrientation.Vertical)
                        {
                            foreach (var carp in BamStorageCarpets.CarpetNames)
                            {
                                if (Txt.MeasureText(carp) > bitDim) Txt.TextSize = Txt.TextSize * InfoGraphics.InitialCenter.X * 2 / Txt.MeasureText(carp);
                                float dis = BoxWidth / 2 + Txt.TextSize / 2;
                                canvas.DrawText(carp, InfoGraphics.InitialCenter.X - Txt.MeasureText(carp) / 2, InfoGraphics.InitialCenter.Y - bitDim / 2 + (BamStorageCarpets.CarpetPosition[i] - 1) * BoxWidth + dis, Txt);
                                i++;
                            }
                        }
                        if (StorageInfo.Orientation == EnumOrientation.Horizontal)
                        {
                            foreach (var carp in BamStorageCarpets.CarpetNames)
                            {
                                if (Txt.MeasureText(carp) > bitDim) Txt.TextSize = Txt.TextSize * bitDim / Txt.MeasureText(carp);
                                float disX = (BamStorageCarpets.CarpetPosition[i] - 1) * BoxWidth + BoxWidth / 2 + Txt.TextSize / 2; float disY = InfoGraphics.InitialCenter.Y + Txt.MeasureText(carp) / 2;
                                canvas.Save(); canvas.RotateDegrees(-90, disX, disY);
                                canvas.DrawText(carp, disX, disY, Txt); canvas.Restore();
                                i++;
                            }
                        }
                    }
                    else
                    {
                        int i = 0; int ActivePos = -1;

                        if (StorageInfo.Orientation == EnumOrientation.Vertical)
                        {
                            foreach (var carp in BamStorageCarpets.CarpetNames)
                            {
                                if (carp == InfoGraphics.ActiveCarpet) ActivePos = BamStorageCarpets.CarpetPosition[i];
                                else
                                {
                                    if (Txt.MeasureText(carp) > bitDim) Txt.TextSize = Txt.TextSize * InfoGraphics.InitialCenter.X * 2 / Txt.MeasureText(carp);
                                    float dis = BoxWidth / 2 + Txt.TextSize / 2;
                                    canvas.DrawText(carp, InfoGraphics.InitialCenter.X - Txt.MeasureText(carp) / 2, InfoGraphics.InitialCenter.Y - bitDim / 2 + (BamStorageCarpets.CarpetPosition[i] - 1) * BoxWidth + dis, Txt);
                                }
                                i++;
                            }
                        }
                        if (StorageInfo.Orientation == EnumOrientation.Horizontal)
                        {
                            foreach (var carp in BamStorageCarpets.CarpetNames)
                            {
                                if (carp == InfoGraphics.ActiveCarpet) ActivePos = BamStorageCarpets.CarpetPosition[i];
                                else
                                {
                                    if (Txt.MeasureText(carp) > bitDim) Txt.TextSize = Txt.TextSize * bitDim / Txt.MeasureText(carp);
                                    float disX = (BamStorageCarpets.CarpetPosition[i] - 1) * BoxWidth + BoxWidth / 2 + Txt.TextSize / 2; float disY = InfoGraphics.InitialCenter.Y + Txt.MeasureText(carp) / 2;
                                    canvas.Save(); canvas.RotateDegrees(-90, disX, disY);
                                    canvas.DrawText(carp, disX, disY, Txt); canvas.Restore();
                                }
                                i++;
                            }
                        }


                        if (InfoGraphics.CanvasBlink)
                        {

                            if (StorageInfo.Orientation == EnumOrientation.Vertical)
                            {
                                canvas.DrawRect(0, InfoGraphics.InitialCenter.Y - bitDim / 2 + (ActivePos - 1) * BoxWidth, InfoGraphics.InitialCenter.X * 2, BoxWidth, fillColor);
                                if (Txt.MeasureText(InfoGraphics.ActiveCarpet) > bitDim) Txt.TextSize = Txt.TextSize * InfoGraphics.InitialCenter.X * 2 / Txt.MeasureText(InfoGraphics.ActiveCarpet);
                                float dis = BoxWidth / 2 + Txt.TextSize / 2;
                                canvas.DrawText(InfoGraphics.ActiveCarpet, InfoGraphics.InitialCenter.X - Txt.MeasureText(InfoGraphics.ActiveCarpet) / 2, InfoGraphics.InitialCenter.Y - bitDim / 2 + (ActivePos - 1) * BoxWidth + dis, Txt);
                            }
                            if (StorageInfo.Orientation == EnumOrientation.Horizontal)
                            {
                                canvas.DrawRect((ActivePos - 1) * BoxWidth, InfoGraphics.InitialCenter.Y - bitDim / 2, BoxWidth, bitDim, fillColor);
                                if (Txt.MeasureText(InfoGraphics.ActiveCarpet) > bitDim) Txt.TextSize = Txt.TextSize * bitDim / Txt.MeasureText(InfoGraphics.ActiveCarpet);
                                float disX = (ActivePos - 1) * BoxWidth + BoxWidth / 2 + Txt.TextSize / 2; float disY = InfoGraphics.InitialCenter.Y + Txt.MeasureText(InfoGraphics.ActiveCarpet) / 2;
                                canvas.Save(); canvas.RotateDegrees(-90, disX, disY);
                                canvas.DrawText(InfoGraphics.ActiveCarpet, disX, disY, Txt); canvas.Restore();
                            }

                        }

                    }

                }
                #endregion

                #region Position of the carpet which needs to be scanned
                if (Identifier == 0 && QRIndex < StorageInfo.GridSize && InfoGraphics.CanvasBlink)
                {
                    SKBitmap QRBitmap;
                    Assembly assembly = GetType().GetTypeInfo().Assembly;

                    //If the image needs to be converted to byte[] data then it has to be stored in the shared project. In this case the directory that is mentioned.
                    //After storing select it and go to properties on right bottom and in Build Action select Embedded resource
                    using (Stream stream = assembly.GetManifestResourceStream("BeeMobileApp.Media.QRcodeIcon.png"))
                    {
                        QRBitmap = SKBitmap.Decode(stream);
                    }
                    if (StorageInfo.Orientation == EnumOrientation.Vertical)
                    {

                        canvas.DrawRect(0, InfoGraphics.InitialCenter.Y - bitDim / 2 + QRIndex * BoxWidth, InfoGraphics.InitialCenter.X * 2, BoxWidth, fillColor);
                        SKRect QRrect = new SKRect(InfoGraphics.InitialCenter.X - BoxWidth / 2, InfoGraphics.InitialCenter.Y - bitDim / 2 + QRIndex * BoxWidth, InfoGraphics.InitialCenter.X + BoxWidth / 2, InfoGraphics.InitialCenter.Y - bitDim / 2 + (QRIndex + 1) * BoxWidth);
                        canvas.DrawBitmap(QRBitmap, QRrect);
                    }
                    if (StorageInfo.Orientation == EnumOrientation.Horizontal)
                    {
                        canvas.DrawRect(QRIndex * BoxWidth, InfoGraphics.InitialCenter.Y - bitDim / 2, BoxWidth, bitDim, fillColor);
                        SKRect QRrect = new SKRect(QRIndex * BoxWidth, InfoGraphics.InitialCenter.Y - BoxWidth / 2, (QRIndex + 1) * BoxWidth, InfoGraphics.InitialCenter.Y + BoxWidth / 2);
                        canvas.DrawBitmap(QRBitmap, QRrect);
                    }
                }
                #endregion

                //Check at which position the user tapped on the screen
                #region Check the tap position
                //if (Identifier == 0)
                //{
                //    if (InfoGraphics.PointReleased)
                //    {
                //        if (Methods.CompareDrag(InfoGraphics.PressL.X, InfoGraphics.PressL.Y, InfoGraphics.ReleaseL.X, InfoGraphics.ReleaseL.Y) <= 5)
                //        {
                //            int pos = 1;
                //            foreach (var path in CList)
                //            {
                //                if (path.Contains(InfoGraphics.ReleaseL.X, InfoGraphics.ReleaseL.Y))
                //                {
                //                    InfoGraphics.PressL = new SKPoint(-1, -1); InfoGraphics.ReleaseL = new SKPoint(-1, -1);
                //                    App.Current.MainPage.Navigation.PushAsync(new StorageQR(pos.ToString()));
                //                }
                //                pos++;
                //            }
                //
                //        }
                //
                //    }
                //}
                #endregion

                canvas.Restore();
            }
            catch { return; }

        }
    }


}
