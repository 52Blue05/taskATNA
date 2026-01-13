using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.RelationshipFeature.Queries
{
     public record Relationship_GetByIdQuery(Guid Id) : IRequest<Result<RelationshipDto>>;
     public class Relationship_GetByIdQueryHandler : IRequestHandler<Relationship_GetByIdQuery, Result<RelationshipDto>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;

          public Relationship_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
          {
               _context = context;
               _mapper = mapper;
          }

          public async Task<Result<RelationshipDto>> Handle(Relationship_GetByIdQuery request, CancellationToken cancellationToken)
          {
               RelationshipDto? Relationship = await (from cv in _context.Relationships
                                                      join customer in _context.Customers on cv.CustomerId equals customer.Id
                                                      join user in _context.ApplicationUsers on cv.ApplicationUserId equals user.Id
                                                      join curLv in _context.RelationshipLevels on cv.CurrentRelationshipId equals curLv.Id
                                                      join tarLv in _context.RelationshipLevels on cv.TargetRelationshipId equals tarLv.Id
                                                      join status in _context.RelationshipStatuses on cv.RelationshipStatusId equals status.Id
                                                      where cv.DeleteFlag != true & cv.Id == request.Id && user.DeleteFlag != true && curLv.DeleteFlag != true && tarLv.DeleteFlag != true
                                                      select new RelationshipDto()
                                                      {
                                                           Id = cv.Id,
                                                           Position = cv.Position ?? "",
                                                           Reason = cv.Reason ?? "",
                                                           Point = cv.Point ?? 0,
                                                           ActualPoint = cv.ActualPoint ?? 0,
                                                           CurrentRelationshipLevel = curLv.Code ?? "",
                                                           TargetRelationshipLevel = tarLv.Code ?? "",
                                                           Customer = customer.Fullname ?? "",
                                                           CustomerName = cv.CustomerName ?? "",
                                                           WorkPlace = cv.WorkPlace ?? "",
                                                           ApplicationUser = new UserBasicInfoDto()
                                                           {
                                                                Id = user.Id,
                                                                FirstName = user.FirstName ?? "",
                                                                LastName = user.LastName ?? "",
                                                                Email = user.Email ?? "",
                                                                Phone = user.PhoneNumber ?? "",
                                                                FullName = (user.FirstName ?? "") + (user.LastName ?? "")
                                                           },
                                                           RelationshipStatus = new RelationshipStatusDto()
                                                           {
                                                                Id = status.Id,
                                                                Name = status.Name,
                                                                Code = status.Code
                                                           }
                                                      }).AsNoTracking().FirstOrDefaultAsync();
               return Result<RelationshipDto>.Success(Relationship);
          }
     }
}
