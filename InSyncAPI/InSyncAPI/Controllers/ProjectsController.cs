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
    public class ProjectsController : ControllerBase
    {
        private IProjectRepository _projectRepo;
        private IUserRepository _userRepo;
        private ILogger<ProjectsController> _logger;
        private readonly string TAG = nameof(ProjectsController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        public static readonly string[] includes = new string[]
           {
                nameof(Project.User)
           };

        public ProjectsController(IProjectRepository projectRepo, IUserRepository userRepo
            , IMapper mapper, ILogger<ProjectsController> logger)
        {
            _projectRepo = projectRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Project>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetProjects()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all projects at {RequestTime}.", DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            try
            {
                var projects = _projectRepo.GetAll().AsQueryable();
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved all projects in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving projects. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving projects: {ex.Message}");
            }
        }




        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllProject(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all projects at {RequestTime}.", DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<Project> listProject = new List<Project>(); ;
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listProject = _projectRepo.GetMulti(c => c.ProjectName.ToLower().Contains(keySearch), includes);
                    total = listProject.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

                    listProject = _projectRepo.GetMultiPaging(c => c.ProjectName.ToLower().Contains(keySearch), out total, index.Value, size.Value, includes);
                }

                var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
                var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved projects in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving projects. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving projects: {ex.Message}");
            }
        }



        [HttpGet("project-user-is-publish/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllProjectIsPublishOfUser([FromRoute] Guid userId, int? index, int? size, bool? isPublish, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve projects for user {UserId} with index {Index}, size {Size}, isPublish {IsPublish}, keySearch '{KeySearch}' at {RequestTime}",
                userId, index, size, isPublish, keySearch, DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<Project> listProject = new List<Project>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listProject = _projectRepo.GetMulti(
                        c => c.UserId.Equals(userId) &&
                        c.ProjectName.ToLower().Contains(keySearch) &&
                        (isPublish == null || c.IsPublish == isPublish),
                        includes
                    );
                    total = listProject.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listProject = _projectRepo.GetMultiPaging(
                        c => c.UserId.Equals(userId) &&
                        c.ProjectName.ToLower().Contains(keySearch) &&
                        (isPublish == null || c.IsPublish == isPublish),
                        out total, index.Value, size.Value, includes
                    );
                }

                var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
                var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved projects for user {UserId} with a total of {TotalProjects} projects in {ElapsedMilliseconds}ms.",
                    userId, total, stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving projects for user {UserId}. Total time taken: {ElapsedMilliseconds}ms.",
                    userId, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving projects.");
            }
        }



        [HttpGet("project-user-clerk-is-publish/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllProjectIsPublishByUserIdClerk([FromRoute] string userIdClerk, bool? isPublish, int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve projects for clerk {UserIdClerk} with index {Index}, size {Size}, isPublish {IsPublish}, keySearch '{KeySearch}' at {RequestTime}",
                userIdClerk, index, size, isPublish, keySearch, DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<Project> listProject = new List<Project>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listProject = _projectRepo.GetMulti(
                        c => c.User.UserIdClerk.Equals(userIdClerk) &&
                        c.ProjectName.ToLower().Contains(keySearch) &&
                        (isPublish == null || c.IsPublish == isPublish),
                        includes
                    );
                    total = listProject.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listProject = _projectRepo.GetMultiPaging(
                        c => c.User.UserIdClerk.Equals(userIdClerk) &&
                        c.ProjectName.ToLower().Contains(keySearch) &&
                        (isPublish == null || c.IsPublish == isPublish),
                        out total, index.Value, size.Value, includes
                    );
                }

                var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
                var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved projects for clerk {UserIdClerk} with a total of {TotalProjects} projects in {ElapsedMilliseconds}ms.",
                    userIdClerk, total, stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving projects for clerk {UserIdClerk}. Total time taken: {ElapsedMilliseconds}ms.",
                    userIdClerk, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving projects.");
            }
        }





        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewProjectDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetProjectById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve project with ID {ProjectId} at {RequestTime}", id, DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var project = await _projectRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
                if (project == null)
                {
                    _logger.LogWarning("No project found with ID {ProjectId}.", id);
                    return NotFound("No project has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewProjectDto>(project);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved project with ID {ProjectId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while retrieving project with ID {ProjectId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the project.");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddProject(AddProjectDto newProject)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to add a new project with UserId {UserId} at {RequestTime}",
                newProject.UserId, DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkUserExist = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newProject.UserId));
            if (!checkUserExist)
            {
                _logger.LogWarning("User with ID {UserId} does not exist.", newProject.UserId);
                return BadRequest("Information of user not correct. Please check again! " + newProject.UserId.ToString());
            }

            Project project = _mapper.Map<Project>(newProject);
            project.DateCreated = DateTime.UtcNow;
            project.DateUpdated = DateTime.UtcNow;

            try
            {
                var response = await _projectRepo.Add(project);

                if (response == null)
                {
                    _logger.LogError("Error occurred while adding the project.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the project.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added project with ID {ProjectId} in {ElapsedMilliseconds}ms.",
                    response.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionProjectResponse { Message = "Project added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding project into Database. Total time taken: {ElapsedMilliseconds}ms.",
                    stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding project into Database " + ex.Message);
            }
        }

        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddProjectUserClerk([FromBody] AddProjectClerkDto newProject)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to add a new project for clerk with UserIdClerk {UserIdClerk} at {RequestTime}",
                newProject.UserIdClerk, DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkUserExist = await _userRepo.GetSingleByCondition(c => c.UserIdClerk.Equals(newProject.UserIdClerk));
            if (checkUserExist == null)
            {
                _logger.LogWarning("User with UserIdClerk {UserIdClerk} does not exist.", newProject.UserIdClerk);
                return BadRequest("Information of user not correct. Please check again! " + newProject.UserIdClerk.ToString());
            }

            Project project = _mapper.Map<Project>(newProject);
            project.UserId = checkUserExist.Id;
            project.DateCreated = DateTime.UtcNow;
            project.DateUpdated = DateTime.UtcNow;

            try
            {
                var response = await _projectRepo.Add(project);

                if (response == null)
                {
                    _logger.LogError("Error occurred while adding the project.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the project.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added project with ID {ProjectId} for clerk with UserIdClerk {UserIdClerk} in {ElapsedMilliseconds}ms.",
                    response.Id, newProject.UserIdClerk, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionProjectResponse { Message = "Project added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while adding project into Database. Total time taken: {ElapsedMilliseconds}ms.",
                    stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding project into Database " + ex.Message);
            }
        }



        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateProject([FromRoute] Guid id, [FromBody] UpdateProjectDto updateProject)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to update project with ID {ProjectId} at {RequestTime}", id, DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateProject.Id)
            {
                _logger.LogWarning("Project ID {ProjectId} does not match the update object ID {UpdateProjectId}.", id, updateProject.Id);
                return BadRequest("Project ID information does not match");
            }

            // Fetch the existing project to ensure it exists
            var existingProject = await _projectRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingProject == null)
            {
                _logger.LogWarning("Project with ID {ProjectId} not found.", id);
                return NotFound("Project not found.");
            }

            // Map the updated fields
            _mapper.Map(updateProject, existingProject);
            existingProject.DateUpdated = DateTime.UtcNow;

            try
            {
                await _projectRepo.Update(existingProject);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated project with ID {ProjectId} in {ElapsedMilliseconds}ms.",
                    existingProject.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionProjectResponse { Message = "Project updated successfully.", Id = existingProject.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while updating project with ID {ProjectId}. Total time taken: {ElapsedMilliseconds}ms.",
                    existingProject.Id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating project: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteProject([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to delete project with ID {ProjectId} at {RequestTime}", id, DateTime.UtcNow);

            if (_projectRepo == null || _userRepo == null || _mapper == null)
            {
                _logger.LogError("Project repository or User repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkProjectExist = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkProjectExist)
            {
                _logger.LogWarning("Project with ID {ProjectId} does not exist for deletion.", id);
                return NotFound($"Don't exist project with ID {id} to delete");
            }

            try
            {
                await _projectRepo.DeleteProject(id);
                stopwatch.Stop();
                _logger.LogInformation("Successfully deleted project with ID {ProjectId} in {ElapsedMilliseconds}ms.",
                    id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionProjectResponse { Message = "Project deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while deleting project with ID {ProjectId}. Total time taken: {ElapsedMilliseconds}ms.",
                    id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error delete project: {ex.Message}");
            }
        }


    }
}

