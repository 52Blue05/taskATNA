using Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Dto;
namespace Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Queries;

public record Relationship_GainsQuestion_GetByRelationshipIdQuery(Guid Id) : IRequest<Result<IEnumerable<Relationship_GainsQuestionDto>>>;
public class Relationship_GainsQuestion_GetByRelationshipIdQueryHandler : IRequestHandler<Relationship_GainsQuestion_GetByRelationshipIdQuery, Result<IEnumerable<Relationship_GainsQuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public Relationship_GainsQuestion_GetByRelationshipIdQueryHandler(IMapper mapper, IApplicationDbContext context)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<Relationship_GainsQuestionDto>>> Handle(Relationship_GainsQuestion_GetByRelationshipIdQuery request, CancellationToken cancellationToken)
    {
        await InitialGainsQuestion(cancellationToken);

        await InitialRelationshipGainsQuestion(request.Id, cancellationToken);

        const int PERSONAL_INFORMATION = 1;
        const int EXPERIENCE_INFORMATION = 2;
        const int FAMILY_INFORMATION = 3;
        const int GOAL_TARGET = 4;

        var exceptIds = new List<int>() { PERSONAL_INFORMATION, EXPERIENCE_INFORMATION, FAMILY_INFORMATION, GOAL_TARGET };

        var gainsQuestion = await _context.GainsQuestions.Where(s => s.DeleteFlag != true
                                                                  && s.Code.HasValue
                                                                  && !exceptIds.Contains(s.Code.Value))
                                          .Select(s => new Relationship_GainsQuestionDto()
                                          {
                                              Id = Guid.NewGuid(),
                                              RelationshipId = request.Id,
                                              GainsQuestionId = s.Id,
                                              Question = s.Content ?? "",
                                              Answer = false,
                                              CreatedDate = s.CreatedDate,                                          
                                          })
                                          .OrderBy(s => s.CreatedDate)
                                          .ToListAsync();

        var answers = await _context.Relationship_GainsQuestions
                                    .Where(s => s.RelationshipId == request.Id)
                                    .ToListAsync();

        foreach (var gains in gainsQuestion)
        {
            foreach (var answer in answers)
            {
                if (answer.GainsQuestionId == gains.GainsQuestionId)
                {
                    gains.Answer = true;
                    gains.AnswerDetail = answer.AnswerDetail??"";
                }
            }
        }

        return Result<IEnumerable<Relationship_GainsQuestionDto>>.Success(gainsQuestion);
    }

    private async Task InitialGainsQuestion(CancellationToken cancellationToken)
    {
        if (!_context.GainsQuestions.Any())
        {
            var gainsQuestions1 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 1,
                Content = "Thông tin cá nhân của khách hàng",
                Description = "Thông tin cá nhân của khách hàng",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions1);

            var gainsQuestions2 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 2,
                Content = "Thông tin kinh nghiệm của khách hàng",
                Description = "Thông tin kinh nghiệm của khách hàng",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions2);

            var gainsQuestions3 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 3,
                Content = "Thành viên gia đình",
                Description = "Thành viên gia đình",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions3);

            var gainsQuestions4 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 4,
                Content = "Mục tiêu năm tới của họ",
                Description = "Mục tiêu năm tới của họ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions4);

            var gainsQuestions5 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 5,
                Content = "Bạn có thể gọi họ mặc dù rất muộn nếu bạn thực sự cần giúp đỡ",
                Description = "Bạn có thể gọi họ mặc dù rất muộn nếu bạn thực sự cần giúp đỡ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions5);

            var gainsQuestions6 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 6,
                Content = "Bạn không ngần ngại khi đề nghị họ giúp về đời sống",
                Description = "Bạn không ngần ngại khi đề nghị họ giúp về đời sống",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions6);

            var gainsQuestions7 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 7,
                Content = "Bạn không ngần ngại khi đề nghị họ giúp về công việc kinh doanh",
                Description = "Bạn không ngần ngại khi đề nghị họ giúp về công việc kinh doanh",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions7);

            var gainsQuestions8 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 8,
                Content = "Bạn có cảm thấy thú vị khi dành thời gian cùng nhau (Cafe, thể thao,...)",
                Description = "Bạn có cảm thấy thú vị khi dành thời gian cùng nhau (Cafe, thể thao,...)",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions8);

            var gainsQuestions9 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 9,
                Content = "Người đó có nằm trong tâm trí của bạn khi cần giúp đỡ, khi bạn có thời gian rảnh, khi muốn quan tâm",
                Description = "Người đó có nằm trong tâm trí của bạn khi cần giúp đỡ, khi bạn có thời gian rảnh, khi muốn quan tâm",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions9);

            var gainsQuestions10 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 10,
                Content = "Bạn có thể trao đổi thẳng thắn, cởi mở việc giúp đỡ lẫn nhau hoặc người khác",
                Description = "Bạn có thể trao đổi thẳng thắn, cởi mở việc giúp đỡ lẫn nhau hoặc người khác",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions10);

            var gainsQuestions11 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 11,
                Content = "Bạn và người đó đã có những khoảnh khắc cùng nhau làm việc gì đáng nhớ",
                Description = "Bạn và người đó đã có những khoảnh khắc cùng nhau làm việc gì đáng nhớ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions11);

            var gainsQuestions12 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 12,
                Content = "Bạn đã được đối tác chia sẻ khó khăn",
                Description = "Bạn đã được đối tác chia sẻ khó khăn",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions12);

            var gainsQuestions13 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 13,
                Content = "Bạn đã được đối tác nhờ giúp đỡ",
                Description = "Bạn đã được đối tác nhờ giúp đỡ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions13);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task InitialRelationshipGainsQuestion(Guid relationshipId, CancellationToken cancellationToken)
    {
        const int GAINS_QUESTION_FROM_NUMBER = 5;

        var listRelationshipGainsQuestion = await _context.Relationship_GainsQuestions.Where(x => x.DeleteFlag != true && x.RelationshipId == relationshipId)
                                                                                      .AsNoTracking()
                                                                                      .CountAsync();

        if (listRelationshipGainsQuestion > 0)
        {
            return;
        }

        var listGainsQuestion = await _context.GainsQuestions.Where(x => x.DeleteFlag != true && x.Code >= GAINS_QUESTION_FROM_NUMBER)
                                                             .OrderBy(x => x.Code)
                                                             .AsNoTracking()
                                                             .ToListAsync();

        foreach (var item in listGainsQuestion)
        {
            var relationshipGainsQuestion = new Relationship_GainsQuestion()
            {
                Id = Guid.NewGuid(),
                RelationshipId = relationshipId,
                Answer = false,
                GainsQuestionId = item.Id,
            };

            _context.Relationship_GainsQuestions.Add(relationshipGainsQuestion);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
