﻿using AutoMapper;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Project>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetProjects()
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

        public async Task<IActionResult> GetAllProject(int? index, int? size, string? keySearch = "")
        {
            if (_projectRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            IEnumerable<Project> listProject = new List<Project>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            if (index == null || size == null)
            {
                listProject = _projectRepo.GetMulti
                    (c =>
                     c.ProjectName.ToLower().Contains(keySearch), includes
                    );
                total = listProject.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listProject = _projectRepo.GetMultiPaging
                (c =>
                c.ProjectName.ToLower().Contains(keySearch)
                , out total, index.Value, size.Value, includes
                );
            }


            var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
            var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);

        }


        [HttpGet("project-user-is-publish/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllProjectIsPublishOfUser(Guid userId, int? index, int? size, bool? isPublish, string? keySearch = "" )
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

            IEnumerable<Project> listProject = new List<Project>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            if (index == null || size == null)
            {
                listProject = _projectRepo.GetMulti
                    (c => c.UserId.Equals(userId) && c.ProjectName.ToLower().Contains(keySearch)
                        && (isPublish == null || c.IsPublish == isPublish), includes
                    );
                total = listProject.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listProject = _projectRepo.GetMultiPaging
                (c => c.UserId.Equals(userId) && c.ProjectName.ToLower().Contains(keySearch)
                     && (isPublish == null || c.IsPublish == isPublish)
                , out total, index.Value, size.Value, includes
                );
            }

            var response = _mapper.Map<IEnumerable<ViewProjectDto>>(listProject);
            var responsePaging = new ResponsePaging<IEnumerable<ViewProjectDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }


        [HttpGet("project-user-clerk-is-publish/{userIdClerk}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewProjectDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllProjectIsPublishByUserIdClerk(string userIdClerk, bool? isPublish, int? index, int? size, string? keySearch = "")
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
            IEnumerable<Project> listProject = new List<Project>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            if (index == null || size == null)
            {
                listProject = _projectRepo.GetMulti
                    (c => c.User.UserIdClerk.Equals(userIdClerk) && c.ProjectName.ToLower().Contains(keySearch)
                     && (isPublish == null || c.IsPublish == isPublish), includes
                    );
                total = listProject.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listProject = _projectRepo.GetMultiPaging
                (c => c.User.UserIdClerk.Equals(userIdClerk) && c.ProjectName.ToLower().Contains(keySearch)
                     && (isPublish == null || c.IsPublish == isPublish)
                , out total, index.Value, size.Value, includes
                );
            }

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

        public async Task<IActionResult> GetProjectById(Guid id)
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
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkUserExist = await _userRepo.CheckContainsAsync(c => c.Id.Equals(newProject.UserId));
            if (!checkUserExist)
            {
                return BadRequest("Information of user not correct . please check again! " + newProject.UserId.ToString());
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
                        value: "An error occurred while adding project into Database " + ex.Message);
            }

        }
        [HttpPost("ByUserClerk")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddProjectUserClerk(AddProjectClerkDto newProject)
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
            var checkUserExist = await _userRepo.GetSingleByCondition(c => c.UserIdClerk.Equals(newProject.UserIdClerk));
            if (checkUserExist == null)
            {
                return BadRequest("Information of user not correct . please check again! " + newProject.UserIdClerk.ToString());
            }
            Project project = _mapper.Map<Project>(newProject);
            project.UserId = checkUserExist.Id;
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
                        value: "An error occurred while adding project into Database " + ex.Message);
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
                return BadRequest(new ValidationProblemDetails(ModelState));
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
            var checkProjectExist = await _projectRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkProjectExist)
            {
                return NotFound($"Dont exist project with id {id.ToString()} to delete");
            }
            try
            {
                await _projectRepo.DeleteProject(id);
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

