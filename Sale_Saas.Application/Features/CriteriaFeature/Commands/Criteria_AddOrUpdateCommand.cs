using Sale_Saas.Application.Features.CriteriaFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

public record Criteria_AddOrUpdateCommand(Guid UserId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<CriteriaDto>>>;

public class Criteria_AddOrUpdateCommandHandler : IRequestHandler<Criteria_AddOrUpdateCommand, Result<List<CriteriaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;

    public Criteria_AddOrUpdateCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
    }

    public async Task<Result<List<CriteriaDto>>> Handle(Criteria_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        Criteria? obj = null;
        List<CriteriaDto> result = new List<CriteriaDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
            }

            if (addOrUpdateRequest.Id == null)
            {
                // check code
                var checkCodeDuplicated = await _context.Criterias.CountAsync(x => x.DeleteFlag != true
                                                                                && !string.IsNullOrEmpty(x.Code)
                                                                                && x.Code.ToLower().Trim().Equals(addOrUpdateRequest.Data["Code"].ToLower().Trim()));

                if (checkCodeDuplicated > 0)
                {
                    throw new ApplicationException("Code này đã được sử dụng");
                }

                // add data
                var checkNameDuplicated = await _context.Criterias.CountAsync(x => x.DeleteFlag != true
                                                                        && !string.IsNullOrEmpty(x.Name)
                                                                        && x.Name.ToLower().Trim().Equals(addOrUpdateRequest.Data["Name"].ToLower().Trim()));

                if (checkNameDuplicated > 0)
                {
                    throw new ApplicationException("Tên này đã được sử dụng");
                }

                obj = new Criteria()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                    CreatedDate = DateTime.Now
                };
            }
            else
            {
                // update data
                obj = await _context.Criterias.FindAsync(addOrUpdateRequest.Id);

                if (obj == null)
                {
                    throw new ApplicationException("Không tìm thấy tiêu chí để cập nhật");
                }
            }

            obj = (Criteria)_internalService.MapValueToObject(new Criteria(), addOrUpdateRequest.Data, obj);

            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
            obj.LastModifiedDate = DateTime.Now;

            EventLog eventLog = new EventLog()
            {
                Code = "CriteriaFeature",
                Name = "CriteriaFeature",
                Notes = addOrUpdateRequest.Data.ToPairString(),
                CreatedDate = DateTime.Now,
                CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId,
            };

            if (addOrUpdateRequest.Id == null)
            {
                _context.Criterias.Add(obj);
                eventLog.Action = "CriteriaFeature_AddCommand";
            }
            else
            {
                _context.Criterias.Update(obj);
                eventLog.Action = "CriteriaFeature_UpdateCommand";
            }

            _context.EventLogs.Add(eventLog);
            result.Add(new CriteriaDto()
            {
                Id = obj.Id,
                Code = obj.Code,
                Name = obj.Name,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<CriteriaDto>>.Success(result);
    }
}
