using BeeAPPLIB;
using BeeMobileApp.Classes;
using System.Diagnostics;

namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataUpload : ContentPage
    {
        public DataUpload(List<byte[]> data)
        {
            InitializeComponent();
            ShowProgress.WidthRequest = App.ScreenWidth * 0.75f;
            Send(data);
        }
        protected override async void OnAppearing()     //triggers when the page is resumed. When the user lands on this page
        {
            await Globals.Wait1MicSec();
            base.OnAppearing();
        }
        private async void Send(List<byte[]> DataList)
        {
            if (DataList.Count == 0)
            {
                await DisplayAlert("No files", "No bee files were found", "Ok"); Process.GetCurrentProcess().Kill();
            }
            DeviceDisplay.KeepScreenOn = true;
            WaitStack.IsVisible = false; LoadStack.IsVisible = true; await Globals.Wait1MicSec();
            var upload = new UploadFiles(DataList[0]);      //Initialise the UploadFiles class from BeeAPPLIB. This class is used to upload the data to server
            var isconnected = upload.Connect(Generic.GetID(), Generic.GetPassword(), Constants.IPfromOutside, Constants.PortFromOutside);   //Connect to the server
            if (isconnected)
            {
                int ind = 1;
                foreach (var data in DataList)
                {
                    Sending.Text = string.Format("Sending {0} of {1} ...", ind, DataList.Count);
                    upload.AddFile(data); //Adds a bee file to collection for upload
                    double progress = (double)ind / DataList.Count;
                    await ShowProgress.ProgressTo(progress, 10, Easing.Linear);
                    ind++; await Globals.Wait1MicSec();
                }
                WaitStack.IsVisible = true; LoadStack.IsVisible = false; await Globals.Wait1MicSec();
                var IsSent = upload.Send(out string error);
                if (!IsSent) await DisplayAlert("Error", error, "Ok");
            }
            else
            {
                await DisplayAlert("Error", BeeMobileApp.Res.AppResources.FirstSignin, "Ok");
            }

            /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
            Vor:
                        upload.Disconnect(); App.TcpClient.DisConnect(); App.FtpDBConn.DisConnect(); //Disconnect from all servers
            Nach:
                        upload.Disconnect(); App.TcpClient.DisConnect(); AppHelpers.FtpDBConn.DisConnect(); //Disconnect from all servers
            */
            upload.Disconnect(); App.TcpClient.DisConnect(); global::AppHelpers.FtpDBConn.DisConnect(); //Disconnect from all servers
            DeviceDisplay.KeepScreenOn = false;
            Process.GetCurrentProcess().Kill();
        }
    }
}