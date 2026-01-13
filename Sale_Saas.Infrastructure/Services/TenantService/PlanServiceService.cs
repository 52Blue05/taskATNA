using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sale_Saas.Infrastructure.Services.TenantService
{

    public class PlanServiceService : IPlanService
    {
        private readonly TenantDbContext _context;
        private readonly IMapper _mapper;

        public PlanServiceService(TenantDbContext conetxt, IMapper mapper)
        {
            _context = conetxt;
            _mapper = mapper;
        }

        public async Task<Result<PlanServiceDto>> CreatePlanService(Guid userId, CreatePlanServiceDto request)
        {
            try
            { 
                //decimal price = 0;
                //try
                //{
                //    price = decimal.Parse(request.Price.ToString());
                //}
                //catch (Exception ex)
                //{
                //    return Result<PlanService>.Failure("Vui lòng nhập đúng đơn giá của gói dịch vụ");
                //}

                //int time = 0;
                //try
                //{
                //    time = int.Parse(request.Time.ToString());
                //}
                //catch (Exception ex)
                //{
                //    return Result<PlanService>.Failure("Vui lòng nhập thời hạn của gói dịch vụ là số nguyên");
                //}

                if(request.Time < 0 || request.Time > 1000)
                {
                    return Result<PlanServiceDto>.Failure("Thời hạn của gói dịch vụ là từ 1 đến 1000");
                }

                foreach(var item in request.Codes)
                {
                    var getCode = await _context.ModuleTenants.Where(mt => mt.Code.Contains(item)).FirstOrDefaultAsync();
                    if(getCode == null)
                    {
                        return Result<PlanServiceDto>.Failure($"Không tìm thấy module {item}. Vui lòng liên hệ quản trị viên");
                    }
                }

                var checkPlanserviceName = await _context.PlanServices.Where(ps => ps.Name.ToLower() == request.Name.ToLower() 
                                                                                    && ps.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();

                if(checkPlanserviceName != null)
                {
                    return Result<PlanServiceDto>.Failure("Tên gói dịch vụ đã được sử dụng");
                }

                PlanService planService = new PlanService
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Price = request.Price ?? 0,
                    Time = request.Time ?? 0,
                    LimitChild = request.LimitChild ?? 0,
                    LimitUser = request.LimitUser ?? 0,
                    DeleteFlag = false,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    CreatedApplicationUserId = userId,
                    LastModifiedApplicationUserId = userId
                };

                List<PlanServiceModuleTenant> listPlanServiceModuleTenant = new List<PlanServiceModuleTenant>();

                foreach (var item in request.Codes)
                {
                    PlanServiceModuleTenant tempPlanServiceModuleTenant = new PlanServiceModuleTenant
                    {
                        Id = Guid.NewGuid(),
                        PlanServiceId = planService.Id,
                        ModuleTenantId = item,
                        DeleteFlag = false,
                        CreatedDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        CreatedApplicationUserId = userId,
                        LastModifiedApplicationUserId = userId
                    };

                    listPlanServiceModuleTenant.Add(tempPlanServiceModuleTenant);
                }

                _context.Add(planService);
                _context.AddRange(listPlanServiceModuleTenant);
                await _context.SaveChangesAsync();

                var query = from planModule in _context.PlanServiceModuleTenants 
                            where planModule.PlanServiceId == planService.Id
                            join moduleTenant in _context.ModuleTenants on planModule.ModuleTenantId equals moduleTenant.Code
                            select new
                            {
                                ModuleTenant = moduleTenant
                            };

                PlanServiceDto planServiceDto = new PlanServiceDto
                {
                    Id = planService.Id,
                    Name = planService.Name,
                    Price = planService.Price,
                    Time = planService.Time,
                    LimitChild = planService.LimitChild,
                    LimitUser = planService.LimitUser
                };

                foreach (var item in query)
                {
                    planServiceDto.listModuleTenants.Add(_mapper.Map<ModuleTenantDto>(item.ModuleTenant));
                }

                return Result<PlanServiceDto>.Success(planServiceDto);
            }
            catch (Exception ex)
            {
                return Result<PlanServiceDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<PaginatedList<PlanServiceDto>>> GetListAllWithPagination(GetListWithPaginationQueryRequest request)
        {
            try
            {
                var query = from planService in _context.PlanServices
                            where planService.DeleteFlag != true
                            join planModule in _context.PlanServiceModuleTenants on planService.Id equals planModule.PlanServiceId
                            join moduleTenant in _context.ModuleTenants on planModule.ModuleTenantId equals moduleTenant.Code
                            select new
                            {
                                PlanService = planService,
                                ModuleTenant = moduleTenant
                            };

                if (!string.IsNullOrEmpty(request.TextSearch))
                {
                    string textSearchLower = request.TextSearch.ToLower();
                    query = query.Where(q =>
                        q.PlanService.Name.ToLower().Contains(textSearchLower) ||
                        q.PlanService.LimitChild.ToString().ToLower().Contains(textSearchLower) ||
                        q.PlanService.LimitUser.ToString().ToLower().Contains(textSearchLower) ||
                        q.PlanService.Price.ToString().Contains(request.TextSearch) ||
                        q.PlanService.Time.ToString().Contains(request.TextSearch) ||
                        q.ModuleTenant.Name.ToLower().Contains(textSearchLower)
                    );
                }

                var filteredPlanServiceIds = query.Select(q => q.PlanService.Id).Distinct().ToList();

                var groupedQuery = from planService in _context.PlanServices
                                   where filteredPlanServiceIds.Contains(planService.Id)
                                   join planModule in _context.PlanServiceModuleTenants on planService.Id equals planModule.PlanServiceId
                                   join moduleTenant in _context.ModuleTenants on planModule.ModuleTenantId equals moduleTenant.Code
                                   select new
                                   {
                                       PlanService = planService,
                                       ModuleTenant = moduleTenant
                                   };

                var result = groupedQuery
                    .GroupBy(q => q.PlanService.Id)
                    .Select(g => new PlanServiceDto
                    {
                        Id = g.Key,
                        Name = g.First().PlanService.Name,
                        Price = g.First().PlanService.Price,
                        Time = g.First().PlanService.Time,
                        LimitChild = g.First().PlanService.LimitChild,
                        LimitUser = g.First().PlanService.LimitUser,
                        listModuleTenants = g.Select(x => new ModuleTenantDto
                        {
                            Code = x.ModuleTenant.Code,
                            Name = x.ModuleTenant.Name,
                            NameEn = x.ModuleTenant.NameEn
                        }).ToList()
                    });

                int totalRecord = await result.CountAsync();

                var paginatedResult = await result
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return Result<PaginatedList<PlanServiceDto>>.Success(new PaginatedList<PlanServiceDto>(paginatedResult, totalRecord, request.PageIndex, request.PageSize));

            }
            catch (Exception ex)
            {
                return Result<PaginatedList<PlanServiceDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<PlanServiceDto>> GetPlanServiceById(Guid id)
        {
            try
            {
                var query = from planService in _context.PlanServices
                            where planService.DeleteFlag != true && planService.Id == id
                            join planModule in _context.PlanServiceModuleTenants on planService.Id equals planModule.PlanServiceId
                            join moduleTenant in _context.ModuleTenants on planModule.ModuleTenantId equals moduleTenant.Code
                            select new
                            {
                                PlanService = planService,
                                ModuleTenant = moduleTenant
                            };

                var groupedQuery = await query
                    .GroupBy(q => q.PlanService.Id)
                    .Select(g => new PlanServiceDto
                    {
                        Id = g.Key,
                        Name = g.First().PlanService.Name,
                        Price = g.First().PlanService.Price,
                        Time = g.First().PlanService.Time,
                        LimitChild = g.First().PlanService.LimitChild,
                        LimitUser = g.First().PlanService.LimitUser,
                        listModuleTenants = g.Select(x => new ModuleTenantDto
                        {
                            Code = x.ModuleTenant.Code,
                            Name = x.ModuleTenant.Name,
                            NameEn = x.ModuleTenant.NameEn
                        }).ToList()
                    }).FirstOrDefaultAsync();

                return Result<PlanServiceDto>.Success(groupedQuery);
            }
            catch(Exception ex)
            {
                return Result<PlanServiceDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<PlanServiceDto>> UpdatePlanService(Guid userId, UpdatePlanServiceDto request)
        {
            try
            {
                var getPlanService = await _context.PlanServices.Where(pl => pl.Id == request.Id).FirstOrDefaultAsync();
                if (getPlanService == null)
                {
                    return Result<PlanServiceDto>.Failure($"Không tìm thấy gói {request.Name} trong cơ sở dữ liệu. Vui lòng liên hệ quản trị viên");
                }

                if (request.Time < 0 || request.Time > 1000)
                {
                    return Result<PlanServiceDto>.Failure("Thời hạn của gói dịch vụ là từ 1 đến 1000");
                }

                if (getPlanService.Name.ToLower() != request.Name.ToLower())
                {
                    var checkPlanserviceName = await _context.PlanServices.Where(ps => ps.Name.ToLower() == request.Name.ToLower()
                                                                    && ps.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();

                    if (checkPlanserviceName != null)
                    {
                        return Result<PlanServiceDto>.Failure("Tên gói dịch vụ đã được sử dụng");
                    }
                }

                foreach (var item in request.Codes)
                {
                    var getCode = await _context.ModuleTenants.Where(mt => mt.Code.Contains(item)).FirstOrDefaultAsync();
                    if (getCode == null)
                    {
                        return Result<PlanServiceDto>.Failure($"Không tìm thấy module {item}. Vui lòng liên hệ quản trị viên");
                    }
                }

                var listPlanServiceModuleTenant = await _context.PlanServiceModuleTenants.Where(pm => pm.PlanServiceId == request.Id).ToListAsync();

                _context.PlanServiceModuleTenants.RemoveRange(listPlanServiceModuleTenant);

                List<PlanServiceModuleTenant> tempListPlanServiceModuleTenant = new List<PlanServiceModuleTenant>();

                foreach (var item in request.Codes)
                {
                    PlanServiceModuleTenant tempPlanServiceModuleTenant = new PlanServiceModuleTenant
                    {
                        Id = Guid.NewGuid(),
                        PlanServiceId = getPlanService.Id,
                        ModuleTenantId = item,
                        DeleteFlag = false,
                        CreatedDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        CreatedApplicationUserId = userId,
                        LastModifiedApplicationUserId = userId
                    };

                    tempListPlanServiceModuleTenant.Add(tempPlanServiceModuleTenant);
                }

                getPlanService.Name = request.Name;
                getPlanService.Price = request.Price ?? 0;
                getPlanService.Time = request.Time ?? 0;
                getPlanService.LimitChild = request.LimitChild ?? 0;
                getPlanService.LimitUser = request.LimitUser ?? 0;
                getPlanService.DeleteFlag = false;
                getPlanService.LastModifiedDate = DateTime.Now;
                getPlanService.LastModifiedApplicationUserId = userId;

                _context.Update(getPlanService);
                _context.AddRange(tempListPlanServiceModuleTenant);
                await _context.SaveChangesAsync();

                var query = from planModule in _context.PlanServiceModuleTenants
                            where planModule.PlanServiceId == getPlanService.Id
                            join moduleTenant in _context.ModuleTenants on planModule.ModuleTenantId equals moduleTenant.Code
                            select new
                            {
                                ModuleTenant = moduleTenant
                            };

                PlanServiceDto planServiceDto = new PlanServiceDto
                {
                    Id = getPlanService.Id,
                    Name = getPlanService.Name,
                    Price = getPlanService.Price,
                    Time = getPlanService.Time,
                    LimitChild = getPlanService.LimitChild,
                    LimitUser = getPlanService.LimitUser
                };

                foreach (var item in query)
                {
                    planServiceDto.listModuleTenants.Add(_mapper.Map<ModuleTenantDto>(item.ModuleTenant));
                }

                return Result<PlanServiceDto>.Success(planServiceDto);
            }
            catch (Exception ex)
            {
                return Result<PlanServiceDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<PlanServiceDto>> DeletePlanServiceById(Guid userId, List<Guid> listIds)
        {
            try
            {
                foreach (var id in listIds)
                {
                    var getPlanService = await _context.PlanServices.Where(pl => pl.Id == id).FirstOrDefaultAsync();
                    if (getPlanService == null)
                    {
                        return Result<PlanServiceDto>.Failure($"Không tìm thấy gói này trong cơ sở dữ liệu. Vui lòng liên hệ quản trị viên");
                    }

                    //var checkPlanService = await (from plan in _context.PlanServices
                    //                              join order in _context.Orders on plan.Id equals order.PlanServiceId
                    //                              join groupTenant in _context.GroupTenants on order.GroupTenantId equals groupTenant.Id
                    //                              where groupTenant.DeleteFlag != true && order.DeleteFlag != true && plan.DeleteFlag != true
                    //                                    && order.IsActived == true && plan.Id == id && groupTenant.ExpiryTime >= DateTime.Now.Date 
                    //                              select new {
                    //                                  tenant = groupTenant
                    //                               })
                    //                             .AsNoTracking().FirstOrDefaultAsync();

                    //if(checkPlanService != null)
                    //    return Result<PlanServiceDto>.Failure($"Có công ty đang sử dụng gói dịch vụ {getPlanService.Name} nên không thể xóa gói dịch vụ này.");

                    getPlanService.DeleteFlag = true;
                    getPlanService.LastModifiedApplicationUserId = userId;
                    getPlanService.LastModifiedDate = DateTime.Now;

                    _context.PlanServices.Update(getPlanService);
                    await _context.SaveChangesAsync();
                }

                return Result<PlanServiceDto>.Success(null);
            }
            catch (Exception ex)
            {
                return Result<PlanServiceDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<PlanServiceBasic>>> GetPlanServicForAddGroupTenant()
        {
            try
            {
                var listPlanService = await _context.PlanServices.Where(pl => pl.DeleteFlag != true).Select(pl => new PlanServiceBasic
                {
                    Id = pl.Id,
                    Name = pl.Name
                }).ToListAsync();

                return Result<List<PlanServiceBasic>>.Success(listPlanService);

            } catch (Exception ex)
            {
                return Result<List<PlanServiceBasic>>.Failure(ex.Message);
            }
        }

        public async Task<Result<ChangePlanServiceDto>> GetPlanServicForChangePlanService(Guid groupTenantId)
        {
            try
            {
                var query = await (from groups in _context.GroupTenants
                                   join orders in _context.Orders on groups.Id equals orders.GroupTenantId
                                   join plans in _context.PlanServices on orders.PlanServiceId equals plans.Id
                                   where orders.DeleteFlag != true && groups.Id == groupTenantId && orders.IsActived == true
                                   select new
                                   {
                                       planServiceName = plans.Name,
                                       planServiceId = plans.Id
                                   }).AsNoTracking().FirstOrDefaultAsync();

                int countChild = await _context.Tenants.Where(t => t.GroupTenantId == groupTenantId && t.DeleteFlag != true).CountAsync();

                int countUser = await _context.Users.Where(u => u.GroupTenantId == groupTenantId && u.DeleteFlag != true).CountAsync();

                var listPlanService = await _context.PlanServices
                                                    .Where(pl => pl.DeleteFlag != true
                                                                 && pl.LimitChild >= countChild
                                                                 && pl.LimitUser >= countUser 
                                                                 && pl.Id != query.planServiceId)
                                                    .Select(pl => new PlanServiceBasic
                                                    {
                                                        Id = pl.Id,
                                                        Name = pl.Name
                                                    }).ToListAsync();

                ChangePlanServiceDto changePlanServiceDto = new ChangePlanServiceDto
                {
                    Name = query.planServiceName,
                    ListPlanService = listPlanService
                };
                return Result<ChangePlanServiceDto>.Success(changePlanServiceDto);

            }
            catch (Exception ex)
            {
                return Result<ChangePlanServiceDto>.Failure(ex.Message);
            }
        }
    }
}
