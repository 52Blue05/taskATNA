using Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Commands
{
    public record Relationship_GainsQuestion_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<Relationship_GainsQuestionDto>>>;

    public class Relationship_GainsQuestion_AddOrUpdateCommandHandler : IRequestHandler<Relationship_GainsQuestion_AddOrUpdateCommand, Result<List<Relationship_GainsQuestionDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Relationship_GainsQuestion_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<Relationship_GainsQuestionDto>>> Handle(Relationship_GainsQuestion_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
			List<Relationship_GainsQuestionDto> list = new List<Relationship_GainsQuestionDto>();
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
                
                var tmp = (Relationship_GainsQuestionDto)_internalService.MapValueToObject(new Relationship_GainsQuestionDto(), addOrUpdateRequest.Data, new Relationship_GainsQuestionDto());
                if (tmp.RelationshipId == null || tmp.GainsQuestionId == null)
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");

                if (tmp.Answer) {
                    Relationship_GainsQuestion data = new Relationship_GainsQuestion()
                    {
                        Id = Guid.NewGuid(),
                        Answer = false,
                        RelationshipId = tmp.RelationshipId,
                        GainsQuestionId = tmp.GainsQuestionId,
                        DeleteFlag = false,
                        CreatedDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        AnswerDetail=tmp.AnswerDetail,
                    };
                    tmp.Id = data.Id;
					_context.Relationship_GainsQuestions.Add(data);
				}
				list.Add(tmp);
			}

			List<Relationship_GainsQuestionDto> updatedSuccess = list.Where(s => s.Answer == true).ToList();
            int valid = list.Select(s => s.RelationshipId).Distinct().Count();
            if (valid > 1) throw new ApplicationException($"Chỉ có thể cập nhật câu trả lời của 1 mối quan hệ");
			Guid? relationshipId = list.Select(s => s.RelationshipId).FirstOrDefault();

			await RemoveOldItem(updatedSuccess, relationshipId);
			await UpdatePoint(updatedSuccess, relationshipId);

			await _context.SaveChangesAsync(cancellationToken);

            return Result<List<Relationship_GainsQuestionDto>>.Success(list);
        }

        private async Task RemoveOldItem(List<Relationship_GainsQuestionDto> updatedSuccess, Guid? relationshipId)
        {
            var query = _context.Relationship_GainsQuestions.Where(s => s.RelationshipId == relationshipId);
            if (updatedSuccess.Any())
            {
                List<Guid> ids = updatedSuccess.Select(s => s.Id).ToList();
				query = query.Where(s => !ids.Contains(s.Id));
            }
			List<Relationship_GainsQuestion> deletedItems = await query.ToListAsync();
			if (deletedItems.Any()) _context.Relationship_GainsQuestions.RemoveRange(deletedItems);
		}

        private async Task UpdatePoint(List<Relationship_GainsQuestionDto> updatedSuccess,Guid? RelationshipId)
        {
			int rightAnswer = updatedSuccess.Count();
			int totalQuestion = await _context.GainsQuestions.Where(s => s.DeleteFlag != true).CountAsync();
			int point = (int)Math.Round((double)rightAnswer / totalQuestion * 100);

			Relationship? relationship = await _context.Relationships.Where(s => s.Id == RelationshipId && s.TargetRelationship != null)
																	 .Include(s => s.TargetRelationship).FirstOrDefaultAsync();
			if (relationship != null)
            {
				//if (point >= relationship.TargetRelationship!.PointTo)
				//{
				//	relationship.CurrentRelationship = relationship.TargetRelationship;
				//	relationship.RelationshipStatus = await _context.RelationshipStatuses
				//											.FirstOrDefaultAsync(s => s.Code == RelationshipStatusEnum.COMPLETED.ToString() && !s.DeleteFlag)
				//									  ?? relationship.RelationshipStatus;
				//}
				//else
				{
					RelationshipLevel? level = await _context.RelationshipLevels.FirstOrDefaultAsync(s =>
																s.DeleteFlag != true && s.PointFrom <= point && s.PointTo >= point &&
																s.PointTo < relationship.TargetRelationship!.PointFrom);
					relationship.RelationshipStatus = await _context.RelationshipStatuses
															.FirstOrDefaultAsync(s => s.Code == RelationshipStatusEnum.PROCESSING.ToString() && !s.DeleteFlag)
													  ?? relationship.RelationshipStatus;
					relationship.CurrentRelationship = level ?? relationship.CurrentRelationship;
				}

				_context.Relationships.Update(relationship);
			}
		}
    }
}
