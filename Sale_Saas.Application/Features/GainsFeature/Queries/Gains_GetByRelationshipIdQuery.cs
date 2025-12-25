using Sale_Saas.Application.Features.GainsFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsFeature.Queries
{
     public record Gains_GetByRelationshipIdQuery(Guid userId, Guid Id) : IRequest<Result<GainsDto>>;
     public class Gains_GetByRelationshipIdQueryHandler : IRequestHandler<Gains_GetByRelationshipIdQuery, Result<GainsDto>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;
          private readonly IEventLogService _eventLogService;

          public Gains_GetByRelationshipIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
          {
               _context = context;
               _mapper = mapper;
               _eventLogService = eventLogService;
          }

          public async Task<Result<GainsDto>> Handle(Gains_GetByRelationshipIdQuery request, CancellationToken cancellationToken)
          {
               var relationship = await _context.Relationships.FirstOrDefaultAsync(s => s.Id == request.Id && s.DeleteFlag != true);
               if (relationship == null) throw new ApplicationException($"Không tìm thấy Mối quan hệ có id: {request.Id}");

               var gains = await _context.Gains.FirstOrDefaultAsync(s => s.RelationshipId == request.Id && !s.DeleteFlag);

               if (gains == null)
               {
                    DateTime now = DateTime.Now;
                    gains = new Gains()
                    {
                         Id = new Guid(),
                         DayOfBirth = null,
                         NativePlace = "",
                         SchoolAndYear = "",
                         Email = "",
                         Phone = "",
                         Company = "",
                         Position = "",
                         Degree = "",
                         PositionTerm = "",
                         FormerJob = "",
                         Family = "",
                         Speciality = "",
                         Habit = "",
                         Hobby = "",
                         Dislike = "",
                         StrongPoint = "",
                         WeakPoint = "",
                         FaviroteActivity = "",
                         NearGoal = "",
                         LongGoal = "",
                         Idol = "",
                         Aspiration = "",
                         Clubs = "",
                         Address = "",
                         WeekendActivity = "",
                         PlaceOfBirth = "",
                         Archivement = "",
                         RelationshipId = relationship.Id,
                         Relationship = relationship,
                         LastModifiedApplicationUserId = relationship.CreatedApplicationUserId,
                         CreatedApplicationUserId = relationship.CreatedApplicationUserId,
                         CreatedDate = now,
                         LastModifiedDate = now,
                         DeleteFlag = false
                    };
                    _context.Gains.Add(gains);
                    await _context.SaveChangesAsync(cancellationToken);
               }

               GainsDto data = _mapper.Map<GainsDto>(gains);

               var eventLog = await _eventLogService.Create("GainsFeature", "GainsFeature",
                                       "Gains_GetByRelationshipIdQuery", request.userId);

               return Result<GainsDto>.Success(data);
          }
     }
}
