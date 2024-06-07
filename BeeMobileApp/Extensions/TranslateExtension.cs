using System.Reflection;
using System.Resources;


namespace BeeMobileApp.Extensions
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        private const string ResourceID = "BeeMobileApp.Res.AppResources";
        private static readonly Lazy<ResourceManager> resMng = new Lazy<ResourceManager>(() => new ResourceManager(ResourceID, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public string Text { get; set; }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null) return string.Empty;

            var ci = Thread.CurrentThread.CurrentCulture;
            var translate = resMng.Value.GetString(Text, ci);
            if (translate == null) return Text;
            return translate;
        }
    }
}
