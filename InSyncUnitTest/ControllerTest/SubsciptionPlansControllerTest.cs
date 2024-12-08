using AutoMapper;
using AutoMapper.Internal;
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InSyncUnitTest.Controller
{
    public class SubsciptionPlansControllerTest
    {
        private ISubscriptionPlanRepository _subPlanRepo;
        private IUserRepository _userRepo;
        private IMapper _mapper;
        private ILogger<SubscriptionPlansController> _logger;
        private SubscriptionPlansController _controller;
        public SubsciptionPlansControllerTest()
        {
            _subPlanRepo = A.Fake<ISubscriptionPlanRepository>();
            _userRepo = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<SubscriptionPlansController>>();
            _controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
        }
        #region GetSubscriptionPlans
        [Fact]
        public async Task GetSubscriptionPlans_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new SubscriptionPlansController(null, null, null, _logger);

            // Act
            var result = await controller.GetSubscriptionPlans();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetSubscriptionPlans_WithSubscriptionPlans_ShouldReturnsOkResult()
        {
            // Arrange
            var subsciptionPlans = new[] { new SubscriptionPlan(), new SubscriptionPlan() }.AsQueryable();
            A.CallTo(() => _subPlanRepo.GetAll(A<string[]>._)).Returns(subsciptionPlans);

            // Act
            var result = await _controller.GetSubscriptionPlans();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as IQueryable<SubscriptionPlan>;
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(subsciptionPlans);
            response.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSubsciptionPlans_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving subscription plans.";
            var response = $"Error retrieving subscription plans: {message}";

            A.CallTo(() => _subPlanRepo.GetAll(A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetSubscriptionPlans();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion
        #region GetAllSubsciptionPlan
        [Fact]
        public async Task GetAllSubsciptionPlan_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new SubscriptionPlansController(null, null, null, _logger);
            var result = await controller.GetSubscriptionPlans();


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllSubsciptionPlan_WithValidInput_ShouldReturnOkResultWithSubscriptionPlans()
        {
            // Arrange

            IEnumerable<SubscriptionPlan> SubscriptionPlans = new List<SubscriptionPlan> { new SubscriptionPlan(), new SubscriptionPlan() };
            IEnumerable<ViewSubscriptionPlanDto> viewSubscriptionPlans = new List<ViewSubscriptionPlanDto> { new ViewSubscriptionPlanDto(), new ViewSubscriptionPlanDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _subPlanRepo.GetMultiPaging(A<Expression<Func<SubscriptionPlan, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(SubscriptionPlans);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewSubscriptionPlanDto>>(SubscriptionPlans)).Returns(viewSubscriptionPlans);

            // Act
            var result = await _controller.GetAllSubsciptionPlan(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedSubscriptionPlans = okResult.Value as ResponsePaging<IEnumerable<ViewSubscriptionPlanDto>>;
            returnedSubscriptionPlans.Should().NotBeNull();
            returnedSubscriptionPlans.data.Should().BeEquivalentTo(viewSubscriptionPlans);
        }
        [Fact]
        public async Task GetAllSubsciptionPlan_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithSubscriptionPlans()
        {
            // Arrange

            IEnumerable<SubscriptionPlan> SubscriptionPlans = new List<SubscriptionPlan> { new SubscriptionPlan(), new SubscriptionPlan() };
            IEnumerable<ViewSubscriptionPlanDto> viewSubscriptionPlans = new List<ViewSubscriptionPlanDto> { new ViewSubscriptionPlanDto(), new ViewSubscriptionPlanDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _subPlanRepo.GetMultiPaging(A<Expression<Func<SubscriptionPlan, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(SubscriptionPlans);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewSubscriptionPlanDto>>(SubscriptionPlans)).Returns(viewSubscriptionPlans);

            // Act
            var result = await _controller.GetAllSubsciptionPlan(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedSubscriptionPlans = okResult.Value as ResponsePaging<IEnumerable<ViewSubscriptionPlanDto>>;
            returnedSubscriptionPlans.Should().NotBeNull();
            returnedSubscriptionPlans.data.Should().BeEquivalentTo(viewSubscriptionPlans);
        }

        [Fact]
        public async Task GetAllSubsciptionPlan_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithSubscriptionPlans()
        {
            // Arrange

            IEnumerable<SubscriptionPlan> SubscriptionPlans = new List<SubscriptionPlan> { new SubscriptionPlan(), new SubscriptionPlan() };
            IEnumerable<ViewSubscriptionPlanDto> viewSubscriptionPlans = new List<ViewSubscriptionPlanDto> { new ViewSubscriptionPlanDto(), new ViewSubscriptionPlanDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _subPlanRepo.GetMulti(A<Expression<Func<SubscriptionPlan, bool>>>._,  A<string[]>._))
                .Returns(SubscriptionPlans);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewSubscriptionPlanDto>>(SubscriptionPlans)).Returns(viewSubscriptionPlans);

            // Act
            var result = await _controller.GetAllSubsciptionPlan(null, null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedSubscriptionPlans = okResult.Value as ResponsePaging<IEnumerable<ViewSubscriptionPlanDto>>;
            returnedSubscriptionPlans.Should().NotBeNull();
            returnedSubscriptionPlans.data.Should().BeEquivalentTo(viewSubscriptionPlans);
        }
        [Fact]
        public async Task GetAllSubsciptionPlan_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving subscription plans.";
            var response = $"Error retrieving subscription plans: {message}";

            A.CallTo(() => _subPlanRepo.GetMulti(A<Expression<Func<SubscriptionPlan, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllSubsciptionPlan(null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }

        #endregion

        #region GetSubsciptionPlanById
        [Fact]
        public async Task GetSubscriptionPlanById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new SubscriptionPlansController(null, null, null, _logger);
            //Act
            var result = await controller.GetSubsciptionPlanById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetSubscriptionPlanById_WhenSubscriptionPlanDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var SubscriptionPlanId = Guid.NewGuid();
            A.CallTo(() => _subPlanRepo.GetSingleByCondition(A<Expression<Func<SubscriptionPlan, bool>>>._, A<string[]>._)).Returns(Task.FromResult<SubscriptionPlan>(null));

            // Act
            var result = await _controller.GetSubsciptionPlanById(SubscriptionPlanId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No subscription plan has an ID: {SubscriptionPlanId}");
        }

        [Fact]
        public async Task GetSubscriptionPlanById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetSubsciptionPlanById(Guid.Empty);

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
        public async Task GetSubscriptionPlanById_WithIdExist_ShouldReturnOkResultWithSubscriptionPlan()
        {
            // Arrange
            var SubscriptionPlanId = Guid.NewGuid();
            var SubscriptionPlan = new SubscriptionPlan();
            var viewSubscriptionPlan = new ViewSubscriptionPlanDto();

            A.CallTo(() => _subPlanRepo.GetSingleByCondition(A<Expression<Func<SubscriptionPlan, bool>>>._, A<string[]>._)).Returns(Task.FromResult(SubscriptionPlan));
            A.CallTo(() => _mapper.Map<ViewSubscriptionPlanDto>(SubscriptionPlan)).Returns(viewSubscriptionPlan);

            // Act
            var result = await _controller.GetSubsciptionPlanById(SubscriptionPlanId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewSubscriptionPlanDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewSubscriptionPlan);
        }

        [Fact]
        public async Task GetSubsciptionPlanById_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving subsciption plans.";
            var response = $"Error retrieving subscription plan: {message}";

            A.CallTo(() => _subPlanRepo.GetSingleByCondition(A<Expression<Func<SubscriptionPlan, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetSubsciptionPlanById(Guid.NewGuid());

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region AddSubscriptionPlan
        [Fact]
        public async Task AddSubscriptionPlan_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new SubscriptionPlansController(null, null, null, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto();

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "SubscriptionsName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "SubscriptionsName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyPriceInvalidFomatBoolean_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "Status";
            string messageError = $"The field {key} invalid fomat bool.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyPriceNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "Price";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyPriceInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "Price";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyPriceSmaller0_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "Price";
            string messageError = $"{key} must be greater than 0.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyContentNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "Content";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyMaxProjectInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "MaxProject";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyMaxScenarioInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "MaxScenario";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyMaxUsersAccessInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "MaxUsersAccess";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyStorageLimitInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "StorageLimit";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyDataRetentionPeriodNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "DataRetentionPeriod";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenSubscriptionPlanPropertyDataRetentionPeriodPriceInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            string key = "DataRetentionPeriod";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlan(newSubscriptionPlan);

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
        public async Task AddSubscriptionPlan_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            var SubscriptionPlan = new SubscriptionPlan { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(true));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Returns(Task.FromResult<SubscriptionPlan>(null));

            // Act
            var result = await _controller.AddSubsciptionPlan(newSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the subscription plan.");
        }
        [Fact]
        public async Task AddSubscriptionPlan_WhenUserDontExist_ShouldReturnsNotFoundResult()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanDto {  };
            var SubscriptionPlan = new SubscriptionPlan { };
            var addedSubscriptionPlan = new SubscriptionPlan { };

            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(false));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Returns(Task.FromResult(addedSubscriptionPlan));

            // Act
            var result = await _controller.AddSubsciptionPlan(newSubscriptionPlan);

            // Assert
            var notFound = result as BadRequestObjectResult;
            notFound.Should().NotBeNull();
            notFound.Value.Should().Be($"Don't exist user with id {newSubscriptionPlan.UserId.ToString()} to add Subscription Plan");


        }
        [Fact]
        public async Task AddSubscriptionPlan_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            var SubscriptionPlan = new SubscriptionPlan { };
            string messageException = "Subsciption Plan existed";
            var message = "An error occurred while adding Subscription Plan into Database: " + messageException;

            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(true));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddSubsciptionPlan(newSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddSubscriptionPlan_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanDto { };
            var SubscriptionPlan = new SubscriptionPlan { };
            var addedSubscriptionPlan = new SubscriptionPlan { };

            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(true));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Returns(Task.FromResult(addedSubscriptionPlan));

            // Act
            var result = await _controller.AddSubsciptionPlan(newSubscriptionPlan);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionSubsciptionPlanResponse;
            response.Message.Should().Be("Subscription plan added successfully.");
            response.Id.Should().Be(addedSubscriptionPlan.Id);
        }




        #endregion

        #region AddSubsciptionPlanUserClerk
        [Fact]
        public async Task AddSubsciptionPlanUserClerk_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new SubscriptionPlansController(null, null, null, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto();

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "SubscriptionsName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "SubscriptionsName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyPriceInvalidFomatBoolean_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "Status";
            string messageError = $"The field {key} invalid fomat bool.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyPriceNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "Price";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyPriceInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "Price";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyPriceSmaller0_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "Price";
            string messageError = $"{key} must be greater than 0.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyContentNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "Content";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyMaxProjectInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "MaxProject";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyMaxScenarioInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "MaxScenario";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyMaxUsersAccessInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "MaxUsersAccess";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyStorageLimitInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "StorageLimit";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyDataRetentionPeriodNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "DataRetentionPeriod";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenSubscriptionPlanPropertyDataRetentionPeriodPriceInvalidFomatNumber_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            string key = "DataRetentionPeriod";
            string messageError = $"The field {key} invalid fomat.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

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
        public async Task AddSubsciptionPlanUserClerk_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            var SubscriptionPlan = new SubscriptionPlan { };
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(A.Fake<User>()));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Returns(Task.FromResult<SubscriptionPlan>(null));

            // Act
            var result = await _controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the subscription plan.");
        }
        [Fact]
        public async Task AddSubsciptionPlanUserClerk_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            var SubscriptionPlan = new SubscriptionPlan { };
            string messageException = "Subsciption Plan existed";
            var message = "An error occurred while adding Subscription Plan into Database: " + messageException;

            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(A.Fake<User>()));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }

        [Fact]
        public async Task AddSubsciptionPlanUserClerk_WhenUserDontExist_ShouldReturnsNotFoundResult()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { UserIdClerk = "" };
            var SubscriptionPlan = new SubscriptionPlan { };
            var addedSubscriptionPlan = new SubscriptionPlan { };

            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(null));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Returns(Task.FromResult(addedSubscriptionPlan));

            // Act
            var result = await _controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

            // Assert
            var notFound = result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
            notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFound.Value.Should().Be($"Don't exist user with id {newSubscriptionPlan.UserIdClerk.ToString()} to add Subscription Plan");


        }
        [Fact]
        public async Task AddSubsciptionPlanUserClerk_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newSubscriptionPlan = new AddSubscriptionPlanUserClerkDto { };
            var SubscriptionPlan = new SubscriptionPlan { };
            var addedSubscriptionPlan = new SubscriptionPlan { };

            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(A.Fake<User>()));
            A.CallTo(() => _mapper.Map<SubscriptionPlan>(newSubscriptionPlan)).Returns(SubscriptionPlan);
            A.CallTo(() => _subPlanRepo.Add(SubscriptionPlan)).Returns(Task.FromResult(addedSubscriptionPlan));

            // Act
            var result = await _controller.AddSubsciptionPlanUserClerk(newSubscriptionPlan);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionSubsciptionPlanResponse;
            response.Message.Should().Be("Subscription plan added successfully.");
            response.Id.Should().Be(addedSubscriptionPlan.Id);
        }




        #endregion


        #region UdpateSubscriptionPlan
        [Fact]
        public async Task UpdatSubscriptionPlanSubscriptionPlan_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new SubscriptionPlansController(null, null, null, _logger);
            var updateSubscriptionPlan = new UpdateSubscriptionPlanDto();

            // Act
            var result = await controller.UpdateSubscriptionPlan(Guid.NewGuid(), updateSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task UpdateSubscriptionPlan_WhenSubscriptionPlanPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            var updateSubscriptionPlan = new UpdateSubscriptionPlanDto();
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.UpdateSubscriptionPlan(Guid.NewGuid(), updateSubscriptionPlan);

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
        public async Task UpdateSubscriptionPlan_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var updateSubscriptionPlan = new UpdateSubscriptionPlanDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateSubscriptionPlan(differentId, updateSubscriptionPlan);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Subscription plan ID information does not match");
        }

        [Fact]
        public async Task UpdateSubscriptionPlan_WhenSubscriptionPlanDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var updateSubscriptionPlan = new UpdateSubscriptionPlanDto { Id = Guid.NewGuid() };
            A.CallTo(() => _subPlanRepo.GetSingleByCondition(A<Expression<Func<SubscriptionPlan, bool>>>._, A<string[]>._)).Returns(Task.FromResult<SubscriptionPlan>(null));

            // Act
            var result = await _controller.UpdateSubscriptionPlan(updateSubscriptionPlan.Id, updateSubscriptionPlan);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Subscription plan not found.");
        }
        [Fact]
        public async Task UpdateSubscriptionPlan_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var updateSubscriptionPlan = new UpdateSubscriptionPlanDto { Id = Guid.NewGuid() };
            var existSubscriptionPlan = new SubscriptionPlan { Id = updateSubscriptionPlan.Id };
            A.CallTo(() => _subPlanRepo.GetSingleByCondition(A<Expression<Func<SubscriptionPlan, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existSubscriptionPlan));
            A.CallTo(() => _mapper.Map(updateSubscriptionPlan, existSubscriptionPlan));
            A.CallTo(() => _subPlanRepo.Update(existSubscriptionPlan)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateSubscriptionPlan(updateSubscriptionPlan.Id, updateSubscriptionPlan);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionSubsciptionPlanResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Subscription plan updated successfully.");
            response.Id.Should().Be(existSubscriptionPlan.Id);
        }
        [Fact]
        public async Task UpdateSubscriptionPlan_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var updateSubscriptionPlan = new UpdateSubscriptionPlanDto { Id = Guid.NewGuid() };
            var existSubscriptionPlan = new SubscriptionPlan { Id = updateSubscriptionPlan.Id };
            A.CallTo(() => _subPlanRepo.GetSingleByCondition(A<Expression<Func<SubscriptionPlan, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existSubscriptionPlan));
            A.CallTo(() => _mapper.Map(updateSubscriptionPlan, existSubscriptionPlan));
            A.CallTo(() => _subPlanRepo.Update(existSubscriptionPlan)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateSubscriptionPlan(updateSubscriptionPlan.Id, updateSubscriptionPlan);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating subscription plan: Update failed");
        }


        #endregion


        #region DeleteSubscriptionPlan
        [Fact]
        public async Task DeleteSubscriptionPlan_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new SubscriptionPlansController(null, null, null, _logger);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteSubsciptionPlan(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteSubscriptionPlan_WhenSubscriptionPlanDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _subPlanRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteSubsciptionPlan(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Don't exist subscription plan with id {id} to delete");

        }

        [Fact]
        public async Task DeleteSubscriptionPlan_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _subPlanRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _subPlanRepo.DeleteSubsciptionPlan(A<Guid>._)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteSubsciptionPlan(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionSubsciptionPlanResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Subscription plan deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DeleteSubscriptionPlan_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _subPlanRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _subPlanRepo.DeleteSubsciptionPlan(A<Guid>._)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteSubsciptionPlan(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error delete subscription plan: Delete failed");
        }
        [Fact]
        public async Task DeleteSubscriptionPlan_WhenSubscriptionPlanPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new SubscriptionPlansController(_subPlanRepo, _userRepo, _mapper, _logger);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeleteSubsciptionPlan(Guid.NewGuid());

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
