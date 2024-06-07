using BeeAPPLIB;
using BeeMobileApp.Classes;
using CommunityToolkit.Maui.Views;
using Preferences = Microsoft.Maui.Storage.Preferences;



namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            InfoBtn.EditControl(App.ScreenWidth / 8, App.ScreenWidth / 8, App.ScreenWidth / 16);
            ViewVideoBtn.EditControl(App.ScreenWidth / 8, App.ScreenWidth / 8, App.ScreenWidth / 16);
            SiteRankingBtn.EditControl(App.ScreenWidth / 8, App.ScreenWidth / 8, App.ScreenWidth / 16);
            GameRankingBtn.EditControl(App.ScreenWidth / 8, App.ScreenWidth / 8, App.ScreenWidth / 16);
            ProjectBtn.EditControl(App.ScreenWidth / 3, App.ScreenWidth / 3, App.ScreenWidth / 6);
            GameBtn.EditControl(App.ScreenWidth / 3, App.ScreenWidth / 3, App.ScreenWidth / 6);
            ProfileBtn.EditControl(App.ScreenWidth / 8, App.ScreenWidth / 8, App.ScreenWidth / 16);
            VideoFrame.EditControl(App.ScreenHeight * 0.8, App.ScreenWidth * 0.8, 25);
            CloseVideo.EditControl(App.ScreenHeight / 10, App.ScreenHeight / 10);

            /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
            Vor:
                        Globals.ProfileImage = App.FtpDBConn.DownloadBytes(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt);
            Nach:
                        Globals.ProfileImage = AppHelpers.FtpDBConn.DownloadBytes(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt);
            */
            Globals.ProfileImage = global::AppHelpers.FtpDBConn.DownloadBytes(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt);
        }

        private async void Logout_Clicked(object sender, EventArgs e)   //Triggers when logout on toolbar is clicked. Logout user and delete the user info from device
        {

            // TODO Xamarin.Forms.Application.Properties is not longer supported. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#other-changes
            Preferences.Remove("ID"); // TODO Xamarin.Forms.Application.Properties is not longer supported. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#other-changes
            Preferences.Remove("Pass");
            await SecureStorage.SetAsync("ID", "Pass");
            int length = Navigation.NavigationStack.Count - 2;
            for (int i = 0; i < length; i++)
            {
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
            }
            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
            App.TcpClient.DisConnect();
        }

        protected override void OnAppearing()   //triggers when the page is resumed. When the user lands on this page
        {
            if (Globals.ProfileImage != null || Globals.ProfileImage.Length > 0) { ProfileBtn.Source = ImageSource.FromStream(() => new MemoryStream(Globals.ProfileImage)); ProfileBtn.BackgroundColor = Colors.White; }
            else { ProfileBtn.Source = "ProfileIcon.png"; }
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
            base.OnAppearing(); 
        }
        private async void ProjectBtn_Clicked(object sender, EventArgs e)
        {
            //Globals.GameOrSite = Constants.SiteStr;
            //using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
            //await Navigation.PushAsync(new ProjectList(false));
        }

        private async void InfoBtn_Clicked(object sender, EventArgs e)
        {
            //using WaitPopup wpop = new (); this.ShowPopup(wpop);
            //await Globals.Wait1MicSec(); Globals.GameOrSite = Constants.SiteStr;
            //await Navigation.PushAsync(new ProjectList(true));
        }

        private async void ProfileBtn_Clicked(object sender, EventArgs e)
        {
            using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
            await Navigation.PushAsync(new UserProfile());
        }

        private async void GameBtn_Clicked(object sender, EventArgs e)
        {
            //Globals.GameOrSite = Constants.GameStr;
            //using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
            //await Navigation.PushAsync(new ProjectList(false));
        }

        private async void WebsiteLogo_Clicked(object sender, EventArgs e)  
        {
            
        }

        private async void SiteRankingBtn_Clicked(object sender, EventArgs e)
        {
            //using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec(); Globals.GameOrSite = Constants.SiteStr;
            //await Navigation.PushAsync(new RankingList());
        }

        private async void GameRankingBtn_Clicked(object sender, EventArgs e)
        {
            //using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec(); Globals.GameOrSite = Constants.GameStr;
            //Globals.ProjName = "GP02_BeeApp Game Project"; //Ranking is only for this game for now
            //await Navigation.PushAsync(new RankingList());
        }
        private async void ViewVideoBtn_Clicked(object sender, EventArgs e)    //Shows the video on How to use BeeAPP.
        {
           
        }
        private void CloseVideo_Clicked(object sender, EventArgs e)
        {
            BeeAPPVideo.ShouldShowPlaybackControls = false;
            BeeAPPVideo.Stop();
            VideoGrid.IsVisible = false;
        }
    }
}