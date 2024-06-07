using BeeAPPLIB;
using BeeMobileApp.Classes;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Xml;



namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductionAndLogistics : ContentPage
    {
        public class StrucMemInfo
        {
            public string StrucMemName { get; set; }  //Structural member name
            public ObservableCollection<BindingProp> ProductionLogisticInfo { get; set; }
        }
        public class BindingProp
        {
            public string Carpet { get; set; }          //Carpet name
            public ImageSource ImagePL { get; set; }    //Image of the selected option. ProductionIcon.png or LogisticIcon.png
            public double PLFrameCorner { get; set; }   //Corner radius of image frame
            public double PLFrameHeight { get; set; }   //Height of the image frame or row.
            public double PLPadding { get; set; }       //Image frame padding size. This is filled with the color
            public Color BackColor { get; set; }        //Color of the brim of the image frame. red if it is not scanned, green if scanned.
        }
        private string ProdLog { get; set; } = null;
        public ProductionAndLogistics(string prodlog)
        {
            InitializeComponent();
            CloseBtn.EditControl(App.ScreenWidth / 10, App.ScreenWidth / 10);
            ScanBtn.EditControl(App.ScreenWidth / 6, App.ScreenWidth / 6, App.ScreenWidth / 12);
            ImgSuccess.EditControl(App.ScreenWidth / 2, App.ScreenWidth / 2); ImgSuccess.IsVisible = false;
            FlashBtn.EditControl(App.ScreenWidth / 6, App.ScreenWidth / 6, App.ScreenWidth / 12);
            TextBtn.EditControl(App.ScreenWidth / 6, App.ScreenWidth / 6, App.ScreenWidth / 12);
            ProdLog = prodlog;
            try
            {
                App.TcpClient.ClientRefresh();
                App.TcpClient.ProductionLogisticQR("info", Globals.ProjName, null);
                var doc = new XmlDocument(); doc.LoadXml(App.TcpClient.ReturnData);
                PopulateOverview(doc);
            }
            catch { Globals.IsLoaded = false; }
        }
        private void PopulateOverview(XmlDocument doc)    //Polpulate the list of carpets with respect to the scanned results.
        {
            var strucmem_nodes = doc.SelectNodes("/XML/StructuralMember");
            var StucMemList = new ObservableCollection<StrucMemInfo>();
            foreach (XmlNode strucmem_node in strucmem_nodes)
            {
                var Carpetlist = new ObservableCollection<BindingProp>();
                foreach (XmlNode carpetnode in strucmem_node.SelectNodes("Carpet"))
                {
                    ImageSource image = ProdLog == Constants.Production ? "ProductionIcon.png" : "LogisticIcon.png";
                    var color = carpetnode.GetAtrrib(ProdLog) == "1" ? Color.FromArgb("#029939") : Color.FromArgb("#DC0D15");
                    Carpetlist.Add(new BindingProp { Carpet = carpetnode.GetAtrrib("Name"), ImagePL = image, PLFrameCorner = App.ScreenWidth, PLFrameHeight = App.ScreenWidth / 4, PLPadding = App.ScreenWidth / 40, BackColor = color });
                }
                StucMemList.Add(new StrucMemInfo { StrucMemName = strucmem_node.GetAtrrib("Name"), ProductionLogisticInfo = Carpetlist });
            }
            OverViewList.ItemsSource = null; OverViewList.ItemsSource = StucMemList;
        }
        protected override void OnAppearing()   //triggers when the page is resumed. When the user lands on this page
        {
            if (!Globals.IsLoaded) { Globals.IsLoaded = true; App.LoadPageAgain(new ProductionAndLogistics(ProdLog)); return; }
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
            base.OnAppearing();
        }
        private async void TextBtn_Clicked(object sender, EventArgs e)
        {
            var qrtext = await DisplayPromptAsync(Res.AppResources.CarpetNr, null, "Ok", null);
            if (string.IsNullOrWhiteSpace(qrtext)) return;
            AnalyseQR(qrtext.ToUpper().Replace(',', '.').Trim() + " " + Globals.ProjName.Split('_')[0]);
        }
        private async void AnalyseQR(string qrtext)
        {
            try
            {
                using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
                App.TcpClient.ProductionLogisticQR(ProdLog, Globals.ProjName, qrtext);
                /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
                if (App.TcpClient.Valid)
                {
                    var doc = new XmlDocument(); doc.LoadXml(App.TcpClient.ReturnData);
                    PopulateOverview(doc);
                    ImgSuccess.IsVisible = true; await ImgSuccess.RotateTo(360, 1000, Easing.SpringIn); await ImgSuccess.ScaleTo(0, 500);   //Animation of bee star
                    ImgSuccess.IsVisible = false; ImgSuccess.Rotation = 0; ImgSuccess.Scale = 1;
                    ScanView.IsVisible = false; QRScanner.IsDetecting = false; await Globals.Wait1MicSec();
                }
                else { await DisplayAlert("Error!", Res.AppResources.CarpetNotAvailable, "Ok"); QRScanner.IsDetecting = true; }
            }
            catch { await DisplayAlert("Error!", null, "Ok"); /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/ QRScanner.IsDetecting = true; }
        }
        private void CloseBtn_Clicked(object sender, EventArgs e)
        {
            ScanView.IsVisible = false; QRScanner.IsDetecting = false;
            if (QRScanner.IsTorchOn) { QRScanner.IsTorchOn = false; FlashBtn.BackgroundColor = Color.FromArgb("#9D9D9C"); }
        }

        private void FlashBtn_Clicked(object sender, EventArgs e)
        {
            QRScanner.IsTorchOn = true;
            if (QRScanner.IsTorchOn) FlashBtn.BackgroundColor = Color.FromArgb("#029939");
            else FlashBtn.BackgroundColor = Color.FromArgb("#9D9D9C");
        }

        private async void ScanBtn_Clicked(object sender, EventArgs e)
        {
            if (!await Generic.CameraPermission()) return;
            QRScanner.IsDetecting = true; ScanView.IsVisible = true;
        }

        private void QRScanner_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    QRScanner.IsDetecting = false;
                    App.TcpClient.ClientRefresh();
                    string result = e.Results.ToString().Split(' ')[1];
                    if (Globals.ProjName.Split('_')[0] == result)
                    {
                        AnalyseQR(result);
                    }
                    else { await DisplayAlert("Error!", Res.AppResources.WrongProject, "Ok"); QRScanner.IsDetecting = true; }
                }
                catch { await DisplayAlert("Error!", null, "Ok"); /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/ QRScanner.IsDetecting = true; }
            });
        }

    }
}