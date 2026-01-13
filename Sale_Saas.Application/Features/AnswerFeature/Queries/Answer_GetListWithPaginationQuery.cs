using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.AnswerFeature.Dto;
using Sale_Saas.Application.Features.QuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.AnswerFeature.Queries;

public record Answer_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<AnswerDto>>>;

public class Answer_GetListWithPaginationQueryHandler : IRequestHandler<Answer_GetListWithPaginationQuery, Result<PaginatedList<AnswerDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Answer_GetListWithPaginationQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<PaginatedList<AnswerDto>>> Handle(Answer_GetListWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = from answer in _context.Answers
                    join question in _context.Questions
                    on answer.QuestionId equals question.Id into answer_question
                    from question in answer_question.DefaultIfEmpty()
                    where answer.DeleteFlag != true
                    select new { answer, question };

        // text search
        if (request.RequestData.TextSearch != null)
        {
            query = query.Where(x => x.answer.Content.ToLower().Trim().Contains(request.RequestData.TextSearch.ToLower().Trim()));
        }

        // filter


        // default sort asc by id
        query = query.OrderBy(x => x.question.Id);

        // result
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
                                .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

        var eventLog = await _eventLogService.Create("AnswerFeature", "AnswerFeature", "Answer_GetListWithPaginationQuery", Guid.Empty);

        return Result<PaginatedList<AnswerDto>>.Success(result);
    }
}
