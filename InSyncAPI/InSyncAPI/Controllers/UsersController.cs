using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositorys;
using System.Diagnostics;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserRepository _userRepo;
        private IMapper _mapper;
        private ILogger<UsersController> _logger;
        private readonly string TAG = nameof(UsersController) + " - ";
        //Retrive payload from clerk

        public UsersController(IUserRepository userRepo, IMapper mapper, ILogger<UsersController> logger)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpPost("clerk-web-hook-create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to create a new user at {RequestTime}", DateTime.UtcNow);

            if (_userRepo == null || _mapper == null)
            {
                _logger.LogError("User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid user data received: {@UserData}", userDto);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var user = _mapper.Map<User>(userDto);
            user.DateCreated = DateTime.UtcNow;
            user.DateUpdated = DateTime.UtcNow;

            try
            {
                var response = await _userRepo.Add(user);
                if (response == null)
                {
                    _logger.LogError("Error occurred while adding the user: {@UserData}", user);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the user.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully created user with ID: {UserId} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error creating user in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating user: {ex.Message}");
            }
        }

        [HttpPost("clerk-web-hook-update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionUserResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto userDto)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update user at {RequestTime}", DateTime.UtcNow);

            if (_userRepo == null || _mapper == null)
            {
                _logger.LogError("User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid user data received: {@UserData}", userDto);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var user = _mapper.Map<User>(userDto);

            try
            {
                // Fetch the existing user to ensure it exists
                var existingUser = await _userRepo.GetSingleByCondition(c => c.Email.Equals(user.Email));
                if (existingUser == null)
                {
                    _logger.LogWarning("User with email {Email} not found.", user.Email);
                    return NotFound("User not found.");
                }

                // Map the updated fields
                existingUser.UserName = user.UserName;
                existingUser.DisplayName = user.DisplayName;
                existingUser.ImageUrl = user.ImageUrl;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.DateUpdated = DateTime.UtcNow; // Update the DateUpdated to current time
                existingUser.StatusUser = user.StatusUser;
                if (userDto.Data.Public_Metadata.ContainsKey("role"))
                {
                    existingUser.Role = (string?)userDto.Data.Public_Metadata["role"];
                }
               

                await _userRepo.Update(existingUser);

                stopwatch.Stop();
                _logger.LogInformation("Successfully updated user with email: {Email} in {ElapsedMilliseconds}ms.", user.Email, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionUserResponse { Message = "User updated successfully.", Id = existingUser.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating user in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating user: {ex.Message}");
            }
        }


    }
}
