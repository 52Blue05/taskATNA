using AutoMapper;
using Sale_Saas.Domain.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService.DTOs
{
    public class CreateModuleTenantDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class ModuleTenantDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
		public string? NameEn { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ModuleTenant, ModuleTenantDto>();
            }
        }
    }
}
