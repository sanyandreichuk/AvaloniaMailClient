using MailClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailClient.Services
{
    public interface IEmailService
    {
        IObservable<IReadOnlyCollection<EmailEnvelop>> Download(ConnectionOptions options);
        Task<string> DownloadBodyAsync(string id, ConnectionOptions options);
    }
}
