using System;

namespace MailClient.Models
{
    public record EmailEnvelop(string Id, string Subject, string From, DateTime Date);
}
