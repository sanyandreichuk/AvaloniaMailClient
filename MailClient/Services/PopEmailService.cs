namespace MailClient.Services
{

    public class PopEmailService : EmailServiceBase
    {
        public PopEmailService() : base(() => new PopEmailClient(), Constants.MaxPopConnectionCount, Constants.DownloadChunkSize)
        {

        }
    }
}
