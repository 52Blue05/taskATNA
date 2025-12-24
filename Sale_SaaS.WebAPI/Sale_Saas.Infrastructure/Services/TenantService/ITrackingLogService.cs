using Sale_Saas.Application.Common.Models;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public interface ITrackingLogService
    {
        //Task<Result<List<TrackingLogAverageResult>>> GetAllAverageRequest(TrackingLogRequest request);
        //Task<Result<float>> GetAverageRequest(TrackingLogRequest request);
        //Task<Result<int>> GetCountErrorRequest(TrackingLogRequest request);
        //Task<Result<int>> GetAverageRequestByMinute(string? method = null);
        //Task<Result<int>> GetCountRequestByTime(TrackingLogRequest request);
        //Task<Result<float>> GetAverageDowntime(TrackingLogRequest request);

        Task<Result<List<TrackingLogDto>>> GetListAllAverageRequest(TrackingLogRequest request);
        Task<Result<List<TrackingLogDto>>> GetListErrorAverageRequest(TrackingLogRequest request);
        Task<Result<List<TrackingLogDto>>> GetListDowntimeAverageRequest(TrackingLogRequest request);
        Task<Result<List<TrackingLogAverageResult>>> GetListDetailByTime(TrackingLogRequest request);
    }
}
