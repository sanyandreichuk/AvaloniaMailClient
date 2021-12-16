using Limilabs.Client;
using MailClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailClient.Services
{
    public interface IEmailClient : IDisposable
    {
        public SSLConfiguration SSLConfiguration { get; }
        public Task ConnectAsync(string host, int port);
        public Task ConnectSSLAsync(string host, int port);
        public Task LoginAsync(string user, string password);
        public Task UseBestLoginAsync(string user, string password);
        public Task StartTLSAsync();
        public Task<IReadOnlyCollection<string>> GetAllAsync();
        public Task<IReadOnlyCollection<EmailEnvelop>> GetEnvelopsAsync(IEnumerable<string> ids);
        public Task<byte[]> GetMessageAsync(string id);
    }
}
