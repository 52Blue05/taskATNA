using AutoMapper;
using AutoMapper.QueryableExtensions;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.SupplierFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities.Tenant;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
	public class NotificationService : INotificationService
	{
		private readonly CancellationToken _cancellationToken = new CancellationToken();
		private readonly TenantDbContext _context;
		private readonly IMapper _mapper;
		
		public NotificationService(TenantDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<int> CountUnseenAsync(Guid user)
		{
			return await _context.Notifications.Where(s => s.ApplicationUserId == user && s.DeleteFlag != true && s.ReadFlag !=true).CountAsync();
		}

		public async Task<Result<string>> DeleteByIds(DeleteRequest request)
		{
			string result = string.Empty;

			if (request.Ids == null) ExceptionHelper.RequestEmpty(request.Locale);

			List<Guid> ids = request.Ids!.Select(m => Guid.Parse(m)).ToList();
			var query = await _context.Notifications.Where(m => ids.Contains(m.Id)).ToListAsync();
			if (query == null || query.Count == 0) ExceptionHelper.NotFound(ids, request.Locale);

			foreach (var item in query)
			{
				item.DeleteFlag = true;
				item.LastModifiedDate = DateTime.Now;
				item.LastModifiedApplicationUserId = request.ApplicationUserId;
			}

			_context.Notifications.UpdateRange(query);

			await _context.SaveChangesAsync(_cancellationToken);

			return Result<string>.Success(string.Empty);
		}

		public async Task<Result<PaginatedList<NotificationDto>>> GeListWithPaginationQuery(GetListWithPaginationQueryRequest request)
		{
			if (request.UserId == null || request.UserId == Guid.Empty)
				throw new ApplicationException("Không tìm thấy người dùng");

			var query = _context.Notifications
								.Where(m => m.DeleteFlag != true && m.ApplicationUserId == request.UserId)
								.OrderByDescending(x => x.CreatedDate)
								.ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
								.AsNoTracking();

			if (!string.IsNullOrEmpty(request.TextSearch))
			{
				query = query.Where(x => x.Title.Contains(request.TextSearch) ||
										 x.Message.Contains(request.TextSearch));
			}

			if (!string.IsNullOrEmpty(request.TenantId))
			{
				query = query.Where(x => x.TenantId == request.TenantId);
			}

			if (request.Time.HasValue)
			{
				query = query.Where(x => x.CreatedDate < request.Time.Value);
			}
			return Result<PaginatedList<NotificationDto>>.Success(await query.PaginatedListAsync(request.PageIndex, request.PageSize));
		}

		public async Task<NotificationDto?> GetById(Guid id, Guid userId)
		{
           
            var target= await _context.Notifications
								 .Where(s => s.Id == id && s.DeleteFlag != true)
								 .Select(s => new NotificationDto
								 {
									 Id = s.Id,
									 Title = s.Title,
									 Message = s.Message,
									 SeenFlag = s.SeenFlag,
									 Type = s.Type,
									 ApplicationUserId = s.ApplicationUserId,
									 CreatedDate = s.CreatedDate,
									 SeenDate = s.SeenDate,
									 ActionId = s.ActionId,
									 Navigate = s.Navigate,
									 TenantId = s.TenantId,
									 ReadDate = s.ReadDate,
									 ReadFlag = s.ReadFlag
								 })
								 .FirstOrDefaultAsync();
			if(target!=null && target.ReadFlag!=true)
			{
                target.ReadFlag = true;
				target.ReadDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
			return target;

        }

		public async Task<Result<PaginatedList<NotificationDto>>> GetListQuery(FilterQueryRequest request)
		{
			if (request.UserId == null || request.UserId == Guid.Empty)
				throw new ApplicationException("Không tìm thấy người dùng");

			var query = from noti in _context.Notifications
						where noti.DeleteFlag != true && noti.ApplicationUserId == request.UserId
						orderby noti.CreatedDate descending
						select new NotificationDto()
						{
							Id = noti.Id,
							Message = noti.Message ?? "",
							Title = noti.Title ?? "",
							Type = noti.Type ?? "",
							ApplicationUserId = noti.ApplicationUserId ?? Guid.Empty,
							TenantId = noti.TenantId ?? "",
							SeenFlag = noti.SeenFlag,
							ActionId = noti.ActionId,
							CreatedDate = noti.CreatedDate,
							SeenDate = noti.SeenDate,
							Navigate = noti.Navigate,
							ReadFlag = noti.ReadFlag,
							ReadDate = noti.ReadDate
						};

			if (!string.IsNullOrEmpty(request.TextSearch))
			{
				query = query.Where(x => x.Title.Contains(request.TextSearch) ||
										 x.Message.Contains(request.TextSearch));
			}

			if (!string.IsNullOrEmpty(request.TenantId))
			{
				query = query.Where(x => x.TenantId == request.TenantId);
			}

			if (request.Time.HasValue)
			{
				query = query.Where(x => x.CreatedDate < request.Time.Value);
			}

			var count = await query.CountAsync();

			if (request.Skip != null)
			{
				query = query.Skip(request.Skip.Value);
			}

			if (request.TotalRecord != null)
			{
				query = query.Take(request.TotalRecord.Value);
			}

			var data = await query.AsNoTracking().ToListAsync();
			var pagination = new PaginatedList<NotificationDto>(data, count, 1, count);

			return Result<PaginatedList<NotificationDto>>.Success(pagination);
		}

        public async Task<Result<PaginatedList<NotificationDto>>> GetListQueryByMobile(GetListWithPaginationQueryRequest request)
		{
            if (request.UserId == null || request.UserId == Guid.Empty)
                throw new ApplicationException("Không tìm thấy người dùng");

            var query = from noti in _context.Notifications
                        where noti.DeleteFlag != true && noti.ApplicationUserId == request.UserId
                        orderby noti.CreatedDate descending
                        select new NotificationDto()
                        {
                            Id = noti.Id,
                            Message = noti.Message ?? "",
                            Title = noti.Title ?? "",
                            Type = noti.Type ?? "",
                            ApplicationUserId = noti.ApplicationUserId ?? Guid.Empty,
                            TenantId = noti.TenantId ?? "",
                            SeenFlag = noti.SeenFlag,
                            ActionId = noti.ActionId,
                            CreatedDate = noti.CreatedDate,
                            SeenDate = noti.SeenDate,
                            Navigate = noti.Navigate,
                            ReadFlag = noti.ReadFlag,
                            ReadDate = noti.ReadDate
                        };

            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                query = query.Where(x => x.Title.Contains(request.TextSearch) ||
                                         x.Message.Contains(request.TextSearch));
            }

            if (!string.IsNullOrEmpty(request.TenantId))
            {
                query = query.Where(x => x.TenantId == request.TenantId);
            }

            if (request.Time.HasValue)
            {
                query = query.Where(x => x.CreatedDate < request.Time.Value);
            }

			return Result<PaginatedList<NotificationDto>>.Success(await query.PaginatedListAsync(request.PageIndex, request.PageSize));
        }


        public async Task<List<NotificationDto>> PushAsync(List<PushNotificationRequest> request)
		{
			try
			{
				List<NotificationDto> sucesses = new List<NotificationDto>();
				foreach (var item in request)
				{
					Notification notification = new Notification()
					{
						Title = item.Title,
						Message = item.Message,
						Type = item.Type,
						ApplicationUserId = item.ApplicationUserId,
						TenantId = item.TenantId,
						ActionId = item.ActionId,
						Navigate = item.Navigate,
						SeenFlag = false,
						ReadFlag = false,
						CreatedApplicationUserId = item.CreatedApplicationUserId,
						LastModifiedApplicationUserId = item.LastModifiedApplicationUserId,
					};
					_context.Add(notification);
					sucesses.Add(_mapper.Map<NotificationDto>(notification));
				}
				await _context.SaveChangesAsync(_cancellationToken);
				return sucesses;
			}
			catch(Exception ex)
			{
				return new List<NotificationDto>();
			}
		}

		public async Task<Result<bool>> ReadAllMessageAsync(Guid user)
		{
			var notifications = await _context.Notifications.Where(s => s.ApplicationUserId == user && s.ReadFlag == false).ToListAsync();
			if (notifications.Any())
			{
				foreach(var notification in notifications)
				{
					notification.ReadFlag = true;
					notification.ReadDate = DateTime.Now;
				}
				_context.Notifications.UpdateRange(notifications);
			}
			var rows = await _context.SaveChangesAsync();
			return Result<bool>.Success(rows > 0);
		}

		public async Task<bool> ReadMessageAsync(DeleteRequest request)
		{
			if (request.Ids == null) ExceptionHelper.RequestEmpty(request.Locale);
			List<Guid> ids = request.Ids!.Select(m => Guid.Parse(m)).ToList();
			var notifications = await _context.Notifications.Where(s => ids.Contains(s.Id) && s.ReadFlag == false).ToListAsync();
			if (notifications.Any())
			{
				foreach (var notification in notifications)
				{
					notification.ReadFlag = true;
					notification.ReadDate = DateTime.Now;
				}
				_context.Notifications.UpdateRange(notifications);
			}
			var rows = await _context.SaveChangesAsync();
			return rows > 0;
		}

        public async Task<Result<bool>> ReadMessageByMobile(DeleteRequest request)
        {
            if (request.Ids == null) ExceptionHelper.RequestEmpty(request.Locale);
            List<Guid> ids = request.Ids!.Select(m => Guid.Parse(m)).ToList();
            var notifications = await _context.Notifications.Where(s => ids.Contains(s.Id) && s.ReadFlag == false).ToListAsync();
            if (notifications.Any())
            {
                foreach (var notification in notifications)
                {
                    notification.ReadFlag = true;
                    notification.ReadDate = DateTime.Now;
                }
                _context.Notifications.UpdateRange(notifications);
            }
            var rows = await _context.SaveChangesAsync();
            return Result<bool>.Success(rows > 0);
        }

        public async Task SeenMessageAsync(Guid user)
		{
			try
			{
				var messages = await _context.Notifications
								  .Where(s => s.SeenFlag != true && s.DeleteFlag != true && s.ApplicationUserId == user)
								  .ToListAsync();
				if (messages.Any())
				{
					foreach (var message in messages)
					{
						message.SeenFlag = true;
						message.SeenDate = DateTime.Now;
					}
					_context.Notifications.UpdateRange(messages);
					await _context.SaveChangesAsync();
				}
			}
			catch(Exception ex) {}
		}
	}
}
