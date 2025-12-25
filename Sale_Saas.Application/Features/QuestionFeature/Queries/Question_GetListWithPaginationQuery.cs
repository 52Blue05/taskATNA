using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.AnswerFeature.Dto;
using Sale_Saas.Application.Features.QuestionFeature.Dto;
using Sale_Saas.Application.Features.UnitQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.QuestionFeature.Queries;

public record Question_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<QuestionDto>>>;

public class Question_GetListWithPaginationQueryHandler : IRequestHandler<Question_GetListWithPaginationQuery, Result<PaginatedList<QuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Question_GetListWithPaginationQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<PaginatedList<QuestionDto>>> Handle(Question_GetListWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = from question in _context.Questions
                    join unitQuestion in _context.UnitQuestions
                    on question.UnitQuestionId equals unitQuestion.Id into question_unitQuestion
                    from unitQuestion in question_unitQuestion.DefaultIfEmpty()
                    where question.DeleteFlag != true
                    select new { question, unitQuestion };

        // text search
        if (request.RequestData.TextSearch != null)
        {
            query = query.Where(x => x.question.Name.ToLower().Trim().Contains(request.RequestData.TextSearch.ToLower().Trim()));
        }

        // filter


        // default sort asc by id
        query = query.OrderBy(x => x.question.CreatedDate);

        // result
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
                                .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

        var eventLog = await _eventLogService.Create("QuestionFeature", "QuestionFeature", "Question_GetListWithPaginationQuery", Guid.Empty);

        return Result<PaginatedList<QuestionDto>>.Success(result);
    }
}

