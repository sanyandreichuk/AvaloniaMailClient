using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MailClient.ViewModels;
using System;
using System.ComponentModel;
using System.Text;
using Xilium.CefGlue.Avalonia;

namespace MailClient.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            UpdateBrowserContent("<center>Fill required fields and click the 'Start' button to download emails. </center>");
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is MainWindowViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is MainWindowViewModel vm)
            {
                if (e.PropertyName == nameof(vm.Content))
                {
                    UpdateBrowserContent(vm.Content);
                }
            }
        }

        private void UpdateBrowserContent(string content)
        {
            var browser = this.FindControl<AvaloniaCefBrowser>("Browser");
            browser.Address = "data:text/html;charset=utf-8;base64," + Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(content));
        }
    }
}
