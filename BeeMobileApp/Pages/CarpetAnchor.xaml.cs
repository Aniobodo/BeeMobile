using BamtecMobileApp.Classes;
using CommunityToolkit.Maui.Views;
using SkiaScene;
using SkiaScene.TouchManipulation;
using SkiaSharp;
using System.Xml;

namespace BamtecMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CarpetAnchor : ContentPage
    {
        private float Ewidth { get; set; }
        private float CofGravity { get; set; }
        public CarpetAnchor()
        {
            InitializeComponent();
            SlingDocBtn.EditControl(App.ScreenWidth / 6, App.ScreenWidth / 6, App.ScreenWidth / 12);
            topstack.EditControl(App.ScreenWidth / 4); bottomstack.EditControl(App.ScreenWidth / 4);
            try
            {
                App.TcpClient.ClientRefresh();
                App.TcpClient.Anchorage(Globals.ProjName, Globals.StrucMemName, InfoGraphics.ActiveCarpet, Globals.GameOrSite); // Get the anchor points of the carpet                
                var doc = new XmlDocument(); doc.LoadXml(App.TcpClient.ReturnData);
                Ewidth = int.TryParse(doc.SelectSingleNode("/XML/Width").InnerText, out int width) ? width : 0;
                CofGravity = int.TryParse(doc.SelectSingleNode("/XML/CofG").InnerText, out int CofG) ? CofG : 0;
            }
            catch { Globals.IsLoaded = false; }
        }
        protected override void OnAppearing()   //triggers when the page is resumed. When the user lands on this page
        {
            if (!Globals.IsLoaded) { Globals.IsLoaded = true; App.LoadPageAgain(new CarpetAnchor()); return; }
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
            base.OnAppearing();
        }

        private ISKScene _scene;                                    //Initialisation of skia scene

        private ITouchGestureRecognizer _touchGestureRecognizer;    //Used to recognize the type of touch from the user

        private ISceneGestureResponder _sceneGestureResponder;      //This will handle the touch event properties such as speed or scale.

        private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)    //This will trigger as soon as this page is loaded
        {
            if (_scene == null) InitSceneObjects();
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;   //get the canvas to edit from the CanvasView paint surface 
            _scene.Render(canvas);          //This will trigger the Render method overriden in the class assigned in the initialisation of _scene(SKScene)
        }

        private void CanvasView_TouchAction(object sender, TouchTracking.TouchActionEventArgs args)    //Triggers everytime the user touches the screen. Gets the location of the touch and process the touch event 
        {
            var viewPoint = args.Location;

            SKPoint point = new SKPoint((float)(CanvasView.CanvasSize.Width * viewPoint.X / CanvasView.Width),
                                        (float)(CanvasView.CanvasSize.Height * viewPoint.Y / CanvasView.Height));
            var actionType = args.Type;
            _touchGestureRecognizer.ProcessTouchEvent(args.Id, actionType, point);
        }

        private void InitSceneObjects()     //Initialisation of the skia scene _scene(SKScene).
        {
            _scene = new SKScene(new RenderAnchorage(Ewidth, CofGravity))   //Initialise the SKScene with the the desire render class. All the rederisation will be done in this class
            {
                MaxScale = 1000,
                MinScale = 0.1f,
            };

            SetSceneCenter();
            _touchGestureRecognizer = new TouchGestureRecognizer();

            _sceneGestureResponder = new SceneGestureRenderingResponder(() => CanvasView.InvalidateSurface(), _scene, _touchGestureRecognizer)

            {

                TouchManipulationMode = TouchManipulationMode.IsotropicScale,

                MaxFramesPerSecond = 10,

            };

            _sceneGestureResponder.StartResponding();
        }

        private void SetSceneCenter()   //Get the center of the device(on which the the BamAPP is being used)
        {
            if (_scene == null) return;

            var centerPoint = new SKPoint(CanvasView.CanvasSize.Width / 2, CanvasView.CanvasSize.Height / 2);
            _scene.ScreenCenter = centerPoint;
            InfoGraphics.InitialCenter = centerPoint;
        }

        private void SlingDocBtn_Clicked(object sender, EventArgs e)
        {
            SlingsOpt.IsVisible = true;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            SlingsOpt.IsVisible = false;
        }

        private async void Sling_Selected(object sender, EventArgs e)
        {
            using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
            var btn = (ImageButton)sender;
            var sltd = string.Empty;
            switch (btn.ClassId)         //The app will show the pdf document from pfeifer according to the sling selected. 
            {
                case "0":
                    sltd = "https://landings.pfeifer.info/fileadmin/user_upload/portal/Lifting/betriebsanleitungen/BA_Anschlagseile_Grummet_Kabelschlag_2012-12.pdf";
                    break;
                case "1":
                    sltd = "https://landings.pfeifer.info/fileadmin/user_upload/portal/Lifting/betriebsanleitungen/428665_BA_Polytex_Hebebaender_2021-12.pdf";
                    break;
                case "2":
                    sltd = "https://landings.pfeifer.info/fileadmin/user_upload/portal/Lifting/betriebsanleitungen/442582_BA_Rundschlingen_2021-12.pdf";
                    break;
                case "3":
                    sltd = "https://landings.pfeifer.info/fileadmin/user_upload/portal/Lifting/betriebsanleitungen/371781_BA_Anschlagketten_2021-12.pdf";
                    break;
                case "4":
                    sltd = "https://landings.pfeifer.info/fileadmin/user_upload/portal/Lifting/betriebsanleitungen/BA_Drahtseil_Ausgleichs_2021-12.pdf";
                    break;
            }
            var httpclient = new HttpClient();
            var bytes = await httpclient.GetByteArrayAsync(sltd);   //Get the file from internet
            var doc = Path.Combine(FileSystem.CacheDirectory, "Pfeifer.pdf");
            await File.WriteAllBytesAsync(doc, bytes);      //Save the file locally
            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(doc) }); //Open the file
            SlingsOpt.IsVisible = false;
        }
    }
}