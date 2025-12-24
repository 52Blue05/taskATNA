using Sale_Saas.Application.Features.AnswerFeature.Dto;
using Sale_Saas.Application.Features.QuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.AnswerFeature.Queries;

public record Answer_GetByIdQuery(Guid Id) : IRequest<Result<AnswerDto>>;

public class Answer_GetByIdQueryHandler : IRequestHandler<Answer_GetByIdQuery, Result<AnswerDto>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Answer_GetByIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<AnswerDto>> Handle(Answer_GetByIdQuery request, CancellationToken cancellationToken)
    {
        var query = from answer in _context.Answers
                    join question in _context.Questions
                    on answer.QuestionId equals question.Id into answer_question
                    from question in answer_question.DefaultIfEmpty()
                    where answer.DeleteFlag != true && answer.Id == request.Id
                    select new { answer, question };

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
                                .FirstOrDefaultAsync();

        var eventLog = await _eventLogService.Create("AnswerFeature", "AnswerFeature", "Answer_GetByIdQuery", request.Id);

        return Result<AnswerDto>.Success(result);
    }
}
