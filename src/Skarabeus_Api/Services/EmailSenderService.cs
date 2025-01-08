using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using NodaTime;
using Skarabeus_Api.Settings;
using Skarabeus_Data;
using Skarabeus_Data.Interfaces;
using System.Net.Mail;

namespace ProjectManager.Api.Services;

public class EmailSenderService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public EmailSenderService(
        IClock clock,
        ApplicationDbContext dbContext,
        IOptionsSnapshot<SmtpSettings> smtpSettings
        )
    {
        _clock = clock;
        _dbContext = dbContext;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task AddEmailToSendAsync(
        string receiver,
        string subject,
        string body
        )
    {
        var now = _clock.GetCurrentInstant();

        var newMail = new Email
        {
            Body = body,
            Subject = subject,
            Receiver = receiver,
            Sender = _smtpSettings.Sender,
            ScheduledAt = now,
        }.SetCreateBySystem(now);

        _dbContext.Emails.Add(newMail);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SendEmailsAsync()
    {
        var mails = await _dbContext.Set<Email>().Where(x => x.SentAt == null).ToListAsync();
        foreach (var mail in mails)
        {
            using var notif = new MailMessage
            {
                Subject = mail.Subject,
                Body = mail.Body,
                IsBodyHtml = false,
                From = new MailAddress(mail.Sender),
            };
            notif.To.Add(new MailAddress(mail.Receiver));

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port);
                await smtp.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await smtp.SendAsync((MimeMessage)notif);

                mail.SentAt = _clock.GetCurrentInstant();
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // log error, notify someone
            }
            finally
            {
                // nothing to do right now
            }
        }
    }
}
