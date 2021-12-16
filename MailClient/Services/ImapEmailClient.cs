using Limilabs.Client;
using Limilabs.Client.IMAP;
using MailClient.Extensions;
using MailClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailClient.Services
{
    public class ImapEmailClient : IEmailClient
    {
        private readonly Imap imap;

        public ImapEmailClient()
        {
            imap = new Imap();
        }

        public SSLConfiguration SSLConfiguration => imap.SSLConfiguration;

        public Task ConnectAsync(string host, int port) => imap.ConnectAsync(host, port);

        public Task ConnectSSLAsync(string host, int port) => imap.ConnectSSLAsync(host, port);

        public Task SelectInboxAsync() => imap.SelectInboxAsync();

        public async Task<IReadOnlyCollection<EmailEnvelop>> GetEnvelopsAsync(IEnumerable<string> ids)
        {
            var messageIds = ids
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => Convert.ToInt64(id))
                .ToList();

            var messages = await imap.GetMessageInfoByUIDAsync(messageIds).ConfigureAwait(false);
            return messages.Select(x => x.ToEmailEnvelop()).ToList();
        }

        public async Task<IReadOnlyCollection<string>> GetAllAsync()
        {
            var ids = await imap.SearchAsync(Flag.All);
            return ids.Select(id => Convert.ToString(id)).ToList();
        }

        public Task<byte[]> GetMessageAsync(string id) => imap.GetMessageByUIDAsync(Convert.ToInt64(id));

        public Task LoginAsync(string user, string password) => imap.LoginAsync(user, password);

        public Task StartTLSAsync() => imap.StartTLSAsync();
        public Task UseBestLoginAsync(string user, string password) => imap.UseBestLoginAsync(user, password);
        public void Dispose() => imap.Dispose();
    }
}
