
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class Answer : BaseAuditableEntity
    {
        public string? Content { get; set; }
        public bool? isCorrect { get; set; }

        public Guid QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }
    }
}
