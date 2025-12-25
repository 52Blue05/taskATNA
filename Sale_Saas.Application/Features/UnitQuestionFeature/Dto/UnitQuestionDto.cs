using Sale_Saas.Application.Features.UnitFeature.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.UnitQuestionFeature.Dto
{
    public class UnitQuestionDto : BaseEntityDto
    {
        public int? MaxScore { get; set; }
        public int? MinScore { get; set; }
        public int? MaxNumberQuestion { get; set; }
        public int? MinNumberQuestion { get; set; }
        public int? Time { get; set; }

        public Guid UnitId { get; set; }

        public UnitDto? Unit { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<UnitQuestions, UnitQuestionDto>()
                    .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Units));
            }
        }
    }
}
