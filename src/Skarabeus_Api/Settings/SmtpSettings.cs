namespace Skarabeus_Api.Settings;

public class SmtpSettings
{
    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Sender { get; set; } = null!;
}