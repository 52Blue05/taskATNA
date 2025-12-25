namespace Sale_Saas.Domain.Entities
{
     public class Gains : BaseAuditableEntity
     {
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
          public Relationship? Relationship { get; set; }

          public ICollection<GainsSchool>? GainsSchools { get; set; }
          public ICollection<GainsFamily>? GainsFamilies { get; set; }

     }
}
