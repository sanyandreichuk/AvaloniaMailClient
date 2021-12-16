using MailClient.Models;
using System.Threading.Tasks;

namespace MailClient.Services
{
    public class ImapEmailService : EmailServiceBase
    {
        public ImapEmailService() : base(() => new ImapEmailClient(), Constants.MaxImapConnectionCount, Constants.DownloadChunkSize)
        {

        }

        protected override async Task<IEmailClient> ConnectAsync(ConnectionOptions options)
        {
            var connection = await base.ConnectAsync(options);

            var imap = (ImapEmailClient)connection;

            await imap.SelectInboxAsync();

            return imap;
        }
    }
}