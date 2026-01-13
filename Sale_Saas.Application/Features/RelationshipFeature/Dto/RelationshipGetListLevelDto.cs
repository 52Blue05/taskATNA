namespace Sale_Saas.Application.Features.RelationshipFeature.Dto
{
     public class RelationshipGetListLevelDto
     {
          public string? CustomerName { get; set; }
          public string? Position { get; set; }
          public List<ListRelationshipCurrent>? CurrentLevels { get; set; }
          public List<ListRelationshipTarget>? TargetLevels { get; set; }
     }

     public class ListRelationshipCurrent
     {
          public string? CurrentLevel { get; set; }
          public DateTime? CreateDate { get; set; }
     }

     public class ListRelationshipTarget
     {
          public string? TargetLevel { get; set; }
          public DateTime? CompletionDate { get; set; }
     }
}
