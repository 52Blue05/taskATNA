using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Commands
{
    public record RelationshipLevel_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<RelationshipLevelDto>>>;

    public class RelationshipLevel_AddOrUpdateCommandHandler : IRequestHandler<RelationshipLevel_AddOrUpdateCommand, Result<List<RelationshipLevelDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFeaturePermissionService _featurepermissionService;
        public RelationshipLevel_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IFeaturePermissionService featurepermissionService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _featurepermissionService = featurepermissionService;
        }

        public async Task<Result<List<RelationshipLevelDto>>> Handle(RelationshipLevel_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            RelationshipLevel? obj = null;
            List<RelationshipLevelDto> updatedSuccess = new List<RelationshipLevelDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
                if (string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code")))
                {
                    throw new ApplicationException($"Mã dữ liệu không thể để trống");
                }

                StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code"), true);
                StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Description"), true);
                StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Review"), true);

                if (addOrUpdateRequest.Id == null)
                {
                    await _featurepermissionService.HasPermission(
                            MenuType.DM_MDQH,
                            FeatureType.CREATE,
                            request.userId,
                            true
                         );

                    obj = new RelationshipLevel()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    await _featurepermissionService.HasPermission(
                            MenuType.DM_MDQH,
                            FeatureType.UPDATE,
                            request.userId,
                            true
                        );

                    obj = await _context.RelationshipLevels.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (RelationshipLevel)_internalService.MapValueToObject(new RelationshipLevel(), addOrUpdateRequest.Data, obj);
                if (obj.PointFrom != null && (obj.PointFrom < 0 || obj.PointFrom > 100))
                {
                    throw new ApplicationException("Giá trị không thể nhỏ hơn 0 hoặc lớn hơn 100");
                }
                if (obj.PointTo != null && (obj.PointTo < 0 || obj.PointTo > 100))
                {
                    throw new ApplicationException("Giá trị không thể nhỏ hơn 0 hoặc lớn hơn 100");
                }
                if (obj.PointTo != null && obj.PointFrom != null)
                    obj.Review = $"Từ {obj.PointFrom}% - {obj.PointTo} là YES";
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                var count = await _context.RelationshipLevels.Where(s => s.DeleteFlag != true)
                                                             .AsNoTracking()
                                                             .CountAsync();

                var query = _context.RelationshipLevels.Where(s => s.DeleteFlag != true && s.Code == obj.Code).AsNoTracking();
                if (addOrUpdateRequest.Id == null)
                {
                    await HandleSortOrderWithCaseCreate(obj, count);

                    _context.RelationshipLevels.Add(obj);
                }
                else
                {
                    await HandleSortOrderWithCaseUpdate(obj, count);
                    query = query.Where(s => s.Id != obj.Id);
                    _context.RelationshipLevels.Update(obj);
                }
                var duplicate = await query.CountAsync();
                if (duplicate > 0) throw new ApplicationException($"Mã dữ liệu đã được sử dụng {obj.Code}");
                updatedSuccess.Add(_mapper.Map<RelationshipLevelDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<RelationshipLevelDto>>.Success(updatedSuccess);
        }

        public async Task HandleSortOrderWithCaseCreate(RelationshipLevel obj, int count)
        {
            if (obj.SortOrder == null || obj.SortOrder <= 0 || obj.SortOrder >= count + 1)
            {
                obj.SortOrder = count + 1;
            }
            else
            {
                // 1 <= sortOrder <= n
                var listRelationshipLevel = await _context.RelationshipLevels.Where(s => s.DeleteFlag != true && s.SortOrder >= obj.SortOrder)
                                                                             .OrderBy(s => s.SortOrder)
                                                                             .ToListAsync();

                var index = obj.SortOrder + 1;
                foreach (var item in listRelationshipLevel)
                {
                    item.SortOrder = index;
                    index = index + 1;
                }

                _context.RelationshipLevels.UpdateRange(listRelationshipLevel);
            }
        }

        public async Task HandleSortOrderWithCaseUpdate(RelationshipLevel obj, int count)
        {
            var relationshipLevel = await _context.RelationshipLevels.Where(x => x.Id == obj.Id && x.DeleteFlag != true)
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync();

            if (relationshipLevel.SortOrder == obj.SortOrder)
            {
                return;
            }

            if (obj.SortOrder == null || obj.SortOrder <= 0 || obj.SortOrder >= count)
            {
                obj.SortOrder = count;
            }

            // 1 <= count <= n
            List<RelationshipLevel> listRelationshipLevel;

            // origin > target
            if (relationshipLevel.SortOrder > obj.SortOrder)
            {
                listRelationshipLevel = await _context.RelationshipLevels.Where(s => s.DeleteFlag != true
                                                                                  && s.SortOrder >= obj.SortOrder
                                                                                  && s.SortOrder < relationshipLevel.SortOrder)
                                                                         .OrderBy(s => s.SortOrder)
                                                                         .ToListAsync();

                var index = obj.SortOrder + 1;

                foreach (var item in listRelationshipLevel)
                {
                    item.SortOrder = index;
                    index = index + 1;
                }
            }
            else
            {
                listRelationshipLevel = await _context.RelationshipLevels.Where(s => s.DeleteFlag != true
                                                                                  && s.SortOrder <= obj.SortOrder
                                                                                  && s.SortOrder > relationshipLevel.SortOrder)
                                                                         .OrderBy(s => s.SortOrder)
                                                                         .ToListAsync();

                var index = relationshipLevel.SortOrder;
                foreach (var item in listRelationshipLevel)
                {
                    item.SortOrder = index;
                    index = index + 1;
                }
            }

            _context.RelationshipLevels.UpdateRange(listRelationshipLevel);
        }
    }
}
