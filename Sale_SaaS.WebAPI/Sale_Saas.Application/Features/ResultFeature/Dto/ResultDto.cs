using Sale_Saas.Application.Features.UnitFeature.Dto;

namespace Sale_Saas.Application.Features.ResultFeature.Dto;

public class ResultDto : BaseEntityDto
{
    public float? Score { get; set; }
    public int? CorrectQuestion { get; set; }
    public int? ToTalQuestion { get; set; }
    public bool? isOldResult { get; set; }
    public bool? isQualified { get; set; }
    public int? TimeCompletion { get; set; }

    public Guid ApplicationUserId { get; set; }
    public Guid UnitId { get; set; }
    public UnitDto? Unit { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Result, ResultDto>()
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(x => x.Units));
        }
    }
}
