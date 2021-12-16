using MailClient.Models;

namespace MailClient.Services
{
    public static class EmailServiceFactory
    {
        public static IEmailService GetEmailService(ServerType serverType)
        {
            return serverType switch
            {
                ServerType.IMAP => new ImapEmailService(),
                ServerType.POP3 => new PopEmailService(),
                _ => new ImapEmailService()
            };
        }
    }
}
