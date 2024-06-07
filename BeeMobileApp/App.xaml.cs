using BeeAPPLIB;
using BeeMobileApp.Classes;
using BeeMobileApp.Pages;
using CommunityToolkit.Maui.Views;
using System.Xml;
using Constants = BeeAPPLIB.Constants;
using MainPage = BeeMobileApp.Pages.MainPage;

namespace BeeMobileApp
{
    public partial class App : Application
    {
        public static int ScreenHeight, ScreenWidth, HeightPixels, WidthPixels;
        //static WaitPopup _IsWaiting;//23.04.2023
        //public static WaitPopup IsWaiting;                             //This is the loading popup page
        //{ get { _IsWaiting ??= new(); return _IsWaiting; } set => _IsWaiting = value; }//Als Property ab 23.04.2023
        public static MobileClient TcpClient = new MobileClient();      //Initialisation of MobileClient class
        public static FtpDB FtpDBConn = new FtpDB();

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new SplashView()) { BarBackgroundColor = Color.FromArgb("#c7c7c7"), BarTextColor = Colors.White }; //Sets the background color and text color for each page//Only top bar properties           
        }

        protected async override void OnStart()     //This method is triggered when the app starts
        {
            //IsWaiting = new WaitPopup(); 
            await TcpClient.ConnectClient();
        }

        protected override void OnSleep()   //This method is triggered when the app goes in the background of the device
        {
            TcpClient.DisConnect(); 
            FtpDBConn.DisConnect();
        }

        protected async override void OnResume()        //This method is triggered when the app is focused or resumes in the device
        {
            await TcpClient.ConnectClient();
            if (!FtpDBConn.IsConnected) FtpDBConn.Connect();
        }

        public async void SendData(List<byte[]> data)       //share data to the app. more info in DataUpload page
        {
            await MainPage.Navigation.PushAsync(new DataUpload(data));
        }

        public bool IsBeeFile(byte[] data)       //Checks if the file in byte[] format is a bee file. geometry(.poly) or pxml(.PXML)
        {
            try
            {
                XmlDocument doc = new XmlDocument(); doc.Load(new MemoryStream(data));
                return doc.DocumentElement.Name == Constants.PXML_Document || doc.DocumentElement.Name == Constants.Geom_Document;
            }
            catch { return false; }
        }
        public static async void LoadPageAgain(Page page)
        {
            await Task.Delay(500);
            using WaitPopup wpop = new(); Current.MainPage.ShowPopup(wpop); await Globals.Wait1MicSec();//Current.MainPage.ShowPopup(IsWaiting); await Globals.Wait1MicSec();
            await Current.MainPage.Navigation.PopAsync();
            
            var reload = new Reload() { WaitforSelect = true };
            Current.MainPage.ShowPopup(reload);
            while (reload.WaitforSelect) { await Task.Delay(100); }
            if (reload.DoReload) await Current.MainPage.Navigation.PushAsync(page);
        }
    }
}
