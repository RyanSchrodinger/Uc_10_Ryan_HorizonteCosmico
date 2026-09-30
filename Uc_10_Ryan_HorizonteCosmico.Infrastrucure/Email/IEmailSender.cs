namespace Uc_10_Ryan_HorizonteCosmico.Infrastrucure.Email;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody);
}
