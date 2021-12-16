using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MailClient.Models;
using MailClient.Services;
using MailClient.ViewModels;
using MailClient.Views;

namespace MailClient
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(EmailServiceFactory.GetEmailService),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }


    }
}
