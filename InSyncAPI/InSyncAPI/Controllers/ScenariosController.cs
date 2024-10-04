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
    public class ScenariosController : ControllerBase
    {
        private IScenarioRepository _scenarioRepo;
        private IProjectRepository _projectRepo;
        private IUserRepository _userRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private string[] includes = new string[]
          {
                nameof(Scenario.Project),
                nameof(Scenario.CreatedByNavigation)
          };

        public ScenariosController(IScenarioRepository scenarioRop, IUserRepository userRepo,
           IProjectRepository projectRepo, IMapper mapper)
        {
            _scenarioRepo = scenarioRop;
            _userRepo = userRepo;
            _projectRepo = projectRepo;
            _mapper = mapper;
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Scenario>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetScenarios()
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var response = _scenarioRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllScenarios(string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower(); ;


            var listScenario = _scenarioRepo.GetMultiPaging
            (c => c.ScenarioName.ToLower().Contains(keySearch)
            , out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario
            );

            var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewScenarioDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetScenarioById(Guid id)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var scenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
            if (scenario == null)
            {
                return NotFound("No scenario has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewScenarioDto>(scenario);
            return Ok(response);
        }
        [HttpGet("scenarios-project/{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetScenarioOfProject(Guid projectId, Guid createdBy, string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower(); ;

            var scenario = _scenarioRepo.GetMultiPaging(
                c => c.ProjectId.Equals(projectId) && c.CreatedBy.Equals(createdBy) && c.ScenarioName.ToLower().Contains(keySearch),
                out int total, index.Value, size.Value, includes);


            var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(scenario);
            var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }

        [HttpGet("scenarios-project-useridclerk/{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetScenarioOfProjectByUserClerk(Guid projectId, string userIdClerk, string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower(); ;

            var scenario = _scenarioRepo.GetMultiPaging(
                c => c.ProjectId.Equals(projectId) && c.CreatedByNavigation.UserIdClerk.Equals(userIdClerk) && c.ScenarioName.ToLower().Contains(keySearch),
                out int total, index.Value, size.Value, includes);

            var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(scenario);
            var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }


        [HttpGet("scenarios-user-clerk/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllScenarioByUserIdClerk(string userIdClerk, string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower(); ;

            var listScenario = _scenarioRepo.GetMultiPaging(c =>
            c.CreatedByNavigation.UserIdClerk.Equals(userIdClerk) && c.ScenarioName.ToLower().Contains(keySearch),
             out int total, index.Value, size.Value, includes
             );

            var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
            var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }

        [HttpGet("scenarios-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllScenarioByUserId(Guid userId, string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower(); ;

            var listScenario = _scenarioRepo.GetMultiPaging
            (c => c.CreatedBy.Equals(userId) && c.ScenarioName.ToLower().Contains(keySearch),
             out int total, index.Value, size.Value, includes);

            var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
            var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddScenario(AddScenarioDto newScenario)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            bool checkExistUser = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.CreatedBy));
            if (!checkExistUser)
            {
                return BadRequest("Don't exist user has id by : " + newScenario.CreatedBy);
            }
            bool checkExistProject = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.ProjectId));
            if (!checkExistProject)
            {
                return BadRequest("Don't exist project has id by : " + newScenario.ProjectId);
            }

            Scenario scenario = _mapper.Map<Scenario>(newScenario);
            scenario.DateCreated = DateTime.Now;

            try
            {
                var response = await _scenarioRepo.Add(scenario);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the scenario.");
                }

                return Ok(new ActionScenarioResponse { Message = "Scenario added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding scenario into Database " + ex.Message);
            }

        }
        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddScenarioByUserClerk(AddScenarioUserClerkDto newScenario)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkExistUser = await _userRepo.GetSingleByCondition(c => c.UserIdClerk.Equals(newScenario.UserIdClerk));
            if (checkExistUser == null)
            {
                return BadRequest("Don't exist user has id by : " + newScenario.UserIdClerk);
            }
            bool checkExistProject = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.ProjectId));
            if (!checkExistProject)
            {
                return BadRequest("Don't exist project has id by : " + newScenario.ProjectId);
            }

            Scenario scenario = _mapper.Map<Scenario>(newScenario);
            scenario.DateCreated = DateTime.Now;
            scenario.CreatedBy = checkExistUser.Id;

            try
            {
                var response = await _scenarioRepo.Add(scenario);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the scenario.");
                }

                return Ok(new ActionScenarioResponse { Message = "Scenario added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding scenario into Database " + ex.Message);
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateScenario(Guid id, UpdateScenarioDto updateScenario)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateScenario.Id)
            {
                return BadRequest("Scenario ID information does not match");
            }
            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingScenario == null)
            {
                return NotFound("Scenario not found.");
            }
            
            existingScenario.DateUpdated = DateTime.Now;
            _mapper.Map(updateScenario, existingScenario);

            try
            {
                await _scenarioRepo.Update(existingScenario);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating scenario: {ex.Message}");
            }
        }


        [HttpPut("toggle-favorite/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> ToggleFavoriteScenario(Guid id)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingScenario == null)
            {
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.IsFavorites = !existingScenario.IsFavorites;
            try
            {
                await _scenarioRepo.Update(existingScenario);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated favorite successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }
        [HttpPut("update-web-json/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateWebJsonScenario(Guid id, string webjson)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

          

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingScenario == null)
            {
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.StepsWeb = webjson;
            try
            {
                await _scenarioRepo.Update(existingScenario);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated web step successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating scenario: {ex.Message}");
            }
        }
        [HttpPut("update-android-json/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateAndroidJsonScenario(Guid id, string androidjson)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingScenario == null)
            {
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.StepsWeb = androidjson;
            try
            {
                await _scenarioRepo.Update(existingScenario);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated android step successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating scenario: {ex.Message}");
            }
        }
        [HttpPut("rename-scenario/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> RenameScenario(Guid id, UpdateRenameScenarioDto renameScenario)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingScenario == null)
            {
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.ScenarioName = renameScenario.ScenarioName;
            try
            {
                await _scenarioRepo.Update(existingScenario);
                return Ok(new ActionScenarioResponse { Message = "Scenario rename successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating scenario: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteScenario(Guid id)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkScenarioExist = await _scenarioRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkScenarioExist)
            {
                return NotFound($"Dont exist scenario with id {id.ToString()} to delete");
            }
            try
            {
                await _scenarioRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionScenarioResponse { Message = "Scenario deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"Error delete scenario: {ex.Message}");
            }

        }
    }
}
