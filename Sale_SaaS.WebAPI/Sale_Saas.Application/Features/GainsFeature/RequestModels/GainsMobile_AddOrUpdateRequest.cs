namespace Sale_Saas.Application.Features.GainsFeature.RequestModels
{
    public class GainsMobile_AddOrUpdateRequest
    {
        public Guid? Id { get; set; }
        public DateTime? DayOfBirth { get; set; }         // Ngày sinh
        public string? NativePlace { get; set; }          // Nguyên Quán
        public string? SchoolAndYear { get; set; }        // Trường học
        public string? Email { get; set; }                // email
        public string? Phone { get; set; }                // phone
        public string? Company { get; set; }              // Công ty
        public string? Position { get; set; }             // Chức danh
        public string? Degree { get; set; }               // Bằng cấp
        public string? PositionTerm { get; set; }         // Thời gian nắm giữ
        public string? FormerJob { get; set; }            // Công việc đã trải qua
        public string? Family { get; set; }               // Gia đình
        public string? Speciality { get; set; }           // Chuyên môn
        public string? Habit { get; set; }                // Thói quen
        public string? Hobby { get; set; }                // Sở thích
        public string? Dislike { get; set; }              // Sở ghét
        public string? StrongPoint { get; set; }          // điểm mạnh
        public string? WeakPoint { get; set; }            // điểm yếu
        public string? FaviroteActivity { get; set; }     // hoạt động yêu thích
        public string? NearGoal { get; set; }             // 1-2-3 tháng tới
        public string? LongGoal { get; set; }             // 1-2 năm tới
        public string? Idol { get; set; }                 // thần tượng
        public string? Aspiration { get; set; }           // khát vọng
        public string? Clubs { get; set; }                // câu lạc bộ
        public string? Archivement { get; set; }          // thành tựu
        public string? Address { get; set; }              // địa chỉ
        public string? WeekendActivity { get; set; }      // t7-cn làm gì
        public string? PlaceOfBirth { get; set; }         // Nơi sinh
        public Guid? RelationshipId { get; set; }

        public List<GainsSchoolRequest>? GainsSchools { get; set; }
        public List<GainsFamilyRequest>? GainsFamilies { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<GainsSchoolRequest, GainsSchool>().ReverseMap();
                CreateMap<GainsFamilyRequest, GainsFamily>().ReverseMap();

                CreateMap<GainsMobile_AddOrUpdateRequest, Gains>()
                     .ForMember(dest => dest.GainsSchools, opt => opt.MapFrom(src => src.GainsSchools))
                     .ForMember(dest => dest.GainsFamilies, opt => opt.MapFrom(src => src.GainsFamilies))
                     .ReverseMap();
            }
        }
    }

    public class GainsSchoolRequest
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public int? Year { get; set; }
        public Guid? GainsId { get; set; }
    }

    public class GainsFamilyRequest
    {
        public Guid? Id { get; set; }
        public string? Relationship { get; set; }
        public string? Name { get; set; }
        public int? YearOfBirth { get; set; }
        public Guid? GainsId { get; set; }
    }

}
