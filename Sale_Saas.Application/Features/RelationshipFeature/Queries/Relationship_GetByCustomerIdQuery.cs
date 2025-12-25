using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.RelationshipFeature.Queries
{
    public record Relationship_GetByCustomerIdQuery(GetAllQueryRequest RequestData) : IRequest<Result<RelationshipGetListLevelDto>>;
    public class Relationship_GetByCustomerIdQueryHandler : IRequestHandler<Relationship_GetByCustomerIdQuery, Result<RelationshipGetListLevelDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Relationship_GetByCustomerIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<RelationshipGetListLevelDto>> Handle(Relationship_GetByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            var customers = (await (from cv in _context.Relationships
                                    join relationshipCustomer in _context.RelationshipCustomers on cv.RelationshipCustomerId equals relationshipCustomer.Id
                                    join user in _context.ApplicationUsers on cv.ApplicationUserId equals user.Id
                                    join curLv in _context.RelationshipLevels on cv.CurrentRelationshipId equals curLv.Id
                                    join tarLv in _context.RelationshipLevels on cv.TargetRelationshipId equals tarLv.Id
                                    join realLv in _context.RelationshipLevels on cv.YearToDateId equals realLv.Id
                                    join status in _context.RelationshipStatuses on cv.RelationshipStatusId equals status.Id
                                    where cv.DeleteFlag != true
                                       && user.DeleteFlag != true
                                       && curLv.DeleteFlag != true
                                       && tarLv.DeleteFlag != true
                                       && realLv.DeleteFlag != true
                                       && relationshipCustomer.DeleteFlag != true
                                       && cv.RelationshipCustomerId == request.RequestData.UserId
                                    orderby cv.CreatedDate ascending
                                    select new RelationshipDto()
                                    {
                                        Id = cv.Id,
                                        Position = cv.Position ?? "",
                                        CurrentRelationshipLevel = curLv.Code ?? "",
                                        TargetRelationshipLevel = tarLv.Code ?? "",
                                        YearToDateLevel = realLv.Code ?? "",
                                        //Customer = customer.Fullname ?? "",
                                        CustomerName = cv.CustomerName ?? "",
                                        CompletionDate = cv.CompletionDate,
                                        CreatedDate = cv.CreatedDate,
                                        LastModifiedDate = cv.LastModifiedDate,
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
                                    })
                                    .AsNoTracking()
                                    .ToListAsync())
                                    .AsReadOnly();

            string customerName = string.Empty;
            string position = string.Empty;
            List<ListRelationshipCurrent> currentLevel = new List<ListRelationshipCurrent>();
            List<ListRelationshipTarget> targetLevel = new List<ListRelationshipTarget>();

            foreach (var customer in customers)
            {
                var currentRelation = new ListRelationshipCurrent()
                {
                    CurrentLevel = customer.CurrentRelationshipLevel,
                    CreateDate = customer.CreatedDate,
                };
                var targetRelation = new ListRelationshipTarget()
                {
                    TargetLevel = customer.TargetRelationshipLevel,
                    CompletionDate = customer.CompletionDate,
                };
                customerName = customer.CustomerName;
                position = customer.Position;

                currentLevel.Add(currentRelation);
                targetLevel.Add(targetRelation);
            }

            if (customers[customers.Count - 1].YearToDateLevel != customers[customers.Count - 1].CurrentRelationshipLevel)
            {
                var realRelation = new ListRelationshipCurrent()
                {
                    CurrentLevel = customers[customers.Count - 1].YearToDateLevel,
                    CreateDate = customers[customers.Count - 1].LastModifiedDate
                };

                currentLevel.Add(realRelation);
            }


            RelationshipGetListLevelDto listCustomerRelations = new RelationshipGetListLevelDto()
            {
                CurrentLevels = currentLevel,
                TargetLevels = targetLevel,
                CustomerName = customerName,
                Position = position
            };

            return Result<RelationshipGetListLevelDto>.Success(listCustomerRelations);
        }
    }
}
