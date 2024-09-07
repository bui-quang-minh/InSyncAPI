using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositorys;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserRepository _userRepo;
        private IMapper _mapper;
        //Retrive payload from clerk

        public UsersController(IUserRepository userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }
        [HttpPost("clerk-web-hook-create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            if (_userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                       value: "Application service has not been created");
            }
            var user = _mapper.Map<User>(userDto);
            var response = await _userRepo.Add(user);
            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Error occurred while adding the project.");
            }
            return Ok(user);
        }

        [HttpPost("clerk-web-hook-update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto userDto)
        {
            if (_userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = _mapper.Map<User>(userDto);
            // Fetch the existing customer review to ensure it exists
            var existingUser = await _userRepo.GetSingleByCondition(c => c.Email.Equals(user.Email));
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }


            // Map the updated fields
            _mapper.Map(user, existingUser);
            try
            {
                await _userRepo.Update(existingUser);
                return Ok(new { message = "User updated successfully.", Id = existingUser.Email });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating user: {ex.Message}");
            }
        }
    }
}
