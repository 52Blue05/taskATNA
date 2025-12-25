using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Features.RelationshipFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.Commands
{
    public record Relationship_UpdateResultCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<RelationshipDto>>>;

    public class Relationship_UpdateResultCommandHandler : IRequestHandler<Relationship_UpdateResultCommand, Result<List<RelationshipDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Relationship_UpdateResultCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<RelationshipDto>>> Handle(Relationship_UpdateResultCommand request, CancellationToken cancellationToken)
        {
            Relationship? obj = null;
            List<RelationshipDto> updatedSuccess = new List<RelationshipDto>();
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null || addOrUpdateRequest.Id == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "ActualPoint"), true);

                obj = await RelationshipService.GetRelationship(addOrUpdateRequest.Id.Value, _context);
                var tmp = (RelationshipUpdateResult)_internalService.MapValueToObject(new RelationshipUpdateResult(), addOrUpdateRequest.Data, new RelationshipUpdateResult());
                obj.ActualPoint = tmp.ActualPoint;

                if (tmp.YearToDateId != null)
                {
                    var level = await RelationshipService.GetLevel(tmp.YearToDateId, _context);
                    obj.YearToDateId = level.Id;
                    obj.YearToDate = level;
                }
                obj.ActualPoint = tmp.ActualPoint;
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (obj?.YearToDateId == obj?.TargetRelationshipId && obj?.ActualPoint >= obj.Point)
                {
                    obj.RelationshipStatus = await RelationshipService.GetStatus(RelationshipStatusEnum.COMPLETED.ToString(), _context);
                }

                _context.Relationships.Update(obj);
                updatedSuccess.Add(_mapper.Map<RelationshipDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<RelationshipDto>>.Success(updatedSuccess);
        }
    }
}
