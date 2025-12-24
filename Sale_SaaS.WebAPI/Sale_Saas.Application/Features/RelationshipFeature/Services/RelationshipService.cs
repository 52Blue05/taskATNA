using Microsoft.EntityFrameworkCore;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.Services;

public static class RelationshipService
{
    public static async Task<Relationship> GetRelationship(Guid id, IApplicationDbContext context)
    {
        var data = await context.Relationships.Where(s => s.Id == id && !s.DeleteFlag )
                                                       .Include(s => s.ApplicationUser)
                                                       .Include(s => s.Customer)
                                                       .Include(s => s.RelationshipStatus)
                                                       .Include(s => s.CurrentRelationship)
                                                       .Include(s => s.TargetRelationship)
                                                       .Include(s => s.RelationshipCustomer)
                                                       .Include(s => s.YearToDate)
                                                       .FirstOrDefaultAsync();

        if (data == null) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {id}");
        if(data.RelationshipStatusId==null)
        {
            var relationshipStatus = await GetStatus(RelationshipStatusEnum.PENDING.ToString(), context);
            data.RelationshipStatusId= relationshipStatus!=null? relationshipStatus.Id : null;
        }
        return data;
    }

    public static async Task<RelationshipLevel> GetLevel(Guid? id, IApplicationDbContext context)
    {
        if (id == null || id == Guid.Empty) throw new ApplicationException($"Không tìm thấy trong dữ liệu");
        var data = await context.RelationshipLevels.FirstOrDefaultAsync(s => s.Id == id && !s.DeleteFlag);
        if (data == null) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {id}");
        return data;
    }

    public static async Task<RelationshipStatus> GetStatus(string code, IApplicationDbContext context)
    {
        var data = await context.RelationshipStatuses.FirstOrDefaultAsync(s => s.Code == code && !s.DeleteFlag);
        if (data == null) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Code: {code}");
        return data;
    }

    public static async Task<RelationshipLevel> GetLevel(string code, IApplicationDbContext context)
    {
        var data = await context.RelationshipLevels.FirstOrDefaultAsync(s => s.Code == code && !s.DeleteFlag);
        if (data == null) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Code: {code}");
        return data;
    }

    public static async Task<RelationshipCustomer> GetRelationshipCustomer(Guid id, IApplicationDbContext context)
    {
        var data = await context.RelationshipCustomers.FirstOrDefaultAsync(s => s.Id == id && s.DeleteFlag != true);

        if (data == null) throw new ApplicationException("Không tìm thấy khách hàng này");

        return data;
    }

    public static async Task<RelationshipHistory> AddHistory(Guid id, Guid prevLevelId, Guid nextLevelId, Guid userId, IApplicationDbContext context)
    {
        try
        {
            var history = new RelationshipHistory()
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = userId,
                RelationshipId = id,
                PreviousLevelId = prevLevelId,
                UpdatedLevelId = nextLevelId,

                DeleteFlag = false,
                CreatedApplicationUserId = userId,
                LastModifiedApplicationUserId = userId,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };
            await context.RelationshipHistories.AddAsync(history);
            return history;
        }
        catch (Exception ex) { return null; }
    }

    public static async Task<RelationshipCustomer> AddRelationshipCustomer(Guid id, string name, Guid user, IApplicationDbContext context)
    {
        try
        {
            var relationshipCustomer = new RelationshipCustomer()
            {
                Id = id,
                Name = name,
                DeleteFlag = false,
                CreatedApplicationUserId = user,
                LastModifiedApplicationUserId = user,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };
            await context.RelationshipCustomers.AddAsync(relationshipCustomer);
            return relationshipCustomer;
        }
        catch (Exception ex)
        {

            return null;
        }
    }

    public static async Task<RelationshipCustomer> IsExistsName(string name, IApplicationDbContext context)
         => await context.RelationshipCustomers.Where(x => x.DeleteFlag != true && x.Name.ToLower().Trim() == name.ToLower().Trim()).FirstOrDefaultAsync();


    public static async Task<RelationshipCustomer> GetById(Guid id, IApplicationDbContext context)
         => await context.RelationshipCustomers.Where(x => x.DeleteFlag != true && x.Id == id).FirstOrDefaultAsync();


}
