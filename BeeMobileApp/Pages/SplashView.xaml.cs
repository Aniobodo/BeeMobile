using BeeMobileApp.Classes;

namespace BeeMobileApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplashView : ContentPage
    {
        public SplashView()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Waiting.IsVisible = false;
            LeftPane.EditControl(width: App.ScreenWidth * 0.25); RightPane.EditControl(width: App.ScreenWidth * 0.5); StartImage.EditControl(width: App.ScreenWidth * 0.5);
            await RightPane.TranslateTo(App.ScreenWidth * 0.5f, 0, 1500);

            Waiting.IsVisible = true; await Globals.Wait1MicSec();

            if (Preferences.ContainsKey("ID") && Preferences.ContainsKey("Pass")) //Check if the login data is saved? If so then sign in automatically
            {
                App.TcpClient.ClientRefresh();
                App.TcpClient.UserSignIn(Generic.GetID(), Generic.GetPassword());

                if (App.TcpClient.Valid) await Navigation.PushAsync(new HomePage());
                else await Navigation.PushAsync(new MainPage());
            }
            else await Navigation.PushAsync(new MainPage());

            RightPane.TranslationX = 0;
        }
    }
}