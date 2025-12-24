using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Commands;

public record Unit_CreateOrUpdatePrerequisiteUnitCommand(Guid UserId, CreateOrUpdateUnitPrerequisiteRequest RequestData) : IRequest<Result<bool>>;

public class Unit_CreateOrUpdatePrerequisiteUnitCommandHandler : IRequestHandler<Unit_CreateOrUpdatePrerequisiteUnitCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Unit_CreateOrUpdatePrerequisiteUnitCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(Unit_CreateOrUpdatePrerequisiteUnitCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        var prerequisiteUnit = await _context.UnitDependencies.Where(x => x.DeleteFlag != true
                                                                       && (x.UnitId.HasValue 
                                                                                && request.RequestData.UnitId.HasValue 
                                                                                && x.UnitId == request.RequestData.UnitId)
                                                                       && (x.PrerequisiteUnitId.HasValue
                                                                                && request.RequestData.PrerequisiteUnitId.HasValue
                                                                                && x.PrerequisiteUnitId == request.RequestData.PrerequisiteUnitId)
                                                                       && (x.SyllabusId.HasValue
                                                                                && request.RequestData.SyllabusId.HasValue
                                                                                && x.SyllabusId == request.RequestData.SyllabusId))
                                                              .FirstOrDefaultAsync();

        if (prerequisiteUnit == null)
        {
            // create
            var newUnitDependency = new UnitDependency()
            {
                SyllabusId = request.RequestData.SyllabusId,
                UnitId = request.RequestData.UnitId,
                PrerequisiteUnitId = request.RequestData.PrerequisiteUnitId,
            };

            _context.UnitDependencies.Add(newUnitDependency);
        }
        else
        {
            prerequisiteUnit.SyllabusId = request.RequestData.SyllabusId == Guid.Empty ? prerequisiteUnit.SyllabusId : request.RequestData.SyllabusId;
            prerequisiteUnit.UnitId = request.RequestData.UnitId == Guid.Empty ? prerequisiteUnit.UnitId : request.RequestData.UnitId;
            prerequisiteUnit.PrerequisiteUnitId = request.RequestData.PrerequisiteUnitId == Guid.Empty ? prerequisiteUnit.PrerequisiteUnitId : request.RequestData.PrerequisiteUnitId;

            prerequisiteUnit.LastModifiedApplicationUserId = request.UserId;
            prerequisiteUnit.LastModifiedDate = DateTime.Now;

            _context.UnitDependencies.Update(prerequisiteUnit);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _eventLogService.Create("UnitFeature", "UnitFeature", "Unit_CreateOrUpdatePrerequisiteUnitCommand", request.UserId);

        return Result<bool>.Success(true);
    }
}
