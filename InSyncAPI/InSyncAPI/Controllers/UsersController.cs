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
        [HttpPost("clerk-web-hook")]
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
    }
}
