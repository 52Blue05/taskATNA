using Sale_Saas.Application.Features.GainsQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GainsQuestionFeature.Commands
{
    public record GainsQuestion_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<GainsQuestionDto>>>;

    public class GainsQuestion_AddOrUpdateCommandHandler : IRequestHandler<GainsQuestion_AddOrUpdateCommand, Result<List<GainsQuestionDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFeaturePermissionService _permissionService;
        private readonly IEventLogService _eventLogService;

        public GainsQuestion_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IFeaturePermissionService permissionService,
                                                IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _permissionService = permissionService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<GainsQuestionDto>>> Handle(GainsQuestion_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            GainsQuestion? obj = null;
            List<GainsQuestionDto> updatedSuccess = new List<GainsQuestionDto>();
            int code = 0;
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                var tmp = new GainsQuestion();
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code"), true);
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Content"), true);
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Description"), true);

				if (addOrUpdateRequest.Id == null)
                {
                    await _permissionService.HasPermission(
                                                        MenuType.DM_GAINS,
                                                        FeatureType.CREATE,
                                                        request.userId,
                                                        true
                                                        );

                    obj = new GainsQuestion()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    await _permissionService.HasPermission(
                                    MenuType.DM_GAINS,
                                    FeatureType.UPDATE,
                                    request.userId,
                                    true
                                    );

                    obj = await _context.GainsQuestions.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");
                    PropertiesExtension.Copy(obj, tmp);
                }

                obj = (GainsQuestion)_internalService.MapValueToObject(new GainsQuestion(), addOrUpdateRequest.Data, obj);
                obj.Description = "";
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

               /* var query = _context.GainsQuestions.Where(s => s.Code == obj.Code && !s.DeleteFlag).AsNoTracking();
                if (addOrUpdateRequest.Id != null) query = query.Where(s => s.Id != addOrUpdateRequest.Id);
                var exist = await query.FirstOrDefaultAsync();
                if (exist != null)
                    throw new ApplicationException($"Số thứ tự đã được sử dụng: {obj.Code}");*/

                if (addOrUpdateRequest.Id == null)
                {
                    if(code == 0)
                    {
                        code = _context.GainsQuestions.Where(s => s.Code != null).Max(s => s.Code) ?? code;
                    }
                    code = code + 1;
                    obj.Code = code;

                    _context.GainsQuestions.Add(obj);
                }
                else
                {
                    obj.Code = tmp.Code ?? obj.Code;
                    _context.GainsQuestions.Update(obj);
                }

                updatedSuccess.Add(_mapper.Map<GainsQuestionDto>(obj));
            }

            var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                                    "GainsQuestion_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<GainsQuestionDto>>.Success(updatedSuccess);
        }
    }
}
