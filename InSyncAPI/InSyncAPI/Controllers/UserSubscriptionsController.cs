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
    public class UserSubscriptionsController : ControllerBase
    {
        private IUserSubscriptionRepository _userSubRepo;
        private IUserRepository _userRepo;
        private ISubscriptionPlanRepository _subRepo;
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
            IMapper mapper)
        {
            _userSubRepo = userSubRepo;
            _userRepo = userRepo;
            _subRepo = subRepo;
            _mapper = mapper;
        }


        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<UserSubscription>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubscriptions()
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _userSubRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubscription(int? index, int? size)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<UserSubscription> listUserSubsciption = new List<UserSubscription>();
            int total = 0;
          
            if (index == null || size == null)
            {
                listUserSubsciption = _userSubRepo.GetMulti
                    (c => true, includes);
                total = listUserSubsciption.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listUserSubsciption = _userSubRepo.GetMultiPaging
            (c => true, out total, index.Value, size.Value, includes
            );
            }
            var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
            var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubsciptionOfUser(Guid userId, int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<UserSubscription> listUserSubsciption = new List<UserSubscription>();
            int total = 0;

            if (index == null || size == null)
            {
                listUserSubsciption = _userSubRepo.GetMulti
                    (c => c.UserId.Equals(userId), includes);
                total = listUserSubsciption.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listUserSubsciption = _userSubRepo.GetMultiPaging
            (c => c.UserId.Equals(userId), out total, index.Value, size.Value, includes
            );
            }

            var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
            var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }
        [HttpGet("user/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubsciptionOfUserClerk(string userIdClerk, int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<UserSubscription> listUserSubsciption = new List<UserSubscription>();
            int total = 0;

            if (index == null || size == null)
            {
                listUserSubsciption = _userSubRepo.GetMulti
                    (c => c.User.UserIdClerk.Equals(userIdClerk), includes);
                total = listUserSubsciption.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listUserSubsciption = _userSubRepo.GetMultiPaging
            (c => c.User.UserIdClerk.Equals(userIdClerk), out total, index.Value, size.Value, includes
            );
            }
           
            var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
            var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }


        [HttpGet("user-no-expired/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ViewUserSubsciptionDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubsciptionOfUserNoExpired(Guid userId)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var now = DateTime.Now;
            var listUserSubsciption = _userSubRepo.GetMulti(c => c.UserId.Equals(userId) && c.StripeCurrentPeriodEnd >= now);
            var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
           
            return Ok(response);
        }
        [HttpGet("user-clerk-no-expired/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ViewUserSubsciptionDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubsciptionOfUserClerkNoExpired(string userIdClerk)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var now = DateTime.Now;
            var listUserSubsciption = _userSubRepo.GetMulti(c => c.User.UserIdClerk.Equals(userIdClerk) && c.StripeCurrentPeriodEnd >= now);
            var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);

            return Ok(response);
        }


        [HttpGet("subsciption/{subId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllUserSubsciptionOfSubsciption(Guid subId, int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<UserSubscription> listUserSubsciption = new List<UserSubscription>();
            int total = 0;

            if (index == null || size == null)
            {
                listUserSubsciption = _userSubRepo.GetMulti
                    (c => c.SubscriptionPlanId.Equals(subId), includes);
                total = listUserSubsciption.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listUserSubsciption = _userSubRepo.GetMultiPaging
            (c => c.SubscriptionPlanId.Equals(subId), out total, index.Value, size.Value, includes
            );
            }

            var response = _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(listUserSubsciption);
            var responsePaging = new ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUserSubsciptionDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetUserSubsciptionById(Guid id)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var userSubscription = await _userSubRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (userSubscription == null)
            {
                return NotFound("No user subsciption has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewUserSubsciptionDto>(userSubscription);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddUserSubsciption(AddUserSubsciptionDto newUserSub)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkUserExist = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newUserSub.UserId));
            if (!checkUserExist)
            {
                return BadRequest("Information of user provide invalid");
            }
            var checkSubPlaneExist = await _subRepo.CheckContainsAsync(c => c.Id.Equals(newUserSub.SubscriptionPlanId));
            if (!checkSubPlaneExist)
            {
                return BadRequest("Information of subsciption plan provide invalid");
            }

            var userSubsciption = _mapper.Map<UserSubscription>(newUserSub);
            userSubsciption.DateCreated = DateTime.Now;
            try
            {
                var response = await _userSubRepo.Add(userSubsciption);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the user subsciption.");
                }
                return Ok(new ActionUserSubsciptionResponse { Message = "User Subsciption added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       value: "An error occurred while adding User Subsciption into Database " + ex.Message);
            }

        }
        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddUserSubsciptionUserClerk(AddUserSubsciptionUserClerkDto newUserSub)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkUserExist = await _userRepo.GetSingleByCondition(c => c.UserIdClerk.Equals(newUserSub.UserIdClerk));
            if (checkUserExist == null)
            {
                return BadRequest("Information of user provide invalid");
            }
            var checkSubPlaneExist = await _subRepo.CheckContainsAsync(c => c.Id.Equals(newUserSub.SubscriptionPlanId));
            if (!checkSubPlaneExist)
            {
                return BadRequest("Information of subsciption plan provide invalid");
            }

            var userSubsciption = _mapper.Map<UserSubscription>(newUserSub);
            userSubsciption.DateCreated = DateTime.Now;
            userSubsciption.UserId = checkUserExist.Id;
            try
            {
                var response = await _userSubRepo.Add(userSubsciption);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the user subsciption.");
                }
                return Ok(new ActionUserSubsciptionResponse { Message = "User Subsciption added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       value: "An error occurred while adding User Subsciption into Database " + ex.Message);
            }

        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateUserSubsciption(Guid id, UpdateUserSubsciptionDto updateUserSub)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateUserSub.Id)
            {
                return BadRequest("User Subsciption ID information does not match");
            }

            // Fetch the existing customer review to ensure it exists
            var existingUserSub = await _userSubRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingUserSub == null)
            {
                return NotFound("User Subsciption not found.");
            }

            // Map the updated fields
            _mapper.Map(updateUserSub, existingUserSub);

            try
            {
                await _userSubRepo.Update(existingUserSub);
                return Ok(new ActionUserSubsciptionResponse { Message = "User Subsciption updated successfully.", Id = existingUserSub.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating user subsciption: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserSubsciptionResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteUserSubsciption(Guid id)
        {
            if (_userRepo == null || _userSubRepo == null || _subRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkUserSubExist = await _userSubRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkUserSubExist)
            {
                return NotFound($"Dont exist user subsciption with id {id.ToString()} to delete");
            }
            try
            {
                await _userSubRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionUserSubsciptionResponse { Message = "User subsciption deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"Error delete user subsciption: {ex.Message}");
            }

        }
    }
}
