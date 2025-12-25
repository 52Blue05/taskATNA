using Sale_Saas.Application.Features.RolePositionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RolePositionFeature.Commands
{
	public record RolePosition_AddOrUpdateCommand(Guid UserId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<RolePositionDto>>>;

	public class RolePosition_AddOrUpdateCommandHandler : IRequestHandler<RolePosition_AddOrUpdateCommand, Result<List<RolePositionDto>>>
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IInternalService _internalService;
		public RolePosition_AddOrUpdateCommandHandler(IMapper mapper,
												IApplicationDbContext context,
												IInternalService internalService)
		{
			_context = context;
			_mapper = mapper;
			_internalService = internalService;
		}

		public async Task<Result<List<RolePositionDto>>> Handle(RolePosition_AddOrUpdateCommand request, CancellationToken cancellationToken)
		{
			RolePosition? obj = null;
			List<RolePositionDto> updatedSuccess = new List<RolePositionDto>();
			foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
			{
				RolePosition tmp = new RolePosition();
				if (addOrUpdateRequest.Data == null)
				{
					throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
				}

				if (addOrUpdateRequest.Id == null)
				{
					obj = new RolePosition()
					{
						CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
					};
				}
				else
				{
					obj = await _context.RolePositions.FindAsync(addOrUpdateRequest.Id.Value);

					if (obj == null)
						throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");
					PropertiesExtension.Copy(obj, tmp);
				}

				obj = (RolePosition)_internalService.MapValueToObject(new RolePosition(), addOrUpdateRequest.Data, obj);

				obj.LastModifiedDate = DateTime.Now;
				obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

				if (addOrUpdateRequest.Id == null)
				{
					obj.IsModified = true;
					_context.RolePositions.Add(obj);
				}
				else
				{
					obj.IsModified = tmp.IsModified;
					_context.RolePositions.Update(obj);
				}

				updatedSuccess.Add(_mapper.Map<RolePositionDto>(obj));
			}

			await _context.SaveChangesAsync(cancellationToken);

			return Result<List<RolePositionDto>>.Success(updatedSuccess);
		}
	}
}
