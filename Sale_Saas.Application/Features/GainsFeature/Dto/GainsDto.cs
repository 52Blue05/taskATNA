using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;

namespace Sale_Saas.Application.Features.GainsFeature.Dto
{
    public class GainsDto : BaseEntityDto
    {
        public DateTime? DayOfBirth { get; set; }
        public string? NativePlace { get; set; }
        public string? SchoolAndYear { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? Degree { get; set; }
        public string? PositionTerm { get; set; }
        public string? FormerJob { get; set; }
        public string? Family { get; set; }
        public string? Speciality { get; set; }
        public string? Habit { get; set; }
        public string? Hobby { get; set; }
        public string? Dislike { get; set; }
        public string? StrongPoint { get; set; }
        public string? WeakPoint { get; set; }
        public string? FaviroteActivity { get; set; }
        public string? NearGoal { get; set; }
        public string? LongGoal { get; set; }
        public string? Idol { get; set; }
        public string? Aspiration { get; set; }
        public string? Clubs { get; set; }
        public string? Archivement { get; set; }
        public string? Address { get; set; }
        public string? WeekendActivity { get; set; }
        public string? PlaceOfBirth { get; set; }
        public Guid? RelationshipId { get; set; }

        public RelationshipStatusDto? RelationshipStatusDto { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Gains, GainsDto>();
            }
        }
    }
}
