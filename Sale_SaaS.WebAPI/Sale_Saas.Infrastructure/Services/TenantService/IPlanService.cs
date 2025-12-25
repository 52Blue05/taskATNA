using Sale_Saas.Application.Common.Models;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public interface IPlanService
    {
        Task<Result<PlanServiceDto>> CreatePlanService(Guid userId, CreatePlanServiceDto request);
        Task<Result<PaginatedList<PlanServiceDto>>> GetListAllWithPagination(GetListWithPaginationQueryRequest request);
        Task<Result<PlanServiceDto>> GetPlanServiceById(Guid id);
        Task<Result<PlanServiceDto>> UpdatePlanService(Guid userId, UpdatePlanServiceDto request);
        Task<Result<PlanServiceDto>> DeletePlanServiceById(Guid userId, List<Guid> listIds);
        Task<Result<List<PlanServiceBasic>>> GetPlanServicForAddGroupTenant();
        Task<Result<ChangePlanServiceDto>> GetPlanServicForChangePlanService(Guid groupTenantId);
    }
}
