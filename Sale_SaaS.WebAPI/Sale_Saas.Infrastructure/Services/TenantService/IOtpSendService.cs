using Sale_Saas.Application.Common.Models;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public interface IOtpSendService
    {
        Task<OtpSend> Create(OtpSend request);
        Task<OtpSend> GetOtpByEmail(string email, string deviceId, string otp);
    }

}
