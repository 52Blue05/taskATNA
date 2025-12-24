using Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Requests;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Commands;

public record RelationshipGainsQuestionMobile_UpdateCommand(Guid userId, RelationshipGainsQuestionUpdateRequest RequestData) : IRequest<Result<bool>>;

public class RelationshipGainsQuestionMobile_UpdateCommandHandler : IRequestHandler<RelationshipGainsQuestionMobile_UpdateCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IEventLogService _eventLogService;

    public RelationshipGainsQuestionMobile_UpdateCommandHandler(IMapper mapper,
                                            IApplicationDbContext context,
                                            IInternalService internalService, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(RelationshipGainsQuestionMobile_UpdateCommand request, CancellationToken cancellationToken)
    {
        if (request.RequestData == null)
        {
            throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
        }

        var item = await _context.Relationship_GainsQuestions.Where(x => x.DeleteFlag != true
                                                                      && x.GainsQuestionId == request.RequestData.GainsQuestionId
                                                                      && x.RelationshipId == request.RequestData.RelationshipId)
                                                             .FirstOrDefaultAsync();

        if (item == null)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu");
        }

        item.Answer = request.RequestData.Answer;
        item.AnswerDetail = request.RequestData.AnswerDetail ?? item.AnswerDetail;

        _context.Relationship_GainsQuestions.Update(item);

        await _context.SaveChangesAsync(cancellationToken);

        var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                                            "GainsQuestion_AddOrUpdateCommand", request.userId);

        return Result<bool>.Success(true);
    }
}
