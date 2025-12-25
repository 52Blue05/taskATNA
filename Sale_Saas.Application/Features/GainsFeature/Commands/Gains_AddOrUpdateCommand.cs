using Sale_Saas.Application.Features.GainsFeature.Dto;
using Sale_Saas.Application.Features.RelationshipFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GainsFeature.Commands
{
    public record Gains_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<GainsDto>>>;

    public class Gains_AddOrUpdateCommandHandler : IRequestHandler<Gains_AddOrUpdateCommand, Result<List<GainsDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public Gains_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<GainsDto>>> Handle(Gains_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Gains? obj = null;
            List<GainsDto> updatedSuccess = new List<GainsDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                var tmp = new Gains();
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new Gains()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.Gains.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Hợp đồng có id: {addOrUpdateRequest.Id.Value}");
                    PropertiesExtension.Copy(obj, tmp);
                }

                obj = (Gains)_internalService.MapValueToObject(new Gains(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
                obj.RelationshipId = tmp.RelationshipId ?? obj.RelationshipId;

                if (obj.RelationshipId == null) throw new ApplicationException($"Không tìm thấy mối quan hệ");

                var exist = await _context.Relationships.FindAsync(obj.RelationshipId);
                if (exist == null) throw new ApplicationException($"Không tìm thấy Mối quan hệ có id: {obj.RelationshipId}");

                var relationshipStatus = await RelationshipService.GetStatus(RelationshipStatusEnum.PROCESSING.ToString(), _context);

                if (exist.RelationshipStatusId != relationshipStatus.Id)
                {
                    // CONFIRMED => PROCESSING
                    exist.RelationshipStatusId = relationshipStatus.Id;
                    _context.Relationships.Update(exist);
                }

                if (addOrUpdateRequest.Id == null)
                {
                    _context.Gains.Add(obj);
                }
                else
                {
                    if (tmp.CreatedDate == tmp.LastModifiedDate)
                    {
                        var status = await _context.RelationshipStatuses
                                           .FirstOrDefaultAsync(s => s.Code == RelationshipStatusEnum.PROCESSING.ToString());
                        if (status != null)
                        {
                            exist.RelationshipStatus = status;
                            exist.RelationshipStatusId = status.Id;
                        }
                    }
                    obj.LastModifiedDate = DateTime.Now;
                    _context.Gains.Update(obj);
                }

                var gainsDto = _mapper.Map<GainsDto>(obj);
                gainsDto.RelationshipStatusDto = new RelationshipStatusFeature.Dto.RelationshipStatusDto()
                {
                    Id = relationshipStatus.Id,
                    Code = relationshipStatus.Code,
                    Name = relationshipStatus.Name
                };
                updatedSuccess.Add(gainsDto);
            }

            var eventLog = await _eventLogService.Create("GainsFeature", "GainsFeature",
                                                "Gains_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<GainsDto>>.Success(updatedSuccess);
        }
    }
}
