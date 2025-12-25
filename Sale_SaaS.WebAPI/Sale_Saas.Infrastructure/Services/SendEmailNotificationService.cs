using System.Net;
using System.Net.Mail;
using Sale_Saas.Application.Features.MailServerInfoFeature.Model;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Domain.Constants.API;

namespace Sale_Saas.Infrastructure.Services;

public class SendEmailNotificationService: ISendEmailNotificationService
{
    private readonly SmtpConfiguration _config;
    private readonly MailServiceConstant _mailServiceConstant;
    public SendEmailNotificationService(IOptions<MailServiceConstant> mailServiceConstant)
    {
        _mailServiceConstant = mailServiceConstant.Value;
        _config = new SmtpConfiguration();
        _config.Username = _mailServiceConstant.ConfigMailServer_Email;
        _config.Password = _mailServiceConstant.ConfigMailServer_Password;
        _config.From = _mailServiceConstant.ConfigMailServer_Email;
        _config.Host = _mailServiceConstant.ConfigMailServer_Host;
        _config.Port = _mailServiceConstant.ConfigMailServer_Port;
        _config.Ssl = _mailServiceConstant.ConfigMailServer_SSL;
    }

    public string SendEmailMessage(EmailMessage message)
    {
        string resultMessage = string.Empty;
        try
        {
            var smtp = new SmtpClient
            {
                Host = _config.Host,
                Port = _config.Port,
                EnableSsl = _config.Ssl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_config.Username, _config.Password),
            };

            using (var smtpMessage = new MailMessage(_config.From, message.ToEmail))
            {
                smtpMessage.Subject = message.Subject;
                smtpMessage.Body = message.Body;
                smtpMessage.IsBodyHtml = message.IsHtml;
                smtp.Send(smtpMessage);
            }
        }
        catch (Exception ex)
        {
            resultMessage = ex.Message;
        }

        return resultMessage;
    }
}
