namespace Sale_Saas.Application.Features.OpportunityFeature.Services;

public static class OpportunityService
{
    public static async Task<Opportunity> GetOpportunity(Guid? id, IApplicationDbContext context)
    {
        if (id == null || id == Guid.Empty)
        {
            throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {id}");
        }
        Opportunity? obj = await context.Opportunities
                                        .Where(s => s.Id == id && s.DeleteFlag != true)
                                        .Include(x => x.OpportunityOpponents)
                                        .Include(x => x.OpportunityStatus)
                                        .Include(x => x.Customer)
                                        .Include(x => x.OpportunityHistories)
                                        .Include(x => x.ApplicationUser)
                                        .Include(x => x.ApplicationRole)
                                        .FirstOrDefaultAsync();
        if (obj == null)
        {
            throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {id}");
        }
        return obj;
    }

    public static async Task<OpportunityStatus> GetStatus(string code, IApplicationDbContext context)
    {
        OpportunityStatus? obj = await context.OpportunityStatuses
                                        .FirstOrDefaultAsync(s => s.Code == code);
        if (obj == null)
        {
            throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {code}");
        }
        return obj;
    }

    public static async Task<OpportunityStatus> GetStatusById(Guid id, IApplicationDbContext context)
    {
        OpportunityStatus? obj = await context.OpportunityStatuses
                                        .FirstOrDefaultAsync(s => s.Id == id);
        if (obj == null)
        {
            throw new ApplicationException($"Không tìm thấy trạng thái");
        }

        return obj;
    }

    public static async Task<OpportunityHistory> GetHistory(Guid? id, IApplicationDbContext context)
    {
        if (id == null || id == Guid.Empty)
        {
            throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {id}");
        }
        OpportunityHistory? obj = await context.OpportunityHistories
                                               .Where(s => s.Id == id && s.DeleteFlag != true)
                                               .Include(s => s.Opportunity)
                                               .FirstOrDefaultAsync();
        if (obj == null)
        {
            throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {id}");
        }
        return obj;
    }

    public static async Task<Customer> GetCustomer(Guid customerId, IApplicationDbContext context)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(x => x.Id == customerId && x.DeleteFlag != true);

        if (customer == null)
        {
            throw new ApplicationException("Không tìm thấy khách hàng");
        }

        return customer;
    }

    public static async Task<ApplicationUser> GetApplicationUser(Guid userId, IApplicationDbContext context)
    {
        var user = await context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == userId && x.DeleteFlag != true);

        if (user == null)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        return user;
    }
}
