using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BeeMobileApp.Classes;
using BeeMobileApp.Platforms.Android.Renderers;
using Google.AR.Core;
using Javax.Microedition.Khronos.Opengles;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using System.Globalization;
using AWR = Android.Widget.RelativeLayout;
using Camera = Google.AR.Core.Camera;
using Color = Android.Graphics.Color;
using ImageButton = Android.Widget.ImageButton;
using View = Android.Views.View;




namespace BeeMobileApp.Platforms.Android.Resources
{
    public class AndroidARRenderer : ViewRenderer<ARClass, AWR>, GLSurfaceView.IRenderer, View.IOnTouchListener
    {
        ScaleGestureDetector mScaleGesture;
        GestureDetector mGestureDetector;
        private static float mScaleFactor = 1.0f;
        private static ImageView mImgView;
        private static System.Drawing.PointF midPoint = new System.Drawing.PointF(0, 0);
        private readonly Context _context;
        GLSurfaceView mSurfaceView;
        Session mSession=null;
        bool isDisposed;

        private int viewWidth = 0;
        private int viewHeight = 0;
        private Anchor initAnchor { get; set; } = null;
        private Anchor endAnchor { get; set; } = null;
        private float AngleBwVectors { get; set; }
        private float BarAngle { get; set; }

        BackgroundRenderer mBackgroundRenderer = new BackgroundRenderer();
        //PlaneRenderer mPlaneRenderer = new PlaneRenderer();
        PointCloudRenderer mPointCloud = new PointCloudRenderer();
        CircleRenderer mCircleRenderer = new CircleRenderer();
        LineRendererBM mLineRenderer = new LineRendererBM();
        BarRenderer mBarRenderer = new BarRenderer();
        BandRenderer mBandRenderer = new BandRenderer();
        ARImageRenderer mImageRenderer = new ARImageRenderer();
        DisplayRotationHelper mDisplayRotationHelper;
        View Rview;
        ImageButton MeasureBtn;
        TextView MeasureDistance;
        TextView LoadSurfacesText;
        ImageButton mImgBtn;
        ImageButton mImgClose;

        private bool StartMeasure { get; set; } = false;
        private bool EndMeasure { get; set; } = false;
        private readonly float[] mPoseTranslation = new float[3];
        private readonly float[] mPoseRotation = new float[4];

        // Tap handling and UI.
        //List<Anchor> mAnchors = new List<Anchor>();
        public AndroidARRenderer(Context context) : base(context)
        {
            AutoPackage = false;
            _context = context;
        }
        protected override void OnElementChanged(ElementChangedEventArgs<ARClass> e)
        {

            base.OnElementChanged(e);
            isDisposed = false;
            var activity = _context as Activity;
            Rview = LayoutInflater.From(activity).Inflate(_Microsoft.Android.Resource.Designer.ResourceConstant.Layout.ARLayout, null);
            activity.AddContentView(Rview, new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));
            mSurfaceView = activity.FindViewById<GLSurfaceView>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.surfaceview);

            initAnchor = null; endAnchor = null; AngleBwVectors = 0; BarAngle = 0;
            GradientDrawable cornerRadText = new GradientDrawable();
            cornerRadText.SetShape(ShapeType.Rectangle);
            cornerRadText.SetColor(Color.Black);
            cornerRadText.SetCornerRadius(Resources.DisplayMetrics.WidthPixels / 12);
            GradientDrawable cornerRadAdd = new GradientDrawable();
            cornerRadAdd.SetShape(ShapeType.Rectangle);
            cornerRadAdd.SetColor(Color.Black);
            cornerRadAdd.SetCornerRadius(Resources.DisplayMetrics.WidthPixels / 12);
            GradientDrawable cornerRadImgC = new GradientDrawable();
            cornerRadImgC.SetShape(ShapeType.Rectangle);
            cornerRadImgC.SetColor(Color.ParseColor("#DC0D15"));
            cornerRadImgC.SetCornerRadius(Resources.DisplayMetrics.WidthPixels * 0.05f);

            MeasureDistance = activity.FindViewById<TextView>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.MeasureText); MeasureDistance.Alpha = 0.75f; MeasureDistance.Visibility = ViewStates.Gone; MeasureDistance.Background = cornerRadText;
            LoadSurfacesText = activity.FindViewById<TextView>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.LoadingText); LoadSurfacesText.Alpha = 0.75f; LoadSurfacesText.Text = "Searching for surfaces....";


            MeasureBtn = activity.FindViewById<ImageButton>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.MeasureBtn); MeasureBtn.Alpha = 0.75f;
            MeasureBtn.Click += MeasureBtn_Click;
            AWR.LayoutParams paramters = (AWR.LayoutParams)MeasureBtn.LayoutParameters;
            paramters.Width = Resources.DisplayMetrics.WidthPixels / 6;
            paramters.Height = Resources.DisplayMetrics.WidthPixels / 6;
            MeasureBtn.LayoutParameters = paramters; MeasureBtn.Background = cornerRadAdd;

            mImgView = activity.FindViewById<ImageView>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.ImgView); mImgView.Alpha = 0.75f; mImgView.Visibility = ViewStates.Gone;
            if (Element.PlotCarpet)
            {
                mImgView.SetImageBitmap(BitmapFactory.DecodeByteArray(Element.GeomImage, 0, Element.GeomImage.Length));
                paramters = (AWR.LayoutParams)mImgView.LayoutParameters;
                paramters.Width = (int)(Resources.DisplayMetrics.WidthPixels * 0.6f);
                paramters.Height = (int)(Resources.DisplayMetrics.HeightPixels * 0.6f);
                mImgView.LayoutParameters = paramters;
            }

            var WindowClose = activity.FindViewById<ImageButton>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.CloseWindow);
            paramters = (AWR.LayoutParams)WindowClose.LayoutParameters;
            paramters.Width = Resources.DisplayMetrics.WidthPixels / 10;
            paramters.Height = Resources.DisplayMetrics.WidthPixels / 10;
            WindowClose.LayoutParameters = paramters; WindowClose.Click += CloseWindow;

            mImgClose = activity.FindViewById<ImageButton>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.ImgClose); mImgClose.Visibility = ViewStates.Gone;
            mImgClose.SetX(Resources.DisplayMetrics.WidthPixels * 0.75f); mImgClose.SetY(Resources.DisplayMetrics.HeightPixels * 0.15f);
            paramters = (AWR.LayoutParams)mImgClose.LayoutParameters; //paramters.SetMargins()
            paramters.Width = (int)(Resources.DisplayMetrics.WidthPixels * 0.1f);
            paramters.Height = (int)(Resources.DisplayMetrics.WidthPixels * 0.1f);
            mImgClose.LayoutParameters = paramters; mImgClose.Click += mImgClose_Click;

            mImgBtn = activity.FindViewById<ImageButton>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.ImgBtn); mImgBtn.Alpha = 0.75f;
            mImgBtn.Click += ImageClick;
            if (Element.PlotCarpet)
            {
                mImgBtn.Visibility = ViewStates.Visible;
                mImgBtn.SetImageBitmap(BitmapFactory.DecodeByteArray(Element.GeomImage, 0, Element.GeomImage.Length));
                paramters = (AWR.LayoutParams)mImgBtn.LayoutParameters;
                paramters.Width = Resources.DisplayMetrics.WidthPixels / 5;
                paramters.Height = Resources.DisplayMetrics.HeightPixels / 5;
                mImgBtn.LayoutParameters = paramters;
            }
            else mImgBtn.Visibility = ViewStates.Gone;

            //Gyroscope.ReadingChanged += Gyroscope_Reading;
            mDisplayRotationHelper = new DisplayRotationHelper(_context);

            mScaleGesture = new ScaleGestureDetector(_context, new SimpleOnScaleGestureListener());
            mGestureDetector = new GestureDetector(_context, new PanGestureListener());

            //mSurfaceView.SetOnTouchListener(this);
            mImgView.SetOnTouchListener((IOnTouchListener?)this);
            // Set up renderer.
            mSurfaceView.PreserveEGLContextOnPause = true;
            mSurfaceView.SetEGLContextClientVersion(2);
            mSurfaceView.SetEGLConfigChooser(8, 8, 8, 8, 16, 0); // Alpha used for plane blending.
            mSurfaceView.SetRenderer(this);
            mSurfaceView.RenderMode = Rendermode.Continuously;
        }
        protected override void OnWindowVisibilityChanged([GeneratedEnum] ViewStates visibility)
        {
            if (isDisposed) return;

            base.OnWindowVisibilityChanged(visibility);
        }
        private void CloseWindow(object sender, EventArgs e)
        {
            var activity = _context as Activity; activity.OnBackPressed();
        }
        private void ImageClick(object sender, EventArgs e)
        {
            mImgView.Visibility = ViewStates.Visible;
            mImgClose.Visibility = ViewStates.Visible;
        }
        private void mImgClose_Click(object sender, EventArgs e)
        {
            mImgView.Visibility = ViewStates.Gone;
            mImgClose.Visibility = ViewStates.Gone;
        }
        private void MeasureBtn_Click(object sender, EventArgs e)
        {
            if (LoadSurfacesText.Visibility == ViewStates.Visible) return;
            if (!Element.PlotCarpet) MeasureDistance.Visibility = ViewStates.Visible;
            if (endAnchor != null) { StartMeasure = true; endAnchor = null; AngleBwVectors = 0; }
            else if (initAnchor == null && endAnchor == null) StartMeasure = true;
            else { EndMeasure = true; }
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            try
            {
                GLES20.GlClearColor(0.1f, 0.1f, 0.1f, 1.0f);

                // Create the texture and pass it to ARCore session to be filled during update().
                mBackgroundRenderer.CreateOnGlThread(_context);


                //mPlaneRenderer.CreateOnGlThread(_context, "trigrid.png");
                mPointCloud.CreateOnGlThread(_context);
                mCircleRenderer.CreateOnGLThread();
                mLineRenderer.CreateOnGLThread();
                mBarRenderer.CreateOnGLThread();
                mBandRenderer.CreateOnGLThread();
            }
            catch { return; }
        }


        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            try
            {
                mDisplayRotationHelper.OnSurfaceChanged(width, height);
                GLES20.GlViewport(0, 0, width, height);
                viewWidth = width; viewHeight = height;
            }
            catch { return; }
        }
        public void OnDrawFrame(IGL10 gl)
        {
            // Clear screen to notify driver it should not load any pixels from previous frame.
            GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
            if (mSession == null) return;


            try
            {
                // Obtain the current frame from ARSession. When the configuration is set to
                // UpdateMode.BLOCKING (it is by default), this will throttle the rendering to the
                // camera framerate.
                Google.AR.Core.Frame frame = mSession.Update();
                Camera camera = frame.Camera;



                // Draw background.
                mBackgroundRenderer.Draw(frame);

                // If not tracking, don't draw 3d objects.
                if (camera.TrackingState == TrackingState.Paused)
                {
                    if (LoadSurfacesText.Visibility == ViewStates.Invisible) { showLoading(); }
                    return;
                }
                // Get projection matrix.
                float[] projmtx = new float[16];
                camera.GetProjectionMatrix(projmtx, 0, 0.1f, 100.0f);

                // Get camera matrix and draw.
                float[] viewmtx = new float[16];
                camera.GetViewMatrix(viewmtx, 0);

                // Compute lighting from average intensity of the image.
                var lightIntensity = frame.LightEstimate.PixelIntensity;

                // Visualize tracked points.
                var pointCloud = frame.AcquirePointCloud();
                mPointCloud.Update(pointCloud);
                mPointCloud.Draw(camera.DisplayOrientedPose, viewmtx, projmtx);

                // App is repsonsible for releasing point cloud resources after using it
                pointCloud.Release();

                var planes = new List<Plane>();
                foreach (var p in mSession.GetAllTrackables(Java.Lang.Class.FromType(typeof(Plane))))
                {
                    var plane = (Plane)p;
                    planes.Add(plane);
                }

                // Check if we detected at least one plane. If so, hide the loading message.                
                if (LoadSurfacesText.Visibility == ViewStates.Visible)
                {
                    foreach (var plane in planes)
                    {
                        if ((plane.GetType() == Plane.Type.HorizontalUpwardFacing || plane.GetType() == Plane.Type.Vertical)
                                && plane.TrackingState == TrackingState.Tracking)
                        {
                            LoadSurfacesText.Visibility = ViewStates.Invisible;
                            break;
                        }
                    }
                }

                // Visualize planes.
                //mPlaneRenderer.DrawPlanes(planes, camera.DisplayOrientedPose, projmtx);

                // Visualize Circle at center.
                var frameHit = frame.HitTest(viewWidth / 2, viewHeight / 2);
                Pose circlePoint = null; Pose finalPoint = null; bool IsHorizontal = true;
                if (camera.TrackingState == TrackingState.Tracking)
                {
                    foreach (var hit in frameHit)
                    {
                        circlePoint = hit.HitPose; finalPoint = circlePoint;
                        if (StartMeasure) { initAnchor = hit.CreateAnchor(); StartMeasure = false; }
                        if (EndMeasure) { endAnchor = hit.CreateAnchor(); EndMeasure = false; }
                        foreach (var plane in planes)
                        {
                            if (plane.IsPoseInPolygon(circlePoint) && plane.GetType() == Plane.Type.Vertical) { IsHorizontal = false; break; }
                            else if (plane.IsPoseInPolygon(circlePoint) && plane.GetType() == Plane.Type.HorizontalUpwardFacing) { IsHorizontal = true; break; }
                        }

                        break;
                    }

                }

                if (circlePoint != null)
                {
                    mCircleRenderer.setVerts(circlePoint.Tx(), circlePoint.Ty(), circlePoint.Tz(), "Point", IsHorizontal, 0);
                    mCircleRenderer.DrawCircle(viewmtx, projmtx);
                    mCircleRenderer.setVerts(circlePoint.Tx(), circlePoint.Ty(), circlePoint.Tz(), "Circle", IsHorizontal, 0);
                    mCircleRenderer.DrawCircle(viewmtx, projmtx);
                    if (initAnchor != null)
                    {
                        var initPoint = GetPose(initAnchor);
                        if (endAnchor != null)
                        {
                            var endPoint = GetPose(endAnchor);
                            mCircleRenderer.setVerts(initPoint.Tx(), initPoint.Ty(), initPoint.Tz(), "Point", IsHorizontal, 0);
                            mCircleRenderer.DrawCircle(viewmtx, projmtx);
                            mCircleRenderer.setVerts(endPoint.Tx(), endPoint.Ty(), endPoint.Tz(), "Point", IsHorizontal, 0);
                            mCircleRenderer.DrawCircle(viewmtx, projmtx);
                            mLineRenderer.setVerts(initPoint.Tx(), initPoint.Ty(), initPoint.Tz(), endPoint.Tx(), endPoint.Ty(), endPoint.Tz()); mLineRenderer.setColor(0, 0, 0, 1);
                            mLineRenderer.DrawLine(viewmtx, projmtx);
                            if (Element.PlotCarpet)
                            {
                                if (AngleBwVectors == 0)
                                {
                                    var vectorX = endPoint.Tx() - initPoint.Tx(); var vectorY = initPoint.Tz() - endPoint.Tz();
                                    var dotV = Element.GeomCoor[0] * vectorX + Element.GeomCoor[1] * vectorY;
                                    var MagV1 = Math.Sqrt(Math.Pow(vectorX, 2) + Math.Pow(vectorY, 2)); var MagV2 = Math.Sqrt(Math.Pow(Element.GeomCoor[0], 2) + Math.Pow(Element.GeomCoor[1], 2));
                                    AngleBwVectors = (float)Math.Acos(dotV / (MagV1 * MagV2));
                                    var CrossProduct = vectorX * Element.GeomCoor[1] - vectorY * Element.GeomCoor[0];
                                    if (CrossProduct > 0) AngleBwVectors = -AngleBwVectors;
                                    double barVecX = Element.BarCoords[0][2] - Element.BarCoords[0][0]; double barVecY = Element.BarCoords[0][3] - Element.BarCoords[0][1];
                                    var dotB = barVecX * vectorX + barVecY * vectorY;
                                    var MagBar = Math.Sqrt(Math.Pow(barVecX, 2) + Math.Pow(barVecY, 2));
                                    BarAngle = (float)Math.Acos(dotB / (MagV1 * MagBar));
                                    var sense = vectorX * barVecY - vectorY * barVecX;
                                    if (sense > 0) BarAngle = -BarAngle;
                                    BarAngle = BarAngle + Element.InitAngle;
                                    if (BarAngle < -Math.PI) { BarAngle = BarAngle + 2 * (float)Math.PI; }
                                    if (BarAngle > Math.PI) { BarAngle = BarAngle - 2 * (float)Math.PI; }
                                }

                                //mCircleRenderer.setVerts(initPoint.Tx() + x1, initPoint.Ty(), initPoint.Tz() - x2, "Diameter", false);


                                mCircleRenderer.DrawCircle(viewmtx, projmtx);
                                foreach (var bar in Element.BarCoords)
                                {
                                    float x1 = bar[0] * (float)Math.Cos(AngleBwVectors) - bar[1] * (float)Math.Sin(AngleBwVectors);
                                    float y1 = bar[0] * (float)Math.Sin(AngleBwVectors) + bar[1] * (float)Math.Cos(AngleBwVectors);
                                    float x2 = bar[2] * (float)Math.Cos(AngleBwVectors) - bar[3] * (float)Math.Sin(AngleBwVectors);
                                    float y2 = bar[2] * (float)Math.Sin(AngleBwVectors) + bar[3] * (float)Math.Cos(AngleBwVectors);

                                    switch ((int)(bar[4] * 2000 * Element.ARScale))
                                    {
                                        //case 8: case 6: mBarRenderer.setColor(0, 0, 255, 1); break; //blau
                                        case 8: case 6: mBarRenderer.setColor(0, 0, 1, 1); break; //blau
                                        //case 10: mBarRenderer.setColor(255, 0, 255, 1); break;  //magenta pink
                                        case 10: mBarRenderer.setColor(1, 0, 1, 1); break;  //magenta pink
                                        //case 12: mBarRenderer.setColor(0, 255, 255, 1); break; //cyan
                                        case 12: mBarRenderer.setColor(0, 1, 1, 1); break; //cyan
                                        //case 14: mBarRenderer.setColor(139, 69, 19, 1); break; //Sattelbraun
                                        case 14: mBarRenderer.setColor(0.55f, 0.27f, 0.075f, 1); break; //Sattelbraun
                                        //case 16: mBarRenderer.setColor(138, 43, 226, 1); break;  //blue violet
                                        case 16: mBarRenderer.setColor(0.54f, 0.17f, 0.89f, 1); break;  //blue violet
                                        //case 18: mBarRenderer.setColor(210, 50, 10, 1); break;// chocolate
                                        case 18: mBarRenderer.setColor(0.82f, 0.2f, 0.04f, 1); break;// chocolate
                                        //case 20: mBarRenderer.setColor(63, 81, 51, 1); break; //light seagreen
                                        case 20: mBarRenderer.setColor(0.25f, 0.32f, 0.2f, 1); break; //light seagreen
                                        //case 22: mBarRenderer.setColor(255, 127, 80, 1); break; //coral                                            
                                        case 22: mBarRenderer.setColor(1, 0.5f, 0.31f, 1); break; //coral
                                        //case int n when n >= 22: mBarRenderer.setColor(128, 128, 0, 1); break; //Olive
                                        case int n when n >= 22: mBarRenderer.setColor(0.5f, 0.5f, 0, 1); break; //Olive
                                    }
                                    mBarRenderer.setVerts(initPoint.Tx() + x1, initPoint.Ty() + bar[4], initPoint.Tz() - y1, initPoint.Tx() + x2, initPoint.Ty() + bar[4], initPoint.Tz() - y2, bar[4], BarAngle, 0);
                                    mBarRenderer.DrawCylinder(viewmtx, projmtx);
                                    mBarRenderer.setVerts(initPoint.Tx() + x1, initPoint.Ty() + bar[4], initPoint.Tz() - y1, initPoint.Tx() + x2, initPoint.Ty() + bar[4], initPoint.Tz() - y2, bar[4], BarAngle, 1);
                                    mBarRenderer.DrawCylinder(viewmtx, projmtx);
                                    //mBarRenderer.setVerts(initPoint.Tx() + x1, initPoint.Ty() + 0.006f, initPoint.Tz() - y1, initPoint.Tx() + x2, initPoint.Ty() + 0.006f, initPoint.Tz() - y2, BarAngle, 2); mBarRenderer.setColor(0, 0, 0, 1);
                                    //mBarRenderer.DrawCylinder(viewmtx, projmtx);
                                    //mBarRenderer.setVerts(initPoint.Tx() + x1, initPoint.Ty() + 0.006f, initPoint.Tz() - y1, initPoint.Tx() + x2, initPoint.Ty() + 0.006f, initPoint.Tz() - y2, BarAngle, 3); mBarRenderer.setColor(0, 0, 0, 1);
                                    //mBarRenderer.DrawCylinder(viewmtx, projmtx);
                                    //mBarRenderer.setVerts(initPoint.Tx() + x1, initPoint.Ty() + 0.006f, initPoint.Tz() - y1, initPoint.Tx() + x2, initPoint.Ty() + 0.006f, initPoint.Tz() - y2, BarAngle, 4); mBarRenderer.setColor(0, 0, 0, 1);
                                    //mBarRenderer.DrawCylinder(viewmtx, projmtx);
                                }
                                foreach (var band in Element.BandCoords)
                                {
                                    float x1 = band[0] * (float)Math.Cos(AngleBwVectors) - band[1] * (float)Math.Sin(AngleBwVectors);
                                    float y1 = band[0] * (float)Math.Sin(AngleBwVectors) + band[1] * (float)Math.Cos(AngleBwVectors);
                                    float x2 = band[2] * (float)Math.Cos(AngleBwVectors) - band[3] * (float)Math.Sin(AngleBwVectors);
                                    float y2 = band[2] * (float)Math.Sin(AngleBwVectors) + band[3] * (float)Math.Cos(AngleBwVectors);
                                    mBandRenderer.setVerts(initPoint.Tx() + x1, initPoint.Ty(), initPoint.Tz() - y1, initPoint.Tx() + x2, initPoint.Ty(), initPoint.Tz() - y2, (float)Math.PI - BarAngle, Element.ARScale); mBandRenderer.setColor(128, 128, 128, 1);
                                    mBandRenderer.DrawBand(viewmtx, projmtx);
                                }
                                mImageRenderer.CreateOnGlThread(_context, Element.RolloutImage); mImageRenderer.setVerts(Element.RolloutBox, initPoint, AngleBwVectors);
                                mImageRenderer.DrawImage(viewmtx, projmtx);
                                float p1x = Element.EdgeToCarpDim[0] * (float)Math.Cos(AngleBwVectors) - Element.EdgeToCarpDim[1] * (float)Math.Sin(AngleBwVectors);
                                float p1y = Element.EdgeToCarpDim[0] * (float)Math.Sin(AngleBwVectors) + Element.EdgeToCarpDim[1] * (float)Math.Cos(AngleBwVectors);
                                float p2x = Element.EdgeToCarpDim[2] * (float)Math.Cos(AngleBwVectors) - Element.EdgeToCarpDim[3] * (float)Math.Sin(AngleBwVectors);
                                float p2y = Element.EdgeToCarpDim[2] * (float)Math.Sin(AngleBwVectors) + Element.EdgeToCarpDim[3] * (float)Math.Cos(AngleBwVectors);
                                mLineRenderer.setVerts(initPoint.Tx() + p1x, initPoint.Ty(), initPoint.Tz() - p1y, initPoint.Tx() + p2x, initPoint.Ty(), initPoint.Tz() - p2y); mLineRenderer.setColor(0, 0, 0, 1);
                                mLineRenderer.DrawLine(viewmtx, projmtx, 8.0f);
                                mImageRenderer.CreateOnGlThread(_context, Element.EdgeToCarpDimImage); mImageRenderer.setVerts(Element.EdgeToCarpDimBox, initPoint, AngleBwVectors);
                                mImageRenderer.DrawImage(viewmtx, projmtx);
                            }
                        }
                        else
                        {
                            mCircleRenderer.setVerts(initPoint.Tx(), initPoint.Ty(), initPoint.Tz(), "Point", IsHorizontal, 0);
                            mCircleRenderer.DrawCircle(viewmtx, projmtx);
                            mLineRenderer.setVerts(initPoint.Tx(), initPoint.Ty(), initPoint.Tz(), finalPoint.Tx(), finalPoint.Ty(), finalPoint.Tz()); mLineRenderer.setColor(0, 0, 0, 1);
                            mLineRenderer.DrawLine(viewmtx, projmtx);

                        }
                    }
                }
                if (initAnchor != null)
                {
                    var initPoint = GetPose(initAnchor);
                    if (endAnchor != null)
                    {
                        var endPoint = GetPose(endAnchor);
                        if (!Element.PlotCarpet) showDistance(CalculateDistance(initPoint, endPoint));
                    }
                    else
                    {
                        if (!Element.PlotCarpet) showDistance(CalculateDistance(initPoint, finalPoint));
                    }
                }

            }
            catch (Exception ex)
            {
                // Avoid crashing the application due to unhandled exceptions.
                Log.Error("Renderer", "Exception on the OpenGL thread", ex.Message);
            }
        }


        private string CalculateDistance(Pose pose0, Pose pose1)
        {
            float dx = pose0.Tx() - pose1.Tx();
            float dy = pose0.Ty() - pose1.Ty();
            float dz = pose0.Tz() - pose1.Tz();
            return Math.Round(Math.Sqrt(dx * dx + dz * dz + dy * dy), 2).ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        private void showDistance(string distance)
        {
            var activity = _context as Activity;
            activity.RunOnUiThread(() =>
            {
                try { MeasureDistance.Text = distance + " m"; }
                catch { return; }
            });
        }
        private void showLoading()
        {
            var activity = _context as Activity;
            activity.RunOnUiThread(() =>
            {
                try { LoadSurfacesText.Visibility = ViewStates.Visible; }
                catch { return; }
            });
        }

        private Pose GetPose(Anchor anchor)
        {
            Pose pose = anchor.Pose;
            pose.GetTranslation(mPoseTranslation, 0);
            pose.GetRotationQuaternion(mPoseRotation, 0);
            return new Pose(mPoseTranslation, mPoseRotation);
        }


        public bool OnTouch(View v, MotionEvent e)
        {
            mScaleGesture.OnTouchEvent(e);
            mGestureDetector.OnTouchEvent(e);
            if (e.PointerCount == 2)
            {
                midPoint.X = (e.GetX(0) + e.GetX(1)) / 2;
                midPoint.Y = (e.GetY(0) + e.GetY(1)) / 2;
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (isDisposed) return;
                mDisplayRotationHelper.OnPause(); mSession.Pause();
                if (mImgView != null) mImgView.Dispose();
                if (midPoint != null) Dispose();
                if (LoadSurfacesText != null) LoadSurfacesText.Dispose();
                //if (_context != null) _context.Dispose(); //Not working (maybe disposed internally)
                if (mSurfaceView != null) mSurfaceView.Dispose();
                if (mSession != null) mSession.Dispose();
                if (initAnchor != null) Dispose();
                if (endAnchor != null) Dispose();
                if (mBackgroundRenderer != null) mBackgroundRenderer.Dispose();
                if (mPointCloud != null) mPointCloud.Dispose();
                if (mCircleRenderer != null) mCircleRenderer.Dispose();
                if (mLineRenderer != null) mLineRenderer.Dispose();
                if (mBarRenderer != null) mBarRenderer.Dispose();
                if (mBandRenderer != null) mBandRenderer.Dispose();
                if (mImageRenderer != null) mImageRenderer.Dispose();
                if (mDisplayRotationHelper != null) mDisplayRotationHelper.Dispose();
                if (MeasureBtn != null) MeasureBtn.Dispose();
                if (MeasureDistance != null) MeasureDistance.Dispose();
                if (mImgBtn != null) mImgBtn.Dispose();
                if (mImgClose != null) mImgClose.Dispose();
                if ((IViewManager)Rview.Parent != null) ((IViewManager)Rview.Parent).RemoveView(Rview);
                if (Rview != null) Rview.Dispose();
                isDisposed = true;
                base.Dispose(disposing);
            }
            catch { return; }
        }

        private class SimpleOnScaleGestureListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            public override bool OnScale(ScaleGestureDetector detector)
            {
                mScaleFactor *= detector.ScaleFactor;
                mScaleFactor = Math.Max(0.1f, Math.Min(mScaleFactor, 10.0f));
                //mImgView.ScaleX = mScaleFactor;
                //mImgView.ScaleY = mScaleFactor;
                var ImgMatrix = mImgView.ImageMatrix;
                ImgMatrix.SetScale(mScaleFactor, mScaleFactor, midPoint.X, midPoint.Y);
                mImgView.ImageMatrix = ImgMatrix;
                return true;
            }

        }
        private class PanGestureListener : GestureDetector.SimpleOnGestureListener
        {
            public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
            {
                mImgView.ScrollBy((int)distanceX, (int)distanceY);
                return true;
            }
        }
    }


}