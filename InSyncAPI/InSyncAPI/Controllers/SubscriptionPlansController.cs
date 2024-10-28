using AutoMapper;
using BusinessObjects.Models;
using DataAccess.ContextAccesss;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlansController : ControllerBase
    {
        private ISubscriptionPlanRepository _subscriptionPlanRepo;
        private IUserRepository _userRepository;
        private ILogger<SubscriptionPlansController> _logger;
        private readonly string TAG = nameof(SubscriptionPlansController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private string[] includes = new string[]
            {
                nameof(SubscriptionPlan.User)
            };

        public SubscriptionPlansController(ISubscriptionPlanRepository subscriptionPlanRepo,
            IUserRepository userRepository,
            IMapper mapper, ILogger<SubscriptionPlansController> logger)
        {
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<SubscriptionPlan>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetSubscriptionPlans()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to get subscription plans at {RequestTime}", DateTime.UtcNow);

            if (_subscriptionPlanRepo == null || _userRepository == null || _mapper == null)
            {
                _logger.LogError("Subscription plan repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            try
            {
                var response = _subscriptionPlanRepo.GetAll().AsQueryable();
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved subscription plans in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving subscription plans in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving subscription plans: {ex.Message}");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewSubscriptionPlanDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllSubsciptionPlan(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to get all subscription plans at {RequestTime}", DateTime.UtcNow);

            if (_subscriptionPlanRepo == null || _userRepository == null || _mapper == null)
            {
                _logger.LogError("Subscription plan repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            IEnumerable<SubscriptionPlan> listSubsciptionPlan = new List<SubscriptionPlan>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listSubsciptionPlan = _subscriptionPlanRepo.GetMulti
                        (c => c.SubscriptionsName.ToLower().Contains(keySearch), includes);
                    total = listSubsciptionPlan.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listSubsciptionPlan = _subscriptionPlanRepo.GetMultiPaging
                        (c => c.SubscriptionsName.ToLower().Contains(keySearch), out total, index.Value, size.Value, includes);
                }

                var response = _mapper.Map<IEnumerable<ViewSubscriptionPlanDto>>(listSubsciptionPlan);
                var responsePaging = new ResponsePaging<IEnumerable<ViewSubscriptionPlanDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved subscription plans in {ElapsedMilliseconds}ms with total count: {TotalCount}.", stopwatch.ElapsedMilliseconds, total);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving subscription plans in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving subscription plans: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewSubscriptionPlanDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetSubsciptionPlanById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to get subscription plan by ID: {Id} at {RequestTime}", id, DateTime.UtcNow);

            if (_subscriptionPlanRepo == null || _userRepository == null || _mapper == null)
            {
                _logger.LogError("Subscription plan repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to get subscription plan by ID: {Id}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var subsciptionPlan = await _subscriptionPlanRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);

                if (subsciptionPlan == null)
                {
                    _logger.LogWarning("No subscription plan found with ID: {Id}.", id);
                    return NotFound("No subscription plan has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewSubscriptionPlanDto>(subsciptionPlan);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved subscription plan by ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving subscription plan by ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving subscription plan: {ex.Message}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddSubsciptionPlan([FromBody] AddSubscriptionPlanDto newSubscription)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to add a new subscription plan at {RequestTime}", DateTime.UtcNow);

            if (_subscriptionPlanRepo == null || _userRepository == null || _mapper == null)
            {
                _logger.LogError("Subscription plan repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to add subscription plan: {@NewSubscription}", newSubscription);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkUserExist = await _userRepository.CheckContainsAsync(c => c.Id.Equals(newSubscription.UserId));
            if (!checkUserExist)
            {
                _logger.LogWarning("User with ID: {UserId} does not exist when attempting to add subscription plan.", newSubscription.UserId);
                return BadRequest($"Don't exist user with id {newSubscription.UserId.ToString()} to add Subscription Plan");
            }

            SubscriptionPlan subscriptionPlan = _mapper.Map<SubscriptionPlan>(newSubscription);
            subscriptionPlan.DateCreated = DateTime.Now;

            try
            {
                var response = await _subscriptionPlanRepo.Add(subscriptionPlan);
                if (response == null)
                {
                    _logger.LogError("Failed to add subscription plan; response is null.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the subscription plan.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Subscription plan added successfully with ID: {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionSubsciptionPlanResponse { Message = "Subscription plan added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding subscription plan into Database in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding Subscription Plan into Database: {ex.Message}");
            }
        }

        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddSubsciptionPlanUserClerk([FromBody] AddSubscriptionPlanUserClerkDto newSubscription)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to add a new subscription plan for user clerk at {RequestTime}", DateTime.UtcNow);

            if (_subscriptionPlanRepo == null || _userRepository == null || _mapper == null)
            {
                _logger.LogError("Subscription plan repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to add subscription plan: {@NewSubscription}", newSubscription);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkUserExist = await _userRepository.GetSingleByCondition(c => c.UserIdClerk.Equals(newSubscription.UserIdClerk));
            if (checkUserExist == null)
            {
                _logger.LogWarning("User with clerk ID: {UserIdClerk} does not exist when attempting to add subscription plan.", newSubscription.UserIdClerk);
                return NotFound($"Don't exist user with id {newSubscription.UserIdClerk.ToString()} to add Subscription Plan");
            }

            SubscriptionPlan subscriptionPlan = _mapper.Map<SubscriptionPlan>(newSubscription);
            subscriptionPlan.DateCreated = DateTime.Now;
            subscriptionPlan.UserId = checkUserExist.Id;

            try
            {
                var response = await _subscriptionPlanRepo.Add(subscriptionPlan);
                if (response == null)
                {
                    _logger.LogError("Failed to add subscription plan; response is null.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the subscription plan.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Subscription plan added successfully with ID: {Id} for clerk ID: {UserIdClerk} in {ElapsedMilliseconds}ms.", response.Id, newSubscription.UserIdClerk, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionSubsciptionPlanResponse { Message = "Subscription plan added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding subscription plan into Database for clerk ID: {UserIdClerk} in {ElapsedMilliseconds}ms.", newSubscription.UserIdClerk, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding Subscription Plan into Database: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateSubscriptionPlan([FromRoute] Guid id, [FromBody] UpdateSubscriptionPlanDto updateSubsciption)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to update subscription plan with ID: {Id} at {RequestTime}", id, DateTime.UtcNow);

            if (_subscriptionPlanRepo == null || _userRepository == null || _mapper == null)
            {
                _logger.LogError("Subscription plan repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to update subscription plan: {@UpdateSubscription}", updateSubsciption);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateSubsciption.Id)
            {
                _logger.LogWarning("Subscription plan ID information does not match. Expected: {ExpectedId}, Provided: {ProvidedId}", id, updateSubsciption.Id);
                return BadRequest("Subscription plan ID information does not match");
            }

            var existSubsciption = await _subscriptionPlanRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existSubsciption == null)
            {
                _logger.LogWarning("Subscription plan with ID: {Id} not found when attempting to update.", id);
                return NotFound("Subscription plan not found.");
            }

            existSubsciption.DateUpdated = DateTime.Now;
            _mapper.Map(updateSubsciption, existSubsciption);

            try
            {
                await _subscriptionPlanRepo.Update(existSubsciption);
                stopwatch.Stop();
                _logger.LogInformation("Subscription plan with ID: {Id} updated successfully in {ElapsedMilliseconds}ms.", existSubsciption.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionSubsciptionPlanResponse { Message = "Subscription plan updated successfully.", Id = existSubsciption.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating subscription plan with ID: {Id} in {ElapsedMilliseconds}ms.", existSubsciption.Id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating subscription plan: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionSubsciptionPlanResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteSubsciptionPlan([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to delete subscription plan with ID: {Id} at {RequestTime}", id, DateTime.UtcNow);

            if (_subscriptionPlanRepo == null || _userRepository == null || _mapper == null)
            {
                _logger.LogError("Subscription plan repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to delete subscription plan.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkPolicyExist = await _subscriptionPlanRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                _logger.LogWarning("Attempted to delete subscription plan with ID: {Id} that does not exist.", id);
                return NotFound($"Don't exist subscription plan with id {id} to delete");
            }

            try
            {
                await _subscriptionPlanRepo.DeleteSubsciptionPlan(id);
                stopwatch.Stop();
                _logger.LogInformation("Subscription plan with ID: {Id} deleted successfully in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionSubsciptionPlanResponse { Message = "Subscription plan deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting subscription plan with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error delete subscription plan: {ex.Message}");
            }
        }

    }
}
