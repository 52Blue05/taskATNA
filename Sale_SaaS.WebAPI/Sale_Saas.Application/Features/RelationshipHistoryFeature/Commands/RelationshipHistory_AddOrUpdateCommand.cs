using Sale_Saas.Application.Features.RelationshipHistoryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.RelationshipHistoryFeature.Commands
{
     public record RelationshipHistory_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<RelationshipHistoryDto>>>;

     public class RelationshipHistory_AddOrUpdateCommandHandler : IRequestHandler<RelationshipHistory_AddOrUpdateCommand, Result<List<RelationshipHistoryDto>>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IInternalService _internalService;
          private readonly IEventLogService _eventLogService;

          public RelationshipHistory_AddOrUpdateCommandHandler(IApplicationDbContext context,
                                                             IInternalService internalService,
                                                             IEventLogService eventLogService)
          {
               _context = context;
               _internalService = internalService;
               _eventLogService = eventLogService;
          }

          public async Task<Result<List<RelationshipHistoryDto>>> Handle(RelationshipHistory_AddOrUpdateCommand request, CancellationToken cancellationToken)
          {
               RelationshipHistory? obj = null;
               List<RelationshipHistoryDto> updatedSuccess = new List<RelationshipHistoryDto>();

               foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
               {
                    if (addOrUpdateRequest.Data == null)
                    {
                         throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                    }

                    if (addOrUpdateRequest.Id == null)
                    {
                         obj = new RelationshipHistory()
                         {
                              CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                         };
                    }
                    else
                    {
                         obj = await _context.RelationshipHistories.FindAsync(addOrUpdateRequest.Id.Value);

                         if (obj == null)
                              throw new ApplicationException($"Không tìm thấy Lịch sử có id: {addOrUpdateRequest.Id.Value}");
                    }

                    obj = (RelationshipHistory)_internalService.MapValueToObject(new RelationshipHistory(), addOrUpdateRequest.Data, obj);
                    obj.LastModifiedDate = DateTime.Now;
                    obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                    if (addOrUpdateRequest.Id == null)
                    {
                         _context.RelationshipHistories.Add(obj);
                    }
                    else
                    {
                         _context.RelationshipHistories.Update(obj);
                    }

                    updatedSuccess.Add(new RelationshipHistoryDto() { Id = obj.Id });
               }

               var eventLog = await _eventLogService.Create("RelationshipHistoryFeature", "RelationshipHistoryFeature",
                                                                   "RelationshipHistory_AddOrUpdateCommand", request.userId);

               await _context.SaveChangesAsync(cancellationToken);

               return Result<List<RelationshipHistoryDto>>.Success(updatedSuccess);
          }
     }

}
