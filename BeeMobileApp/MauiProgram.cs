using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;

namespace BeeMobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .UseSkiaSharp()
            .UseMauiCommunityToolkitMediaElement();
//            .ConfigureMauiHandlers(handlers =>
//            {
//#if ANDROID
//                        handlers.AddHandler(GLSurfaceView, BeeMobileApp.Platforms.Android.Resources.AndroidARRenderer);
//#elif IOS
//                        handlers.AddHandler(typeof(IOSEditorRender), typeof(BeeMobileApp.Platforms.iOS.Renderers.IOSEditorRender));
//#endif
//            });

        return builder.Build();
    }
}
