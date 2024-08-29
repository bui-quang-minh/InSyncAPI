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

        public ScenariosController(IScenarioRepository scenarioRop, IUserRepository userRepo,
           IProjectRepository projectRepo, IMapper mapper)
        {
            _scenarioRepo = scenarioRop;
            _userRepo = userRepo;
            _projectRepo = projectRepo; 
            _mapper = mapper;
        }
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _scenarioRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("get-all-scenario")]
        public async Task<IActionResult> GetAllScenarios(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            string[] include = new string[]
            {
                nameof(Scenario.Project),
                nameof(Scenario.CreatedByNavigation)
            };
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listScenario = _scenarioRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, include);
            var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
            return Ok(response);
        }


        [HttpGet("get-scenario/{id}")]
        public async Task<IActionResult> GetScenarioById(Guid id)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            string[] include = new string[]
          {
                nameof(Scenario.Project),
                nameof(Scenario.CreatedByNavigation)
          };

            var scenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id), include);
            if (scenario == null)
            {
                return NotFound("No scenario has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewScenarioDto>(scenario);
            return Ok(response);
        }
        [HttpGet("get-scenarios-project/{projectId}")]
        public async Task<IActionResult> GetScenarioOfProject(Guid projectId, Guid createdBy)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            string[] include = new string[]
          {
                nameof(Scenario.Project),
                nameof(Scenario.CreatedByNavigation)
          };

            var scenario =  _scenarioRepo.GetMulti(c => c.ProjectId.Equals(projectId) && c.CreatedBy.Equals(createdBy), include);
            if (!scenario.Any())
            {
                return NotFound("No scenario in project by id : " + projectId.ToString());
            }
            var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(scenario);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddScenario(AddScenarioDto newScenario)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bool checkExistUser = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.CreatedBy));
            if (!checkExistUser)
            {
                return NotFound("Don't exist user has id by : " + newScenario.CreatedBy);
            }
            bool checkExistProject = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.ProjectId));
            if (!checkExistProject)
            {
                return NotFound("Don't exist project has id by : " + newScenario.ProjectId);
            }

            Scenario scenario = _mapper.Map<Scenario>(newScenario);
            scenario.DateCreated = DateTime.Now;

            var response = await _scenarioRepo.Add(scenario);

            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Error occurred while adding the tutorial.");
            }

            return Ok(new { message = "Scenario added successfully.", Id = response.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScenario(Guid id, UpdateScenarioDto updateScenario)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
            bool checkExistProject = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(updateScenario.ProjectId));
            if (!checkExistProject)
            {
                return NotFound("Don't exist project has id by : " + updateScenario.ProjectId);
            }

            existingScenario.DateUpdated = DateTime.Now;
            _mapper.Map(updateScenario, existingScenario);

            try
            {
                await _scenarioRepo.Update(existingScenario);
                return Ok(new { message = "Scenario updated successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }


        [HttpPut("toggle-favorite/{id}")]
        public async Task<IActionResult> ToggleFavoriteScenario(Guid id)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
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
                return Ok(new { message = "Scenario updated favorite successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }
        [HttpPut("update-web-json/{id}")]
        public async Task<IActionResult> UpdateWebJsonScenario(Guid id, string webjson)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
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
                return Ok(new { message = "Scenario updated web step successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }
        [HttpPut("update-android-json/{id}")]
        public async Task<IActionResult> UpdateAndroidJsonScenario(Guid id, string androidjson)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
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
                return Ok(new { message = "Scenario updated android step successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }
        [HttpPut("rename-scenario/{id}")]
        public async Task<IActionResult> RenameScenario(Guid id, UpdateRenameScenarioDto renameScenario)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                return Ok(new { message = "Scenario rename successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTutorial(Guid id)
        {
            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var checkScenarioExist = await _scenarioRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkScenarioExist)
            {
                return NotFound($"Dont exist scenario with id {id.ToString()} to delete");
            }
            await _scenarioRepo.DeleteMulti(c => c.Id.Equals(id));
            return Ok(new { message = "Tutorial deleted successfully.", Id = id });
        }
    }
}
