using Sale_Saas.Application.Features.AnswerFeature.Dto;
using Sale_Saas.Application.Features.QuestionFeature.Dto;
using Sale_Saas.Application.Features.UnitQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.QuestionFeature.Queries;

public record Question_GetListByUnitQuestionIdQuery(Guid UnitQuestionId) : IRequest<Result<IEnumerable<QuestionDto>>>;

public class Question_GetListByUnitQuestionIdQueryHandler : IRequestHandler<Question_GetListByUnitQuestionIdQuery, Result<IEnumerable<QuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Question_GetListByUnitQuestionIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<IEnumerable<QuestionDto>>> Handle(Question_GetListByUnitQuestionIdQuery request, CancellationToken cancellationToken)
    {
        var query = from question in _context.Questions
                    join unitQuestion in _context.UnitQuestions
                    on question.UnitQuestionId equals unitQuestion.Id into question_unitQuestion
                    from unitQuestion in question_unitQuestion.DefaultIfEmpty()
                    where question.DeleteFlag != true && question.UnitQuestionId == request.UnitQuestionId
                    select new { question, unitQuestion };

        // default sort asc by id
        query = query.OrderBy(x => x.question.CreatedDate);


        var result = await query.AsNoTracking()
                                .Select(x => new QuestionDto()
                                {
                                    Id = x.question.Id,
                                    Name = x.question.Name,
                                    UnitQuestionId = x.question.UnitQuestionId,
                                    UnitQuestion = x.unitQuestion != null ? new UnitQuestionDto()
                                    {
                                        Id = x.unitQuestion.Id,
                                        MinNumberQuestion = x.unitQuestion.MinNumberQuestion,
                                        MaxNumberQuestion = x.unitQuestion.MaxNumberQuestion,
                                        MinScore = x.unitQuestion.MinScore,
                                        MaxScore = x.unitQuestion.MaxScore,
                                        Time = x.unitQuestion.Time
                                    } : null,
                                    Answers = x.question.Answers != null ? x.question.Answers
                                            .OrderBy(m => m.CreatedDate)
                                            .Select(m => new AnswerDto()
                                            {
                                                Id = m.Id,
                                                Content = m.Content,
                                                isCorrect = m.isCorrect,
                                                QuestionId = m.QuestionId
                                            }).ToList() : null
                                })
                                .ToListAsync();

        if (!result.Any())
        {
            throw new ApplicationException("Bài kiểm tra chưa sẵn sàng");
        }

        var eventLog = await _eventLogService.Create("QuestionFeature", "QuestionFeature", "Question_GetListByUnitQuestionIdQuery", Guid.Empty);

        return Result<IEnumerable<QuestionDto>>.Success(result.AsReadOnly());
    }
}
