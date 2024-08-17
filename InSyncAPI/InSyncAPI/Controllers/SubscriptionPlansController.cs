using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlansController : ControllerBase
    {
        private ISubscriptionPlanRepository _subscriptionPlanRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public SubscriptionPlansController(ISubscriptionPlanRepository subscriptionPlanRepo, IMapper mapper)
        {
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _mapper = mapper;
        }
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (_subscriptionPlanRepo == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _subscriptionPlanRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("get-all-subsciption-plan")]
        public async Task<IActionResult> GetAllSubsciptionPlan(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {

            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            string[] includes = new string[]
            {
                nameof(SubscriptionPlan.User)
            };
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listSubsciptionPlan = _subscriptionPlanRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewSubscriptionPlanDto>>(listSubsciptionPlan);
            return Ok(response);
        }
    }
}
