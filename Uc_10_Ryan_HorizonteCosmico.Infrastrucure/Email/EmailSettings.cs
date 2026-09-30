namespace Uc_10_Ryan_HorizonteCosmico.Infrastrucure.Email;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Horizonte Cósmico";
    public bool EnableSsl { get; set; } = true;
}
