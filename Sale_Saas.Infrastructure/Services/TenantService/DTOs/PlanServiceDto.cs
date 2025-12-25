using AutoMapper;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Domain.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService.DTOs
{
    public class CreatePlanServiceDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Time {  get; set; }
        public List<string>? Codes { get; set; }
        public int? LimitChild { get; set; }
        public int? LimitUser { get; set; }

        private class Mapping : Profile
        {
            public Mapping() 
            {
                CreateMap<PlanService, CreatePlanServiceDto>();
            }
        }
    }

    public class UpdatePlanServiceDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Time { get; set; }
        public int? LimitChild { get; set; }
        public int? LimitUser { get; set; }
        public List<string>? Codes { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<PlanService, UpdatePlanServiceDto>();
            }
        }
    }

    public class PlanServiceDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set;}
        public decimal? Price { get; set; }
        public int? Time { get; set; }
        public int? LimitChild { get; set; }
        public int? LimitUser { get; set; }
        public List<ModuleTenantDto> listModuleTenants { get; set; } = new List<ModuleTenantDto>();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<PlanService, PlanServiceDto>();
            }
        }
    }

    public class PlanServiceBasic
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
    }

    public class ChangePlanServiceDto
    {
        public string? Name { get; set; }
        public List<PlanServiceBasic> ListPlanService { get; set; } = new List<PlanServiceBasic>();
    }
}
