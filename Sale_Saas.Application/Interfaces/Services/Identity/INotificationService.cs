using Sale_Saas.Application.Models.Notification;

namespace Sale_Saas.Application.Interfaces.Services.Identity
{
	public interface INotificationService
	{
		Task<int> CountUnseenAsync(Guid user);
		Task<List<NotificationDto>> PushAsync(List<PushNotificationRequest> request);
		Task<Result<string>> DeleteByIds(DeleteRequest request);
		Task<NotificationDto?> GetById(Guid id, Guid userId);
		Task<Result<PaginatedList<NotificationDto>>> GetListQuery(FilterQueryRequest request);
        Task<Result<PaginatedList<NotificationDto>>> GetListQueryByMobile(GetListWithPaginationQueryRequest request);
        Task<Result<PaginatedList<NotificationDto>>> GeListWithPaginationQuery(GetListWithPaginationQueryRequest request);
		Task SeenMessageAsync(Guid user);
		Task<bool> ReadMessageAsync(DeleteRequest request);
		Task<Result<bool>> ReadMessageByMobile(DeleteRequest request);
		Task<Result<bool>> ReadAllMessageAsync(Guid user);
	}
}
