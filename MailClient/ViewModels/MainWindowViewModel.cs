using DynamicData;
using DynamicData.Binding;
using MailClient.Models;
using MailClient.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace MailClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ServerType serverType = ServerType.IMAP;
        public ServerType ServerType
        {
            get => serverType;
            set => this.RaiseAndSetIfChanged(ref serverType, value);
        }

        private Encryption encryption = Encryption.SSL_TLS;
        public Encryption Encryption
        {
            get => encryption;
            set => this.RaiseAndSetIfChanged(ref encryption, value);
        }

        private string server = "imap.gmail.com";
        public string Server
        {
            get => server;
            set => this.RaiseAndSetIfChanged(ref server, value);
        }

        private int port = 993;
        public int Port
        {
            get => port;
            set => this.RaiseAndSetIfChanged(ref port, value);
        }

        private string username = string.Empty;
        public string Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
        }

        private string password = string.Empty;
        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        public SourceCache<EmailEnvelop, string> source = new(e => e.Id);
        private readonly ReadOnlyObservableCollection<EmailEnvelop> envelops;
        public ReadOnlyObservableCollection<EmailEnvelop> Envelops => envelops;

        private EmailEnvelop? selectedEnvelop;
        public EmailEnvelop? SelectedEnvelop
        {
            get => selectedEnvelop;
            set => this.RaiseAndSetIfChanged(ref selectedEnvelop, value);
        }

        private string content = string.Empty;
        public string Content
        {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }

        public ReactiveCommand<Unit, IReadOnlyCollection<EmailEnvelop>> StartCommand { get; }
        public ReactiveCommand<EmailEnvelop, Unit> ShowContentCommand { get; }

        private IEmailService emailService;

        public MainWindowViewModel(Func<ServerType, IEmailService> emailServiceFactory)
        {
            emailService = emailServiceFactory(ServerType);

            source
                .Connect()
                .Sort(SortExpressionComparer<EmailEnvelop>.Descending(t => t.Date))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out envelops)
                .Subscribe();

            StartCommand =
                ReactiveCommand
                .CreateFromObservable(
                    () =>
                    {
                        source.Clear();

                        var connection = new ConnectionOptions(Encryption, Server, Port, Username, Password);

                        emailService = emailServiceFactory(ServerType);

                        return emailService.Download(connection);
                    },
                    this.WhenAnyValue(
                        vm => vm.IsBusy, vm => vm.Username, vm => vm.Password,
                        (isBusy, userName, password) => !isBusy && !string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
                    );

            StartCommand
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    ev => source.AddOrUpdate(ev));

            StartCommand
                .ThrownExceptions
                .Subscribe(ex => Content = ex.Message);

            this.WhenAnyObservable(vm => vm.StartCommand.IsExecuting)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(IsBusy), out isBusy);

            ShowContentCommand =
                ReactiveCommand
                .CreateFromTask<EmailEnvelop>(
                    async envelop =>
                    {
                        if (envelop == null)
                        {
                            return;
                        }

                        Content = "Loading...";

                        var connection = new ConnectionOptions(Encryption, Server, Port, Username, Password);

                        Content = await emailService.DownloadBodyAsync(envelop.Id, connection);
                    });
        }
    }
}
