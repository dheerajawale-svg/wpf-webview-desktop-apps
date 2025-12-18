using System;
using System.Windows;
using DesktopHostWpf.Services;
using Microsoft.Web.WebView2.Core;

namespace DesktopHostWpf;

public partial class MainWindow : Window
{
    private readonly InMemoryAuthService _auth = new();
    private WebMessageRouter? _router;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await WebView.EnsureCoreWebView2Async();

            var core = WebView.CoreWebView2;
#if DEBUG
            core.Settings.AreDevToolsEnabled = true;
#else
            core.Settings.AreDevToolsEnabled = false;
#endif
            core.Settings.IsStatusBarEnabled = false;

            _router = new WebMessageRouter(core, _auth);
            _router.Initialize();

            var wwwRoot = WwwRootResolver.Resolve();
            core.SetVirtualHostNameToFolderMapping(
                hostName: "app",
                folderPath: wwwRoot,
                accessKind: CoreWebView2HostResourceAccessKind.Allow);

            WebView.Source = new Uri("https://app/index.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
