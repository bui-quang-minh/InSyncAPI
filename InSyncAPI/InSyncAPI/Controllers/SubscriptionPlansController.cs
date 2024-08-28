using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;
using System.Linq.Expressions;

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

        [HttpGet("get-subscription-plan/{id}")]
        public async Task<IActionResult> GetSubsciptionPlanById(Guid id)
        {
            string[] includes = new string[]
            {
                nameof(SubscriptionPlan.User)
            };
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var subsciptionPlan = await _subscriptionPlanRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
            if (subsciptionPlan == null)
            {
                return NotFound("No subsciption plan has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewSubscriptionPlanDto>(subsciptionPlan);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubsciptionPlan(AddSubscriptionPlanDto newSubscription)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SubscriptionPlan subscriptionPlan = _mapper.Map<SubscriptionPlan>(newSubscription);
            subscriptionPlan.DateCreated = DateTime.Now;

            
            var response = await _subscriptionPlanRepo.Add(subscriptionPlan);

            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Error occurred while adding the subsciption plan.");
            }

            return Ok(new { message = "Subscription plan added successfully.", Id = response.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrivacyPolicy(Guid id, UpdateSubscriptionPlanDto updateSubsciption)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updateSubsciption.Id)
            {
                return BadRequest("Subsciption plan ID information does not match");
            }

           
            var existSubsciption = await _subscriptionPlanRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existSubsciption == null)
            {
                return NotFound("Subsciption plan not found.");
            }
            existSubsciption.DateUpdated = DateTime.Now;
          
            _mapper.Map(updateSubsciption, existSubsciption);

            try
            {
                await _subscriptionPlanRepo.Update(existSubsciption);
                return Ok(new { message = "Subsciption plan updated successfully.", Id = existSubsciption.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating subsciption plan: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubsciptionPlan(Guid id)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var checkPolicyExist = await _subscriptionPlanRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                return NotFound($"Dont exist subsciption plan with id {id.ToString()} to delete");
            }
            await _subscriptionPlanRepo.DeleteMulti(c => c.Id.Equals(id));
            return Ok(new { message = "Subsciption plan deleted successfully.", Id = id });
        }
    }
}
