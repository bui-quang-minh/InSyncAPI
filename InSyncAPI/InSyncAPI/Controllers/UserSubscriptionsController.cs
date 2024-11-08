
using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;
using Stripe;
using System.Diagnostics;
using System.Text.Json;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSubscriptionsController : ControllerBase
    {
        private IUserSubscriptionRepository _userSubRepo;
        private IUserRepository _userRepo;
        private ISubscriptionPlanRepository _subRepo;
        private ILogger<UserSubscriptionsController> _logger;
        private readonly string TAG = nameof(UserSubscriptionsController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private IConfiguration _config;
        private readonly string _webhookSecretCheckoutSessionSucceeded = "";
        private readonly string _webhookSecretInvoicePaymentSucceeded = "";
        private string[] includes = new string[]
        {
            nameof(UserSubscription.User),
            nameof(UserSubscription.SubscriptionPlan)
        };

        public UserSubscriptionsController(IUserSubscriptionRepository userSubRepo,
            IUserRepository userRepo,
            ISubscriptionPlanRepository subRepo,
            IMapper mapper,
            ILogger<UserSubscriptionsController> logger,
            IConfiguration config
            )
        {
            _userSubRepo = userSubRepo;
            _userRepo = userRepo;
            _subRepo = subRepo;
            _mapper = mapper;
            _logger = logger;
            _config = config;
            _webhookSecretCheckoutSessionSucceeded = _config.GetValue<string>("StripConfig:WebhookSecretCheckoutSessionSucceeded");
            _webhookSecretInvoicePaymentSucceeded = _config.GetValue<string>("StripConfig:WebhookSecretInvoicePaymentSucceeded");
        }


        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<UserSubscription>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubscriptions()
        {
            var stopwatch = Stopwatch.StartNew();
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "User repository or UserSubscription repository or SubsciptionPlan repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            try
            {
                _logger.LogInformation(TAG + "Fetching user subsciptions from the database.");

                var response = _userSubRepo.GetAll().AsQueryable();

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved {UserSubsciptionCount} user subscriptions from the database in {ElapsedMilliseconds}ms.",
                    response.Count(), stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving user subscriptions. Total time taken: {ElapsedMilliseconds}ms.",
                    stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: $"Error retrieving user subscriptions: {ex.Message}");
            }

        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubscription(int? index, int? size)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all user subscriptions at {RequestTime}", DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            IEnumerable<UserSubscription> listUserSubsciption;
            int total;

            try
            {
                if (index == null || size == null)
                {
                    listUserSubsciption = _userSubRepo.GetMulti(c => true, includes);
                    total = listUserSubsciption.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listUserSubsciption = _userSubRepo.GetMultiPaging(c => true, out total, index.Value, size.Value, includes);
                }

                if (listUserSubsciption == null || !listUserSubsciption.Any())
                {
                    _logger.LogWarning("No user subscriptions found.");
                    return NotFound("No user subscriptions found.");
                }

                var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
                var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved {Count} user subscriptions in {ElapsedMilliseconds}ms.", total, stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving user subscriptions in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving user subscriptions: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubsciptionOfUser([FromRoute] Guid userId, int? index, int? size)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all subscriptions for user {UserId} at {RequestTime}", userId, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user subscriptions request.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<UserSubscription> listUserSubsciption;
            int total;

            try
            {
                if (index == null || size == null)
                {
                    listUserSubsciption = _userSubRepo.GetMulti(c => c.UserId.Equals(userId), includes);
                    total = listUserSubsciption.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listUserSubsciption = _userSubRepo.GetMultiPaging(c => c.UserId.Equals(userId), out total, index.Value, size.Value, includes);
                }


                var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
                var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved {Count} subscriptions for user {UserId} in {ElapsedMilliseconds}ms.", total, userId, stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving subscriptions for user {UserId} in {ElapsedMilliseconds}ms.", userId, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving user subscriptions: {ex.Message}");
            }
        }

        [HttpGet("user-clerk/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubsciptionOfUserClerk([FromRoute] string userIdClerk, int? index, int? size)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all subscriptions for clerk {UserIdClerk} at {RequestTime}", userIdClerk, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user subscriptions request.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<UserSubscription> listUserSubsciption;
            int total;

            try
            {
                if (index == null || size == null)
                {
                    listUserSubsciption = _userSubRepo.GetMulti(c => c.User.UserIdClerk.Equals(userIdClerk), includes);
                    total = listUserSubsciption.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listUserSubsciption = _userSubRepo.GetMultiPaging(c => c.User.UserIdClerk.Equals(userIdClerk), out total, index.Value, size.Value, includes);
                }

                var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
                var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved {Count} subscriptions for clerk {UserIdClerk} in {ElapsedMilliseconds}ms.", total, userIdClerk, stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving subscriptions for clerk {UserIdClerk} in {ElapsedMilliseconds}ms.", userIdClerk, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving user subscriptions: {ex.Message}");
            }
        }



        [HttpGet("user-no-expired/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ViewUserSubsciptionDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubsciptionOfUserNoExpired([FromRoute] Guid userId)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get non-expired subscriptions for user {UserId} at {RequestTime}", userId, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for getting non-expired subscriptions.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var now = DateTime.Now;
                var listUserSubsciption = _userSubRepo.GetMulti(c => c.UserId.Equals(userId) && c.StripeCurrentPeriodEnd >= now);


                var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved {Count} non-expired subscriptions for user {UserId} in {ElapsedMilliseconds}ms.", response.Count(), userId, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving non-expired subscriptions for user {UserId} in {ElapsedMilliseconds}ms.", userId, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving non-expired subscriptions: {ex.Message}");
            }
        }

        [HttpGet("user-clerk-no-expired/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ViewUserSubsciptionDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubsciptionOfUserClerkNoExpired([FromRoute] string userIdClerk)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get non-expired subscriptions for clerk {UserIdClerk} at {RequestTime}", userIdClerk, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for getting non-expired subscriptions.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var now = DateTime.Now;
                var listUserSubsciption = _userSubRepo.GetMulti(c => c.User.UserIdClerk.Equals(userIdClerk) && c.StripeCurrentPeriodEnd >= now);



                var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved {Count} non-expired subscriptions for clerk {UserIdClerk} in {ElapsedMilliseconds}ms.", response.Count(), userIdClerk, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving non-expired subscriptions for clerk {UserIdClerk} in {ElapsedMilliseconds}ms.", userIdClerk, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving non-expired subscriptions: {ex.Message}");
            }
        }



        [HttpGet("subsciption/{subId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubsciptionOfSubsciption([FromRoute] Guid subId, int? index, int? size)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all user subscriptions for subscription ID {SubId} at {RequestTime}", subId, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for getting user subscriptions.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<UserSubscription> listUserSubsciption = new List<UserSubscription>();
            int total = 0;

            try
            {
                if (index == null || size == null)
                {
                    listUserSubsciption = _userSubRepo.GetMulti(c => c.SubscriptionPlanId.Equals(subId), includes);
                    total = listUserSubsciption.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listUserSubsciption = _userSubRepo.GetMultiPaging(c => c.SubscriptionPlanId.Equals(subId), out total, index.Value, size.Value, includes);
                }

                if (listUserSubsciption == null || !listUserSubsciption.Any())
                {
                    _logger.LogWarning("No subscriptions found for subscription ID {SubId}.", subId);
                    return NotFound($"No subscriptions found for subscription ID {subId}.");
                }

                var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
                var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved {Count} subscriptions for subscription ID {SubId} in {ElapsedMilliseconds}ms.", response.Count(), subId, stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving subscriptions for subscription ID {SubId} in {ElapsedMilliseconds}ms.", subId, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving subscriptions: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUserSubsciptionDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubsciptionById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get user subscription by ID {Id} at {RequestTime}", id, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for getting user subscription by ID.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var userSubscription = await _userSubRepo.GetSingleByCondition(c => c.Id.Equals(id));

                if (userSubscription == null)
                {
                    _logger.LogWarning("No user subscription found for ID {Id}.", id);
                    return NotFound("No user subscription has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewUserSubsciptionDto>(userSubscription);

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved user subscription for ID {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving user subscription for ID {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving user subscription: {ex.Message}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddUserSubsciption([FromBody] AddUserSubsciptionDto newUserSub)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add user subscription at {RequestTime}", DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while adding user subscription.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var checkUserExist = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newUserSub.UserId));
                if (!checkUserExist)
                {
                    _logger.LogWarning("Invalid user information provided: {UserId}", newUserSub.UserId);
                    return BadRequest("Information of user provided is invalid");
                }

                var checkSubPlaneExist = await _subRepo.CheckContainsAsync(c => c.Id.Equals(newUserSub.SubscriptionPlanId));
                if (!checkSubPlaneExist)
                {
                    _logger.LogWarning("Invalid subscription plan information provided: {SubscriptionPlanId}", newUserSub.SubscriptionPlanId);
                    return BadRequest("Information of subscription plan provided is invalid");
                }

                var userSubscription = _mapper.Map<UserSubscription>(newUserSub);
                userSubscription.DateCreated = DateTime.UtcNow;


                var response = await _userSubRepo.Add(userSubscription);
                if (response == null)
                {
                    _logger.LogError("Error occurred while adding the user subscription.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the user subscription.");
                }

                stopwatch.Stop();
                _logger.LogInformation("User subscription added successfully with ID {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionUserSubsciptionResponse { Message = "User Subscription added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding User Subscription into Database in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding User Subscription into Database: " + ex.Message);
            }
        }

        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddUserSubsciptionUserClerk([FromBody] AddUserSubsciptionUserClerkDto newUserSub)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add user subscription for clerk at {RequestTime}", DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while adding user subscription for clerk.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var checkUserExist = await _userRepo.GetSingleByCondition(c => c.UserIdClerk.Equals(newUserSub.UserIdClerk));
                if (checkUserExist == null)
                {
                    _logger.LogWarning("Invalid user information provided for clerk: {UserIdClerk}", newUserSub.UserIdClerk);
                    return BadRequest("Information of user provided is invalid");
                }

                var checkSubPlaneExist = await _subRepo.CheckContainsAsync(c => c.Id.Equals(newUserSub.SubscriptionPlanId));
                if (!checkSubPlaneExist)
                {
                    _logger.LogWarning("Invalid subscription plan information provided: {SubscriptionPlanId}", newUserSub.SubscriptionPlanId);
                    return BadRequest("Information of subscription plan provided is invalid");
                }

                var userSubscription = _mapper.Map<UserSubscription>(newUserSub);
                userSubscription.DateCreated = DateTime.UtcNow;
                userSubscription.UserId = checkUserExist.Id;

                var response = await _userSubRepo.Add(userSubscription);
                if (response == null)
                {
                    _logger.LogError("Error occurred while adding the user subscription for clerk.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the user subscription.");
                }

                stopwatch.Stop();
                _logger.LogInformation("User subscription for clerk added successfully with ID {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionUserSubsciptionResponse { Message = "User Subscription added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding User Subscription for clerk into Database in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding User Subscription into Database: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateUserSubsciption([FromRoute] Guid id, [FromBody] UpdateUserSubsciptionDto updateUserSub)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update user subscription with ID {UserSubscriptionId} at {RequestTime}", id, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while updating user subscription with ID {UserSubscriptionId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateUserSub.Id)
            {
                _logger.LogWarning("User Subscription ID mismatch: provided ID {ProvidedId}, expected ID {ExpectedId}.", updateUserSub.Id, id);
                return BadRequest("User Subscription ID information does not match");
            }

            // Fetch the existing user subscription to ensure it exists
            var existingUserSub = await _userSubRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingUserSub == null)
            {
                _logger.LogWarning("User Subscription not found for ID {UserSubscriptionId}.", id);
                return NotFound("User Subscription not found.");
            }

            // Map the updated fields
            _mapper.Map(updateUserSub, existingUserSub);


            try
            {
                await _userSubRepo.Update(existingUserSub);
                stopwatch.Stop();
                _logger.LogInformation("User Subscription with ID {UserSubscriptionId} updated successfully in {ElapsedMilliseconds}ms.", existingUserSub.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionUserSubsciptionResponse { Message = "User Subscription updated successfully.", Id = existingUserSub.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error occurred while updating user subscription with ID {UserSubscriptionId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating user subscription: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteUserSubsciption([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to delete user subscription with ID {UserSubscriptionId} at {RequestTime}", id, DateTime.UtcNow);

            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                _logger.LogError("User repository, user subscription repository, subscription repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while deleting user subscription with ID {UserSubscriptionId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkUserSubExist = await _userSubRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkUserSubExist)
            {
                _logger.LogWarning("Attempted to delete non-existent user subscription with ID {UserSubscriptionId}.", id);
                return NotFound($"Don't exist user subscription with id {id} to delete");
            }

            try
            {
                await _userSubRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation("User subscription with ID {UserSubscriptionId} deleted successfully in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionUserSubsciptionResponse { Message = "User subscription deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error occurred while deleting user subscription with ID {UserSubscriptionId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting user subscription: {ex.Message}");
            }
        }


        [HttpPost("webhook_checkout_session_completed")]
        public async Task<IActionResult> HandleStripeEventCheckoutCompleted()
        {
            // Đọc nội dung của yêu cầu
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            Event stripeEvent;

            try
            {
                // Xác minh chữ ký webhook
                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecretCheckoutSessionSucceeded);
            }
            catch (Exception ex)
            {
                // Nếu xác minh không thành công, trả về mã 400
                _logger.LogError(ex, $"Webhook signature verification failed. {ex.Message}");
                return BadRequest();
            }

            // Kiểm tra loại sự kiện
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                // Lấy đối tượng Checkout.Session từ event
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
               
                if (session != null)
                {
                    var userSubscription = new UserSubscription
                    {
                        SubscriptionPlanId = Guid.Parse("8DB99EDF-775B-4DCB-A8D4-6344B18C466F"),
                        UserId = Guid.Parse("64ED4C77-60E0-463C-817C-0B07D7AB1DAD"),
                        StripeCurrentPeriodEnd = DateTime.UtcNow,
                        StripeCustomerId =  json,
                        StripePriceId = "falsdfjals",
                        StripeSubscriptionId = session.Metadata["subscriptionPlanId"],
                        DateCreated = DateTime.UtcNow,

                    };
                    await  _userSubRepo.Add(userSubscription);
                    // Truy xuất thông tin từ session
                    //var customerEmail = session.CustomerDetails.Email;
                    //var customerExist = await _userRepo.GetSingleByCondition(c => c.Email.ToLower().Equals(customerEmail.ToLower()));
                    //if (customerExist == null)
                    //{
                    //    return BadRequest($"Customer information is incorrect with InSync system {customerEmail}.");
                    //}
                    //// subscription id stripe
                    //var stripeSubscriptionId = session.SubscriptionId;

                    //var subscriptionService = new SubscriptionService();
                    //var subscription = subscriptionService.Get(stripeSubscriptionId);
                    //// lấy current period end
                    //var stripeCurrentPeriodEnd = subscription.CurrentPeriodEnd;
                    //// customer id stripe
                    //var stripeCustomerId = session.CustomerId;         // ID khách hàng trong Stripe
                   
                  
                    ////price id
                    //var stripePriceId = session.LineItems.Data.FirstOrDefault()?.Price.Id;



                }
            }

            return Ok(); // Trả về 200 để Stripe biết rằng webhook đã được xử lý thành công
        }

        [HttpPost("webhook_invoice_payment_succeeded")]
        public async Task<IActionResult> HandleStripeEventInvoicePayment()
        {
            // Đọc nội dung của yêu cầu
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            Event stripeEvent;

            try
            {
                // Xác minh chữ ký webhook
                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecretInvoicePaymentSucceeded);
            }
            catch (Exception ex)
            {
                // Nếu xác minh không thành công, trả về mã 400
                Console.WriteLine($"Webhook signature verification failed. {ex.Message}");
                return BadRequest();
            }

            // Kiểm tra loại sự kiện
            if (stripeEvent.Type == EventTypes.InvoicePaymentSucceeded)
            {
                // Lấy đối tượng Invoice từ event
                var invoice = stripeEvent.Data.Object as Stripe.Invoice;

                if (invoice != null)
                {
                    // Truy xuất thông tin từ hóa đơn
                    var customerId = invoice.CustomerId;            // ID khách hàng trong Stripe
                    var invoiceId = invoice.Id;                     // ID của hóa đơn
                    var amountPaid = invoice.AmountPaid;            // Số tiền đã thanh toán (cents)
                    var subscriptionId = invoice.SubscriptionId;    // ID đăng ký, nếu là hóa đơn đăng ký
                    var metadata = invoice.Metadata;
                    // Metadata tùy chỉnh, nếu có

                    Console.WriteLine($"Customer ID: {customerId}");
                    Console.WriteLine($"Invoice ID: {invoiceId}");
                    Console.WriteLine($"Amount Paid: {amountPaid}");
                    Console.WriteLine($"Subscription ID: {subscriptionId}");

                    // Thực hiện các xử lý khác, ví dụ: cập nhật trạng thái đăng ký, gửi biên lai cho khách hàng, v.v.
                }
            }

            return Ok(); // Trả về mã 200 để Stripe biết rằng webhook đã được xử lý thành công
        }
    }


}
