using AutoMapper;
using BusinessObjects.Models;
using Castle.Core.Logging;
using FakeItEasy;
using FluentAssertions;
using InSyncAPI.Controllers;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace InSyncUnitTest.Controller
{
    public class ProjectsControllerTest
    {
        private IProjectRepository _projectRepo;
        private IUserRepository _userRepo;
        private IMapper _mapper;
        private ILogger<ProjectsController> _logger;
        private ProjectsController _controller;
        public static readonly string[] includes = new string[]
          {
                nameof(Project.User)
          };
        private readonly string userIdClerk = "UserIdClerk";
        public ProjectsControllerTest()
        {
            _projectRepo = A.Fake<IProjectRepository>();
            _userRepo = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<ProjectsController>>();
            _controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
        }


        #region GetProjects
        [Fact]
        public async Task GetProjects_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ProjectsController(null, null, null, _logger);

            // Act
            var result = await controller.GetProjects();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetProjects_WithProjects_ShouldReturnsOkResult()
        {
            // Arrange
            var Projects = new[] { new Project(), new Project() }.AsQueryable();
            A.CallTo(() => _projectRepo.GetAll(A<string[]>._)).Returns(Projects);

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as IQueryable<Project>;
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(Projects);

        }

        [Fact]
        public async Task GetProjects_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving projects.";
            var response = $"Error retrieving projects: {message}";

            A.CallTo(() => _projectRepo.GetAll(A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region GetAllProject
        [Fact]
        public async Task GetAllProject_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ProjectsController(null, null, null, _logger);
            var result = await controller.GetAllProject(0, 2, "");


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllProject_WithValidInput_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _projectRepo.GetMultiPaging(A<Expression<Func<Project, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProject(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }
        [Fact]
        public async Task GetAllProject_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _projectRepo.GetMultiPaging(A<Expression<Func<Project, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProject(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }

        [Fact]
        public async Task GetAllProject_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _projectRepo.GetMulti(A<Expression<Func<Project, bool>>>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProject(null, null, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }
        [Fact]
        public async Task GetAllProject_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving projects.";
            var response = $"Error retrieving projects: {message}";

            A.CallTo(() => _projectRepo.GetMulti(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllProject(null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }

        #endregion

        #region GetAllProjectIsPublishOfUser
        [Fact]
        public async Task GetAllProjectIsPublishOfUser_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ProjectsController(null, null, null, _logger);
            var result = await controller.GetAllProjectIsPublishOfUser(Guid.NewGuid(), null, null, null);

            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllProjectIsPublishOfUser_WithValidInput_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _projectRepo.GetMultiPaging(A<Expression<Func<Project, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProjectIsPublishOfUser(Guid.NewGuid(), index, size, true, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }
        [Fact]
        public async Task GetAllProjectIsPublishOfUser_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _projectRepo.GetMultiPaging(A<Expression<Func<Project, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProjectIsPublishOfUser(Guid.NewGuid(), index, size, true, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }

        [Fact]
        public async Task GetAllProjectIsPublishOfUser_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _projectRepo.GetMulti(A<Expression<Func<Project, bool>>>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProjectIsPublishOfUser(Guid.NewGuid(), null, null, true, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }
        [Fact]
        public async Task GetAllProjectIsPublishOfUser_WithUserIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetAllProjectIsPublishOfUser(Guid.Empty, 0, 2, true, "");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("id");
            errorResponse.Errors["id"].Should().Contain($"The value '{invalidGuid}' is not valid.");
        }

        [Fact]
        public async Task GetAllProjectIsPublishOfUser_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
           
            var response = $"An error occurred while retrieving projects.";

            A.CallTo(() => _projectRepo.GetMulti(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Throws(new Exception(""));

            // Act
            var result = await _controller.GetAllProjectIsPublishOfUser(Guid.NewGuid(), null, null, true, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region GetAllProjectIsPublishByUserIdClerk
        [Fact]
        public async Task GetAllProjectIsPublishByUserIdClerk_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ProjectsController(null, null, null, _logger);
            var result = await controller.GetAllProjectIsPublishByUserIdClerk(userIdClerk, null, null, null, "");

            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllProjectIsPublishByUserIdClerk_WithValidInput_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _projectRepo.GetMultiPaging(A<Expression<Func<Project, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProjectIsPublishByUserIdClerk(userIdClerk, true, index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }
        [Fact]
        public async Task GetAllProjectIsPublishByUserIdClerk_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _projectRepo.GetMultiPaging(A<Expression<Func<Project, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProjectIsPublishByUserIdClerk(userIdClerk, true, index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }

        [Fact]
        public async Task GetAllProjectIsPublishByUserIdClerk_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithProjects()
        {
            // Arrange

            IEnumerable<Project> Projects = new List<Project> { new Project(), new Project() };
            IEnumerable<ViewProjectDto> viewProjects = new List<ViewProjectDto> { new ViewProjectDto(), new ViewProjectDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _projectRepo.GetMulti(A<Expression<Func<Project, bool>>>._, A<string[]>._))
                .Returns(Projects);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewProjectDto>>(Projects)).Returns(viewProjects);

            // Act
            var result = await _controller.GetAllProjectIsPublishByUserIdClerk(userIdClerk, true, null, null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedProjects = okResult.Value as ResponsePaging<IEnumerable<ViewProjectDto>>;
            returnedProjects.Should().NotBeNull();
            returnedProjects.data.Should().BeEquivalentTo(viewProjects);
        }
        [Fact]
        public async Task GetAllProjectOfUserClerk_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
          
            var response = $"An error occurred while retrieving projects.";

            A.CallTo(() => _projectRepo.GetMulti(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Throws(new Exception(""));

            // Act
            var result = await _controller.GetAllProjectIsPublishByUserIdClerk(userIdClerk,true, null, null,"");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }

        #endregion

        #region GetProjectById
        [Fact]
        public async Task GetProjectById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new ProjectsController(null, null, null, _logger);
            //Act
            var result = await controller.GetProjectById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetProjectById_WhenProjectDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var ProjectId = Guid.NewGuid();
            A.CallTo(() => _projectRepo.GetSingleByCondition(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Project>(null));

            // Act
            var result = await _controller.GetProjectById(ProjectId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No project has an ID: {ProjectId}");
        }

        [Fact]
        public async Task GetProjectById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetProjectById(Guid.Empty);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("id");
            errorResponse.Errors["id"].Should().Contain($"The value '{invalidGuid}' is not valid.");
        }
        [Fact]
        public async Task GetProjectById_WithIdExist_ShouldReturnOkResultWithProject()
        {
            // Arrange
            var ProjectId = Guid.NewGuid();
            var Project = new Project();
            var viewProject = new ViewProjectDto();

            A.CallTo(() => _projectRepo.GetSingleByCondition(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Returns(Task.FromResult(Project));
            A.CallTo(() => _mapper.Map<ViewProjectDto>(Project)).Returns(viewProject);

            // Act
            var result = await _controller.GetProjectById(ProjectId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewProjectDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewProject);
        }
        [Fact]
        public async Task GetProjectById_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            
            var response = $"An error occurred while retrieving the project.";

            A.CallTo(() => _projectRepo.GetSingleByCondition(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Throws(new Exception());

            // Act
            var result = await _controller.GetProjectById(Guid.NewGuid());

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }

        #endregion

        #region AddProject
        [Fact]
        public async Task AddProject_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ProjectsController(null, null, null, _logger);
            var newProject = new AddProjectDto();

            // Act
            var result = await controller.AddProject(newProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddProject_WhenProjectPropertyProjectNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var newProject = new AddProjectDto { };
            string key = "ProjectName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddProject(newProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(message);
        }
        [Fact]
        public async Task AddProject_WhenProjectPropertyNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var newProject = new AddProjectDto { };
            string key = "ProjectName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddProject(newProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(messageError);
        }

        [Fact]
        public async Task AddProject_WhenProjectPropertyUseridNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var newProject = new AddProjectDto { };
            string key = "UserId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddProject(newProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(message);
        }

        [Fact]
        public async Task AddProject_WithUserIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.AddProject(A.Fake<AddProjectDto>());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("id");
            errorResponse.Errors["id"].Should().Contain($"The value '{invalidGuid}' is not valid.");
        }

        [Fact]
        public async Task AddProject_WhenUserDontExist_ShouldReturnsbadRequest()
        {
            // Arrange
            var newProject = new AddProjectDto { };
            var Project = new Project { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(false));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Returns(Task.FromResult<Project>(Project));

            // Act
            var result = await _controller.AddProject(newProject);

            // Assert
            var statusCodeResult = result as BadRequestObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            statusCodeResult.Value.Should().Be("Information of user not correct. Please check again! " + newProject.UserId.ToString());
        }

        [Fact]
        public async Task AddProject_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newProject = new AddProjectDto { };
            var Project = new Project { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(true));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Returns(Task.FromResult<Project>(null));

            // Act
            var result = await _controller.AddProject(newProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the project.");
        }
        [Fact]
        public async Task AddProject_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newProject = new AddProjectDto { };
            var Project = new Project { };
            string messageException = "Project existed";
            var message = "An error occurred while adding project into Database " + messageException;

            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(true));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddProject(newProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddProject_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newProject = new AddProjectDto { };
            var Project = new Project { };
            var addedProject = new Project { };

            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(true));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Returns(Task.FromResult(addedProject));

            // Act
            var result = await _controller.AddProject(newProject);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionProjectResponse;
            response.Message.Should().Be("Project added successfully.");
            response.Id.Should().Be(addedProject.Id);
        }




        #endregion

        #region AddProjectClerkDto
        [Fact]
        public async Task AddProjectUserClerk_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ProjectsController(null, null, null, _logger);
            var newProject = new AddProjectClerkDto();

            // Act
            var result = await controller.AddProjectUserClerk(newProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddProjectUserClerk_WhenProjectPropertyProjectNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var newProject = new AddProjectClerkDto { };
            string key = "ProjectName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddProjectUserClerk(newProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(message);
        }
        [Fact]
        public async Task AddProjectUserClerk_WhenProjectPropertyNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var newProject = new AddProjectClerkDto { };
            string key = "ProjectName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddProjectUserClerk(newProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(messageError);
        }

        [Fact]
        public async Task AddProjectUserClerk_WhenProjectPropertyUseridNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var newProject = new AddProjectClerkDto { };
            string key = "UserIdClerk";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddProjectUserClerk(newProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(message);
        }

        [Fact]
        public async Task AddProjectUserClerk_WhenUserDontExist_ShouldReturnsbadRequest()
        {
            // Arrange
            var newProject = new AddProjectClerkDto { UserIdClerk = "" };
            var Project = new Project { };
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(null));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Returns(Task.FromResult<Project>(Project));

            // Act
            var result = await _controller.AddProjectUserClerk(newProject);

            // Assert
            var statusCodeResult = result as BadRequestObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            statusCodeResult.Value.Should().Be("Information of user not correct. Please check again! " + newProject.UserIdClerk.ToString());
        }

        [Fact]
        public async Task AddProjectUserClerk_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newProject = new AddProjectClerkDto { UserIdClerk = "" };
            var Project = new Project { };
            var userExist = new User();
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(userExist));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Returns(Task.FromResult<Project>(null));

            // Act
            var result = await _controller.AddProjectUserClerk(newProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the project.");
        }
        [Fact]
        public async Task AddProjectUserClerk_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newProject = new AddProjectClerkDto { UserIdClerk = "" };
            var Project = new Project { };
            var userExist = new User();
            string messageException = "Project existed";
            var message = "An error occurred while adding project into Database " + messageException;

            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(userExist));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddProjectUserClerk(newProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddProjectUserClerk_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newProject = new AddProjectClerkDto { UserIdClerk = "" };
            var Project = new Project { };
            var addedProject = new Project { };
            var userExist = new User();
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(userExist));
            A.CallTo(() => _mapper.Map<Project>(newProject)).Returns(Project);
            A.CallTo(() => _projectRepo.Add(Project)).Returns(Task.FromResult(addedProject));

            // Act
            var result = await _controller.AddProjectUserClerk(newProject);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionProjectResponse;
            response.Message.Should().Be("Project added successfully.");
            response.Id.Should().Be(addedProject.Id);
        }



        #endregion

        #region UdpateProject
        [Fact]
        public async Task UpdatProjectProject_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ProjectsController(null, null, null, _logger);
            var UpdateProject = new UpdateProjectDto();

            // Act
            var result = await controller.UpdateProject(Guid.NewGuid(), UpdateProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task UpdateProject_WhenProjectPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var UpdateProject = new UpdateProjectDto();
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.UpdateProject(Guid.NewGuid(), UpdateProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("id");
            errorResponse.Errors["id"].Should().Contain("The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");
        }
        [Fact]
        public async Task UpdateProject_WhenProjectPropertyProjectNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var updateProject = new UpdateProjectDto { Id = Guid.NewGuid() };
            string key = "ProjectName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateProject(updateProject.Id, updateProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(message);
        }
        [Fact]
        public async Task UpdateProject_WhenProjectPropertyNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            var updateProject = new UpdateProjectDto { Id = Guid.NewGuid() };
            string key = "ProjectName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.UpdateProject(updateProject.Id, updateProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(messageError);
        }



        [Fact]
        public async Task UpdateProject_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var UpdateProject = new UpdateProjectDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateProject(differentId, UpdateProject);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Project ID information does not match");
        }

        [Fact]
        public async Task UpdateProject_WhenProjectDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var UpdateProject = new UpdateProjectDto { Id = Guid.NewGuid() };
            A.CallTo(() => _projectRepo.GetSingleByCondition(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Project>(null));

            // Act
            var result = await _controller.UpdateProject(UpdateProject.Id, UpdateProject);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Project not found.");
        }
        [Fact]
        public async Task UpdateProject_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var UpdateProject = new UpdateProjectDto { Id = Guid.NewGuid() };
            var existProject = new Project { Id = UpdateProject.Id };
            A.CallTo(() => _projectRepo.GetSingleByCondition(A<Expression<Func<Project, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existProject));
            A.CallTo(() => _mapper.Map(UpdateProject, existProject));
            A.CallTo(() => _projectRepo.Update(existProject)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateProject(UpdateProject.Id, UpdateProject);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionProjectResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Project updated successfully.");
            response.Id.Should().Be(existProject.Id);
        }
        [Fact]
        public async Task UpdateProject_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var UpdateProject = new UpdateProjectDto { Id = Guid.NewGuid() };
            var existProject = new Project { Id = UpdateProject.Id };
            A.CallTo(() => _projectRepo.GetSingleByCondition(A<Expression<Func<Project, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existProject));
            A.CallTo(() => _mapper.Map(UpdateProject, existProject));
            A.CallTo(() => _projectRepo.Update(existProject)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateProject(UpdateProject.Id, UpdateProject);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating project: Update failed");
        }


        #endregion

        #region DeleteProject
        [Fact]
        public async Task DeleteProject_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ProjectsController(null, null, null, _logger);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteProject(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteProject_WhenProjectDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteProject(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Don't exist project with ID {id} to delete");

        }

        [Fact]
        public async Task DeleteProject_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepo.DeleteProject(Guid.NewGuid())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteProject(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionProjectResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Project deleted successfully.");
            response.Id.Should().Be(id);
        }
        [Fact]
        public async Task DeleteProject_WhenDeleteFails_ShouldReturnInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepo.DeleteProject(id)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteProject(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error delete project: Delete failed");
        }

        [Fact]
        public async Task DeleteProject_WhenProjectPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ProjectsController(_projectRepo, _userRepo, _mapper, _logger);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeleteProject(Guid.NewGuid());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(key);
            errorResponse.Errors[key].Should().Contain(message);
        }
        #endregion
    }
}
