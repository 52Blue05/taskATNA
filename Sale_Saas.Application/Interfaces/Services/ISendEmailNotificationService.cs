using Sale_Saas.Application.Features.MailServerInfoFeature.Model;

namespace Sale_Saas.Application.Interfaces.Services;
public interface ISendEmailNotificationService
{
    public string SendEmailMessage(EmailMessage message);
}
