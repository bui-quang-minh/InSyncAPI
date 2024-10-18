using AutoMapper;
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
    public class ScenariosController : ControllerBase
    {
        private IScenarioRepository _scenarioRepo;
        private IProjectRepository _projectRepo;
        private IUserRepository _userRepo;
        private ILogger<ScenariosController> _logger;
        private readonly string TAG = nameof(ScenariosController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private string[] includes = new string[]
          {
                nameof(Scenario.Project),
                nameof(Scenario.CreatedByNavigation)
          };

        public ScenariosController(IScenarioRepository scenarioRop, IUserRepository userRepo,
           IProjectRepository projectRepo, IMapper mapper, ILogger<ScenariosController> logger)
        {
            _scenarioRepo = scenarioRop;
            _userRepo = userRepo;
            _projectRepo = projectRepo;
            _mapper = mapper;
            _logger = logger;   
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Scenario>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetScenarios()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve all scenarios at {RequestTime}", DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            try
            {
                var response = _scenarioRepo.GetAll().AsQueryable();
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved scenarios in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving scenarios. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving scenarios.");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllScenarios(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve all scenarios with index {Index}, size {Size}, and key search '{KeySearch}' at {RequestTime}", index, size, keySearch, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            IEnumerable<Scenario> listScenario = new List<Scenario>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listScenario = _scenarioRepo.GetMulti(
                        c => c.ScenarioName.ToLower().Contains(keySearch),
                        includes
                    );
                    total = listScenario.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listScenario = _scenarioRepo.GetMultiPaging(
                        c => c.ScenarioName.ToLower().Contains(keySearch),
                        out total, index.Value, size.Value, includes
                    );
                }

                var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
                var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved scenarios with a total of {TotalScenarios} scenarios in {ElapsedMilliseconds}ms.", total, stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving scenarios. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving scenarios.");
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewScenarioDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetScenarioById(Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve scenario with ID: {ScenarioId} at {RequestTime}", id, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to get scenario with ID: {ScenarioId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var scenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
                if (scenario == null)
                {
                    _logger.LogWarning("No scenario found with ID: {ScenarioId}.", id);
                    return NotFound("No scenario has an ID : " + id.ToString());
                }

                var response = _mapper.Map<ViewScenarioDto>(scenario);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved scenario with ID: {ScenarioId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving scenario with ID: {ScenarioId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the scenario.");
            }
        }

        [HttpGet("scenarios-project/{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetScenarioOfProject(Guid projectId, Guid createdBy, int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve scenarios for project ID: {ProjectId} created by: {CreatedBy} at {RequestTime}", projectId, createdBy, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to get scenarios of project ID: {ProjectId}.", projectId);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<Scenario> listScenario = new List<Scenario>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listScenario = _scenarioRepo.GetMulti
                        (c => c.ProjectId.Equals(projectId) && c.CreatedBy.Equals(createdBy) && c.ScenarioName.ToLower().Contains(keySearch), includes
                        );
                    total = listScenario.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listScenario = _scenarioRepo.GetMultiPaging
                        (c => c.ProjectId.Equals(projectId) && c.CreatedBy.Equals(createdBy) && c.ScenarioName.ToLower().Contains(keySearch)
                        , out total, index.Value, size.Value, includes
                     );
                }

                var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
                var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved scenarios for project ID: {ProjectId} created by: {CreatedBy} in {ElapsedMilliseconds}ms.", projectId, createdBy, stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving scenarios for project ID: {ProjectId} created by: {CreatedBy}. Total time taken: {ElapsedMilliseconds}ms.", projectId, createdBy, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving scenarios.");
            }
        }


        [HttpGet("scenarios-project-useridclerk/{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetScenarioOfProjectByUserClerk(Guid projectId, string userIdClerk, int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve scenarios for project ID: {ProjectId} created by clerk ID: {UserIdClerk} at {RequestTime}", projectId, userIdClerk, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to get scenarios of project ID: {ProjectId}.", projectId);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<Scenario> listScenario = new List<Scenario>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listScenario = _scenarioRepo.GetMulti
                        (c => c.ProjectId.Equals(projectId) && c.CreatedByNavigation.UserIdClerk.Equals(userIdClerk) && c.ScenarioName.ToLower().Contains(keySearch), includes
                        );
                    total = listScenario.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listScenario = _scenarioRepo.GetMultiPaging
                        (c => c.ProjectId.Equals(projectId) && c.CreatedByNavigation.UserIdClerk.Equals(userIdClerk) && c.ScenarioName.ToLower().Contains(keySearch)
                        , out total, index.Value, size.Value, includes
                     );
                }

                var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
                var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved scenarios for project ID: {ProjectId} created by clerk ID: {UserIdClerk} in {ElapsedMilliseconds}ms.", projectId, userIdClerk, stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving scenarios for project ID: {ProjectId} created by clerk ID: {UserIdClerk}. Total time taken: {ElapsedMilliseconds}ms.", projectId, userIdClerk, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving scenarios.");
            }
        }



        [HttpGet("scenarios-user-clerk/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllScenarioByUserIdClerk(string userIdClerk, int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve all scenarios for clerk ID: {UserIdClerk} at {RequestTime}", userIdClerk, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to get scenarios for clerk ID: {UserIdClerk}.", userIdClerk);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<Scenario> listScenario = new List<Scenario>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listScenario = _scenarioRepo.GetMulti
                        (c => c.CreatedByNavigation.UserIdClerk.Equals(userIdClerk) && c.ScenarioName.ToLower().Contains(keySearch), includes
                        );
                    total = listScenario.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listScenario = _scenarioRepo.GetMultiPaging
                        (c => c.CreatedByNavigation.UserIdClerk.Equals(userIdClerk) && c.ScenarioName.ToLower().Contains(keySearch), out total, index.Value, size.Value, includes
                        );
                }

                var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
                var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved scenarios for clerk ID: {UserIdClerk} in {ElapsedMilliseconds}ms.", userIdClerk, stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving scenarios for clerk ID: {UserIdClerk}. Total time taken: {ElapsedMilliseconds}ms.", userIdClerk, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving scenarios.");
            }
        }


        [HttpGet("scenarios-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewScenarioDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllScenarioByUserId(Guid userId, string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve all scenarios for user ID: {UserId} at {RequestTime}", userId, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request to get scenarios for user ID: {UserId}.", userId);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<Scenario> listScenario = new List<Scenario>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listScenario = _scenarioRepo.GetMulti
                        (c => c.CreatedBy.Equals(userId) && c.ScenarioName.ToLower().Contains(keySearch), includes
                        );
                    total = listScenario.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listScenario = _scenarioRepo.GetMultiPaging
                        (c => c.CreatedBy.Equals(userId) && c.ScenarioName.ToLower().Contains(keySearch), out total, index.Value, size.Value, includes
                        );
                }

                var response = _mapper.Map<IEnumerable<ViewScenarioDto>>(listScenario);
                var responsePaging = new ResponsePaging<IEnumerable<ViewScenarioDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved scenarios for user ID: {UserId} in {ElapsedMilliseconds}ms.", userId, stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving scenarios for user ID: {UserId}. Total time taken: {ElapsedMilliseconds}ms.", userId, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving scenarios.");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddScenario(AddScenarioDto newScenario)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to add a new scenario at {RequestTime}", DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for new scenario: {@NewScenario}.", newScenario);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                bool checkExistUser = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.CreatedBy));
                if (!checkExistUser)
                {
                    _logger.LogWarning("User with ID {UserId} does not exist.", newScenario.CreatedBy);
                    return BadRequest("Don't exist user has id by : " + newScenario.CreatedBy);
                }

                bool checkExistProject = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.ProjectId));
                if (!checkExistProject)
                {
                    _logger.LogWarning("Project with ID {ProjectId} does not exist.", newScenario.ProjectId);
                    return BadRequest("Don't exist project has id by : " + newScenario.ProjectId);
                }

                Scenario scenario = _mapper.Map<Scenario>(newScenario);
                scenario.DateCreated = DateTime.Now;

                var response = await _scenarioRepo.Add(scenario);
                if (response == null)
                {
                    _logger.LogError("Error occurred while adding the scenario.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the scenario.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Scenario added successfully with ID: {ScenarioId} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding scenario into Database. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding scenario into Database: " + ex.Message);
            }
        }

        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddScenarioByUserClerk(AddScenarioUserClerkDto newScenario)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to add a new scenario by user clerk at {RequestTime}", DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for new scenario by user clerk: {@NewScenario}.", newScenario);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var checkExistUser = await _userRepo.GetSingleByCondition(c => c.UserIdClerk.Equals(newScenario.UserIdClerk));
                if (checkExistUser == null)
                {
                    _logger.LogWarning("User clerk with ID {UserIdClerk} does not exist.", newScenario.UserIdClerk);
                    return BadRequest("Don't exist user has id by : " + newScenario.UserIdClerk);
                }

                bool checkExistProject = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(newScenario.ProjectId));
                if (!checkExistProject)
                {
                    _logger.LogWarning("Project with ID {ProjectId} does not exist.", newScenario.ProjectId);
                    return BadRequest("Don't exist project has id by : " + newScenario.ProjectId);
                }

                Scenario scenario = _mapper.Map<Scenario>(newScenario);
                scenario.DateCreated = DateTime.Now;
                scenario.CreatedBy = checkExistUser.Id;

                var response = await _scenarioRepo.Add(scenario);
                if (response == null)
                {
                    _logger.LogError("Error occurred while adding the scenario.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the scenario.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Scenario added successfully with ID: {ScenarioId} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding scenario into Database. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding scenario into Database: " + ex.Message);
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateScenario(Guid id, UpdateScenarioDto updateScenario)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to update scenario with ID {ScenarioId} at {RequestTime}", id, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating scenario: {@UpdateScenario}.", updateScenario);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateScenario.Id)
            {
                _logger.LogWarning("Scenario ID information does not match. Expected ID: {ExpectedId}, Provided ID: {ProvidedId}.", id, updateScenario.Id);
                return BadRequest("Scenario ID information does not match");
            }

            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingScenario == null)
            {
                _logger.LogWarning("Scenario with ID {ScenarioId} not found.", id);
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            _mapper.Map(updateScenario, existingScenario);

            try
            {
                await _scenarioRepo.Update(existingScenario);
                stopwatch.Stop();
                _logger.LogInformation("Scenario with ID {ScenarioId} updated successfully in {ElapsedMilliseconds}ms.", existingScenario.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating scenario with ID {ScenarioId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating scenario: {ex.Message}");
            }
        }



        [HttpPut("toggle-favorite/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> ToggleFavoriteScenario(Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to toggle favorite status for scenario with ID {ScenarioId} at {RequestTime}", id, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when trying to toggle favorite status for scenario ID {ScenarioId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingScenario == null)
            {
                _logger.LogWarning("Scenario with ID {ScenarioId} not found.", id);
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.IsFavorites = !existingScenario.IsFavorites;

            try
            {
                await _scenarioRepo.Update(existingScenario);
                stopwatch.Stop();
                _logger.LogInformation("Favorite status for scenario with ID {ScenarioId} toggled successfully in {ElapsedMilliseconds}ms.", existingScenario.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated favorite successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error toggling favorite status for scenario with ID {ScenarioId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating scenario: {ex.Message}");
            }
        }

        [HttpPut("update-web-json/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateWebJsonScenario(Guid id, string webjson)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to update web JSON steps for scenario with ID {ScenarioId} at {RequestTime}", id, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when trying to update web JSON steps for scenario ID {ScenarioId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingScenario == null)
            {
                _logger.LogWarning("Scenario with ID {ScenarioId} not found.", id);
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.StepsWeb = webjson;

            try
            {
                await _scenarioRepo.Update(existingScenario);
                stopwatch.Stop();
                _logger.LogInformation("Web JSON steps for scenario with ID {ScenarioId} updated successfully in {ElapsedMilliseconds}ms.", existingScenario.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated web step successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating web JSON steps for scenario with ID {ScenarioId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating scenario: {ex.Message}");
            }
        }

        [HttpPut("update-android-json/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateAndroidJsonScenario(Guid id, string androidjson)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to update Android JSON steps for scenario with ID {ScenarioId} at {RequestTime}", id, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when trying to update Android JSON steps for scenario ID {ScenarioId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingScenario == null)
            {
                _logger.LogWarning("Scenario with ID {ScenarioId} not found.", id);
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.StepsAndroid = androidjson;

            try
            {
                await _scenarioRepo.Update(existingScenario);
                stopwatch.Stop();
                _logger.LogInformation("Android JSON steps for scenario with ID {ScenarioId} updated successfully in {ElapsedMilliseconds}ms.", existingScenario.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario updated Android step successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating Android JSON steps for scenario with ID {ScenarioId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating scenario: {ex.Message}");
            }
        }

        [HttpPut("rename-scenario/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> RenameScenario(Guid id, UpdateRenameScenarioDto renameScenario)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to rename scenario with ID {ScenarioId} at {RequestTime}", id, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when trying to rename scenario ID {ScenarioId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var existingScenario = await _scenarioRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingScenario == null)
            {
                _logger.LogWarning("Scenario with ID {ScenarioId} not found.", id);
                return NotFound("Scenario not found.");
            }

            existingScenario.DateUpdated = DateTime.Now;
            existingScenario.ScenarioName = renameScenario.ScenarioName;

            try
            {
                await _scenarioRepo.Update(existingScenario);
                stopwatch.Stop();
                _logger.LogInformation("Scenario with ID {ScenarioId} renamed successfully in {ElapsedMilliseconds}ms.", existingScenario.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario renamed successfully.", Id = existingScenario.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error renaming scenario with ID {ScenarioId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating scenario: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionScenarioResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteScenario(Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to delete scenario with ID {ScenarioId} at {RequestTime}", id, DateTime.UtcNow);

            if (_scenarioRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Scenario repository, user repository, or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when trying to delete scenario ID {ScenarioId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkScenarioExist = await _scenarioRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkScenarioExist)
            {
                _logger.LogWarning("Scenario with ID {ScenarioId} does not exist.", id);
                return NotFound($"Don't exist scenario with id {id} to delete");
            }

            try
            {
                await _scenarioRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation("Scenario with ID {ScenarioId} deleted successfully in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionScenarioResponse { Message = "Scenario deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting scenario with ID {ScenarioId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error delete scenario: {ex.Message}");
            }
        }

    }
}
