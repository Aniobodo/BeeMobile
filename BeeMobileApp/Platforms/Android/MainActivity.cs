using Android.App;
using Android.Content;
using Android.Content.PM;
//using Plugin.Media;
using Android.OS;
using Android.Runtime;
using Plugin.Media;
using System.IO.Compression;

namespace BeeMobileApp.Platforms.Android
{
    [Activity(Label = "BeeAPP", Icon = "@mipmap/BeeAppIcon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter([Intent.ActionSend], Categories = [Intent.CategoryDefault], DataMimeType = "application/*")]
    [IntentFilter([Intent.ActionSendMultiple], Categories = [Intent.CategoryDefault], DataMimeType = "application/*")]
    public class MainActivity : MauiAppCompatActivity
    {
        App MainForm;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            //TabLayoutResource = _Microsoft.Android.Resource.Designer.ResourceConstant.Layout.Tabbar;
            //ToolbarResource = _Microsoft.Android.Resource.Designer.ResourceConstant.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            await CrossMedia.Current.Initialize();
            //VideoPlayerRenderer.Init();

            Platform.Init(this, savedInstanceState);




            MainForm = new App();
            //LoadApplication(MainForm);
            ScreenDimensions();

            if (Intent.Action == Intent.ActionSend)
            {

                // Get the info from ClipData 
                var zip = Intent.ClipData.GetItemAt(0); var UnzipStream = ContentResolver.OpenInputStream(zip.Uri);
                // Save it over 
                MemoryStream memOfFile = new MemoryStream(); UnzipStream.CopyTo(memOfFile);
                List<byte[]> DataBytes = [];
                var tempbyte = memOfFile.ToArray();
                if (MainForm.IsBeeFile(tempbyte))
                {
                    DataBytes.Add(tempbyte);
                }
                else
                {
                    try
                    {
                        var zipArchive = new ZipArchive(memOfFile, ZipArchiveMode.Read);
                        foreach (ZipArchiveEntry entry in zipArchive.Entries)
                        {
                            byte[] buffer = new byte[entry.Length];
                            entry.Open().Read(buffer);
                            if (MainForm.IsBeeFile(buffer))
                            {
                                DataBytes.Add(buffer);
                            }
                        }
                    }
                    catch { }
                }
                MainForm.SendData(DataBytes);

            }
            else if (Intent.Action == Intent.ActionSendMultiple)
            {

                // Get the info from ClipData 
                int count = Intent.ClipData.ItemCount;
                List<byte[]> DataBytes = new List<byte[]>();
                for (int i = 0; i < count; i++)
                {
                    var pxml = Intent.ClipData.GetItemAt(i);
                    var PxmlStream = ContentResolver.OpenInputStream(pxml.Uri);
                    // Save it over 
                    var memOfFile = new MemoryStream(); PxmlStream.CopyTo(memOfFile);
                    var tempbyte = memOfFile.ToArray();
                    if (MainForm.IsBeeFile(tempbyte))
                    {
                        DataBytes.Add(tempbyte);
                    }
                    else
                    {
                        try
                        {
                            var zipArchive = new ZipArchive(memOfFile, ZipArchiveMode.Read);
                            foreach (ZipArchiveEntry entry in zipArchive.Entries)
                            {
                                byte[] buffer = new byte[entry.Length];
                                entry.Open().Read(buffer);
                                if (MainForm.IsBeeFile(buffer))
                                {
                                    DataBytes.Add(buffer);
                                }
                            }
                        }
                        catch { }
                    }
                }
                MainForm.SendData(DataBytes);
            }

            else
            {
                //var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

                ////This creates the full file path to your "testfile.txt" file.
                //var filePath = Path.Combine(documentsPath, "testfile.txt");

                ////Now create the file.
                //File.Create(filePath);
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ScreenDimensions()
        {
            var pixels = Resources.DisplayMetrics.WidthPixels;
            App.WidthPixels = pixels;
            var scale = Resources.DisplayMetrics.Density;
            var dps = (double)((pixels - 0.5f) / scale);
            var width = (int)dps;
            App.ScreenWidth = width;
            pixels = Resources.DisplayMetrics.HeightPixels;
            App.HeightPixels = pixels;
            dps = (double)((pixels - 0.5f) / scale);
            var height = (int)dps;
            App.ScreenHeight = height;
            
        }
    }
}