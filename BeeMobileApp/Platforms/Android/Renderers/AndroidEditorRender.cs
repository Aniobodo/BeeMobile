using Android.Content;
using Android.Graphics.Drawables;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Color = Android.Graphics.Color;



namespace BeeMobileApp.Platforms.Android.Renderers
{

    public class AndroidEditorRender : EditorRenderer
    {
        public AndroidEditorRender(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
            GradientDrawable gd = new GradientDrawable();
            gd.SetCornerRadius(50); gd.SetStroke(2, Color.ParseColor("#FF000000"));
            Control.SetBackground(gd);
        }
    }
}