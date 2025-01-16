using ProjectManager.Api.Services;

namespace Skarabeus_Api.BackgroundServices;
public class EmailSenderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public EmailSenderBackgroundService(
        IServiceProvider serviceProvider
        )
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SendEmailsAsync(stoppingToken);
    }

    private async Task SendEmailsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<EmailSenderService>();
            await service!.SendEmailsAsync();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}