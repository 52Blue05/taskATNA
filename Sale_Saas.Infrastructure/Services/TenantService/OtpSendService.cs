using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Data;
using System.Threading;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public class OtpSendService : IOtpSendService
    {

        private readonly TenantDbContext _context; // database context
        private readonly IConfiguration _configuration;

		public OtpSendService(TenantDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

		}

        public async Task<OtpSend> Create(OtpSend request)
        {
            var result= await _context.OtpSends.AddAsync(request);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<OtpSend> GetOtpByEmail(string email, string deviceId, string otp)
        {
            var query = _context.OtpSends.Where(x => x.PhoneNo == email && x.Otp == otp);
            var temp = await query.ToListAsync();

            if(!string.IsNullOrEmpty(deviceId))
            {
                query=query.Where(x => x.DeviceId == deviceId);
            }

            var result=await query.OrderByDescending(x=>x.DateInput).FirstOrDefaultAsync();

            return result;
        }
    }
}
