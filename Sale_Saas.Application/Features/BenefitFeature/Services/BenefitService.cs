using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.BenefitFeature.Services;

public static class BenefitService
{
    public static async Task<Benefit> GetBenefit(Guid id, IApplicationDbContext context)
    {

        var obj = await context.Benefits
                               .Include(s => s.BenefitStatus)
                               .Include(s => s.ApplicationUser)
                               .Include(s => s.ApplicationRole)
                               .Include(s => s.TargetFluctuations)
                               .FirstOrDefaultAsync(s => s.Id == id);
        if (obj == null)
            throw new ApplicationException($"Không tìm thấy Quyền lợi có id: {id}");
        return obj;
    }

    public static async Task<BenefitStatus> GetStatus(string code, IApplicationDbContext context)
    {
        var status = await context.BenefitStatuses
                                   .Where(m => m.Code == code && !m.DeleteFlag)
                                   .FirstOrDefaultAsync();

        if (status == null)
            throw new ApplicationException($"Không tìm thấy trạng thái có Code: {code}");

        return status;
    }

    public static async Task<BenefitStatus> MobileGetStatus(string code, IApplicationDbContext context)
    {
        if (code == BenefitStatusEnum.REJECT.ToString())
        {
            return new BenefitStatus()
            {
                Code = code
            };
        }

        var status = await context.BenefitStatuses
                                   .Where(m => m.Code == code && !m.DeleteFlag)
                                   .FirstOrDefaultAsync();

        if (status == null)
            throw new ApplicationException($"Không tìm thấy trạng thái có Code: {code}");

        return status;
    }

    public static async Task<BenefitHistory> AddHistory(Guid id, Guid? preStatus, Guid nextStatus, Guid user, IApplicationDbContext context)
    {
        try
        {
            var history = new BenefitHistory()
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user,
                BenefitId = id,
                PreviousStatusId = preStatus ?? null,
                UpdatedStatusId = nextStatus,
                DeleteFlag = false,
                CreatedApplicationUserId = user,
                LastModifiedApplicationUserId = user,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };
            await context.BenefitHistories.AddAsync(history);
            return history;
        }
        catch (Exception ex) { return null; }
    }

}
