using AsyncKeyedLock;
using Limilabs.Mail;
using MailClient.Extensions;
using MailClient.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace MailClient.Services
{
    public abstract class EmailServiceBase : IEmailService
    {
        protected readonly ConcurrentDictionary<string, string> BodyCache = new();
        protected static readonly AsyncKeyedLocker<string> Locker = new();
        protected readonly Func<IEmailClient> clientFactory;
        private readonly int connectionCount;
        private readonly int chunkSize;

        public EmailServiceBase(Func<IEmailClient> clientFactory, int connectionCount, int chunkSize)
        {
            this.clientFactory = clientFactory;
            this.connectionCount = connectionCount;
            this.chunkSize = chunkSize;
        }

        public IObservable<IReadOnlyCollection<EmailEnvelop>> Download(ConnectionOptions options)
        {
            var subject = new Subject<IReadOnlyCollection<EmailEnvelop>>();

            Task.Run(async () =>
            {
                try
                {
                    await DownloadAsync(options, connectionCount, subject);
                }
                catch (Exception ex)
                {
                    subject.OnError(ex);
                }
            });

            return subject;
        }

        private async Task DownloadAsync(ConnectionOptions options, int connectionCount, Subject<IReadOnlyCollection<EmailEnvelop>> subject)
        {
            var connectionTasks = new List<Task<IEmailClient>>();

            for (int i = 0; i < connectionCount; i++)
            {
                var task = ConnectAsync(options);
                connectionTasks.Add(task);
            }

            var completedConnectionTask = await Task.WhenAny(connectionTasks);
            var completedConnection = await completedConnectionTask;

            var ids = await completedConnection.GetAllAsync().ConfigureAwait(false);

            if (ids is null || !ids.Any())
            {
                subject.OnCompleted();

                return;
            }

            ids = ids.Reverse().ToList();

            var partitions = ids.Split(connectionCount).ToList();

            var downloadTasks = new List<Task>();
            for (int i = 0; i < partitions.Count; i++)
            {
                var connectionTask = connectionTasks[i];
                var idsPart = partitions[i];

                var task = DownloadTask(connectionTask, idsPart, subject);
                downloadTasks.Add(task);
            }

            await Task.WhenAll(downloadTasks);

            foreach (var connectionTask in connectionTasks)
            {
                var connection = await connectionTask;
                connection.Dispose();
            }

            subject.OnCompleted();
        }

        private Task DownloadTask(Task<IEmailClient> connectionTask, IEnumerable<string> ids, Subject<IReadOnlyCollection<EmailEnvelop>> subject)
        {
            return Task.Run(async () =>
            {
                var chunks = ids.ChunkBy(chunkSize);

                var connection = await connectionTask;

                foreach (var chunk in chunks)
                {
                    var envelops = await connection.GetEnvelopsAsync(chunk);

                    subject.OnNext(envelops);

                    foreach (var id in chunk)
                    {
                        await DownloadBodyCoreAsync(connection, id);
                    }
                }
            });
        }

        public async Task<string> DownloadBodyAsync(string id, ConnectionOptions options)
        {
            if (BodyCache.TryGetValue(id, out var emailBody))
            {
                return emailBody;
            }

            using var imap = await ConnectAsync(options);

            return await DownloadBodyCoreAsync(imap, id);
        }

        private async Task<string> DownloadBodyCoreAsync(IEmailClient client, string id)
        {
            using (await Locker.LockAsync(id))
            {
                if (BodyCache.TryGetValue(id, out var body))
                {
                    return body;
                }

                var eml = await client.GetMessageAsync(id).ConfigureAwait(false);
                var email = new MailBuilder().CreateFromEml(eml);

                body = email.GetBodyAsHtml();

                BodyCache.TryAdd(id, body);

                return body ?? string.Empty;
            }
        }

        protected virtual async Task<IEmailClient> ConnectAsync(ConnectionOptions options)
        {
            var client = clientFactory();

            switch (options.Encryption)
            {
                case Encryption.Unencrypted:
                    await client.ConnectAsync(options.Server, options.Port).ConfigureAwait(false);
                    await client.LoginAsync(options.Username, options.Password).ConfigureAwait(false);
                    break;

                case Encryption.SSL_TLS:
                    client.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                    await client.ConnectSSLAsync(options.Server, options.Port).ConfigureAwait(false);
                    await client.UseBestLoginAsync(options.Username, options.Password).ConfigureAwait(false);
                    break;

                case Encryption.STARTTLS:
                    client.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                    await client.ConnectAsync(options.Server, options.Port).ConfigureAwait(false);
                    await client.StartTLSAsync().ConfigureAwait(false);
                    await client.UseBestLoginAsync(options.Username, options.Password).ConfigureAwait(false);
                    break;
            }

            return client;
        }

    }
}
