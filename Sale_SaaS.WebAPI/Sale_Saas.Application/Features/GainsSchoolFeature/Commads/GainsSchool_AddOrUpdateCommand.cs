using Sale_Saas.Application.Features.GainsSchoolFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.GainsSchoolFeature.Commads
{
     public record GainsSchool_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<GainsSchoolDto>>>;

     public class GainsSchool_AddOrUpdateCommandHandler : IRequestHandler<GainsSchool_AddOrUpdateCommand, Result<List<GainsSchoolDto>>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;
          private readonly IInternalService _internalService;
          public GainsSchool_AddOrUpdateCommandHandler(IMapper mapper,
                                                  IApplicationDbContext context,
                                                  IInternalService internalService)
          {
               _context = context;
               _mapper = mapper;
               _internalService = internalService;
          }
          public async Task<Result<List<GainsSchoolDto>>> Handle(GainsSchool_AddOrUpdateCommand request, CancellationToken cancellationToken)
          {
               GainsSchool? obj = null;
               List<GainsSchoolDto> updatedSuccess = new List<GainsSchoolDto>();

               List<GainsSchoolDto> listSchool = new List<GainsSchoolDto>();

               foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
               {
                    if (addOrUpdateRequest.Data == null)
                    {
                         throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                    }
                    if (!string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Name")))
                    {
                         if (string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Year")))
                         {
                              throw new ApplicationException($"Năm học không thể để trống");
                         }
                    }

                    var duplicateSchool = listSchool.Where(x =>
                    x.Name.ToLower().Trim() == addOrUpdateRequest.Data["Name"].ToLower().Trim() &&
                    x.Year == int.Parse(addOrUpdateRequest.Data["Year"]));

                    if (duplicateSchool.Any())
                         throw new ApplicationException($"Trường học không thể trùng nhau");

                    listSchool.Add(new GainsSchoolDto
                    {
                         Name = addOrUpdateRequest.Data["Name"],
                         Year = int.Parse(addOrUpdateRequest.Data["Year"])
                    });

                    if (addOrUpdateRequest.Id == null)
                    {
                         obj = new GainsSchool()
                         {
                              CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                         };
                    }
                    else
                    {
                         obj = await _context.GainsSchools.FindAsync(addOrUpdateRequest.Id.Value);

                         if (obj == null)
                              throw new ApplicationException($"Không tìm thấy năm học có id: {addOrUpdateRequest.Id.Value}");

                    }

                    obj = (GainsSchool)_internalService.MapValueToObject(new GainsSchool(), addOrUpdateRequest.Data, obj);
                    obj.LastModifiedDate = DateTime.Now;
                    obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                    if (addOrUpdateRequest.Id == null)
                    {
                         _context.GainsSchools.Add(obj);
                    }
                    else
                    {
                         _context.GainsSchools.Update(obj);
                    }

                    updatedSuccess.Add(_mapper.Map<GainsSchoolDto>(obj));
               }

               await _context.SaveChangesAsync(cancellationToken);

               return Result<List<GainsSchoolDto>>.Success(updatedSuccess);
          }
     }
}
