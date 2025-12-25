using Sale_Saas.Application.Features.ResultFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.ResultExam;

namespace Sale_Saas.Application.Features.ResultFeature.Commands;

public record Result_CaculateResultOfExamCommand(ResultExamRequest RequestData) : IRequest<Result<ResultDto>>;

public class Result_CaculateResultOfExamHandler : IRequestHandler<Result_CaculateResultOfExamCommand, Result<ResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;
    private readonly IMapper _mapper;

    public Result_CaculateResultOfExamHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
        _mapper = mapper;
    }

    public async Task<Result<ResultDto>> Handle(Result_CaculateResultOfExamCommand request, CancellationToken cancellationToken)
    {
        var unitQuestion = await _context.UnitQuestions.FindAsync(request.RequestData.UnitQuestionId);

        var newResult = new Result()
        {
            ApplicationUserId = request.RequestData.ApplicationUserId,
            UnitId = unitQuestion != null ? unitQuestion.UnitId : Guid.Empty,
            TimeCompletion = request.RequestData.TimeCompletion,
            DeleteFlag = false,
            LastModifiedApplicationUserId = request.RequestData.ApplicationUserId,
            LastModifiedDate = DateTime.Now
        };

        var query = from question in _context.Questions
                    join answer in _context.Answers
                    on question.Id equals answer.QuestionId into question_answer
                    from answer in question_answer.DefaultIfEmpty()
                    where question.DeleteFlag != true
                                && answer.DeleteFlag != true
                                && question.UnitQuestionId == request.RequestData.UnitQuestionId
                    select new { question, answer };

        var questionAnswers = query.ToList();

        var totalQuestions = questionAnswers.Select(qa => qa.question.Id)
                                           .Distinct()
                                           .Count();

        var correctAnswers = questionAnswers.Where(qa => qa.answer != null
                                                  && qa.answer.isCorrect.HasValue && qa.answer.isCorrect.Value
                                                  && request.RequestData.submittedAnswerIds.Contains(qa.answer.Id))
                                             .Count();

        bool isQualified = correctAnswers >= (totalQuestions * 0.8);

        // add database
        newResult.ToTalQuestion = totalQuestions;
        newResult.CorrectQuestion = correctAnswers;
        newResult.isQualified = isQualified;
        // create new result => isOldResult always false value
        newResult.isOldResult = false;

        // check latest result to change isOldResult
        var latestResult = await _context.Results.Where(x => x.DeleteFlag != true
                                    && x.ApplicationUserId == request.RequestData.ApplicationUserId
                                    && x.UnitId == unitQuestion.UnitId && x.isOldResult == false).FirstOrDefaultAsync();

        if (latestResult != null)
        {
            latestResult.isOldResult = true;
            latestResult.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            latestResult.LastModifiedDate = DateTime.Now;
            _context.Results.Update(latestResult);
        }

        _context.Results.Add(newResult);

        await _context.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<Result, ResultDto>(newResult);

        return Result<ResultDto>.Success(resultDto);
    }
}