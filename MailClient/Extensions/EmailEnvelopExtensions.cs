using Limilabs.Client.IMAP;
using Limilabs.Mail;
using MailClient.Models;
using System;
using System.Linq;

namespace MailClient.Extensions
{
    public static class EmailEnvelopExtensions
    {
        public static EmailEnvelop ToEmailEnvelop(this MessageInfo info)
        {
            var envelop = info.Envelope;

            string name = envelop.From.FirstOrDefault()?.Name ?? string.Empty;

            return new EmailEnvelop(info?.UID?.ToString() ?? string.Empty, envelop.Subject, name, envelop.Date ?? DateTime.MinValue);
        }

        public static EmailEnvelop ToEmailEnvelop(this IMail mail, string id)
        {
            string name = mail.From.FirstOrDefault()?.Name ?? string.Empty;

            return new EmailEnvelop(id, mail.Subject, name, mail.Date ?? DateTime.MinValue);
        }

    }
}
