using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.FeaturePermissionFeature.Commands
{
	public record FeaturePermission_UpdateCommand(Guid userId, List<MenuWithFeatureDto> requestData, Guid RoleId, string PositionId) : IRequest<Result<List<MenuWithFeatureDto>>>;

	public class FeaturePermission_UpdateCommandHandler : IRequestHandler<FeaturePermission_UpdateCommand, Result<List<MenuWithFeatureDto>>>
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public FeaturePermission_UpdateCommandHandler(IMapper mapper,
												IApplicationDbContext context,
												IInternalService internalService, IEventLogService eventLogService)
		{
			_context = context;
			_mapper = mapper;
			_internalService = internalService;
			_eventLogService = eventLogService;
		}

		public async Task<Result<List<MenuWithFeatureDto>>> Handle(FeaturePermission_UpdateCommand request, CancellationToken cancellationToken)
		{
			var role = await _context.ApplicationRoles.FindAsync(request.RoleId);
			var position = await _context.RolePositions.FindAsync(request.PositionId);
			if(role == null || position == null)
			{
				throw new ApplicationException("Không tìm thấy dữ liệu");
			}
			var details = await _context.ApplicationRoleDetails
						  .Where(s => s.DeleteFlag != true && s.ApplicationRoleId == request.RoleId)
						  .ToListAsync();
			
			_context.ApplicationRoleDetails.RemoveRange(details);

			foreach(var menu in request.requestData)
			{
				foreach(var fea in menu.Features.Where(s => s.Access == true).ToList())
				{
					var detail = new ApplicationRoleDetail()
					{
						Id = Guid.NewGuid(),
						ApplicationRoleId = request.RoleId,
						MenuId = menu.Id,
						FeatureId = fea.Id,
						CreatedDate = DateTime.Now,
						LastModifiedDate = DateTime.Now
					};
					_context.ApplicationRoleDetails.Add(detail);
				}
			}
			role.RolePositionId = position.Id;
			role.RolePosition = position;

            var eventLog = await _eventLogService.Create("FeaturePermissionFeature", "FeaturePermissionFeature",
                                                            "FeaturePermission_UpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);
			return Result<List<MenuWithFeatureDto>>.Success(request.requestData);
		}
	}
}
