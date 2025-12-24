using Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Dto;

namespace Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Queries
{
    public record Relationship_GainsQuestion_GetByIdQuery(Guid Id) : IRequest<Result<Relationship_GainsQuestionDto>>;
    public class Relationship_GainsQuestion_GetByIdQueryHandler : IRequestHandler<Relationship_GainsQuestion_GetByIdQuery, Result<Relationship_GainsQuestionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Relationship_GainsQuestion_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Relationship_GainsQuestionDto>> Handle(Relationship_GainsQuestion_GetByIdQuery request, CancellationToken cancellationToken)
        {
            Relationship_GainsQuestionDto? Relationship_GainsQuestion = await (from en in _context.Relationship_GainsQuestions
                                                                               where en.DeleteFlag != true && en.Id == request.Id
                                                                               join rl in _context.Relationships on en.RelationshipId equals rl.Id
                                                                               join gq in _context.GainsQuestions on en.GainsQuestionId equals gq.Id
                                                                               select new Relationship_GainsQuestionDto()
                                                                               {
                                                                                   Id = en.Id,
                                                                                   RelationshipId = en.RelationshipId,
                                                                                   GainsQuestionId = en.GainsQuestionId,
                                                                                   Question = gq.Content ?? "",
                                                                                   Answer = en.Answer,
                                                                                   AnswerDetail=en.AnswerDetail??"",
                                                                               }).AsNoTracking().FirstOrDefaultAsync();
            return Result<Relationship_GainsQuestionDto>.Success(Relationship_GainsQuestion);
        }
    }
}
