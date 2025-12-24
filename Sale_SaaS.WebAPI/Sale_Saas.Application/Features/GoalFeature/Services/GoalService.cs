using Sale_Saas.Domain.Enums;
namespace Sale_Saas.Application.Features.GoalFeature.Services;

public static class GoalService
{
    public static async Task SyncData(IApplicationDbContext context, CancellationToken token)
    {
        try
        {
            var done = GoalStatusEnum.COMPLETED.ToString();
            var fail = GoalStatusEnum.FAILED.ToString();
            List<Goal> datas = context.Goals.Where(s => DateTime.Now > s.EndTime &&
                                                        s.GoalStatus != null &&
                                                        s.GoalStatus.Code != done &&
                                                        s.GoalStatus.Code != fail)
                                            .Include(s => s.GoalStatus)
                                            .ToList();
            if (datas.Any())
            {
                foreach (var item in datas)
                {
                    if (item.ActualKPI >= item.TargetKPI && item.ActualPoint >= item.TargetPoint)
                    {
                        item.GoalStatus = await context.GoalStatuses.FirstOrDefaultAsync(s => s.Code == done);
                    }
                    else
                    {
                        item.GoalStatus = await context.GoalStatuses.FirstOrDefaultAsync(s => s.Code == fail);
                    }
                }
                context.Goals.UpdateRange(datas);
                await context.SaveChangesAsync(token);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public static async Task<Goal> GetGoal(Guid id, IApplicationDbContext context)
    {
        var obj = await (from goal in context.Goals
                         join user in context.ApplicationUsers on goal.UserSuggestId equals user.Id
                         join status in context.GoalStatuses on goal.GoalStatusId equals status.Id
                         join role in context.ApplicationRoles on goal.ApplicationRoleId equals role.Id
                         join criteria in context.Criterias on goal.CriteriaId equals criteria.Id
                         where goal.Id == id && goal.DeleteFlag != true && user.DeleteFlag != true && role.DeleteFlag != true
                         select new Goal
                         {
                             Id = goal.Id,
                             CriteriaName = goal.CriteriaName,
                             Criteria = goal.Criteria,
                             CriteriaId = goal.CriteriaId,
                             TargetKPI = goal.TargetKPI,
                             ActualKPI = goal.ActualKPI,
                             TargetPoint = goal.TargetPoint,
                             ActualPoint = goal.ActualPoint,
                             SuggestTargetKPI = goal.SuggestTargetKPI,
                             SuggestTargetPoint = goal.SuggestTargetPoint,
                             SuggestEndTime = goal.SuggestEndTime,
                             Calculate = goal.Calculate,
                             StartTime = goal.StartTime,
                             EndTime = goal.EndTime,
                             Review = goal.Review,
                             UserSuggest = user,
                             UserSuggestId = user.Id,
                             GoalStatus = status,
                             GoalStatusId = status.Id,
                             DeleteFlag = goal.DeleteFlag,
                             CreatedApplicationUserId = goal.Id,
                             CreatedDate = goal.CreatedDate,
                             RolePosition = goal.RolePosition,
                             LastModifiedApplicationUserId = goal.LastModifiedApplicationUserId,
                             LastModifiedDate = goal.LastModifiedDate,
                             ApplicationRole = role,
                             ApplicationRoleId = role.Id
                         })
                         .FirstOrDefaultAsync();

        if (obj == null)
            throw new ApplicationException($"Không tìm thấy Chức vụ có id: {id}");
        return obj;
    }

    public static async Task<GoalStatus> GetStatus(string code, IApplicationDbContext context)
    {
        var status = await context.GoalStatuses.Where(m => m.Code == code && !m.DeleteFlag)
                                               .FirstOrDefaultAsync();

        if (status == null) throw new ApplicationException($"Không tìm thấy trạng thái có Code: {code}");
        return status;
    }

    public static async Task<string> GetCriteriaName(Guid id, IApplicationDbContext context)
    {
        var goal = await context.Goals.Where(x => x.Id == id && x.DeleteFlag != true)
                                      .FirstOrDefaultAsync();

        if (goal == null) throw new ApplicationException("Không tìm thấy mục tiêu");

        return goal.CriteriaName ?? "";
    }

    public static async Task<Criteria> GetCriteria(Guid? id, IApplicationDbContext context)
    {
        var criteria = await context.Criterias.Where(x => x.Id == id && x.DeleteFlag != true)
                                              .FirstOrDefaultAsync();

        if (criteria == null)
        {
            throw new ApplicationException("Không tìm thấy tiêu chí");
        }

        return criteria;
    }

    public static async Task<ApplicationRole> GetApplicationRole(Guid? id, IApplicationDbContext context)
    {
        var role = await context.ApplicationRoles.Where(x => x.Id == id && x.DeleteFlag != true)
                                                     .FirstOrDefaultAsync();

        if (role == null)
        {
            throw new ApplicationException("Không tìm thấy vai trò");
        }

        return role;
    }
}
