using Sale_Saas.Application.Features.GainsFamilyFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.GainsFamilyFeature.Commads
{
     public record GainsFamily_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<GainsFamilyDto>>>;

     public class GainsFamily_AddOrUpdateCommandHandler : IRequestHandler<GainsFamily_AddOrUpdateCommand, Result<List<GainsFamilyDto>>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;
          private readonly IInternalService _internalService;
          public GainsFamily_AddOrUpdateCommandHandler(IMapper mapper,
                                                  IApplicationDbContext context,
                                                  IInternalService internalService)
          {
               _context = context;
               _mapper = mapper;
               _internalService = internalService;
          }
          public async Task<Result<List<GainsFamilyDto>>> Handle(GainsFamily_AddOrUpdateCommand request, CancellationToken cancellationToken)
          {
               GainsFamily? obj = null;
               List<GainsFamilyDto> updatedSuccess = new List<GainsFamilyDto>();

               List<GainsFamilyDto> listFamily = new List<GainsFamilyDto>();

               foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
               {
                    if (addOrUpdateRequest.Data == null)
                    {
                         throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                    }
                    if (!string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Name")))
                    {
                         if (string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "YearOfBirth")))
                         {
                              throw new ApplicationException($"Năm sinh không thể để trống");
                         }
                    }

                    var duplicateFamily = listFamily.Where(x =>
                    x.Name.ToLower().Trim() == addOrUpdateRequest.Data["Name"].ToLower().Trim() &&
                    x.YearOfBirth == int.Parse(addOrUpdateRequest.Data["YearOfBirth"]));

                    if (duplicateFamily.Any())
                         throw new ApplicationException($"Trường học không thể trùng nhau");

                    listFamily.Add(new GainsFamilyDto
                    {
                         Name = addOrUpdateRequest.Data["Name"],
                         YearOfBirth = int.Parse(addOrUpdateRequest.Data["YearOfBirth"])
                    });

                    if (addOrUpdateRequest.Id == null)
                    {
                         obj = new GainsFamily()
                         {
                              CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                         };
                    }
                    else
                    {
                         obj = await _context.GainsFamilies.FindAsync(addOrUpdateRequest.Id.Value);

                         if (obj == null)
                              throw new ApplicationException($"Không tìm thấy năm học có id: {addOrUpdateRequest.Id.Value}");

                    }

                    obj = (GainsFamily)_internalService.MapValueToObject(new GainsFamily(), addOrUpdateRequest.Data, obj);
                    obj.LastModifiedDate = DateTime.Now;
                    obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                    if (addOrUpdateRequest.Id == null)
                    {
                         _context.GainsFamilies.Add(obj);
                    }
                    else
                    {
                         _context.GainsFamilies.Update(obj);
                    }

                    updatedSuccess.Add(_mapper.Map<GainsFamilyDto>(obj));
               }

               await _context.SaveChangesAsync(cancellationToken);

               return Result<List<GainsFamilyDto>>.Success(updatedSuccess);
          }
     }
}
