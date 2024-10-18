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
using Repositorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InSyncUnitTest.Controller
{
    public class ScenariosControllerTest
    {
        private IScenarioRepository _scenarioRepo;
        private IProjectRepository _projectRepo;
        private IUserRepository _userRepo;
        private ILogger<ScenariosController> _logger;
        private ScenariosController _controller;
        private IMapper _mapper;
        string userIdClerk = "userIdClerk";
        public ScenariosControllerTest()
        {
            _scenarioRepo = A.Fake<IScenarioRepository>();
            _projectRepo = A.Fake<IProjectRepository>();
            _userRepo = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<ScenariosController>>();
            _controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);

        }
        #region GetScenarios
        [Fact]
        public async Task GetScenarios_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ScenariosController(null, null, null, null, _logger);

            // Act
            var result = await controller.GetScenarios();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetScenarios_WithScenarios_ShouldReturnsOkResult()
        {
            // Arrange
            var scenarios = new List<Scenario> { new Scenario { Id = Guid.NewGuid(), ScenarioName = "Test Scenario" } }.AsQueryable();
            A.CallTo(() => _scenarioRepo.GetAll(A<string[]>._)).Returns(scenarios);

            // Act
            var result = await _controller.GetScenarios();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as IQueryable<Scenario>;
            response.Should().NotBeNull();
            response.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetScenarios_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving scenarios.";
            A.CallTo(() => _scenarioRepo.GetAll(A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetScenarios();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        #endregion

        #region GetAllScenarios
        [Fact]
        public async Task GetAllScenarios_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ScenariosController(null, null, null, null, _logger);
            var result = await controller.GetAllScenarios(0, 2, "");


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllScenarios_WithValidInput_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarios(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetAllScenarios_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarios(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }

        [Fact]
        public async Task GetAllScenarios_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarios(null, null, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetAllScenario_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving scenarios.";
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllScenarios(null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }

        #endregion

        #region GetScenarioId
        [Fact]
        public async Task GetScenarioId_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new ScenariosController(null, null, null, null, _logger);
            //Act
            var result = await controller.GetScenarioById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetScenarioId_WhenScenarioDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var ScenarioId = Guid.NewGuid();
            A.CallTo(() => _scenarioRepo.GetSingleByCondition(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Scenario>(null));

            // Act
            var result = await _controller.GetScenarioById(ScenarioId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No scenario has an ID : {ScenarioId}");
        }

        [Fact]
        public async Task GetScenarioId_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetScenarioById(Guid.Empty);

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
        public async Task GetScenarioId_WithIdExist_ShouldReturnOkResultWithScenario()
        {
            // Arrange
            var ScenarioId = Guid.NewGuid();
            var Scenario = new Scenario();
            var viewScenario = new ViewScenarioDto();

            A.CallTo(() => _scenarioRepo.GetSingleByCondition(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Returns(Task.FromResult(Scenario));
            A.CallTo(() => _mapper.Map<ViewScenarioDto>(Scenario)).Returns(viewScenario);

            // Act
            var result = await _controller.GetScenarioById(ScenarioId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewScenarioDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewScenario);
        }
        [Fact]
        public async Task GetScenarioById_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving the scenario.";
            A.CallTo(() => _scenarioRepo.GetSingleByCondition(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetScenarioById(Guid.NewGuid());

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }

        #endregion

        #region GetScenarioOfProject
        [Fact]
        public async Task GetScenarioOfProject_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ScenariosController(null, null, null, null, _logger);
            var result = await controller.GetScenarioOfProject(Guid.Empty, Guid.Empty, 0, 2, "");


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetScenarioOfProject_WithValidInput_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetScenarioOfProject(Guid.NewGuid(), Guid.NewGuid(), index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetScenarioOfProject_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetScenarioOfProject(Guid.NewGuid(), Guid.NewGuid(), index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }

        [Fact]
        public async Task GetScenarioOfProject_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetScenarioOfProject(Guid.NewGuid(), Guid.NewGuid(), null, null, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetScenarioOfProject_WithIdProjectInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetScenarioOfProject(Guid.Empty, Guid.Empty, 0, 2, "");

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
        public async Task GetScenarioOfProject_WithCreateByIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetScenarioOfProject(Guid.Empty, Guid.Empty, 0, 2, "");

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
        public async Task GetScenarioOfProject_WithCreateByIdNull_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            controller.ModelState.AddModelError("userIdClerk", $"The userIdClerk field is required.");

            // Act
            var result = await controller.GetScenarioOfProject(Guid.Empty, Guid.Empty, 0, 2, "");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("userIdClerk");
            errorResponse.Errors["userIdClerk"].Should().Contain($"The userIdClerk field is required.");
        }
        [Fact]
        public async Task GetScenarioOfProject_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving scenarios.";
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetScenarioOfProject(Guid.NewGuid(), Guid.NewGuid(), null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }

        #endregion

        #region GetScenarioOfProjectByUserClerk
        [Fact]
        public async Task GetScenarioOfProjectByUserClerk_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ScenariosController(null, null, null, null, _logger);
            var result = await controller.GetScenarioOfProjectByUserClerk(Guid.NewGuid(), "", 0, 2, "");


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetScenarioOfProjectByUserClerk_WithValidInput_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetScenarioOfProjectByUserClerk(Guid.NewGuid(), userIdClerk, 0, 2, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetScenarioOfProjectByUserClerk_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetScenarioOfProjectByUserClerk(Guid.NewGuid(), userIdClerk, index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }

        [Fact]
        public async Task GetScenarioOfProjectByUserClerk_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetScenarioOfProjectByUserClerk(Guid.NewGuid(), userIdClerk, null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetScenarioOfProjectByUserClerk_WithIdProjectInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetScenarioById(Guid.Empty);

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
        public async Task GetScenarioOfProjectByUserClerk_WithUserIdClerkNull_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            controller.ModelState.AddModelError("userIdClerk", $"The userIdClerk field is required.");

            // Act
            var result = await controller.GetScenarioOfProjectByUserClerk(Guid.Empty, "", 0, 2, "");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("userIdClerk");
            errorResponse.Errors["userIdClerk"].Should().Contain($"The userIdClerk field is required.");
        }

        [Fact]
        public async Task GetScenarioOfProjectByUserClerk_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving scenarios.";
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetScenarioOfProjectByUserClerk(Guid.NewGuid(), userIdClerk,null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }

        #endregion


        #region GetAllScenarioByUserId
        [Fact]
        public async Task GetAllScenarioByUserId_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ScenariosController(null, null, null, null, _logger);
            var result = await controller.GetAllScenarioByUserId(Guid.Empty, 0,2);


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllScenarioByUserId_WithValidInput_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarioByUserId(Guid.NewGuid(),  index, size,"");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetAllScenarioByUserId_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarioByUserId(Guid.NewGuid(),  index, size,"");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }

        [Fact]
        public async Task GetAllScenarioByUserId_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarioByUserId(Guid.NewGuid(), null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetAllScenarioByUserId_WithIdUserInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetAllScenarioByUserId(Guid.Empty,0,2);

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
        public async Task GetAllScenarioByUserId_WithUserIdNull_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            controller.ModelState.AddModelError("id", $"The value 'scenarios-user' is not valid.");

            // Act
            var result = await controller.GetAllScenarioByUserId(Guid.Empty, 0,2);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("id");
            errorResponse.Errors["id"].Should().Contain($"The value 'scenarios-user' is not valid.");
        }
        [Fact]
        public async Task GetAllScenarioByUserId_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving scenarios.";
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllScenarioByUserId(Guid.NewGuid(), null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }

        #endregion

        #region GetAllScenarioByUserIdClerk
        [Fact]
        public async Task GetAllScenarioByUserIdClerk_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new ScenariosController(null, null, null, null, _logger);
            var result = await controller.GetAllScenarioByUserIdClerk(userIdClerk, 0, 2, "");


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllScenarioByUserIdClerk_WithValidInput_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarioByUserIdClerk(userIdClerk, index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetAllScenarioByUserIdClerk_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _scenarioRepo.GetMultiPaging(A<Expression<Func<Scenario, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarioByUserIdClerk(userIdClerk, index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }

        [Fact]
        public async Task GetAllScenarioByUserIdClerk_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithScenarios()
        {
            // Arrange

            IEnumerable<Scenario> Scenarios = new List<Scenario> { new Scenario(), new Scenario() };
            IEnumerable<ViewScenarioDto> viewScenarios = new List<ViewScenarioDto> { new ViewScenarioDto(), new ViewScenarioDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._,  A<string[]>._))
                .Returns(Scenarios);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewScenarioDto>>(Scenarios)).Returns(viewScenarios);

            // Act
            var result = await _controller.GetAllScenarioByUserIdClerk(userIdClerk, null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedScenarios = okResult.Value as ResponsePaging<IEnumerable<ViewScenarioDto>>;
            returnedScenarios.Should().NotBeNull();
            returnedScenarios.data.Should().BeEquivalentTo(viewScenarios);
        }
        [Fact]
        public async Task GetAllScenarioByUserIdClerk_WithIdUserInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetAllScenarioByUserIdClerk(userIdClerk, 0, 2, "");

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
        public async Task GetAllScenarioByUserIdClerk_WithUserIdNull_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            controller.ModelState.AddModelError("id", $"The value 'scenarios-user-clerk' is not valid.");

            // Act
            var result = await controller.GetAllScenarioByUserId(Guid.Empty, 0,2);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("id");
            errorResponse.Errors["id"].Should().Contain($"The value 'scenarios-user-clerk' is not valid.");
        }

        [Fact]
        public async Task GetAllScenarioByUserIdClerk_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving scenarios.";
            A.CallTo(() => _scenarioRepo.GetMulti(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllScenarioByUserIdClerk(userIdClerk,null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        #endregion

        #region AddScenario
        [Fact]
        public async Task AddScenario_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ScenariosController(null, null, null, null, _logger);
            var newScenario = new AddScenarioDto();

            // Act
            var result = await controller.AddScenario(newScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddScenario_WhenScenarioPropertyIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioDto { };
            string key = "id";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenario(newScenario);

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
        public async Task AddScenario_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");
            var newScenarioDto = new AddScenarioDto();
            // Act
            var result = await controller.AddScenario(newScenarioDto);

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
        public async Task AddScenario_WhenScenarioPropertyScenarioNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioDto { };
            string key = "ScenarioName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenario(newScenario);

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
        public async Task AddScenario_WhenScenarioPropertyScenarioNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioDto { };
            string key = "ScenarioName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddScenario(newScenario);

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
        public async Task AddScenario_WhenScenarioPropertyCreateByNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioDto { };
            string key = "CreatedBy";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenario(newScenario);

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
        public async Task AddScenario_WithIdInValidCreateByFormat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("CreatedBy", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetScenarioById(Guid.Empty);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("CreatedBy");
            errorResponse.Errors["CreatedBy"].Should().Contain($"The value '{invalidGuid}' is not valid.");
        }

        [Fact]
        public async Task AddScenario_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newScenario = new AddScenarioDto { };
            var Scenario = new Scenario { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult<Scenario>(null));

            // Act
            var result = await _controller.AddScenario(newScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the scenario.");
        }
        [Fact]
        public async Task AddScenario_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newScenario = new AddScenarioDto { };
            var Scenario = new Scenario { };
            string messageException = "Scenario existed";
            var message = "An error occurred while adding scenario into Database: " + messageException;

            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddScenario(newScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddScenario_WhenInformationUserDontExist_ShouldReturnsOkResult()
        {
            // Arrange
            var newScenario = new AddScenarioDto { };
            var Scenario = new Scenario { };
            var addedScenario = new Scenario { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(false));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult(addedScenario));

            // Act
            var result = await _controller.AddScenario(newScenario);

            // Assert
            var okResult = result as BadRequestObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as string;
            response.Should().Be("Don't exist user has id by : " + newScenario.CreatedBy.ToString());

        }
        [Fact]
        public async Task AddScenario_WhenInformationProjectDontExist_ShouldReturnsOkResult()
        {
            // Arrange
            var newScenario = new AddScenarioDto { };
            var Scenario = new Scenario { };
            var addedScenario = new Scenario { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(false));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult(addedScenario));

            // Act
            var result = await _controller.AddScenario(newScenario);

            // Assert
            var okResult = result as BadRequestObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as string;
            response.Should().Be("Don't exist project has id by : " + newScenario.ProjectId.ToString());

        }
        [Fact]
        public async Task AddScenario_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newScenario = new AddScenarioDto { };
            var Scenario = new Scenario { };
            var addedScenario = new Scenario { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult(addedScenario));

            // Act
            var result = await _controller.AddScenario(newScenario);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionScenarioResponse;
            response.Message.Should().Be("Scenario added successfully.");
            response.Id.Should().Be(addedScenario.Id);
        }
        #endregion

        #region AddScenarioByUserClerk
        [Fact]
        public async Task AddScenarioByUserClerk_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ScenariosController(null, null, null, null, _logger);
            var newScenario = new AddScenarioUserClerkDto();

            // Act
            var result = await controller.AddScenarioByUserClerk(newScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddScenarioByUserClerk_WhenScenarioPropertyIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioUserClerkDto { };
            string key = "id";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenarioByUserClerk(newScenario);

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
        public async Task AddScenarioByUserClerk_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");
            var newScenarioClerk = new AddScenarioUserClerkDto();

            // Act
            var result = await controller.AddScenarioByUserClerk(newScenarioClerk);

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
        public async Task AddScenarioByUserClerk_WhenScenarioPropertyScenarioNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioUserClerkDto { };
            string key = "ScenarioName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenarioByUserClerk(newScenario);

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
        public async Task AddScenarioByUserClerk_WhenScenarioPropertyScenarioNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioUserClerkDto { };
            string key = "ScenarioName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddScenarioByUserClerk(newScenario);

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
        public async Task AddScenarioByUserClerk_WhenScenarioPropertyUserIdClerkNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioUserClerkDto { };
            string key = "UserIdClerk";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenarioByUserClerk(newScenario);

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
        public async Task AddScenarioByUserClerk_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newScenario = new AddScenarioUserClerkDto { };
            var Scenario = new Scenario { };
            var existUser = new User();
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existUser));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult<Scenario>(null));

            // Act
            var result = await _controller.AddScenarioByUserClerk(newScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the scenario.");
        }
        [Fact]
        public async Task AddScenarioByUserClerk_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newScenario = new AddScenarioUserClerkDto { };
            var Scenario = new Scenario { };
            string messageException = "Scenario existed";
            var message = "An error occurred while adding scenario into Database: " + messageException;
            var existUser = new User();

            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existUser));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddScenarioByUserClerk(newScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddScenarioByUserClerk_WhenInformationUserDontExist_ShouldReturnsOkResult()
        {
            // Arrange
            var newScenario = new AddScenarioUserClerkDto { UserIdClerk = userIdClerk };
            var Scenario = new Scenario { };
            var addedScenario = new Scenario { };


            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(null));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult(addedScenario));

            // Act
            var result = await _controller.AddScenarioByUserClerk(newScenario);

            // Assert
            var okResult = result as BadRequestObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as string;
            response.Should().Be("Don't exist user has id by : " + newScenario.UserIdClerk.ToString());

        }
        [Fact]
        public async Task AddScenarioByUserCler_WhenInformationProjectDontExist_ShouldReturnsOkResult()
        {
            // Arrange
            var newScenario = new AddScenarioUserClerkDto { };
            var Scenario = new Scenario { };
            var addedScenario = new Scenario { };
            var existUser = new User();
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existUser));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(false));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult(addedScenario));

            // Act
            var result = await _controller.AddScenarioByUserClerk(newScenario);

            // Assert
            var okResult = result as BadRequestObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as string;
            response.Should().Be("Don't exist project has id by : " + newScenario.ProjectId.ToString());

        }
        [Fact]
        public async Task AddScenarioByUserCler_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newScenario = new AddScenarioUserClerkDto { };
            var Scenario = new Scenario { };
            var addedScenario = new Scenario { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Scenario>(newScenario)).Returns(Scenario);
            A.CallTo(() => _scenarioRepo.Add(Scenario)).Returns(Task.FromResult(addedScenario));

            // Act
            var result = await _controller.AddScenarioByUserClerk(newScenario);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionScenarioResponse;
            response.Message.Should().Be("Scenario added successfully.");
            response.Id.Should().Be(addedScenario.Id);
        }
        #endregion

        #region UpdateScenario
        [Fact]
        public async Task UpdatScenario_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ScenariosController(null, null, null, null, _logger);
            var UpdateScenario = new UpdateScenarioDto();

            // Act
            var result = await controller.UpdateScenario(Guid.NewGuid(), UpdateScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task UpdateScenario_WhenScenarioPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var UpdateScenario = new UpdateScenarioDto();
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.UpdateScenario(Guid.NewGuid(), UpdateScenario);

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
        public async Task UpdateScenario_WhenScenarioPropertyIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioDto { };
            string key = "Id";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenario(newScenario);

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
        public async Task UpdateScenario_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");
            var newScenarioDto = new AddScenarioDto();
            // Act
            var result = await controller.AddScenario(newScenarioDto);

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
        public async Task UpdateScenario_WhenScenarioPropertyScenarioNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioDto { };
            string key = "ScenarioName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddScenario(newScenario);

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
        public async Task UpdateScenario_WhenScenarioPropertyScenarioNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            var newScenario = new AddScenarioDto { };
            string key = "ScenarioName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddScenario(newScenario);

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
        public async Task UpdateScenario_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var UpdateScenario = new UpdateScenarioDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateScenario(differentId, UpdateScenario);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Scenario ID information does not match");
        }

        [Fact]
        public async Task UpdateScenario_WhenScenarioDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var UpdateScenario = new UpdateScenarioDto { Id = Guid.NewGuid() };
            A.CallTo(() => _scenarioRepo.GetSingleByCondition(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Scenario>(null));

            // Act
            var result = await _controller.UpdateScenario(UpdateScenario.Id, UpdateScenario);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Scenario not found.");
        }
        [Fact]
        public async Task UpdateScenario_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var UpdateScenario = new UpdateScenarioDto { Id = Guid.NewGuid() };
            var existScenario = new Scenario { Id = UpdateScenario.Id };
            A.CallTo(() => _scenarioRepo.GetSingleByCondition(A<Expression<Func<Scenario, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existScenario));
            A.CallTo(() => _mapper.Map(UpdateScenario, existScenario));
            A.CallTo(() => _scenarioRepo.Update(existScenario)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateScenario(UpdateScenario.Id, UpdateScenario);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionScenarioResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Scenario updated successfully.");
            response.Id.Should().Be(existScenario.Id);
        }
        [Fact]
        public async Task UpdateScenario_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var UpdateScenario = new UpdateScenarioDto { Id = Guid.NewGuid() };
            var existScenario = new Scenario { Id = UpdateScenario.Id };
            A.CallTo(() => _scenarioRepo.GetSingleByCondition(A<Expression<Func<Scenario, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existScenario));
            A.CallTo(() => _mapper.Map(UpdateScenario, existScenario));
            A.CallTo(() => _scenarioRepo.Update(existScenario)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateScenario(UpdateScenario.Id, UpdateScenario);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating scenario: Update failed");
        }


        #endregion

        #region DeleteScenario
        [Fact]
        public async Task DeleteScenario_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new ScenariosController(null, null, null, null, _logger);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteScenario(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteScenario_WhenScenarioDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _scenarioRepo.CheckContainsAsync(A<Expression<Func<Scenario, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteScenario(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Don't exist scenario with id {id} to delete");

        }

        [Fact]
        public async Task DeleteScenario_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _scenarioRepo.CheckContainsAsync(A<Expression<Func<Scenario, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _scenarioRepo.DeleteMulti(A<Expression<Func<Scenario, bool>>>._)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteScenario(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionScenarioResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Scenario deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DeleteScenario_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _scenarioRepo.CheckContainsAsync(A<Expression<Func<Scenario, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _scenarioRepo.DeleteMulti(A<Expression<Func<Scenario, bool>>>._)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteScenario(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error delete scenario: Delete failed");
        }
        [Fact]
        public async Task DeleteScenario_WhenScenarioPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new ScenariosController(_scenarioRepo, _userRepo, _projectRepo, _mapper, _logger);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeleteScenario(Guid.NewGuid());

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
