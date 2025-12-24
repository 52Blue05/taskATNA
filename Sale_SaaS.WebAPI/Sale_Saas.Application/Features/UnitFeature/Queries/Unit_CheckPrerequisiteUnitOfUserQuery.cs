using Sale_Saas.Application.Features.UnitFeature.Dto;

namespace Sale_Saas.Application.Features.UnitFeature.Queries;

public record Unit_CheckPrerequisiteUnitOfUserQuery(Guid UserId, CheckUnitPrerequisiteRequest RequestData) : IRequest<Result<bool>>;

public class Unit_CheckPrerequisiteUnitOfUserQueryHandler : IRequestHandler<Unit_CheckPrerequisiteUnitOfUserQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    
    public Unit_CheckPrerequisiteUnitOfUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(Unit_CheckPrerequisiteUnitOfUserQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.RequestData.UnitId == Guid.Empty || request.RequestData.SyllabusId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy thông tin khoá học hoặc chương trình học");
        }

        var result = from ud in _context.UnitDependencies
                     join res in _context.Results
                     on ud.PrerequisiteUnitId equals res.UnitId into ud_res
                     from res in ud_res.DefaultIfEmpty()
                     where ud.DeleteFlag != true
                        && ud.UnitId == request.RequestData.UnitId
                        && ud.SyllabusId == request.RequestData.SyllabusId
                        && (res == null || (res.DeleteFlag != true
                                        && res.isOldResult == false
                                        && res.ApplicationUserId == request.UserId
                                        && res.isQualified == false))
                     select ud;

        return Result<bool>.Success(!result.Any());
    }
}
