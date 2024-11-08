using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutApiController : ControllerBase
    {
    }
    public class StripeOptions
    {
        public string option { get; set; }
    }
}
