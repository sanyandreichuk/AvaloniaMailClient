using Limilabs.Client;
using Limilabs.Client.POP3;
using Limilabs.Mail;
using MailClient.Extensions;
using MailClient.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailClient.Services
{
    public class PopEmailClient : IEmailClient
    {
        private readonly Pop3 pop;

        public PopEmailClient()
        {
            pop = new Pop3();
        }

        public SSLConfiguration SSLConfiguration => pop.SSLConfiguration;

        public Task ConnectAsync(string host, int port) => pop.ConnectAsync(host, port);

        public Task ConnectSSLAsync(string host, int port) => pop.ConnectSSLAsync(host, port);

        public void Dispose() => pop.Dispose();

        public async Task<IReadOnlyCollection<string>> GetAllAsync()
        {
            var ids = await pop.GetAllAsync().ConfigureAwait(false);
            return ids.ToList();
        }

        public async Task<IReadOnlyCollection<EmailEnvelop>> GetEnvelopsAsync(IEnumerable<string> ids)
        {
            var envelops = new List<EmailEnvelop>();

            foreach (string id in ids)
            {
                var eml = await pop.GetHeadersByUIDAsync(id).ConfigureAwait(false);
                var email = new MailBuilder().CreateFromEml(eml);

                if (email == null)
                {
                    continue;
                }

                envelops.Add(email.ToEmailEnvelop(id));
            }

            return envelops;
        }

        public Task<byte[]> GetMessageAsync(string id) => pop.GetMessageByUIDAsync(id);

        public Task LoginAsync(string user, string password) => pop.LoginAsync(user, password);

        public Task StartTLSAsync() => pop.StartTLSAsync();

        public Task UseBestLoginAsync(string user, string password) => pop.UseBestLoginAsync(user, password);
    }
}
