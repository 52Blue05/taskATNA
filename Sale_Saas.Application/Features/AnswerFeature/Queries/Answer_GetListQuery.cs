using Sale_Saas.Application.Features.AnswerFeature.Dto;
using Sale_Saas.Application.Features.QuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.AnswerFeature.Queries;

public record Answer_GetListQuery : IRequest<Result<IEnumerable<AnswerDto>>>;

public class Answer_GetListQueryHandler : IRequestHandler<Answer_GetListQuery, Result<IEnumerable<AnswerDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Answer_GetListQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<IEnumerable<AnswerDto>>> Handle(Answer_GetListQuery request, CancellationToken cancellationToken)
    {
        var query = from answer in _context.Answers
                    join question in _context.Questions
                    on answer.QuestionId equals question.Id into answer_question
                    from question in answer_question.DefaultIfEmpty()
                    where answer.DeleteFlag != true
                    select new { answer, question };

        // default sort asc by id
        query = query.OrderBy(x => x.question.Id);


        var result = await query.AsNoTracking()
                                .Select(x => new AnswerDto()
                                {
                                    Id = x.answer.Id,
                                    Content = x.answer.Content,
                                    isCorrect = x.answer.isCorrect,
                                    QuestionId = x.answer.QuestionId,
                                    Question = x.question != null ? new QuestionDto()
                                    {
                                        Id = x.question.Id,
                                        Name = x.question.Name,
                                        UnitQuestionId = x.question.UnitQuestionId
                                    } : null
                                })
                                .ToListAsync();

        var eventLog = await _eventLogService.Create("AnswerFeature", "AnswerFeature", "Answer_GetListQuery", Guid.Empty);

        return Result<IEnumerable<AnswerDto>>.Success(result.AsReadOnly());
    }
}
