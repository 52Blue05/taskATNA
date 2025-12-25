using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio.DataModel;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.GroupTenant;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using Sale_Saas.Infrastructure.Services.TenantService.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sale_Saas.Infrastructure.Services.TenantService;

public class GroupTenantService : IGroupTenantService
{
	private readonly TenantDbContext _context;
	private readonly IEventLogService _eventLogService;
	private readonly IFileStorageService _fileStorageService;
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;
	private readonly IApplicationDbContextInitialiser _initialiserService;
	private readonly ITenantService _tenantService;
	private readonly IApplicationUserService _applicationUserService;
	private readonly IUserService _userService;
    private readonly IInternalService _internalService;


    public GroupTenantService(TenantDbContext context, IEventLogService eventLogService, 
								IFileStorageService fileStorageService, IConfiguration configuration, 
								IServiceProvider serviceProvider, IApplicationDbContextInitialiser initialiserService, 
								ITenantService tenantService, IApplicationUserService applicationUserService, 
								IUserService userService, IInternalService internalService)
	{
		_context = context;
		_eventLogService = eventLogService;
		_fileStorageService = fileStorageService;
		_configuration = configuration;
		_serviceProvider = serviceProvider;
		_initialiserService = initialiserService;
		_tenantService = tenantService;
		_applicationUserService = applicationUserService;
		_userService = userService;
		_internalService = internalService;
	}

	public async Task<Result<GroupTenantHomeDto>> AddAsync(GroupTenantAddOrUpdateRequest request, Guid userId)
	{
		GroupTenantDto updated = new GroupTenantDto();
		if (string.IsNullOrEmpty(request.Password))
		{
			throw new ApplicationException("Vui lòng nhập mật khẩu");
		}
		await CheckCode(request.Code);
        if (!string.IsNullOrEmpty(request.Domain))
            await CheckDomain(request.Domain);
        string logo = "";
		if (request.Logo != null)
		{
			try
			{
				var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { request.Logo }, "sass", true);
				if (response.Any())
				{
					logo = response[0].ServerPath;
				}
			}
			catch (Exception ex) { }
		}
		GroupTenant groupTenant = new GroupTenant()
		{
			Id = Guid.NewGuid(),
			Name = request.CompanyName,
			Code = request.Code,
			Logo = logo,
			CreatedApplicationUserId=userId,
			Domain = request.Domain,
			Color1 = request.Color1,
			Color2 = request.Color2
		};
		var result = await _context.GroupTenants.AddAsync(groupTenant);

		await CheckEmail(request.Email,null, request.Domain);
		var status = await _context.UserStatus.Where(s => s.Code == UserStatusEnum.ACTIVE.ToString()).FirstOrDefaultAsync();
		User user = new User()
		{
			Id = Guid.NewGuid(),
			Code = StringHelper.GenerateCode(),
			FirstName = "",
			LastName = "",
			FullName = request.FullName ?? "",
			Email = request.Email ?? "",
			UserName = request.Email ?? "",
			Password = FunctionUtils.GetMd5HashSalt(request.Password),
			Address = request.Address ?? "",
			Phone = request.Phone ?? "",
			IsAdmin = true,
			ApplicationUserStatus = status,
			GroupTenant = groupTenant
			
		};
		await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        // apply service
        List<string> listMenuAtive = null;
        if (request.PlanServiceId.HasValue)
        {
            var listOrderDetail = await ApplyPlanService(userId, request.PlanServiceId.Value, "", groupTenant);
			listMenuAtive = listOrderDetail.Select(x => x.ModuleTenantId).ToList();
        }
        // create seed data
        string dbId = $"{StringHelper.GenerateCode()}";
		string newConnectionString = await _tenantService.InnitTenantDatabase(dbId, "", listMenuAtive);
		Tenant tenant = new()
		{
			Id = dbId,
			Name = groupTenant.Name,
			Logo = groupTenant.Logo,
			ThemeColor = null,
			ConnectionString = newConnectionString,
			IsAdminCreated = true,
			GroupTenantId = groupTenant.Id,
			CreatedApplicationUserId=userId
		};

		_context.Add(tenant);
		await _context.SaveChangesAsync();

		//Add User to Tenant
        if (tenant != null && !string.IsNullOrEmpty(newConnectionString))
		{
			_applicationUserService.SetConnectDB(tenant.ConnectionString);
			await _userService.AddToTenant(new Application.Models.Identity.AddUserToTenantDto()
			{
				Id = user.Id,
				TenantId = tenant.Id
				
			});
		}

		var groupTenantHomeDto = await (from groups in _context.GroupTenants 
							 join orders in _context.Orders on groups.Id equals orders.GroupTenantId 
							 join plans in _context.PlanServices on orders.PlanServiceId equals plans.Id 
							 where groups.Id == groupTenant.Id 
							 select new GroupTenantHomeDto
							 {
                                 Id = groupTenant.Id,
                                 Code = groupTenant.Code,
                                 CompanyName = groupTenant.Name,
                                 FullName = user.FullName,
                                 Logo = groupTenant.Logo,
                                 Email = user.Email,
                                 Password = user.Password,
                                 Address = user.Address,
                                 Phone = user.Phone,
                                 PlanService = plans.Name,
                                 ExpiryTime = groups.ExpiryTime ?? DateTime.Now,
								 Domain = groupTenant.Domain,
								 Color1 = groupTenant.Color1,
								 Color2 = groupTenant.Color2
                             }).AsNoTracking().FirstOrDefaultAsync();

        return Result<GroupTenantHomeDto>.Success(groupTenantHomeDto);
	}

    private async Task<List<OrderDetail>> ApplyPlanService(Guid userId, Guid planServiceId, string newConnectionString, GroupTenant groupTenant, bool isUpdate = false)
	{
        var planService = await _context.PlanServices.AsNoTracking()
                                                         .Where(x => x.Id == planServiceId && x.DeleteFlag != true)
                                                         .FirstOrDefaultAsync();

        if (planService == null)
        {
            throw new ApplicationException("Gói dịch vụ không hợp lệ");
        }

        var monthPackage = planService.Time;
        var monthPackageExpiry = DateTime.Now.AddMonths(monthPackage);


        groupTenant.LimitChild = planService.LimitChild;
        groupTenant.LimitUser = planService.LimitUser;
        groupTenant.ExpiryTime = monthPackageExpiry;

        _context.GroupTenants.Update(groupTenant);

        if (isUpdate == true)
		{
            var order = await _context.Orders.Where(x => x.GroupTenantId == groupTenant.Id && x.DeleteFlag != true && x.IsActived == true).FirstOrDefaultAsync();
			if (order == null)
			{
				throw new ApplicationException("Order không tồn tại");
			}
			order.IsActived = false;
        }

        // create new order
        var newOrder = new Order()
        {
            Id = Guid.NewGuid(),
            GroupTenantId = groupTenant.Id,
            PlanServiceId = planServiceId,
            Price = planService.Price,
            Time = planService.Time,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now,
            CreatedApplicationUserId = userId,
            LastModifiedApplicationUserId = userId,
            IsActived = true
        };
        _context.Orders.Add(newOrder);
        // create order detail
        var query = from planServiceModuleTenant in _context.PlanServiceModuleTenants
                    join moduleTenant in _context.ModuleTenants
                    on planServiceModuleTenant.ModuleTenantId equals moduleTenant.Code
                    where planServiceModuleTenant.DeleteFlag != true && planServiceModuleTenant.PlanServiceId == planServiceId
                    select new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = newOrder.Id,
                        ModuleTenantId = moduleTenant.Code,
                        Name = moduleTenant.Name,
                        NameEn = moduleTenant.NameEn,
                        StartTime = DateTime.Now,
                        EndTime = monthPackageExpiry,
                        DeleteFlag = false,
                        CreatedDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        CreatedApplicationUserId = userId,
                        LastModifiedApplicationUserId = userId
                    };

        var orderDetails = await query.ToListAsync();
        _context.OrderDetails.AddRange(orderDetails);

        await _context.SaveChangesAsync();
        if (isUpdate == true)
            await UpdateMenuActiveAsync(newConnectionString, orderDetails, isUpdate, groupTenant.Id);
		return orderDetails;
    }

    private async Task CheckDomain(string? domain = "", Guid? id = null)
    {
        if (string.IsNullOrEmpty(domain))
        {
            throw new ApplicationException($"Không tìm thấy tên miền công ty");
        }

		// Normalize the domain by removing protocol
		domain = NormalizeDomain(domain);

		// get all group tenant
        var query = _context.GroupTenants.Where(s => s.Domain != null 
											 && s.Domain.ToLower() == domain.ToLower() 
											 && s.DeleteFlag != true)
										 .AsNoTracking();

        if (id != null && id != Guid.Empty)
        {
            query = query.Where(s => s.Id != id);
        }
        int count = await query.CountAsync();
        if (count > 0)
        {
            throw new ApplicationException($"Domain công ty đã được sử dụng: {domain}");
        }
    }

    private async Task CheckCode(string? code = "", Guid? id = null)
	{
		if (string.IsNullOrEmpty(code))
		{
			throw new ApplicationException($"Không tìm thấy mã công ty");
		}
		var query = _context.GroupTenants.Where(s => s.Code != null && s.Code.ToLower() == code.ToLower() && s.DeleteFlag != true).AsNoTracking();
		if (id != null && id != Guid.Empty)
		{
			query = query.Where(s => s.Id != id);
		}
		int count = await query.CountAsync();
		if (count > 0)
		{
			throw new ApplicationException($"Mã công ty đã được sử dụng: {code}");
		}
	}

	private async Task CheckEmail(string? email = "", Guid? id = null, string domain="")
	{
		if (string.IsNullOrEmpty(email))
		{
			throw new ApplicationException($"Không tìm thấy mã công ty");
		}
		var query = _context.Users.Where(s => s.Email != null && s.Email.ToLower() == email.ToLower() && s.IsAdmin == true && s.DeleteFlag != true).AsNoTracking();
		if(!string.IsNullOrEmpty(domain))
		{
			var groupId = await _context.GroupTenants.Where(x => x.Domain == domain && x.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();
			if (groupId != null)
			{
				query = query.Where(x => x.GroupTenantId == groupId.Id);
			}
			else
				return;
        }

		if (id != null && id != Guid.Empty)
		{
			query = query.Where(s => s.Id != id);
		}
		int count = await query.CountAsync();
		if (count > 0)
		{
			throw new ApplicationException($"Email công ty đã được sử dụng: {email}");
		}
	}

    private string NormalizeDomain(string domain)
    {
        domain = domain.Trim();

        if (domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            domain = domain.Substring("http://".Length);
        }
        else if (domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            domain = domain.Substring("https://".Length);
        }

        if (domain.EndsWith("/"))
        {
            domain = domain.Substring(0, domain.Length - 1);
        }

        return domain.ToLower();
    }

    private async Task DeleteTenantByGroup(Guid id)
	{
		var tenants = await _context.Tenants.Where(s => s.GroupTenantId == id).Select(x=> x.Id).ToListAsync();
		if(tenants!=null &&  tenants.Count > 0)
		{
			var user = await _context.Users.Where(u => u.GroupTenantId == id && u.IsAdmin == true).AsNoTracking().FirstOrDefaultAsync();
			await _tenantService.DeleteByIds(new DeleteRequest() { Ids = tenants, ApplicationUserId = user.Id });
		}
		var users = await _context.Users.Where(s => s.GroupTenantId == id).ToListAsync();
		foreach (var user in users)
		{
			user.DeleteFlag = true;
			user.LastModifiedDate = DateTime.Now;
			user.LastModifiedApplicationUserId = Guid.Empty;
		}

	}

	public async Task<Result<string>> DeleteByIds(DeleteRequest request)
	{
		string result = string.Empty;

		if (request.Ids == null) ExceptionHelper.RequestEmpty(request.Locale);

		List<Guid> ids = request.Ids!.Select(m => Guid.Parse(m)).ToList();
		var query = await _context.GroupTenants.Where(m => ids.Contains(m.Id)).ToListAsync();
		if (query == null || query.Count == 0) throw new ApplicationException("Không tìm thấy dữ liệu");

		foreach (var item in query)
		{
			item.DeleteFlag = true;
			item.LastModifiedDate = DateTime.Now;
			item.LastModifiedApplicationUserId = Guid.Empty;
			await DeleteTenantByGroup(item.Id);
		}
		var eventLog = await _eventLogService.Create("GroupTenantService", "GroupTenantService", "GroupTenantService_DeleteByIds", request.ApplicationUserId);
		await _context.SaveChangesAsync();

		return Result<string>.Success(string.Empty);
	}

	private IQueryable<GroupTenantOrderPlanserviceDto> GetOrderQuery(Guid? groupTenant = null, Guid? planService = null)
	{
		var query = from order in _context.Orders
					join plan in _context.PlanServices on order.PlanServiceId equals plan.Id
					where order.DeleteFlag != true && order.GroupTenantId != null && order.IsActived == true
					select new GroupTenantOrderPlanserviceDto
					{
						OrderId = order.Id,
						GroupTenantId = order.GroupTenantId!.Value,
						Name = plan.Name ?? "",
						Price = plan.Price,
						Time = plan.Time,
						PlanServiceId = plan.Id
					};

        //var query = from order in _context.Orders
        //            join plan in _context.PlanServices on order.PlanServiceId equals plan.Id
        //            where order.DeleteFlag != true && plan.DeleteFlag != true && order.GroupTenantId != null
        //            select new GroupTenantOrderPlanserviceDto
        //            {
        //                OrderId = order.Id,
        //                GroupTenantId = order.GroupTenantId!.Value,
        //                Name = plan.Name ?? "",
        //                Price = plan.Price,
        //                Time = plan.Time,
        //                PlanServiceId = plan.Id
        //            };

        if (groupTenant.HasValue && groupTenant.Value != Guid.Empty)
		{
            query = query.Where(s => s.GroupTenantId == groupTenant.Value);
		}

		if (planService.HasValue && planService.Value != Guid.Empty)
		{
			query = query.Where(s => s.PlanServiceId == planService.Value);
		}

		return query.AsNoTracking();
	}

    public async Task<Result<PaginatedList<GroupTenantHomeDto>>> GetListWithPaginationQuery(GetListWithPaginationQueryRequest request)
    {
        var query = from tenant in _context.GroupTenants
                    join user in _context.Users on tenant.Id equals user.GroupTenantId
                    where tenant.DeleteFlag != true && user.IsAdmin == true
                    orderby tenant.CreatedDate descending
                    select new GroupTenantHomeDto
                    {
                        Id = tenant.Id,
                        Code = tenant.Code ?? "",
                        Logo = tenant.Logo ?? "",
                        CompanyName = tenant.Name ?? "",
                        FullName = user.FullName ?? "",
                        Password = user.Password ?? "",
                        Address = user.Address ?? "",
                        Phone = user.Phone ?? "",
                        Email = user.Email ?? "",
                        ExpiryTime = tenant.ExpiryTime.Value,
						Domain = tenant.Domain
                    };

        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            var searchTextLower = request.TextSearch.ToLower();
            query = query.Where(s => s.CompanyName.ToLower().Contains(searchTextLower) ||
                                     s.Code.ToLower().Contains(searchTextLower) ||
                                     s.FullName.ToLower().Contains(searchTextLower) ||
                                     s.Address.ToLower().Contains(searchTextLower) ||
                                     s.Email.ToLower().Contains(searchTextLower) ||
                                     s.Phone.ToLower().Contains(searchTextLower) ||
                                     s.Domain.ToLower().Contains(searchTextLower));
        }

        // Thực hiện câu truy vấn phân trang
        var paginatedQuery = await query.PaginatedListAsync(request.PageIndex, request.PageSize);
        if (paginatedQuery.Items.Any())
        {
            foreach (var item in paginatedQuery.Items)
            {
                var planService = await GetOrderQuery(item.Id).FirstOrDefaultAsync();
                if (planService != null)
                {
                    item.PlanService = planService.Name;
                }
            }
        }
        var eventLog = await _eventLogService.Create("GroupTenant", "GroupTenant", "GroupTenant_GetListWithPaginationQuery", request.UserId);

        return Result<PaginatedList<GroupTenantHomeDto>>.Success(paginatedQuery);
    }

    public async Task<Result<GroupTenant>> FindByDomainAsync(string domain)
    {
        var groupTenant = await _context.GroupTenants.Where(s => s.Domain==domain && s.DeleteFlag != true).FirstOrDefaultAsync();

        if (groupTenant == null)
        {
            throw new ApplicationException($"Không tìm thấy dữ liệu với Domain: {domain}");
        }
        return Result<GroupTenant>.Success(groupTenant); ;
    }
    public async Task<Result<GroupTenantHomeDto>> GetById(Guid id)
	{
		var query = from tenant in _context.GroupTenants
					join user in _context.Users on tenant.Id equals user.GroupTenantId
					where tenant.DeleteFlag != true && user.IsAdmin == true && tenant.Id == id
					select new GroupTenantHomeDto
                    {
						Id = tenant.Id,
						Code = tenant.Code ?? "",
						Logo = tenant.Logo ?? "",
						CompanyName = tenant.Name ?? "",
						FullName = user.FullName ?? "",
						Password = user.Password ?? "",
						Address = user.Address ?? "",
						Phone = user.Phone ?? "",
						Email = user.Email ?? "",
						Domain=tenant.Domain??"",
						Color1=tenant.Color1??"",
						Color2=tenant.Color2??"",
					};
		var data = await query.FirstOrDefaultAsync();
		
		if (data == null)
		{
			throw new ApplicationException("Không tìm thấy tổ chức");
		}

		data.PlanService = await GetCurrentPlanServiceNameActive(id);

		var eventLog = await _eventLogService.Create("GroupTenatnService", "GetById", "GroupTenant_GetById", id);
		return Result<GroupTenantHomeDto>.Success(data);
	}

	private async Task<GroupTenant> FindAsync(Guid id)
	{
		var groupTenant = await _context.GroupTenants.Where(s => s.Id == id && s.DeleteFlag != true).FirstOrDefaultAsync();

		if (groupTenant == null)
		{
			throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {id}");
		}

		return groupTenant;
	}

	private async Task<User> FindUserByGroupTenantAsync(Guid id)
	{
		var user = await _context.Users.Where(s => s.GroupTenantId == id && s.DeleteFlag != true && s.IsAdmin == true).FirstOrDefaultAsync();

		if (user == null)
		{
			throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {id}");
		}

		return user;
	}

	//public async Task<Result<GroupTenantHomeDto>> UpdateAsync(GroupTenantAddOrUpdateRequest request, Guid userId)
	//{
 //       GroupTenantHomeDto updated = new GroupTenantHomeDto();

	//	if (request.Id == null || request.Id == Guid.Empty)
	//	{
	//		throw new ApplicationException("Không tìm thấy dữ liệu");
	//	}
	//	GroupTenant groupTenant = await FindAsync((Guid)request.Id);
	//	await CheckCode(request.Code, groupTenant.Id);
	//	groupTenant.Code = request.Code;
	//	groupTenant.Name = request.Name;
	//	string logo = "";
	//	if (request.Logo != null)
	//	{
	//		try
	//		{
	//			var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { request.Logo });
	//			if (response.Any())
	//			{
	//				logo = response[0].ServerPath;
	//			}
	//		}
	//		catch (Exception ex) { }
	//	}
	//	groupTenant.Logo = logo;
	//	_context.GroupTenants.Update(groupTenant);

	//	User user = await FindUserByGroupTenantAsync(groupTenant.Id);
	//	await CheckEmail(request.Email, user.Id);
	//	user.Email = request.Email ?? "";
	//	user.UserName = request.Email ?? "";
	//	user.Phone = request.Phone ?? "";
	//	user.Address = request.Address ?? "";
		
	//	if(!string.IsNullOrEmpty(request.Password) && FunctionUtils.GetMd5HashSalt(request.Password) != user.Password)
	//	{
	//		user.Password = FunctionUtils.GetMd5HashSalt(request.Password);
	//	}

	//	_context.Users.Update(user);

	//	await _context.SaveChangesAsync();

	//	// update order => change isActive = false
	//	// apply service
	//	string planServiceName = "";
 //       if (request.PlanServiceId.HasValue)
 //       {
 //           var planService = await _context.PlanServices.FindAsync(request.PlanServiceId);
	//		planServiceName = planService.Name ?? "";
 //           await ApplyPlanService(userId, request.PlanServiceId.Value, "", groupTenant, true);
 //       }

 //       var model = new GroupTenantHomeDto()
 //       {
 //           Id = groupTenant.Id,
 //           Code = groupTenant.Code,
 //           Name = groupTenant.Name,
 //           Logo = groupTenant.Logo,
 //           Address = user.Address,
 //           Password = user.Password,
 //           Email = user.Email,
 //           ExpiryTime = groupTenant.ExpiryTime ?? DateTime.Now,
 //           Phone = user.Phone,
 //           PlanService = planServiceName
 //       };
 //       if (model != null) updated = model;

 //       return Result<GroupTenantHomeDto>.Success(updated);
	//}

    public async Task<Result<GroupTenantExtendPlanServiceDto>> ExtendPlanService(Guid id)
    {
        var query = await (from groups in _context.GroupTenants 
						   join order in _context.Orders on groups.Id equals order.GroupTenantId
							join plan in _context.PlanServices on order.PlanServiceId equals plan.Id
							where order.DeleteFlag != true && plan.DeleteFlag != true && groups.Id == id && order.IsActived == true
                    select new GroupTenantExtendPlanServiceDto
                    {
                        Id = id,
						Name = plan.Name,
						ExpiryTime = groups.ExpiryTime.Value,
						ExtraExpiryTime = groups.ExpiryTime.Value.AddMonths(plan.Time)
                    }).AsNoTracking().FirstOrDefaultAsync();

		if(query == null)
		{
            return Result<GroupTenantExtendPlanServiceDto>.Failure("Gói đã bị xóa nên không thể tiếp tục gia hạn gói. Vui lòng liên hệ quản trị viên");
        }
		return Result<GroupTenantExtendPlanServiceDto>.Success(query);
    }

    public async Task<Result<GroupTenantDto>> AddExtendPlanService(Guid userId, Guid id)
    {
        var query = await (from groupTenants in _context.GroupTenants  
                           join orders in _context.Orders on groupTenants.Id equals orders.GroupTenantId
                           join plans in _context.PlanServices on orders.PlanServiceId equals plans.Id
                           where groupTenants.DeleteFlag != true && orders.DeleteFlag != true 
								&& plans.DeleteFlag != true && groupTenants.Id == id 
								&& orders.IsActived == true
                           select new
                           {
								groupTenant = groupTenants,
								order = orders,
								plan = plans
                           }).AsNoTracking().FirstOrDefaultAsync();

        if (query == null)
        {
            return Result<GroupTenantDto>.Failure("Gói đã bị xóa nên không thể tiếp tục gia hạn gói. Vui lòng liên hệ quản trị viên");
        }

		Order order = new Order
		{
			Id = Guid.NewGuid(),
			GroupTenantId = id,
			PlanServiceId = query.plan.Id,
			Price = query.plan.Price,
			Time = query.plan.Time,
			CreatedDate = DateTime.Now,
			LastModifiedDate = DateTime.Now,
			CreatedApplicationUserId = userId,
			LastModifiedApplicationUserId = userId,
			IsActived = true
		};

		var unActivedOrder = await _context.Orders.Where(o => o.Id == query.order.Id).FirstOrDefaultAsync();
		unActivedOrder.IsActived = false;

		var listOrderDetail = await _context.OrderDetails.Where(od => od.OrderId == query.order.Id).ToListAsync();

		List<OrderDetail> orderDetails = new List<OrderDetail>();

		foreach( OrderDetail detail in listOrderDetail )
		{
			OrderDetail orderDetail = new OrderDetail
			{
				Id = Guid.NewGuid(),
				OrderId = detail.OrderId,
				ModuleTenantId = detail.ModuleTenantId,
				Name = detail.Name,
				NameEn = detail.NameEn,
				StartTime = DateTime.Now,
				EndTime = DateTime.Now.Date.AddMonths(query.plan.Time),
				DeleteFlag = false,
				CreatedDate = DateTime.Now,
				LastModifiedDate = DateTime.Now,
				CreatedApplicationUserId = userId,
				LastModifiedApplicationUserId = userId
			};
			orderDetails.Add(orderDetail);
		}

		var updateTimeGroupTenant = await _context.GroupTenants.Where(gt => gt.Id == id).FirstOrDefaultAsync();

		updateTimeGroupTenant.ExpiryTime = updateTimeGroupTenant.ExpiryTime.Value.AddMonths(query.plan.Time);

		_context.GroupTenants.Update(updateTimeGroupTenant);
		_context.Orders.Add(order);
		_context.Orders.Update(unActivedOrder);
		_context.OrderDetails.AddRange(orderDetails);
		await _context.SaveChangesAsync();

		return Result<GroupTenantDto>.Success(null);
    }

	public async Task<bool> UpdateMenuActiveAsync(string newConnectionString, List<OrderDetail> orderDetails, bool isUpdate = false, Guid groupId = default)
	{
		using (var scope = _serviceProvider.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			if (isUpdate == true && groupId != default)
			{
				// get list connection string
				var connectionStrings = await GetConnectionStringsByGroupTenantAsync(groupId);

				if (connectionStrings == Enumerable.Empty<string>())
				{
                    //throw new ApplicationException("Không thể truy cập connection string");
                    return true;
                }

				foreach (var connectionString in connectionStrings!)
				{
					await UpdateMenuActiveOfTenantAsync(connectionString, orderDetails, dbContext, true);
                }

				return true;
			}

			await UpdateMenuActiveOfTenantAsync(newConnectionString, orderDetails, dbContext);
		}

		return true;
	}

    public async Task<IEnumerable<string>> GetConnectionStringsByGroupTenantAsync(Guid groupId)
    {
		var tenants = await _context.Tenants.AsNoTracking()
											.Where(x => x.GroupTenantId == groupId && x.DeleteFlag != true)
											.ToListAsync();

		var connectionStrings = tenants.Select(x => x.ConnectionString).ToList();

		if (connectionStrings == null || connectionStrings.Count == 0)
		{
			return Enumerable.Empty<string>();
        }

		return connectionStrings!;
    }

    public async Task<bool> UpdateMenuActiveOfTenantAsync(string connectionString, List<OrderDetail> orderDetails, ApplicationDbContext context, bool isUpdate = false)
    {
        context.SetConnectString(connectionString);

        context.Database.GetDbConnection().ConnectionString = connectionString;

        if (isUpdate == true)
		{
            var menus = await context.Menus.Where(x => x.DeleteFlag != true).ToListAsync();
            foreach (var menu in menus)
            {
                menu.IsActivite = false;
            }

            context.Menus.UpdateRange(menus);

            await context.SaveChangesAsync();
        }

        foreach (var orderDetail in orderDetails)
        {
            var code = orderDetail.ModuleTenantId;

            var menu = await context.Menus.Where(x => x.Code == code && x.DeleteFlag != true).FirstOrDefaultAsync();

            if (menu == null)
            {
                throw new ApplicationException("Không tìm thấy tên menu này");
            }

            menu.IsActivite = true;

            var menuChilds = await context.Menus.Where(x => x.ParentId == menu.Id && x.DeleteFlag != true).ToListAsync();

            foreach (var menuChild in menuChilds)
            {
                menuChild.IsActivite = true;
            }

            context.Menus.Update(menu);
            context.Menus.UpdateRange(menuChilds);

            await context.SaveChangesAsync();
        }

		_applicationUserService.ClearConnectDB();

		return true;
    }

    public async Task<Result<GroupTenantHomeDto>> UpdatePlanServiceAsync(Guid userId, GroupTenantUpdatePlanServiceRequest request)
    {
        GroupTenantHomeDto result = new GroupTenantHomeDto();

		if (request.GroupTenantId == Guid.Empty)
		{
			throw new ApplicationException("Không tìm thấy tổ chức");
		}

		GroupTenant groupTenant = await FindAsync(request.GroupTenantId);

        User user = await FindUserByGroupTenantAsync(request.GroupTenantId);

		string planServiceName = "";

		if (request.PlanServiceId == Guid.Empty)
		{
			throw new ApplicationException("Bạn phải truyền id plan service");
		}

        var planService = await _context.PlanServices.FindAsync(request.PlanServiceId);

		if (planService == null)
		{
			throw new ApplicationException("Không tìm thấy gói");
		}

        planServiceName = planService.Name ?? "";

        await ApplyPlanService(userId, request.PlanServiceId, "", groupTenant, true);

        result = new GroupTenantHomeDto()
        {
            Id = groupTenant.Id,
            Code = groupTenant.Code ?? "",
            CompanyName = groupTenant.Name ?? "",
            FullName = user.FullName ?? "",
            Logo = groupTenant.Logo ?? "",
            Address = user.Address ?? "",
            Password = user.Password,
            Email = user.Email ?? "",
            ExpiryTime = groupTenant.ExpiryTime ?? DateTime.Now,
            Phone = user.Phone ?? "",
            PlanService = planServiceName
        };

		if (result == null)
		{
			throw new ApplicationException("Cập nhật Plan service không thành công");
		}

		return Result<GroupTenantHomeDto>.Success(result);
    }

    public async Task<Result<GroupTenantHomeDto>> UpdateGroupTenantAsync(Guid userId, GroupTenantAddOrUpdateBaseRequest request)
    {
        GroupTenantHomeDto updated = new GroupTenantHomeDto();

        if (request.Id == null || request.Id == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy dữ liệu");
        }
        GroupTenant groupTenant = await FindAsync((Guid)request.Id);

        //await CheckCode(request.Code, groupTenant.Id);
        groupTenant.Code = request.Code;
        groupTenant.Name = request.CompanyName;
        string logo = "";
        if (request.Logo != null)
        {
            try
            {
                var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { request.Logo }, "sass", true);
                if (response.Any())
                {
                    logo = response[0].ServerPath;
                    groupTenant.Logo = logo;
                }
            }
            catch (Exception ex) { }
        }
        
		groupTenant.Color1 = request.Color1;
        groupTenant.Color2 = request.Color2;
		if (!string.IsNullOrEmpty(request.Domain) && groupTenant.Domain != request.Domain)
		{ 
			await CheckDomain(request.Domain, groupTenant.Id);
			groupTenant.Domain = request.Domain;

        }
        _context.GroupTenants.Update(groupTenant);

        User user = await FindUserByGroupTenantAsync(groupTenant.Id);
        //await CheckEmail(request.Email, user.Id);
        user.Email = request.Email ?? "";
        user.UserName = request.Email ?? "";
		user.FullName = request.FullName ?? "";
        user.Phone = request.Phone ?? "";
        user.Address = request.Address ?? "";

        _context.Users.Update(user);

        await _context.SaveChangesAsync();

		// get name plan service
		string planServiceName = await GetCurrentPlanServiceNameActive(request.Id ?? Guid.Empty);

        var model = new GroupTenantHomeDto()
        {
            Id = groupTenant.Id,
            Code = groupTenant.Code,
            CompanyName = groupTenant.Name,
            FullName = user.FullName,
            Logo = groupTenant.Logo,
            Address = user.Address,
            Password = user.Password,
            Email = user.Email,
            ExpiryTime = groupTenant.ExpiryTime ?? DateTime.Now,
            Phone = user.Phone,
            PlanService = planServiceName
        };
        if (model != null) updated = model;

        return Result<GroupTenantHomeDto>.Success(updated);
    }

    public async Task<string> GetCurrentPlanServiceNameActive(Guid groupId)
    {
		var order = await _context.Orders.AsNoTracking()
										 .Where(x => x.GroupTenantId == groupId && x.DeleteFlag != true && x.IsActived == true).FirstOrDefaultAsync();

		if (order == null)
		{
			throw new Exception("Không tìm thấy order nào");
		}

		var planService = await _context.PlanServices.FindAsync(order.PlanServiceId);

		if (planService == null)
		{
			throw new ApplicationException("Không tìm thấy plan service nào");
		}

		return planService?.Name ?? "";
    }

    public async Task<Result<GroupTenantHomeDto>> UpdatePassword(UpdatePasswordRequest request)
    {
        if (request.Data == null)
        {
            throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(s => s.GroupTenantId == request.id && s.IsAdmin == true && s.DeleteFlag != true);
        if (user == null) throw new ApplicationException("Không tìm thấy tài khoản");

        var obj = (UpdatePasswordDto)_internalService.MapValueToObject(new UpdatePasswordDto(), request.Data, new UpdatePasswordDto());

        if (obj.NewPassword != obj.ConfirmPassword)
            throw new ApplicationException("Mật khẩu xác nhận không trùng khớp");

        user.Password = FunctionUtils.GetMd5HashSalt(obj.NewPassword);
        user.LastModifiedDate = DateTime.Now;
        user.LastModifiedApplicationUserId = request.id;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var result = await (from groups in _context.GroupTenants
                                   join orders in _context.Orders on groups.Id equals orders.GroupTenantId
                                   join plans in _context.PlanServices on orders.PlanServiceId equals plans.Id
                                   where groups.Id == request.id
                                   select new GroupTenantHomeDto
                                   {
                                       Id = groups.Id,
                                       Code = groups.Code,
                                       CompanyName = groups.Name,
                                       FullName = user.FullName,
                                       Logo = groups.Logo,
                                       Email = user.Email,
                                       Password = user.Password,
                                       Address = user.Address,
                                       Phone = user.Phone,
                                       PlanService = plans.Name,
                                       ExpiryTime = groups.ExpiryTime ?? DateTime.Now
                                   }).AsNoTracking().FirstOrDefaultAsync();

        return Result<GroupTenantHomeDto>.Success(result);
    }
}
