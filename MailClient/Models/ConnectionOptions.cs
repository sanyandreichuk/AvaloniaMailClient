namespace MailClient.Models
{
    public record ConnectionOptions(Encryption Encryption, string Server, int Port, string Username, string Password);
}
