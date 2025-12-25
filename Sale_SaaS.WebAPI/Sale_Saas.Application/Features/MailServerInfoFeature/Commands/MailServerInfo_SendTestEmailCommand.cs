using Sale_Saas.Application.Features.MailServerInfoFeature.Model;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.MailServerInfoFeature.Commands;

public record MailServerInfo_SendTestMailServerInfoCommand : IRequest<Result<string>>
{
    public int Id { get; init; }
    public string Email { get; init; }
    public Guid userId { get; init; }
}
public class MailServerInfo_SendTestMailServerInfoCommandHandler : IRequestHandler<MailServerInfo_SendTestMailServerInfoCommand, Result<string>>
{    
	private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISendEmailNotificationService _sendEmailNotificationService;
    private readonly IEventLogService _eventLogService;

    public MailServerInfo_SendTestMailServerInfoCommandHandler(IMapper mapper, 
                                                IApplicationDbContext context,
												ISendEmailNotificationService sendEmailNotificationService, IEventLogService eventLogService)
    {        
		_context = context;
        _mapper = mapper;
		_sendEmailNotificationService = sendEmailNotificationService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<string>> Handle(MailServerInfo_SendTestMailServerInfoCommand request, CancellationToken cancellationToken)
    {        
        string message = string.Empty;
        MailServerInfo? email = await _context.MailServerInfos.FindAsync(request.Id);
        if(email != null)
        {
            EmailMessage _emailConfig = new EmailMessage();

            _emailConfig.ToEmail = request.Email;
            _emailConfig.FromEmail = email.FromEmail ?? string.Empty;
            _emailConfig.DispalyNameEmail = email.TenEmailGui ?? string.Empty;
            _emailConfig.Subject = "Test gửi email cấu hình";
            _emailConfig.Body = "MailServerInfo đã gửi thành công";
            _emailConfig.IsHtml = true;

            message = _sendEmailNotificationService.SendEmailMessage(_emailConfig);
        }
        else
        {
            message = "Không tìm thấy email gửi.";
        }

        var eventLog = await _eventLogService.Create("MailServerInfoFeature", "MailServerInfoFeature",
                            "MailServerInfo_SendTestMailServerInfoCommand", request.userId);

        return Result<string>.Success(message);
    }
}
