﻿using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;
using System.Diagnostics;

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
        private string[] includes = new string[]
        {
            nameof(UserSubscription.User),
            nameof(UserSubscription.SubscriptionPlan)
        };

        public UserSubscriptionsController(IUserSubscriptionRepository userSubRepo,
            IUserRepository userRepo,
            ISubscriptionPlanRepository subRepo,
            IMapper mapper,
            ILogger<UserSubscriptionsController> logger
            )
        {
            _userSubRepo = userSubRepo;
            _userRepo = userRepo;
            _subRepo = subRepo;
            _mapper = mapper;
            _logger = logger;
          
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
        public async Task<IActionResult> GetAllUserSubsciptionOfUser(Guid userId, int? index , int? size)
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

                if (listUserSubsciption == null || !listUserSubsciption.Any())
                {
                    _logger.LogWarning("No subscriptions found for user {UserId}.", userId);
                    return NotFound($"No subscriptions found for user with ID {userId}.");
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

        [HttpGet("user/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubsciptionOfUserClerk(string userIdClerk, int? index , int? size )
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

                if (listUserSubsciption == null || !listUserSubsciption.Any())
                {
                    _logger.LogWarning("No subscriptions found for clerk {UserIdClerk}.", userIdClerk);
                    return NotFound($"No subscriptions found for clerk with ID {userIdClerk}.");
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
        public async Task<IActionResult> GetUserSubsciptionOfUserNoExpired(Guid userId)
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

                if (listUserSubsciption == null || !listUserSubsciption.Any())
                {
                    _logger.LogWarning("No non-expired subscriptions found for user {UserId}.", userId);
                    return NotFound($"No non-expired subscriptions found for user with ID {userId}.");
                }

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
        public async Task<IActionResult> GetUserSubsciptionOfUserClerkNoExpired(string userIdClerk)
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

                if (listUserSubsciption == null || !listUserSubsciption.Any())
                {
                    _logger.LogWarning("No non-expired subscriptions found for clerk {UserIdClerk}.", userIdClerk);
                    return NotFound($"No non-expired subscriptions found for clerk with ID {userIdClerk}.");
                }

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
        public async Task<IActionResult> GetAllUserSubsciptionOfSubsciption(Guid subId, int? index, int? size)
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
        public async Task<IActionResult> GetUserSubsciptionById(Guid id)
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
        public async Task<IActionResult> AddUserSubsciption(AddUserSubsciptionDto newUserSub)
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
                userSubscription.DateCreated = DateTime.Now;

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
        public async Task<IActionResult> AddUserSubsciptionUserClerk(AddUserSubsciptionUserClerkDto newUserSub)
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
                userSubscription.DateCreated = DateTime.Now;
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
        public async Task<IActionResult> UpdateUserSubsciption(Guid id, UpdateUserSubsciptionDto updateUserSub)
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
        public async Task<IActionResult> DeleteUserSubsciption(Guid id)
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

    }
}
