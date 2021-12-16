using DynamicData;
using DynamicData.Binding;
using MailClient.Models;
using MailClient.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MailClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ServerType serverType = ServerType.IMAP;
        public ServerType ServerType
        {
            get => serverType;
            set
            {
                this.RaiseAndSetIfChanged(ref serverType, value);
                UpdateService();
            }
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

        public ReactiveCommand<Unit, Unit> StartCommand { get; }
        public ReactiveCommand<EmailEnvelop, Unit> ShowContentCommand { get; }


        private readonly Func<ServerType, IEmailService> emailServiceFactory;
        private IEmailService emailService;

        public MainWindowViewModel(Func<ServerType, IEmailService> emailServiceFactory)
        {
            this.emailServiceFactory = emailServiceFactory;

            emailService = emailServiceFactory(ServerType);

            source
                .Connect()
                .Sort(SortExpressionComparer<EmailEnvelop>.Descending(t => t.Date))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out envelops)
                .Subscribe();

            var startCanExecute =
                this.WhenAnyValue(
                    x => x.IsBusy,
                    x => x.Username,
                    x => x.Password,
                    (isBusy, userName, password) => !isBusy && !string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
                .ObserveOn(RxApp.MainThreadScheduler);

            StartCommand = ReactiveCommand.Create(StartExecute, startCanExecute);
            ShowContentCommand = ReactiveCommand.CreateFromTask<EmailEnvelop>(ShowBodyExecute);
        }

        private void StartExecute()
        {
            IsBusy = true;

            source.Clear();

            var connection = new ConnectionOptions(Encryption, Server, Port, Username, Password);

            var observable =
                emailService
                    .Download(connection)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(
                        ev => source.AddOrUpdate(ev),
                        onCompleted: () => IsBusy = false,
                        onError: (ex) =>
                        {
                            IsBusy = false;
                            Content = ex.Message;
                        }
                    );
        }

        private async Task ShowBodyExecute(EmailEnvelop envelop)
        {
            if (envelop == null)
            {
                return;
            }

            Content = "Loading...";

            var connection = new ConnectionOptions(Encryption, Server, Port, Username, Password);

            Content = await emailService.DownloadBodyAsync(envelop.Id, connection);
        }

        private void UpdateService()
        {
            emailService = emailServiceFactory(ServerType);
            source.Clear();
            Content = string.Empty;
        }
    }
}
