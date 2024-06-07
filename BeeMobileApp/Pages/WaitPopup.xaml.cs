using CommunityToolkit.Maui.Views;

namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WaitPopup : Popup,IDisposable
    {
        bool isdisposed;
        public WaitPopup()
        {
            InitializeComponent();
            Size = new Size(App.ScreenWidth / 5, App.ScreenWidth / 5); CenterImage.HeightRequest = App.ScreenWidth / 9;
        }

        public void Dispose() //23.05.2024
        {
            if (isdisposed) { return; }
            Close(); isdisposed = true;
        }
        //public void Dispose() => Close();//23.04.2024
        //public new async void Close(object? result = null)
        //{
        //    if(isdisposed) { return; }
        //    base.Close(result);
        //    isdisposed = true;
        //}
    }
}