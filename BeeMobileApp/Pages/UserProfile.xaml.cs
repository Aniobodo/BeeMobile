using BeeAPPLIB;
using BeeMobileApp.Classes;
using CommunityToolkit.Maui.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserProfile : ContentPage
    {
        public UserProfile()
        {
            InitializeComponent(); PasswordChange.IsVisible = false; PhotoOpt.IsVisible = false; AvatarGrid.IsVisible = false;
            GalleryBtn.EditControl(App.ScreenWidth / 4, App.ScreenWidth / 4, App.ScreenWidth / 8);
            CameraBtn.EditControl(App.ScreenWidth / 4, App.ScreenWidth / 4, App.ScreenWidth / 8);
            AvatarBtn.EditControl(App.ScreenWidth / 4, App.ScreenWidth / 4, App.ScreenWidth / 8);
            DeleteBtn.EditControl(App.ScreenWidth / 4, App.ScreenWidth / 4, App.ScreenWidth / 8);
            ProfileImg.EditControl(App.ScreenWidth / 5, App.ScreenWidth / 5, App.ScreenWidth / 10);
            ProfileGrid.Width = App.ScreenWidth / 5; Saving.EditControl(width: App.ScreenWidth / 2);

            Avatar1.EditControl(App.ScreenHeight / 3, App.ScreenWidth / 3, 25); Avatar2.EditControl(App.ScreenHeight / 3, App.ScreenWidth / 3, 25);
            Avatar3.EditControl(App.ScreenHeight / 3, App.ScreenWidth / 3, 25); Avatar4.EditControl(App.ScreenHeight / 3, App.ScreenWidth / 3, 25);
            //If the image is an embedded resource in a shared project then this is how it can be assigned.
            Avatar1.Source = ImageSource.FromResource("BeeMobileApp.Media.Avatar1.png"); Avatar2.Source = ImageSource.FromResource("BeeMobileApp.Media.Avatar2.png");
            Avatar3.Source = ImageSource.FromResource("BeeMobileApp.Media.Avatar3.png"); Avatar4.Source = ImageSource.FromResource("BeeMobileApp.Media.Avatar4.png");

            List<string> countries = new List<string>();
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);    //Get the country names
            foreach (var culture in cultures)
            {
                RegionInfo region = new RegionInfo(culture.LCID);
                if (!countries.Contains(region.EnglishName)) countries.Add(region.EnglishName);
            }
            countries.Sort(); CountryPicker.ItemsSource = countries;
            try
            {
                App.TcpClient.ClientRefresh();
                App.TcpClient.Register(Constants.GetInfo, Generic.GetID());// Get the user info from the server

                var doc = new XmlDocument(); doc.LoadXml(App.TcpClient.ReturnData);
                var usernode = doc.SelectSingleNode("/XML/User");
                var fullname = usernode.GetAtrrib("Name"); var country = usernode.GetAtrrib("Country");
                if (!string.IsNullOrEmpty(fullname)) { var name = fullname.Split(' '); FirstName.Text = name[0]; LastName.Text = name[1]; }
                if (!string.IsNullOrEmpty(country)) CountryPicker.SelectedItem = country;
                FirmName.Text = usernode.GetAtrrib("FirmName");
                var path = FileSystem.AppDataDirectory; path = Path.Combine(path, Generic.GetID() + Constants.ImageExt);
                if (Globals.ProfileImage != null) { var MemStm = new MemoryStream(Globals.ProfileImage); ProfilePhoto.Source = ImageSource.FromStream(() => MemStm); }
            }
            catch { Globals.IsLoaded = false; }
        }

        protected override void OnAppearing()       //triggers when the page is resumed. When the user lands on this page
        {
            if (!Globals.IsLoaded) { Globals.IsLoaded = true; App.LoadPageAgain(new UserProfile()); return; }
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
            base.OnAppearing();
        }
        private async void SaveInfo_Tapped(object sender, EventArgs e)
        {
            bool TryAgain = false; string Validator;
            if (CountryPicker.SelectedItem == null) { CountryPicker.TextColor = Color.FromArgb("#DC0D15"); TryAgain = true; }
            Validator = @"^(\s*[A-Za-z]*\s*)$";
            if (string.IsNullOrWhiteSpace(FirstName.Text) || !Regex.IsMatch(FirstName.Text, Validator)) { FirstName.TextColor = Color.FromArgb("#DC0D15"); TryAgain = true; }
            if (string.IsNullOrWhiteSpace(LastName.Text) || !Regex.IsMatch(LastName.Text, Validator)) { LastName.TextColor = Color.FromArgb("#DC0D15"); TryAgain = true; }

            if (TryAgain) return;
            using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
            App.TcpClient.ClientRefresh();
            App.TcpClient.Register(Constants.Settings, Generic.GetID() + ";" + FirstName.Text + ";" + LastName.Text + ";" + CountryPicker.SelectedItem.ToString() + ";" + FirmName.Text);// Register the user info on the server
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/

            Saving.IsVisible = true; await Saving.RotateTo(-30, 75); await Saving.RotateTo(30, 75); await Saving.RotateTo(-20, 75); await Saving.RotateTo(20, 75);
            await Saving.RotateTo(-10, 75); await Saving.RotateTo(10, 75); Saving.Rotation = 0; await Saving.ScaleTo(0, 50); Saving.IsVisible = false; Saving.Scale = 1;
            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
        }

        private void CountryPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            CountryPicker.TextColor = Colors.Black;
        }

        private void FirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            FirstName.TextColor = Colors.Black;
        }

        private void LastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LastName.TextColor = Colors.Black;
        }
        private async void GalleryBtn_Clicked(object sender, EventArgs e)
        {
            if (!await Generic.MediaPermission()) return;
            byte[] buffer = default;
            var picphoto = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions { PhotoSize = PhotoSize.Custom, CustomPhotoSize = 15 }); PhotoOpt.IsVisible = false;
            if (picphoto == null) return;
            Stream stream = picphoto.GetStream();
            using (var memstream = new MemoryStream())
            {
                stream.CopyTo(memstream); buffer = memstream.ToArray();
            }
            File.Delete(picphoto.Path); Globals.ProfileImage = buffer;
            MemoryStream MemStm = new MemoryStream(buffer);
            ProfilePhoto.Source = ImageSource.FromStream(() => MemStm);

            /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
            Vor:
                        _ = Task.Run(() => App.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
            Nach:
                        _ = Task.Run(() => AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
            */
            _ = Task.Run(() => global::AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
        }

        private async void CameraBtn_Clicked(object sender, EventArgs e)
        {
            if (!await Generic.CameraPermission()) return;
            byte[] buffer = default;
            var capturephoto = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { AllowCropping = true, DefaultCamera = CameraDevice.Front, PhotoSize = PhotoSize.Custom, CustomPhotoSize = 15 }); PhotoOpt.IsVisible = false;
            if (capturephoto == null) return;
            using (StreamReader stream = new StreamReader(capturephoto.AlbumPath))
            {
                using var memstream = new MemoryStream();
                stream.BaseStream.CopyTo(memstream); buffer = memstream.ToArray();
            }
            File.Delete(capturephoto.AlbumPath); Globals.ProfileImage = buffer;
            var MemStm = new MemoryStream(Globals.ProfileImage); ProfilePhoto.Source = ImageSource.FromStream(() => MemStm);

            /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
            Vor:
                        _ = Task.Run(() => App.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
            Nach:
                        _ = Task.Run(() => AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
            */
            _ = Task.Run(() => global::AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
        }
        private void DeleteBtn_Clicked(object sender, EventArgs e)
        {
            PhotoOpt.IsVisible = false; Globals.ProfileImage = null;
            ProfilePhoto.Source = "ProfileIcon.png";

            /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
            Vor:
                        App.FtpDBConn.DeleteFile(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt);
            Nach:
                        AppHelpers.FtpDBConn.DeleteFile(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt);
            */
            global::AppHelpers.FtpDBConn.DeleteFile(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt);
        }
        private void ChangePassword_Tapped(object sender, EventArgs e)
        {
            PasswordChange.IsVisible = true; DetailStack.IsVisible = false;
            CurrentPassword.Text = ""; CurrentPassword.TextColor = Colors.Black;
            NewPassword.Text = ""; NewPassword.TextColor = Colors.Black;
            RepeatPassword.Text = ""; RepeatPassword.TextColor = Colors.Black;
        }
        private void CancelBtn_Tapped(object sender, EventArgs e)
        {
            PasswordChange.IsVisible = false; DetailStack.IsVisible = true;
        }
        private async void SavePassword_Tapped(object sender, EventArgs e)
        {
            string RegExp = "^(?=.*?[a-z])(?=.*?[0-9]).{8,}$";
            if (string.IsNullOrWhiteSpace(NewPassword.Text) || !Regex.IsMatch(NewPassword.Text, RegExp)) { NewPassword.TextColor = Color.FromArgb("#DC0D15"); return; }
            if (string.IsNullOrWhiteSpace(RepeatPassword.Text) || NewPassword.Text != RepeatPassword.Text) { RepeatPassword.TextColor = Color.FromArgb("#DC0D15"); return; }

            App.TcpClient.ClientRefresh();
            App.TcpClient.Register(Constants.Password, Generic.GetID() + ";" + CurrentPassword.Text + ";" + NewPassword.Text);  // Register the user info on the server

            if (App.TcpClient.Valid)
            {
                Saving.IsVisible = true; await Saving.RotateTo(-30, 75); await Saving.RotateTo(30, 75); await Saving.RotateTo(-20, 75); await Saving.RotateTo(20, 75);
                await Saving.RotateTo(-10, 75); await Saving.RotateTo(10, 75); Saving.Rotation = 0; await Saving.ScaleTo(0, 50); Saving.IsVisible = false; Saving.Scale = 1;
                PasswordChange.IsVisible = false; DetailStack.IsVisible = true;
            }
            else { CurrentPassword.TextColor = Color.FromArgb("#DC0D15"); await DisplayAlert("False", "Incorrect Password", "Ok"); }
        }
        private void CurrentPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentPassword.TextColor = Colors.Black;
        }

        private void NewPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewPassword.TextColor = Colors.Black;
        }

        private void RepeatPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            RepeatPassword.TextColor = Colors.Black;
        }

        private void PassInfoBtn_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Password", "Must contain atleast 8 charachters with atleast 1 capital letter, 1 small letter and 1 number", "Ok");
        }

        private void OptTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            PhotoOpt.IsVisible = false;
        }

        private void ProfilePhoto_Clicked(object sender, EventArgs e)
        {
            PhotoOpt.IsVisible = true;
        }


        private void AvatarBtn_Clicked(object sender, EventArgs e)
        {
            AvatarGrid.IsVisible = true;
        }
        private void AvatarTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            AvatarGrid.IsVisible = false;
        }
        private void Avatar_Clicked(object sender, EventArgs e)
        {
            var btn = (ImageButton)sender;
            var id = "BeeMobileApp.Media.Avatar" + btn.ClassId + ".png";
            var assembly = GetType().GetTypeInfo().Assembly;
            byte[] buffer;
            //If the image needs to be converted to byte[] data then it has to be stored in the shared project. In this case the directory that is mentioned.
            //After storing select it and go to properties on right bottom and in Build Action select Embedded resource
            using (Stream s = assembly.GetManifestResourceStream(id))
            {
                buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
            }
            Globals.ProfileImage = buffer;
            var MemStm = new MemoryStream(Globals.ProfileImage); ProfilePhoto.Source = ImageSource.FromStream(() => MemStm);

            /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
            Vor:
                        _ = Task.Run(() => App.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
            Nach:
                        _ = Task.Run(() => AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
            */
            _ = Task.Run(() => global::AppHelpers.FtpDBConn.UploadData(FtpLinks.ProfileImagePath + Generic.GetID() + Constants.ImageExt, buffer));
            AvatarGrid.IsVisible = false; PhotoOpt.IsVisible = false;
        }
    }
}