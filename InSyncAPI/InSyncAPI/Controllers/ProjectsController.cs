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
    public class ProjectsController : ControllerBase
    {
        private IProjectRepository _projectRepo;
        private IUserRepository _userRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        public static readonly string[] includes = new string[]
           {
                nameof(Project.User)
           };

        public ProjectsController(IProjectRepository projectRepo, IUserRepository userRepo, IMapper mapper)
        {
            _projectRepo = projectRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Project>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> Get()
        {
            if (_projectRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _projectRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllProject(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_projectRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listProject = _projectRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
            var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);

        }

        [HttpGet("projects-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllProjectOfUser(Guid userId, int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_projectRepo == null || _mapper == null)
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

            var listProject = _projectRepo.GetMultiPaging(c => c.UserId.Equals(userId), out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
            var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }

        [HttpGet("project-user-is-publish")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllProjectIsPublishOfUser(Guid userId, bool? isPublish, int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_projectRepo == null || _mapper == null)
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

            var listProject = _projectRepo.GetMultiPaging(c => c.UserId.Equals(userId) && (isPublish == null || c.IsPublish == isPublish), out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
            var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewProjectDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetProject(Guid id)
        {
            if (_projectRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var project = await _projectRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
            if (project == null)
            {
                return NotFound("No project has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewProjectDto>(project);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddProject(AddProjectDto newProject)
        {
            if (_projectRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var checkUserExist = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newProject.UserId));
            if (!checkUserExist)
            {
                return NotFound("Information of user not correct . please check again! " + newProject.UserId.ToString());
            }
            Project project = _mapper.Map<Project>(newProject);
            project.DateCreated = DateTime.Now;

            try
            {
                var response = await _projectRepo.Add(project);

                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the project.");
                }

                return Ok(new ActionProjectResponse { Message = "Project added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding Project into Database " + ex.Message);
            }
            
        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectDto updateProject)
        {
            if (_projectRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updateProject.Id)
            {
                return BadRequest("Project ID information does not match");
            }

            // Fetch the existing customer review to ensure it exists
            var existingProject = await _projectRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingProject == null)
            {
                return NotFound("Project not found.");
            }


            // Map the updated fields
            _mapper.Map(updateProject, existingProject);

            try
            {
                await _projectRepo.Update(existingProject);
                return Ok(new ActionProjectResponse { Message = "Project updated successfully.", Id = existingProject.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating project: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteProject(Guid id)
        {
            if (_projectRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkReviewExist = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkReviewExist)
            {
                return NotFound($"Dont exist project with id {id.ToString()} to delete");
            }
            try
            {
                await _projectRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionProjectResponse { Message = "Project deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"Error delete project: {ex.Message}");
            }
        }

    }
}

