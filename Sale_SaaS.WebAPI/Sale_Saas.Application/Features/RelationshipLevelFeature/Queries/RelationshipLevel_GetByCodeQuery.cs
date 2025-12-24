using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Queries
{
    public record RelationshipLevel_GetByCodeQuery(string code) : IRequest<Result<RelationshipLevelDto>>;
    public class RelationshipLevel_GetByCodeQueryHandler : IRequestHandler<RelationshipLevel_GetByCodeQuery, Result<RelationshipLevelDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RelationshipLevel_GetByCodeQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<RelationshipLevelDto>> Handle(RelationshipLevel_GetByCodeQuery request, CancellationToken cancellationToken)
        {
            RelationshipLevelDto? RelationshipLevel = await (from rl in _context.RelationshipLevels
                                                             where rl.DeleteFlag != true && rl.Code == request.code
                                                             select new RelationshipLevelDto()
                                                             {
                                                                 Id = rl.Id,
                                                                 Code = rl.Code ?? "",
                                                                 Description = rl.Description ?? "",
                                                                 Review = rl.Review ?? "",
                                                                 PointFrom = rl.PointFrom ?? 0,
                                                                 PointTo = rl.PointTo ?? 0
                                                             }).AsNoTracking().FirstOrDefaultAsync();
            return Result<RelationshipLevelDto>.Success(RelationshipLevel);
        }
    }
}
