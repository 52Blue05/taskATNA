using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.CustomerFeature.Commands
{
    public record Customer_DeleteByIdCommand(Guid userId, DeleteRequest RequestData) : IRequest<Result<string>>;
    public class Customer_DeleteByIdCommandHandler : IRequestHandler<Customer_DeleteByIdCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Customer_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<string>> Handle(Customer_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            //string result = string.Empty;

            if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

            //List<Guid> listError = new List<Guid>();
            var relationships = await _context.Relationships.Where(r => ids.Contains((Guid)r.CustomerId) && r.DeleteFlag == false).ToListAsync();
            if (relationships.Any()) throw new ApplicationException("Khách hàng đã có mối quan hệ!");
            //{
            //    var uniqueRelationships = relationships.GroupBy(r => r.CustomerId).Select(g => g.First()).ToList();
            //    listError.AddRange(uniqueRelationships.Select(r => (Guid)r.CustomerId).ToList());
            //    ids.RemoveAll(id => uniqueRelationships.Any(r => r.CustomerId == id));
            //}
            var opportunities = await _context.Opportunities.Where(r => ids.Contains((Guid)r.CustomerId) && r.DeleteFlag == false).ToListAsync();
            if (opportunities.Any()) throw new ApplicationException("Khách hàng đã có thông tin cơ hội!");
            //{
            //    var uniqueOpportunities = opportunities.GroupBy(r => r.CustomerId).Select(g => g.First()).ToList();
            //    listError.AddRange(uniqueOpportunities.Select(r => (Guid)r.CustomerId).ToList());
            //    ids.RemoveAll(id => uniqueOpportunities.Any(r => r.CustomerId == id));
            //}
            var contracts = await _context.Contracts.Where(r => ids.Contains((Guid)r.CustomerId) && r.DeleteFlag == false).ToListAsync();
            if (contracts.Any()) throw new ApplicationException("Khách hàng đã có thông tin trong hợp đồng!");

            var query = await _context.Customers.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (query.Any())
            {
                foreach (var item in query)
                {
                    item.DeleteFlag = true;
                    item.LastModifiedDate = DateTime.Now;
                    item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
                }

                var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                                "Customer_DeleteByIdCommand", request.userId);

                _context.Customers.UpdateRange(query);

                await _context.SaveChangesAsync(cancellationToken);
            }

            //if (listError.Any())
            //{
            //    var listErrorQuery = await _context.Customers.Where(c => listError.Contains(c.Id)).Select(c => c.Code).ToListAsync();
            //    string listCustomerNameError = string.Join(", ", listErrorQuery);
            //    result = "Không thể xóa các khách hàng đã tồn tại trong mối quan hệ và cơ hội: " + listCustomerNameError;

            //    return Result<string>.Failure(result);
            //}

            return Result<string>.Success(string.Empty);
        }
    }

}
