using Sale_Saas.Application.Features.OpportunityStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.OpportunityStatusFeature.Commands
{
    public record OpportunityStatus_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<OpportunityStatusDto>>>;

    public class OpportunityStatus_AddOrUpdateCommandHandler : IRequestHandler<OpportunityStatus_AddOrUpdateCommand, Result<List<OpportunityStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public OpportunityStatus_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<OpportunityStatusDto>>> Handle(OpportunityStatus_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            OpportunityStatus? obj = null;
            List<OpportunityStatusDto> updatedSuccess = new List<OpportunityStatusDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new OpportunityStatus()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.OpportunityStatuses.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (OpportunityStatus)_internalService.MapValueToObject(new OpportunityStatus(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Id == null)
                {
                    _context.OpportunityStatuses.Add(obj);
                }
                else
                {
                    _context.OpportunityStatuses.Update(obj);
                }

                updatedSuccess.Add(new OpportunityStatusDto() { Id = obj.Id });
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<OpportunityStatusDto>>.Success(updatedSuccess);
        }
    }
}
