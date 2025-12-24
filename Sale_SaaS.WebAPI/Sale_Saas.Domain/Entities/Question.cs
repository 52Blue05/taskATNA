
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class Question : BaseAuditableEntity
    {
        public string? Name { get; set; }

        public Guid UnitQuestionId { get; set; }
        [ForeignKey("UnitQuestionId")]
        public UnitQuestions? UnitQuestions { get; set; }

        public ICollection<Answer>? Answers { get; set; }
    }
}
