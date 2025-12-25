using Sale_Saas.Application.Features.QuestionFeature.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.AnswerFeature.Dto
{
    public class AnswerDto : BaseEntityDto
    {
        public string? Content { get; set; }
        public bool? isCorrect { get; set; }

        public Guid QuestionId { get; set; }

        public QuestionDto? Question { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Answer, AnswerDto>()
                    .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.Question));
            }
        }
    }
}
