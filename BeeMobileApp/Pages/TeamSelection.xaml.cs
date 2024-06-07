using BeeAPPLIB;
using BeeMobileApp.Classes;
using CommunityToolkit.Maui.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Reflection;
using System.Xml;

namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TeamSelection : ContentPage
    {
        public List<UserList> ProjPartners = new List<UserList>();
        public List<OldTeamList> OldTeamMembers = new List<OldTeamList>();
        public class UserList
        {
            public bool UserSelected { get; set; }          //Checkbox bool for selection
            public string UserName { get; set; }            //Name of the user
            public string UserEmail { get; set; }           //Email of the user
        }
        public class OldTeamList
        {
            public string TeamIndex { get; set; }           //Just an index
            public string TeamMember { get; set; }          //Name of the team member
        }
        public TeamSelection()
        {
            InitializeComponent();
            CranePersonal.EditControl(App.ScreenWidth / 3, App.ScreenWidth / 3, App.ScreenWidth / 6);
            InstallationPersonal.EditControl(App.ScreenWidth / 3, App.ScreenWidth / 3, App.ScreenWidth / 6);

            TeamFrame.EditControl(App.ScreenHeight * 0.75, App.ScreenWidth * 0.8);
            RejectBtn.EditControl(width: App.ScreenHeight / 10);
            TeamImg.EditControl(App.ScreenWidth / 12); BtmStack.EditControl(App.ScreenHeight / 10);
            CameraBtn.EditControl(width: App.ScreenHeight / 10, cornerradius: App.ScreenHeight / 20);
            SaveBtn.EditControl(width: App.ScreenHeight / 10, cornerradius: App.ScreenHeight / 20);

            OldTeamFrame.EditControl(App.ScreenHeight * 0.75, App.ScreenWidth * 0.8);
            Saving.EditControl(width: App.ScreenWidth / 2); NTBtnStack.EditControl(App.ScreenHeight / 10); PrevTeamName.EditControl(App.ScreenWidth / 10);
            NewTeam.EditControl(width: App.ScreenHeight / 10, cornerradius: App.ScreenHeight / 20);
            ContinueTeam.EditControl(width: App.ScreenHeight / 10, cornerradius: App.ScreenHeight / 20);

            try
            {
                App.TcpClient.ClientRefresh();
                App.TcpClient.TeamInfo(Globals.ProjName, Globals.StrucMemName, Constants.GetInfo); // Get the team information
                if (string.IsNullOrWhiteSpace(App.TcpClient.ReturnData)) return;
                var doc = new XmlDocument(); doc.LoadXml(App.TcpClient.ReturnData);
                var action = doc.DocRoot().SelectSingleNode("Action").InnerText;     //Node "Action" has the information about team. See the server application for details                                

                if (action == "0")
                {
                    var team = Globals.ProjName.Split('_')[0] + "_" + Globals.StrucMemName.Split('_')[0] + "_Team"; //Deafult team name "ProjName_StrucMemName_Team"
                    var assembly = GetType().GetTypeInfo().Assembly;
                    byte[] buffer;
                    //If the image needs to be converted to byte[] data then it has to be stored in the shared project. In this case the directory that is mentioned.
                    //After storing select the image and go to properties on right bottom and in Build Action select Embedded resource
                    using (Stream s = assembly.GetManifestResourceStream("BeeMobileApp.Media.TeamImageIcon.png"))   //Default team image
                    {
                        buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                    }

                    /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
                    Vor:
                                        App.FtpDBConn.CreateDir(FtpLinks.TeamImagesPath + Globals.ProjName);        //This will create a proj directory on ftp server if not available
                                        _ = Task.Run(() => App.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer)); //Save team image on ftp server
                    Nach:
                                        AppHelpers.FtpDBConn.CreateDir(FtpLinks.TeamImagesPath + Globals.ProjName);        //This will create a proj directory on ftp server if not available
                                        _ = Task.Run(() => AppHelpers.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer)); //Save team image on ftp server
                    */
                    global::AppHelpers.FtpDBConn.CreateDir(FtpLinks.TeamImagesPath + Globals.ProjName);        //This will create a proj directory on ftp server if not available
                    _ = Task.Run(() => global::AppHelpers.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer)); //Save team image on ftp server

                    OldTeam.IsVisible = false; TeamView.IsVisible = false; NavigationPage.SetHasBackButton(this, false);

                    foreach (XmlNode partners in doc.SelectNodes("/XML/UAP/User"))
                    {
                        ProjPartners.Add(new UserList { UserName = partners.GetAtrrib("Name"), UserEmail = partners.GetAtrrib("Email"), UserSelected = false });
                    }
                    CheckListView.ItemsSource = ProjPartners;
                }
                else if (action == "1") { OldTeam.IsVisible = false; TeamView.IsVisible = true; TeamList.IsVisible = false; }
                else if (action == "2")
                {
                    OldTeam.IsVisible = true; TeamView.IsVisible = false; TeamList.IsVisible = false;
                    PrevTeamName.Text = doc.SelectSingleNode("/XML/Team").GetAtrrib("Name");
                    int index = 1; OldTeamMembers.Clear();
                    foreach (var member in doc.SelectSingleNode("/XML/Team").GetAtrrib("Members").Split('|', StringSplitOptions.RemoveEmptyEntries))
                    {
                        OldTeamMembers.Add(new OldTeamList { TeamIndex = index.ToString(), TeamMember = member });
                        index++;
                    }
                    TeamMemberList.ItemsSource = OldTeamMembers;
                }
                else { Globals.IsLoaded = false; }
            }
            catch { Globals.IsLoaded = false; }
        }
        protected override void OnAppearing()   //triggers when the page is resumed. When the user lands on this page
        {
            if (!Globals.IsLoaded) { Globals.IsLoaded = true; App.LoadPageAgain(new TeamSelection()); return; }
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
            base.OnAppearing();
        }

        private async void CranePersonal_Clicked(object sender, EventArgs e)
        {
            //using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
            //await Navigation.PushAsync(new SiteCrane());
        }

        private async void InstallationPersonal_Clicked(object sender, EventArgs e)
        {
            //using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
            //await Navigation.PushAsync(new GeomtryAndNavi());
        }

        private void RejectBtn_Clicked(object sender, EventArgs e)      //If the user selects not to add team information
        {
            TeamView.IsVisible = true; TeamList.IsVisible = false; NavigationPage.SetHasBackButton(this, true); OldTeam.IsVisible = false;
        }
        private async void CameraBtn_Clicked(object sender, EventArgs e)    //Take a team photo
        {
            if (!await Generic.CameraPermission()) return;
            NavigationPage.SetHasBackButton(this, true); TeamView.IsVisible = true; TeamList.IsVisible = false; OldTeam.IsVisible = false;
            string selection = "";
            foreach (var user in ProjPartners)
            {
                if (user.UserSelected) selection += user.UserEmail + ";";
            }
            var capturephoto = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { AllowCropping = true, PhotoSize = PhotoSize.Custom, CustomPhotoSize = 15, DefaultCamera = CameraDevice.Front });
            if (capturephoto == null) return;
            byte[] buffer = default;
            using (StreamReader stream = new StreamReader(capturephoto.AlbumPath))
            {
                using var memstream = new MemoryStream();
                stream.BaseStream.CopyTo(memstream); buffer = memstream.ToArray();
            }
            File.Delete(capturephoto.AlbumPath);
            string team = string.IsNullOrEmpty(TeamName.Text) ? Globals.ProjName.Split('_')[0] + "_" + Globals.StrucMemName.Split('_')[0] + "_Team" : TeamName.Text; //Deafult team name "ProjName_StrucMemName_Team"


            /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
            Vor:
                        bool imageuploaded = App.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer);
                        if (!imageuploaded) { Generic.ShowFTPAlert(); App.FtpDBConn.DeleteFile(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt); return; }
            Nach:
                        bool imageuploaded = AppHelpers.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer);
                        if (!imageuploaded) { Generic.ShowFTPAlert(); AppHelpers.FtpDBConn.DeleteFile(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt); return; }
            */
            bool imageuploaded = global::AppHelpers.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer);
            if (!imageuploaded) { Generic.ShowFTPAlert(); global::AppHelpers.FtpDBConn.DeleteFile(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt); return; }

            App.TcpClient.ClientRefresh();
            App.TcpClient.TeamInfo(Globals.ProjName, Globals.StrucMemName, TeamName.Text + "<Team>" + selection); // Set the team Info

            //Animation of saving icon
            Saving.IsVisible = true; await Saving.RotateTo(-30, 75); await Saving.RotateTo(30, 75); await Saving.RotateTo(-20, 75); await Saving.RotateTo(20, 75);
            await Saving.RotateTo(-10, 75); await Saving.RotateTo(10, 75); Saving.Rotation = 0; await Saving.ScaleTo(0, 50); Saving.IsVisible = false; Saving.Scale = 1;
        }
        private async void SaveBtn_Clicked(object sender, EventArgs e)      //Save the team info
        {
            string selection = "";
            foreach (var user in ProjPartners)
            {
                if (user.UserSelected) selection += user.UserEmail + ";";
            }
            if (string.IsNullOrWhiteSpace(TeamName.Text)) { await DisplayAlert("Team Name", "Please enter team name", "Ok"); return; }
            NavigationPage.SetHasBackButton(this, true); TeamView.IsVisible = true; TeamList.IsVisible = false; OldTeam.IsVisible = false;
            App.TcpClient.ClientRefresh();
            App.TcpClient.TeamInfo(Globals.ProjName, Globals.StrucMemName, TeamName.Text + "<Team>" + selection);   //Set the team Info
            Saving.IsVisible = true; await Saving.RotateTo(-30, 75); await Saving.RotateTo(30, 75); await Saving.RotateTo(-20, 75); await Saving.RotateTo(20, 75);
            await Saving.RotateTo(-10, 75); await Saving.RotateTo(10, 75); Saving.Rotation = 0; await Saving.ScaleTo(0, 50); Saving.IsVisible = false; Saving.Scale = 1;
        }

        private async void NewTeam_Clicked(object sender, EventArgs e)  //Make a new team
        {
            try
            {
                var team = Globals.ProjName.Split('_')[0] + "_" + Globals.StrucMemName.Split('_')[0] + "_Team"; //Deafult team name "ProjName_StrucMemName_Team"
                var assembly = GetType().GetTypeInfo().Assembly;
                byte[] buffer;
                //If the image needs to be converted to byte[] data then it has to be stored in the shared project. In this case the directory that is mentioned.
                //After storing select it and go to properties on right bottom and in Build Action select Embedded resource
                using (Stream s = assembly.GetManifestResourceStream("BeeMobileApp.Media.TeamImageIcon.png"))   //Default team image
                {
                    buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                }

                /* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
                Vor:
                                _ = Task.Run(() => App.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer));
                Nach:
                                _ = Task.Run(() => AppHelpers.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer));
                */
                _ = Task.Run(() => global::AppHelpers.FtpDBConn.UploadData(FtpLinks.TeamImagesPath + Globals.ProjName + "/" + team + Constants.ImageExt, buffer));

                using WaitPopup wpop = new (); this.ShowPopup(wpop); await Globals.Wait1MicSec();
                App.TcpClient.ClientRefresh();
                App.TcpClient.TeamInfo(Globals.ProjName, Globals.StrucMemName, Constants.NewTeam); // Set the team Info 
                var doc = new XmlDocument(); doc.LoadXml(App.TcpClient.ReturnData);
                var Users = doc.SelectNodes("/XML/UAP/User");
                foreach (XmlNode user in Users)
                {
                    ProjPartners.Add(new UserList { UserName = user.GetAtrrib("Name"), UserEmail = user.GetAtrrib("Email"), UserSelected = false });
                }
                CheckListView.ItemsSource = ProjPartners;
                OldTeam.IsVisible = false; TeamView.IsVisible = false; TeamList.IsVisible = true; NavigationPage.SetHasBackButton(this, false); /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
            }
            catch { /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/ await DisplayAlert("Error!", null, "Ok"); }
        }

        private void ContinueTeam_Clicked(object sender, EventArgs e)   //Continue with the same team
        {
            App.TcpClient.ClientRefresh();
            App.TcpClient.TeamInfo(Globals.ProjName, Globals.StrucMemName, Constants.SameTeam); // Set the team Info
            TeamView.IsVisible = true; TeamList.IsVisible = false; OldTeam.IsVisible = false;

        }

        private void CheckListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var partner = e.Item as UserList;
            foreach (var user in ProjPartners)
            {
                if (user.UserEmail == partner.UserEmail) { user.UserSelected = !user.UserSelected; break; }
            }
            CheckListView.ItemsSource = null; CheckListView.ItemsSource = ProjPartners;
        }

    }
}