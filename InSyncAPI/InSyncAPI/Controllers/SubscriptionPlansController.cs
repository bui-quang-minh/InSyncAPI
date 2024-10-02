using AutoMapper;
using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlansController : ControllerBase
    {
        private ISubscriptionPlanRepository _subscriptionPlanRepo;
        private IUserRepository _userRepository;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private string[] includes = new string[]
            {
                nameof(SubscriptionPlan.User)
            };

        public SubscriptionPlansController(ISubscriptionPlanRepository subscriptionPlanRepo,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _userRepository = userRepository;
            _mapper = mapper;
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<SubscriptionPlan>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetSubscriptionPlans()
        {
            if (_subscriptionPlanRepo == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var response = _subscriptionPlanRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewSubscriptionPlanDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllSubsciptionPlan(string? keySearch = "",int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {

            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }


            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            keySearch = string.IsNullOrEmpty(keySearch)?"":keySearch.ToLower();;

            var listSubsciptionPlan = _subscriptionPlanRepo.GetMultiPaging
            (c => c.SubscriptionsName.ToLower().Contains(keySearch)
                , out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewSubscriptionPlanDto>>(listSubsciptionPlan);
            var responsePaging = new ResponsePaging<IEnumerable<ViewSubscriptionPlanDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewSubscriptionPlanDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetSubsciptionPlanById(Guid id)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddSubsciptionPlan(AddSubscriptionPlanDto newSubscription)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkUserExist = await _userRepository.CheckContainsAsync(c => c.Id.Equals(newSubscription.UserId));
            if (!checkUserExist)
            {
                return NotFound($"Dont exist user with id {newSubscription.UserId.ToString()} to add Subsciption Plan");
            }

            SubscriptionPlan subscriptionPlan = _mapper.Map<SubscriptionPlan>(newSubscription);
            subscriptionPlan.DateCreated = DateTime.Now;
            try
            {
                var response = await _subscriptionPlanRepo.Add(subscriptionPlan);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the subsciption plan.");
                }

                return Ok(new ActionSubsciptionPlanResponse { Message = "Subscription plan added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding Subsciption Plan into Database " + ex.Message);
            }

        }
        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddSubsciptionPlanUserClerk(AddSubscriptionPlanUserClerkDto newSubscription)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkUserExist = await _userRepository.GetSingleByCondition(c => c.UserIdClerk.Equals(newSubscription.UserIdClerk));
            if (checkUserExist == null)
            {
                return NotFound($"Dont exist user with id {newSubscription.UserIdClerk.ToString()} to add Subsciption Plan");
            }

            SubscriptionPlan subscriptionPlan = _mapper.Map<SubscriptionPlan>(newSubscription);
            subscriptionPlan.DateCreated = DateTime.Now;
            subscriptionPlan.UserId = checkUserExist.Id;
            try
            {
                var response = await _subscriptionPlanRepo.Add(subscriptionPlan);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the subsciption plan.");
                }

                return Ok(new ActionSubsciptionPlanResponse { Message = "Subscription plan added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding Subsciption Plan into Database " + ex.Message);
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, UpdateSubscriptionPlanDto updateSubsciption)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
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
                return Ok(new ActionSubsciptionPlanResponse { Message = "Subsciption plan updated successfully.", Id = existSubsciption.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating subsciption plan: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteSubsciptionPlan(Guid id)
        {
            if (_subscriptionPlanRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkPolicyExist = await _subscriptionPlanRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                return NotFound($"Dont exist subsciption plan with id {id.ToString()} to delete");
            }
            try
            {
                await _subscriptionPlanRepo.DeleteSubsciptionPlan(id);
                return Ok(new ActionSubsciptionPlanResponse { Message = "Subsciption plan deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"Error delete subsciption plan: {ex.Message}");
            }


        }
    }
}
