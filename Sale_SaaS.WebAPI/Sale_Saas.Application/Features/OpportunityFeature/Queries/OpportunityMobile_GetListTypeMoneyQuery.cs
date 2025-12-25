using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries;

public record OpportunityMobile_GetListTypeMoneyQuery(Guid UserId) : IRequest<Result<List<string>>>;

public class OpportunityMobile_GetListTypeMoneyQueryHandler : IRequestHandler<OpportunityMobile_GetListTypeMoneyQuery, Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(OpportunityMobile_GetListTypeMoneyQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        List<string> moneyTypeNames = Enum.GetNames(typeof(MoneyTypeEnum)).ToList();

        return Result<List<string>>.Success(moneyTypeNames);
    }
}
