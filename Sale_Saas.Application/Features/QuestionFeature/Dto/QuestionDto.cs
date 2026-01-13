using Sale_Saas.Application.Features.AnswerFeature.Dto;
using Sale_Saas.Application.Features.UnitQuestionFeature.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.QuestionFeature.Dto
{
    public class QuestionDto : BaseEntityDto
    {
        public string? Name { get; set; }

        public Guid UnitQuestionId { get; set; }

        public UnitQuestionDto? UnitQuestion { get; set; }

        public List<AnswerDto>? Answers { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Question, QuestionDto>()
                    .ForMember(dest => dest.UnitQuestion, opt => opt.MapFrom(src => src.UnitQuestions));
            }
        }
    }
}
