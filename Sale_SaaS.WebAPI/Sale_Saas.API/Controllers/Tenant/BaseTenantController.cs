using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Sale_Saas.API.Controllers.Tenant
{
    [Route("api/[controller]")]
    [ApiController]
	//[Authorize]
	public class BaseTenantController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
       
    }
}
