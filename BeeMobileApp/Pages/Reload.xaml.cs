using BeeMobileApp.Classes;
using CommunityToolkit.Maui.Views;


namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Reload : Popup
    {
        public bool DoReload { get; set; }
        public bool WaitforSelect { get; set; }
        public Reload()
        {
            InitializeComponent();
            ErrorFrame.EditControl(App.ScreenHeight / 6, App.ScreenWidth * 0.8, App.ScreenWidth / 24, 10);
            ClosePage.EditControl(App.ScreenWidth / 6, App.ScreenWidth / 6);
            ReloadPage.EditControl(App.ScreenWidth / 6, App.ScreenWidth / 6, App.ScreenWidth / 3);
        }

        private void ClosePage_Clicked(object sender, System.EventArgs e)
        {
            DoReload = false; WaitforSelect = false;
            Close(null);
        }

        private void ReloadPage_Clicked(object sender, System.EventArgs e)
        {
            DoReload = true; WaitforSelect = false;
            Close(null);
        }
    }
}