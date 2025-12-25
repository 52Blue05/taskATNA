using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public class UserAdminService : IUserAdminService
    {

        private readonly TenantDbContext _context; // database context
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public UserAdminService(TenantDbContext context, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _context = context;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public UserAdmin CreateUserAdmin(UserAdmin request)
        {
            var checkExisted=  _context.UserAdmins.Where(x=>x.UserName==request.UserName).FirstOrDefault();
            if (checkExisted!=null)
            {
                return null;
            }       
            var newData= _context.Add(request);
             _context.SaveChanges();
            return newData.Entity;
        }
        public  UserAdmin GetUserAdmin(string userName)
        {
            var userAdmin =  _context.UserAdmins.Where(x => x.UserName==userName).FirstOrDefault();
            return userAdmin;
        }

        public  PaginatedList<UserAdmin> GetListUserAdmin( GetListWithPaginationQueryRequest request)
        {
            var query = _context.UserAdmins.Where(x => !string.IsNullOrEmpty(x.UserName));
            if (!string.IsNullOrEmpty(request.TextSearch))
                query = query.Where(x => x.FullName.Contains(request.TextSearch) || x.UserName.Contains(request.TextSearch) );
            return (query.OrderBy(x => x.CreatedDate).PaginatedListNoAsync(request.PageIndex , request.PageSize));
        }

        private string GenerateToken( string userName, string fullName, string email, string isSaaS)
        {

            List<Claim> authClaims = new List<Claim>();
            authClaims.AddRange(new List<Claim>{
            new Claim(AppJwtClaimTypeConstant.UserName, userName),
            new Claim(AppJwtClaimTypeConstant.FullName, fullName),
            new Claim(AppJwtClaimTypeConstant.Email, email),
            new Claim(AppJwtClaimTypeConstant.IsSaaS, isSaaS)
        });

            SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(JWTConstant.Secret));

            JwtSecurityToken token = new(
                JWTConstant.ValidIssuer,
                JWTConstant.ValidAudience,
                expires: DateTime.Now.AddDays(7),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }     
        private DateTime GetValidTo(string jwt)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(jwt);
            return jwtSecurityToken.ValidTo;
        }

        public Result<LoginDto> Login(UserAdminDto request)
        {
            var user =  _context.UserAdmins.FirstOrDefault(x=>x.Email==request.UserName);
            if (user == null)
            {
                throw new ApplicationException("Không tìm thấy tài khoản này.");
            }
            else if (!user.Password.Equals( request.Password))
            {
                throw new ApplicationException("Sai mật khẩu.");
            }
            string token = GenerateToken(user.UserName ?? string.Empty, user.UserName, user.Email, "true");                                          
            DateTime ValidTo = GetValidTo(token);
            return Result<LoginDto>.Success(new LoginDto() { Token = token, ValidTo = ValidTo, IsSaaS = true });
        }
    }
}
