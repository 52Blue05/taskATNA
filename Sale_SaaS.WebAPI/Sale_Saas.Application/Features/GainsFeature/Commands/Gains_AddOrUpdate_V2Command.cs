using Sale_Saas.Application.Features.GainsFeature.RequestModels;
using Sale_Saas.Application.Features.GainsFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsFeature.Commands
{
    public record Gains_AddOrUpdate_V2Command(Guid userId, GainsMobile_AddOrUpdateRequest RequestData) : IRequest<Result<GainsMobile_AddOrUpdateRequest>>;

    public class Gains_AddOrUpdate_V2CommandHandler : IRequestHandler<Gains_AddOrUpdate_V2Command, Result<GainsMobile_AddOrUpdateRequest>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public Gains_AddOrUpdate_V2CommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<GainsMobile_AddOrUpdateRequest>> Handle(Gains_AddOrUpdate_V2Command request, CancellationToken cancellationToken)
        {
            if (request.RequestData == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }

            if (request.RequestData.Id == null || request.RequestData.Id == Guid.Empty)
            {
                throw new ApplicationException("Id là bắt buộc");
            }

            List<GainsSchoolRequest>? listSchool = request.RequestData.GainsSchools;
            List<GainsFamilyRequest>? listFamily = request.RequestData.GainsFamilies;

            var obj = await _context.Gains.Where(x => x.Id == request.RequestData.Id.Value).FirstOrDefaultAsync();

            if (obj == null)
                throw new ApplicationException($"Không tìm thấy Hợp đồng có id: {request.RequestData.Id.Value}");

            if (listSchool != null)
                await GainsService.AddOrUpdateSchool(request.RequestData.Id.Value, listSchool, request.userId, _context, cancellationToken);

            if (listFamily != null)
                await GainsService.AddOrUpdateFamily(request.RequestData.Id.Value, listFamily, request.userId, _context, cancellationToken);

            obj.DayOfBirth = request.RequestData.DayOfBirth ?? obj.DayOfBirth;
            obj.PlaceOfBirth = request.RequestData?.PlaceOfBirth ?? obj.PlaceOfBirth;
            obj.NativePlace = request.RequestData?.NativePlace ?? obj.NativePlace;
            obj.Address = request.RequestData?.Address ?? obj.Address;
            obj.Email = request.RequestData?.Email ?? obj.Email;
            obj.Hobby = request.RequestData?.Hobby ?? obj.Hobby;
            obj.StrongPoint = request.RequestData?.StrongPoint ?? obj.StrongPoint;
            obj.Habit = request.RequestData?.Habit ?? obj.Habit;
            obj.WeekendActivity = request.RequestData?.WeekendActivity ?? obj.WeekendActivity;
            obj.Idol = request.RequestData?.Idol ?? obj.Idol;
            obj.Clubs = request.RequestData?.Clubs ?? obj.Clubs;

            obj.Degree = request.RequestData?.Degree ?? obj.Degree;
            obj.Company = request.RequestData?.Company ?? obj.Company;
            obj.Position = request.RequestData?.Position ?? obj.Position;
            obj.PositionTerm = request.RequestData?.PositionTerm ?? obj.PositionTerm;
            obj.Speciality = request.RequestData?.Speciality ?? obj.Speciality;
            obj.FormerJob = request.RequestData?.FormerJob ?? obj.FormerJob;

            obj.NearGoal = request.RequestData?.NearGoal ?? obj.NearGoal;
            obj.LongGoal = request.RequestData?.LongGoal ?? obj.LongGoal;

            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = request.userId;

            //if (request.RequestData.Id == null)
            //{
            //    _context.Gains.Add(obj);
            //}
            //else
            //{
            //    if (tmp.CreatedDate == tmp.LastModifiedDate)
            //    {
            //        var status = await _context.RelationshipStatuses
            //                           .FirstOrDefaultAsync(s => s.Code == RelationshipStatusEnum.PROCESSING.ToString());
            //        if (status != null)
            //        {
            //            exist.RelationshipStatus = status;
            //            exist.RelationshipStatusId = status.Id;
            //        }
            //    }
            //    obj.LastModifiedDate = DateTime.Now;
            //    _context.Gains.Update(obj);
            //}

            _context.Gains.Update(obj);

            await _context.SaveChangesAsync(cancellationToken);

            var gains = await _context.Gains.Where(x => x.Id == obj.Id && x.DeleteFlag != true)
                                            .Include(x => x.GainsSchools.Where(gs => gs.DeleteFlag != true))
                                            .Include(x => x.GainsFamilies.Where(gm => gm.DeleteFlag != true))
                                            .FirstOrDefaultAsync();

            var result = _mapper.Map<GainsMobile_AddOrUpdateRequest>(gains);


            var eventLog = await _eventLogService.Create("GainsFeature", "GainsFeature",
                                                "Gains_AddOrUpdateCommand", request.userId);


            return Result<GainsMobile_AddOrUpdateRequest>.Success(result);
        }
    }
}
