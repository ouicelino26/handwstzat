namespace HandWStat
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            ConfigureAndroidWebViewScrolling();
        }

        private void ConfigureAndroidWebViewScrolling()
        {
#if ANDROID
            blazorWebView.HandlerChanged += (_, _) =>
            {
                if (blazorWebView.Handler?.PlatformView is Android.Webkit.WebView webView)
                {
                    webView.VerticalScrollBarEnabled = true;
                    webView.HorizontalScrollBarEnabled = true;
                    webView.OverScrollMode = Android.Views.OverScrollMode.IfContentScrolls;
                    AndroidX.Core.View.ViewCompat.SetNestedScrollingEnabled(webView, true);
                }
            };
#endif
        }
    }
}
