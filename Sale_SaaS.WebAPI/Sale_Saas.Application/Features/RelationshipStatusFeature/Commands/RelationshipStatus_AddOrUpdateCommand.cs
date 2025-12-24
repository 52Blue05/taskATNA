using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.RelationshipStatusFeature.Commands
{
    public record RelationshipStatus_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<RelationshipStatusDto>>>;

    public class RelationshipStatus_AddOrUpdateCommandHandler : IRequestHandler<RelationshipStatus_AddOrUpdateCommand, Result<List<RelationshipStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _InternalService;

        public RelationshipStatus_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService InternalService)
        {
            _context = context;
            _mapper = mapper;
            _InternalService = InternalService;

        }

        public async Task<Result<List<RelationshipStatusDto>>> Handle(RelationshipStatus_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            RelationshipStatus? obj = null;
            List<RelationshipStatusDto> updatedSuccess = new List<RelationshipStatusDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new RelationshipStatus();
                }
                else
                {
                    obj = await _context.RelationshipStatuses.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (RelationshipStatus)_InternalService.MapValueToObject(new RelationshipStatus(), addOrUpdateRequest.Data, obj);

                /*obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;*/

                if (addOrUpdateRequest.Id == null)
                {
                    _context.RelationshipStatuses.Add(obj);
                }
                else
                {
                    _context.RelationshipStatuses.Update(obj);
                }

                updatedSuccess.Add(new RelationshipStatusDto() { Id = obj.Id });
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<RelationshipStatusDto>>.Success(updatedSuccess);
        }
    }
}
