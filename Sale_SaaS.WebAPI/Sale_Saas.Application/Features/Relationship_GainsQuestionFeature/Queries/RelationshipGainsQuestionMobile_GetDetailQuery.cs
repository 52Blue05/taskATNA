using Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Dto;
using Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Requests;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Queries;

public record RelationshipGainsQuestionMobile_GetDetailQuery(Guid userId, GetDetailRelationshipGainsQuestionRequest RequestData) : IRequest<Result<Relationship_GainsQuestionDto>>;

public class RelationshipGainsQuestionMobile_GetDetailQueryHandler : IRequestHandler<RelationshipGainsQuestionMobile_GetDetailQuery, Result<Relationship_GainsQuestionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IEventLogService _eventLogService;

    public RelationshipGainsQuestionMobile_GetDetailQueryHandler(IMapper mapper,
                                            IApplicationDbContext context,
                                            IInternalService internalService, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<Relationship_GainsQuestionDto>> Handle(RelationshipGainsQuestionMobile_GetDetailQuery request, CancellationToken cancellationToken)
    {
        if (request.RequestData == null)
        {
            throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
        }

        var item = await _context.Relationship_GainsQuestions.Where(x => x.DeleteFlag != true
                                                                      && x.GainsQuestionId == request.RequestData.GainsQuestionId
                                                                      && x.RelationshipId == request.RequestData.RelationshipId)
                                                             .Include(s => s.GainsQuestion)
                                                             .AsNoTracking()
                                                             .FirstOrDefaultAsync();

        if (item == null)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu");
        }

        var result = _mapper.Map<Relationship_GainsQuestionDto>(item);

        var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                                            "GainsQuestion_AddOrUpdateCommand", request.userId);

        return Result<Relationship_GainsQuestionDto>.Success(result);
    }
}
