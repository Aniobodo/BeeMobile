using BeeAPPLIB;
using BeeMobileApp.Classes;
using CommunityToolkit.Maui.Views;
using System.Net.Mail;
using System.Reflection;
using Preferences = Microsoft.Maui.Storage.Preferences;

namespace BeeMobileApp.Pages
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            MainLayout.EditControl(width: App.ScreenWidth * 0.8); LoginLayout.EditControl(width: App.ScreenWidth * 0.8); RegisterLayout.EditControl(width: App.ScreenWidth * 0.8);
            FPLayout.EditControl(width: App.ScreenWidth * 0.8); Emailing.EditControl(width: App.ScreenWidth / 2);
            FPLayout.IsVisible = false; LoginLayout.IsVisible = false; RegisterLayout.IsVisible = false;
        }
        protected override void OnAppearing()   //triggers when the page is resumed. When the user lands on this page
        {
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
            base.OnAppearing();
        }
        private async void SignIn_Tapped(object sender, EventArgs e)
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                using WaitPopup wpop = new (); this.ShowPopup(wpop);  await Globals.Wait1MicSec();
                App.TcpClient.ClientRefresh();
                App.TcpClient.UserSignIn(EmailUser.Text.Trim(), PasswordUser.Text);
                if (true||App.TcpClient.Valid)
                {
                    LoginDetails(EmailUser.Text, PasswordUser.Text);
                    App.TcpClient.ClientToServerConnection();
                    await Navigation.PushAsync(new HomePage());
                }
                else
                {
                    await DisplayAlert(Res.AppResources.IncorrectEm, Res.AppResources.Incorrect, Res.AppResources.Again);
                }
            }
            else await DisplayAlert(Res.AppResources.TitleNoNet, Res.AppResources.CheckNet, "Ok");
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
        }
        private async void SignUp_Tapped(object sender, EventArgs e)
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
                    var email = new MailAddress(EmailRegister.Text.Trim()).Address;
                    App.TcpClient.ClientRefresh();
                    App.TcpClient.Register(Constants.Register, email);// Register the user on the server                    
                    /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
                    if (App.TcpClient.Valid)
                    {
                        var assembly = GetType().GetTypeInfo().Assembly;
                        byte[] buffer;
                        //If the image needs to be converted to byte[] data then it has to be stored in the shared project. In this case the directory that is mentioned.
                        //After storing select it and go to properties on right bottom and in Build Action select Embedded resource
                        using (Stream s = assembly.GetManifestResourceStream("BeeMobileApp.Media.ProfileIcon.png"))     //Default profile image
                        {
                            buffer = new byte[s.Length];
                            s.Read(buffer, 0, buffer.Length);
                        }
                        Globals.ProfileImage = buffer;

                        /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
                        Vor:
                                                _ = Task.Run(() => App.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + email.Trim() + Constants.ImageExt, buffer));
                        Nach:
                                                _ = Task.Run(() => AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + email.Trim() + Constants.ImageExt, buffer));
                        */
                        _ = Task.Run(() => global::AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + email.Trim() + Constants.ImageExt, buffer));
                        Emailing.IsVisible = true; await Emailing.ScaleTo(1.2, 500); await Emailing.ScaleTo(0, 250); Emailing.Scale = 1; Emailing.IsVisible = false;
                        await DisplayAlert(Res.AppResources.RegSuccess, null, "Ok");
                        MainLayout.IsVisible = true; RegisterLayout.IsVisible = false; EmailRegister.Text = string.Empty;
                    }
                    else { await DisplayAlert(Res.AppResources.IncorrectEm, Res.AppResources.EmailExist, Res.AppResources.Again); }
                }
                catch { await DisplayAlert("Invalid email", "Please provide a valid email", "Ok"); /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/ }
            }
            else
            {
                await DisplayAlert(Res.AppResources.TitleNoNet, Res.AppResources.CheckNet, "Ok");
            }
        }

        private async void LoginDetails(string IDUser, string pass) //Save the user login details on the device. Automatic login.
        {
            // TODO Xamarin.Forms.Application.Properties is not longer supported. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#other-changes
            //Application.Current.Properties["ID"] = IDUser.Trim();

            // TODO Xamarin.Forms.Application.Properties is not longer supported. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#other-changes
            Preferences.Set(IDUser.Trim(), pass);
            await SecureStorage.SetAsync("ID", "Pass");
        }

        private void ForgotPassword_Tapped(object sender, EventArgs e)
        {
            FPLayout.IsVisible = true; LoginLayout.IsVisible = false;
        }
        private async void SendPassword_Tapped(object sender, EventArgs e)
        {
            try
            {
                var email = new MailAddress(EmailPass.Text.Trim()).Address;
                App.TcpClient.ClientRefresh();
                App.TcpClient.Register(Constants.Resend, email);// Register the user on the server
                if (App.TcpClient.Valid)
                {
                    await DisplayAlert(Res.AppResources.PassSent, Res.AppResources.PassSentDetail, "Ok");
                    FPLayout.IsVisible = false; LoginLayout.IsVisible = true;
                }
                else await DisplayAlert(Res.AppResources.FalseEmail, null, "Ok");
            }
            catch { await DisplayAlert(Res.AppResources.FalseEmail, null, "Ok"); }
        }

        private void SignInBtn_Clicked(object sender, EventArgs e)
        {
            MainLayout.IsVisible = false; LoginLayout.IsVisible = true;
        }

        private void SignUpBtn_Clicked(object sender, EventArgs e)
        {
            MainLayout.IsVisible = false; RegisterLayout.IsVisible = true;
        }

        private void CancelLogin_Tapped(object sender, EventArgs e)
        {
            MainLayout.IsVisible = true; LoginLayout.IsVisible = false;
        }

        private void CancelRegister_Tapped(object sender, EventArgs e)
        {
            MainLayout.IsVisible = true; RegisterLayout.IsVisible = false;
        }

        private void CancelFP_Tapped(object sender, EventArgs e)
        {
            FPLayout.IsVisible = false; LoginLayout.IsVisible = true;
        }
    }
}
