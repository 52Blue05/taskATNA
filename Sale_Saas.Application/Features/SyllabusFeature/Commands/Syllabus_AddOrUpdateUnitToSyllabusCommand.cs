using Sale_Saas.Application.Features.SyllabusFeature.Requests;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands;

public record Syllabus_AddOrUpdateUnitToSyllabusCommand(Guid UserId, AddOrUpdateUnitIntoSyllabusRequest data) : IRequest<Result<UnitDto>>;

public class Syllabus_AddOrUpdateUnitToSyllabusCommandHandler : IRequestHandler<Syllabus_AddOrUpdateUnitToSyllabusCommand, Result<UnitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Syllabus_AddOrUpdateUnitToSyllabusCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<UnitDto>> Handle(Syllabus_AddOrUpdateUnitToSyllabusCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        var syllabus = await _context.Syllabus.Where(sy => sy.Id == request.data.SyllabusId && sy.DeleteFlag != true)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync();

        if (syllabus == null)
            throw new Exception($"Không thể tìm thấy chương trình học");

        var unit = await _context.Units.Where(u => u.Id == request.data.UnitId && u.DeleteFlag != true)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync();

        if (unit == null)
            throw new Exception($"Không thể tìm thấy khóa học");

        var listSyllabusUnitCount = await _context.SyllabusUnits.CountAsync(su => su.SyllabusId == request.data.SyllabusId
                                                                          && su.DeleteFlag != true);

        var syllabusUnit = await _context.SyllabusUnits.Where(su => su.SyllabusId == request.data.SyllabusId
                                                                 && su.UnitId == request.data.UnitId
                                                                 && su.DeleteFlag != true)
                                                       .AsNoTracking()
                                                       .FirstOrDefaultAsync();
        var sortOrder = request.data.SortOrder;
        if (syllabusUnit == null)
        {
            sortOrder = await HandleSortOrderWithCaseCreate(request.data, listSyllabusUnitCount, cancellationToken);
            // Add
            SyllabusUnits syllabusUnits = new SyllabusUnits
            {
                Id = Guid.NewGuid(),
                UnitId = unit.Id,
                SyllabusId = syllabus.Id,
                SortOrder = sortOrder,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                CreatedApplicationUserId = request.UserId,
                LastModifiedApplicationUserId = request.UserId,
                DeleteFlag = false
            };

            _context.SyllabusUnits.Add(syllabusUnits);
        }
        else
        {
            // edit
            await HandleSortOrderWithCaseUpdate(syllabusUnit, request.data, listSyllabusUnitCount);
            syllabusUnit.SortOrder = request.data.SortOrder;

            _context.SyllabusUnits.Update(syllabusUnit);
        }

        await _context.SaveChangesAsync(cancellationToken);

        UnitDto unitDto = new UnitDto
        {
            Id = unit.Id,
            EndTime = syllabus.EndTime,
            Name = unit.Name,
            ThumbnailPath = unit.ThumbnailPath,
            SortOrder = sortOrder
        };

        return Result<UnitDto>.Success(unitDto);
    }

    private async Task<int> HandleSortOrderWithCaseCreate(AddOrUpdateUnitIntoSyllabusRequest request, int count, CancellationToken cancellationToken)
    {
        if (request.SortOrder <= 0 || request.SortOrder >= count + 1)
        {
            return count + 1;
        }
        else
        {
            // 1 <= sortOrder <= n
            var listSyllabusUnit = await _context.SyllabusUnits.Where(s => s.DeleteFlag != true && s.SortOrder >= request.SortOrder)
                                                               .OrderBy(s => s.SortOrder)
                                                               .ToListAsync();

            var index = request.SortOrder + 1;

            foreach (var item in listSyllabusUnit)
            {
                item.SortOrder = index;
                index = index + 1;
            }

            _context.SyllabusUnits.UpdateRange(listSyllabusUnit);

            await _context.SaveChangesAsync(cancellationToken);

            return request.SortOrder;
        }
    }

    private async Task HandleSortOrderWithCaseUpdate(SyllabusUnits obj, AddOrUpdateUnitIntoSyllabusRequest request, int count)
    {
        if (obj.SortOrder == request.SortOrder)
        {
            return;
        }

        if (request.SortOrder <= 0 || request.SortOrder >= count)
        {
            request.SortOrder = count;
        }

        // 1 <= count <= n
        List<SyllabusUnits> listSyllabusUnit;

        // origin > target
        if (obj.SortOrder > request.SortOrder)
        {
            listSyllabusUnit = await _context.SyllabusUnits.Where(s => s.DeleteFlag != true
                                                                    && s.SortOrder >= request.SortOrder
                                                                    && s.SortOrder < obj.SortOrder)
                                                           .OrderBy(s => s.SortOrder)
                                                           .ToListAsync();

            var index = request.SortOrder + 1;

            foreach (var item in listSyllabusUnit)
            {
                item.SortOrder = index;
                index = index + 1;
            }
        }
        else
        {
            listSyllabusUnit = await _context.SyllabusUnits.Where(s => s.DeleteFlag != true
                                                                    && s.SortOrder <= request.SortOrder
                                                                    && s.SortOrder > obj.SortOrder)
                                                           .OrderBy(s => s.SortOrder)
                                                           .ToListAsync();

            var index = obj.SortOrder;
            foreach (var item in listSyllabusUnit)
            {
                item.SortOrder = index;
                index = index + 1;
            }
        }

        _context.SyllabusUnits.UpdateRange(listSyllabusUnit);
    }
}
