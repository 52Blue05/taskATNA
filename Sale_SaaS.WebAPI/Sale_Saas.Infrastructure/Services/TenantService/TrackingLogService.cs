using Sale_Saas.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Infrastructure.Data;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public class TrackingLogService : ITrackingLogService
    {
        private readonly TenantDbContext _context;

        public TrackingLogService(TenantDbContext tenantDbContext)
        {
            _context = tenantDbContext;
        }      

        public async Task<Result<List<TrackingLogDto>>> GetListAllAverageRequest(TrackingLogRequest request)
        {
            return await GetListTrackingLogByType(request, "api");
        }

        public async Task<Result<List<TrackingLogDto>>> GetListDowntimeAverageRequest(TrackingLogRequest request)
        {
            return await GetListTrackingLogByType(request, "downtime");
        }

        public async Task<Result<List<TrackingLogDto>>> GetListErrorAverageRequest(TrackingLogRequest request)
        {
            return await GetListTrackingLogByType(request, "error");
        }

        public async Task<Result<List<TrackingLogDto>>> GetListTrackingLogByType(TrackingLogRequest request, string? errorType = null)
        {
            if (request.type == null)
            {
                if (request.FromTime == null || request.ToTime == null)
                    throw new ApplicationException("Dữ liệu gửi lên máy chủ rỗng");

                if (request.FromTime > request.ToTime)
                    throw new ApplicationException("Thời gian bắt đầu không thể lớn hơn thời gian kết thúc");

                var result = await GetListTrackingLog((DateTime)request.FromTime, (DateTime)request.ToTime, 24, errorType);
                return Result<List<TrackingLogDto>>.Success(result.Data);
            }
            else
            {
                (DateTime fromTime, DateTime toTime, int step) = request.type switch
                {
                    "day" => (DateTime.Now.AddDays(-1), DateTime.Now, 1),
                    "week" => (DateTime.Now.AddDays(-7), DateTime.Now, 24),
                    "month" => (DateTime.Now.AddDays(-30), DateTime.Now, 24),
                    "year" => (DateTime.Now.AddDays(-365), DateTime.Now, 0),
                    _ => throw new ApplicationException("Định dạng mục sai.")
                };

                var result = await GetListTrackingLog(fromTime, toTime, step, errorType);
                return Result<List<TrackingLogDto>>.Success(result.Data);
            }
        }

        private async Task<Result<List<TrackingLogDto>>> GetListTrackingLog(DateTime fromTime, DateTime toTime, int step, string? type = null)
        {
            var query = _context.TrackingLogs.Where(tl => tl.CreatedDate >= fromTime && tl.CreatedDate <= toTime)
                                              .AsNoTracking();

            if (type != null)
            {
                query = type switch
                {
                    "error" => query.Where(tl => tl.Message != null && tl.Type != "TRASH SERVER" && tl.Type != "TRASH DATABASE"),
                    "downtime" => query.Where(tl => tl.Type == "DOWNTIME SERVER" || tl.Type == "DOWNTIME DATABASE"),
                    "api" => query.Where(tl => tl.Type == "API")
                };
            }

            var logs = await query.ToListAsync();

            List<TrackingLogDto> trackingLogDtos = new List<TrackingLogDto>();

            while (fromTime < toTime)
            {
                var currentTime = DateTime.Now;
                if (step == 0)
                {
                    currentTime = fromTime.AddMonths(1);
                }
                else
                {
                    currentTime = fromTime.AddHours(step);
                }

                var intervalLogs = logs.Where(q => q.CreatedDate >= fromTime && q.CreatedDate <= currentTime).ToList();

                var count = intervalLogs.Count;
                var sumTime = intervalLogs.Sum(q => q.ResponseTimeSec);

                TrackingLogDto trackingLogDto = new TrackingLogDto
                {
                    Average = count == 0 ? 0 : sumTime / count,
                    NumberRequest = count,
                    FromTime = fromTime,
                    ToTime = currentTime
                };

                trackingLogDtos.Add(trackingLogDto);
                fromTime = currentTime;
            }

            if (step != 0)
            {
                trackingLogDtos.RemoveAt(trackingLogDtos.Count - 1);
            }

            return Result<List<TrackingLogDto>>.Success(trackingLogDtos);
        }

        public async Task<Result<List<TrackingLogAverageResult>>> GetListDetailByTime(TrackingLogRequest request)
        {
            if (request.FromTime == null || request.ToTime == null)
                throw new ApplicationException("Dữ liệu gửi lên máy chủ rỗng");

            if (request.FromTime > request.ToTime)
                throw new ApplicationException("Thời gian bắt đầu không thể lớn hơn thời gian kết thúc");

            var query = _context.TrackingLogs.GroupBy(tl => tl.FunctionTime).OrderBy(tl => tl.First().CreatedDate)
                                                        .Select(tl => new TrackingLogAverageResult
                                                        {
                                                            Id = tl.First().Id,
                                                            Average = tl.Average(tl => tl.ResponseTimeMin),
                                                            Url = tl.Key,
                                                            CreateDate = tl.First().CreatedDate
                                                        }).AsNoTracking();

            if (query == null)
                throw new ApplicationException("Không tìm thấy dữ liệu của bất kỳ request nào");

            var result = await query.AsNoTracking().ToListAsync();

            return Result<List<TrackingLogAverageResult>>.Success(result);
        }


        //public async Task<Result<List<TrackingLogDto>>> GetListAllAverageRequest(TrackingLogRequest request)
        //{
        //    if(request.type == null)
        //    {
        //        if(request.FromTime == null || request.ToTime == null)
        //            throw new ApplicationException("Dữ liệu gửi lên máy chủ rỗng");

        //        if (request.FromTime > request.ToTime)
        //            throw new ApplicationException("Thời gian bắt đầu không thể lớn hơn thời gian kết thúc");

        //        var result = await GetListTrackingLog((DateTime)request.FromTime, (DateTime)request.ToTime, 1);

        //        return Result<List<TrackingLogDto>>.Success(result.Data);
        //    }
        //    else
        //    {
        //        switch (request.type)
        //        {
        //            case "day":
        //                DateTime fromTimeDay = DateTime.Now.AddDays(-1);
        //                DateTime toTimeDay = DateTime.Now;
        //                var resultDay = await GetListTrackingLog(fromTimeDay, toTimeDay, 1);
        //                return Result<List<TrackingLogDto>>.Success(resultDay.Data);

        //            case "week":
        //                DateTime fromTimeWeek = DateTime.Now.AddDays(-7);
        //                DateTime toTimeWeek = DateTime.Now;
        //                var resultWeek = await GetListTrackingLog(fromTimeWeek, toTimeWeek, 1);
        //                return Result<List<TrackingLogDto>>.Success(resultWeek.Data);

        //            case "month":
        //                DateTime fromTimeMonth = DateTime.Now.AddDays(-30);
        //                DateTime toTimeWeekMonth = DateTime.Now;
        //                var resultMonth = await GetListTrackingLog( fromTimeMonth, toTimeWeekMonth, 24);
        //                return Result<List<TrackingLogDto>>.Success(resultMonth.Data);

        //            case "year":
        //                DateTime fromTimeYear = DateTime.Now.AddDays(-365);
        //                DateTime toTimeWeekYear = DateTime.Now;
        //                var resultYear = await GetListTrackingLog( fromTimeYear, toTimeWeekYear, 24);
        //                return Result<List<TrackingLogDto>>.Success(resultYear.Data);
        //        }

        //        return Result<List<TrackingLogDto>>.Failure("Định dạng mục sai.");
        //    }
        //}

        //public async Task<Result<List<TrackingLogDto>>> GetListDowntimeAverageRequest(TrackingLogRequest request)
        //{
        //    if (request.type == null)
        //    {
        //        if (request.FromTime == null || request.ToTime == null)
        //            throw new ApplicationException("Dữ liệu gửi lên máy chủ rỗng");

        //        if (request.FromTime > request.ToTime)
        //            throw new ApplicationException("Thời gian bắt đầu không thể lớn hơn thời gian kết thúc");

        //        var result = await GetListTrackingLog((DateTime)request.FromTime, (DateTime)request.ToTime, 1, "error");

        //        return Result<List<TrackingLogDto>>.Success(result.Data);
        //    }
        //    else
        //    {
        //        switch (request.type)
        //        {
        //            case "day":
        //                DateTime fromTimeDay = DateTime.Now.AddDays(-1);
        //                DateTime toTimeDay = DateTime.Now;
        //                var resultDay = await GetListTrackingLog(fromTimeDay, toTimeDay, 1, "error");
        //                return Result<List<TrackingLogDto>>.Success(resultDay.Data);

        //            case "week":
        //                DateTime fromTimeWeek = DateTime.Now.AddDays(-7);
        //                DateTime toTimeWeek = DateTime.Now;
        //                var resultWeek = await GetListTrackingLog(fromTimeWeek, toTimeWeek, 1, "error");
        //                return Result<List<TrackingLogDto>>.Success(resultWeek.Data);

        //            case "month":
        //                DateTime fromTimeMonth = DateTime.Now.AddDays(-30);
        //                DateTime toTimeWeekMonth = DateTime.Now;
        //                var resultMonth = await GetListTrackingLog(fromTimeMonth, toTimeWeekMonth, 24, "error");
        //                return Result<List<TrackingLogDto>>.Success(resultMonth.Data);

        //            case "year":
        //                DateTime fromTimeYear = DateTime.Now.AddDays(-365);
        //                DateTime toTimeWeekYear = DateTime.Now;
        //                var resultYear = await GetListTrackingLog(fromTimeYear, toTimeWeekYear, 24, "error");
        //                return Result<List<TrackingLogDto>>.Success(resultYear.Data);
        //        }

        //        return Result<List<TrackingLogDto>>.Failure("Định dạng mục sai.");
        //    }
        //}

        //public async Task<Result<List<TrackingLogDto>>> GetListErrorAverageRequest(TrackingLogRequest request)
        //{
        //    if (request.type == null)
        //    {
        //        if (request.FromTime == null || request.ToTime == null)
        //            throw new ApplicationException("Dữ liệu gửi lên máy chủ rỗng");

        //        if (request.FromTime > request.ToTime)
        //            throw new ApplicationException("Thời gian bắt đầu không thể lớn hơn thời gian kết thúc");

        //        var result = await GetListTrackingLog((DateTime)request.FromTime, (DateTime)request.ToTime, 1, "downtime");

        //        return Result<List<TrackingLogDto>>.Success(result.Data);
        //    }
        //    else
        //    {
        //        switch (request.type)
        //        {
        //            case "day":
        //                DateTime fromTimeDay = DateTime.Now.AddDays(-1);
        //                DateTime toTimeDay = DateTime.Now;
        //                var resultDay = await GetListTrackingLog(fromTimeDay, toTimeDay, 1, "downtime");
        //                return Result<List<TrackingLogDto>>.Success(resultDay.Data);

        //            case "week":
        //                DateTime fromTimeWeek = DateTime.Now.AddDays(-7);
        //                DateTime toTimeWeek = DateTime.Now;
        //                var resultWeek = await GetListTrackingLog(fromTimeWeek, toTimeWeek, 1, "downtime");
        //                return Result<List<TrackingLogDto>>.Success(resultWeek.Data);

        //            case "month":
        //                DateTime fromTimeMonth = DateTime.Now.AddDays(-30);
        //                DateTime toTimeWeekMonth = DateTime.Now;
        //                var resultMonth = await GetListTrackingLog(fromTimeMonth, toTimeWeekMonth, 24, "downtime");
        //                return Result<List<TrackingLogDto>>.Success(resultMonth.Data);

        //            case "year":
        //                DateTime fromTimeYear = DateTime.Now.AddDays(-365);
        //                DateTime toTimeWeekYear = DateTime.Now;
        //                var resultYear = await GetListTrackingLog(fromTimeYear, toTimeWeekYear, 24, "downtime");
        //                return Result<List<TrackingLogDto>>.Success(resultYear.Data);
        //        }

        //        return Result<List<TrackingLogDto>>.Failure("Định dạng mục sai.");
        //    }
        //}

        //private async Task<Result<List<TrackingLogDto>>> GetListTrackingLog(DateTime fromTime, DateTime toTime, int step, string? type = null)
        //{
        //    var query = await _context.TrackingLogs.Where(tl => tl.CreatedDate >= fromTime && tl.CreatedDate <= toTime)
        //                                           .AsNoTracking().ToListAsync();

        //    if(type != null)
        //    {
        //        if(type == "error")
        //        {
        //            query = query.Where(tl => tl.Message != null).ToList();
        //        }

        //        if (type == "downtime")
        //        {
        //            query = query.Where(tl => tl.Type.Contains("DOWNTIME SERVER") || tl.Type.Contains("DOWNTIME DATABASE")).ToList();
        //        }
        //    }

        //    List<TrackingLogDto> trackingLogDtos = new List<TrackingLogDto>();

        //    while (fromTime <= toTime)
        //    {
        //        var currentTime = fromTime.AddHours(step);

        //        var curentLogs = query.Where(q => q.CreatedDate >= fromTime && q.CreatedDate < currentTime).ToList();

        //        var count = curentLogs.Count;
        //        var sumTime = curentLogs.Sum(q => q.ResponseTimeSec);

        //        TrackingLogDto trackingLogDto = new TrackingLogDto
        //        {
        //            Average = count == 0 ? 0 : sumTime / count,
        //            NumberRequest = count,
        //            FromTime = fromTime,
        //            ToTime = currentTime
        //        };

        //        trackingLogDtos.Add(trackingLogDto);
        //        fromTime = currentTime;
        //    }

        //    return Result<List<TrackingLogDto>>.Success(trackingLogDtos);
        //}
    }
}
