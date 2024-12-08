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

namespace InSyncUnitTest.Controller
{
    public class UserSubscriptionsControllerTest
    {
        private IUserSubscriptionRepository _userSubRepo;
        private IUserRepository _userRepo;
        private ISubscriptionPlanRepository _subRepo;
        private ILogger<UserSubscriptionsController> _logger;
        private IMapper _mapper;
        private UserSubscriptionsController _controller;
        
        public UserSubscriptionsControllerTest()
        {
            _userSubRepo = A.Fake<IUserSubscriptionRepository>();
            _userRepo = A.Fake<IUserRepository>();
            _subRepo = A.Fake<ISubscriptionPlanRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<UserSubscriptionsController>>();
            _controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
        }

        #region GetUserSubscriptions
        [Fact]
        public async Task GetUserSubscriptions_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);

            // Act
            var result = await controller.GetUserSubscriptions();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetUserSubscriptions_WithUserSubscriptions_ShouldReturnsOkResult()
        {
            // Arrange
            var userSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() }.AsQueryable();
            A.CallTo(() => _userSubRepo.GetAll(A<string[]>._)).Returns(userSubscriptions);

            // Act
            var result = await _controller.GetUserSubscriptions();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as IQueryable<UserSubscription>;
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(userSubscriptions);
            response.Should().HaveCount(2);
        }
        [Fact]
        public async Task GetUserSubscriptions_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving user subscriptions.";
            var response = $"Error retrieving user subscriptions: {message}";
            A.CallTo(() => _userSubRepo.GetAll( A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetUserSubscriptions();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }

        #endregion

        #region GetAllUserSubscription
        [Fact]
        public async Task GetAllUserSubscription_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);
            var result = await controller.GetAllUserSubscription(0, 2);


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllUserSubscription_WithValidInput_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange

            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _userSubRepo.GetMultiPaging(A<Expression<Func<UserSubscription, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubscription(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }
        [Fact]
        public async Task GetAllUserSubscription_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange

            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _userSubRepo.GetMultiPaging(A<Expression<Func<UserSubscription, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubscription(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }

        [Fact]
        public async Task GetAllUserSubscription_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange

            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _userSubRepo.GetMulti(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubscription(null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }
        [Fact]
        public async Task GetAllUserSubscription_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving user subscriptions.";
            var response = $"Error retrieving user subscriptions: {message}";
            A.CallTo(() => _userSubRepo.GetMulti(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllUserSubscription(null, null);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region GetAllUserSubsciptionOfUser
        [Fact]
        public async Task GetAllUserSubsciptionOfUser_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);
            var result = await controller.GetAllUserSubsciptionOfUser(Guid.NewGuid(), null, null);


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllUserSubsciptionOfUser_WithValidInput_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange
            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _userSubRepo.GetMultiPaging(A<Expression<Func<UserSubscription, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUser(Guid.NewGuid(), index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }
        [Fact]
        public async Task GetAllUserSubsciptionOfUser_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange

            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _userSubRepo.GetMultiPaging(A<Expression<Func<UserSubscription, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUser(Guid.NewGuid(), index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }

        [Fact]
        public async Task GetAllUserSubsciptionOfUser_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange

            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _userSubRepo.GetMulti(A<Expression<Func<UserSubscription, bool>>>._,  A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUser(Guid.NewGuid(), null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }
        [Fact]
        public async Task GetAllUserSubsciptionOfUser_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            string key = "id";
            string messageError = $"The value '{invalidGuid}' is not valid.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.GetAllUserSubsciptionOfUser(Guid.Empty, null,null);

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
        public async Task GetAllUserSubscriptionsOfUser_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving user subscriptions.";
            var response = $"Error retrieving user subscriptions: {message}";
            A.CallTo(() => _userSubRepo.GetMulti(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUser(Guid.NewGuid(), null, null);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region GetAllUserSubsciptionOfUserClerk
        [Fact]
        public async Task GetAllUserSubsciptionOfUserClerk_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);
            string userIdClerk = "user_id_clerk";
            var result = await controller.GetAllUserSubsciptionOfUserClerk(userIdClerk, null, null);


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllUserSubsciptionOfUserClerk_WithValidInput_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange
            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            int index = 0, size = 2;
            string userIdClerk = "user_id_clerk";
            A.CallTo(() => _userSubRepo.GetMultiPaging(A<Expression<Func<UserSubscription, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUserClerk(userIdClerk, index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }
        [Fact]
        public async Task GetAllUserSubsciptionOfUserClerk_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange

            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            int index = -1, size = -3;
            string userIdClerk = "user_id_clerk";
            A.CallTo(() => _userSubRepo.GetMultiPaging(A<Expression<Func<UserSubscription, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUserClerk(userIdClerk, index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }

        [Fact]
        public async Task GetAllUserSubsciptionOfUserClerk_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithUserSubscriptions()
        {
            // Arrange

            IEnumerable<UserSubscription> UserSubscriptions = new List<UserSubscription> { new UserSubscription(), new UserSubscription() };
            IEnumerable<ViewUserSubsciptionDto> viewUserSubscriptions = new List<ViewUserSubsciptionDto> { new ViewUserSubsciptionDto(), new ViewUserSubsciptionDto() };
            int total;
            string userIdClerk = "user_id_clerk";
            string[] includes = new string[] { };
            A.CallTo(() => _userSubRepo.GetMulti(A<Expression<Func<UserSubscription, bool>>>._,  A<string[]>._))
                .Returns(UserSubscriptions);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewUserSubsciptionDto>>(UserSubscriptions)).Returns(viewUserSubscriptions);

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUserClerk(userIdClerk, null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedUserSubscriptions = okResult.Value as ResponsePaging<IEnumerable<ViewUserSubsciptionDto>>;
            returnedUserSubscriptions.Should().NotBeNull();
            returnedUserSubscriptions.data.Should().BeEquivalentTo(viewUserSubscriptions);
        }

        [Fact]
        public async Task GetAllUserSubscriptionsOfUserClerk_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving user subscriptions.";
            var response = $"Error retrieving user subscriptions: {message}";
            A.CallTo(() => _userSubRepo.GetMulti(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllUserSubsciptionOfUserClerk("userIdClerk", null, null);


            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region GetUserSubscriptionById
        [Fact]
        public async Task GetUserSubscriptionById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);
            //Act
            var result = await controller.GetUserSubsciptionById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetUserSubscriptionById_WhenUserSubscriptionDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var UserSubscriptionId = Guid.NewGuid();
            A.CallTo(() => _userSubRepo.GetSingleByCondition(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Returns(Task.FromResult<UserSubscription>(null));

            // Act
            var result = await _controller.GetUserSubsciptionById(UserSubscriptionId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No user subscription has an ID: " + UserSubscriptionId.ToString());
        }

        [Fact]
        public async Task GetUserSubscriptionById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            string key = "id";
            string messageError = $"The value '{invalidGuid}' is not valid.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.GetUserSubsciptionById(Guid.Empty);

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
        public async Task GetUserSubscriptionById_WithIdExist_ShouldReturnOkResultWithUserSubscription()
        {
            // Arrange
            var UserSubscriptionId = Guid.NewGuid();
            var UserSubscription = new UserSubscription();
            var viewUserSubscription = new ViewUserSubsciptionDto();

            A.CallTo(() => _userSubRepo.GetSingleByCondition(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Returns(Task.FromResult(UserSubscription));
            A.CallTo(() => _mapper.Map<ViewUserSubsciptionDto>(UserSubscription)).Returns(viewUserSubscription);

            // Act
            var result = await _controller.GetUserSubsciptionById(UserSubscriptionId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewUserSubsciptionDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewUserSubscription);
        }

        [Fact]
        public async Task GetUserSubsciptionById_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving user subscriptions.";
            var response = $"Error retrieving user subscription: {message}";
            A.CallTo(() => _userSubRepo.GetSingleByCondition(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetUserSubsciptionById(Guid.NewGuid());

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region AddUserSubsciption
        [Fact]
        public async Task AddUserSubsciption_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);
            var newUserSubsciption = new AddUserSubsciptionDto();

            // Act
            var result = await controller.AddUserSubsciption(newUserSubsciption);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertySubscriptionPlanIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newUserSubsciption = new AddUserSubsciptionDto { };
            string key = "SubscriptionPlanId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddUserSubsciption(newUserSubsciption);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertySubscriptionPlanIdInvalidFormatGuid_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newPrivacyPolicy = new AddUserSubsciptionDto { };
            string key = "SubscriptionPlanId";
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            string messageError = $"The value '{invalidGuid}' is not valid.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddUserSubsciption(newPrivacyPolicy);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertyUserIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newUserSubsciption = new AddUserSubsciptionDto { };
            string key = "UserId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddUserSubsciption(newUserSubsciption);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertyUserIdInvalidFormatGuid_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newPrivacyPolicy = new AddUserSubsciptionDto { };
            string key = "UserId";
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            string messageError = $"The value '{invalidGuid}' is not valid.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddUserSubsciption(newPrivacyPolicy);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertyStripeCurrentPeriodEndNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newUserSubsciption = new AddUserSubsciptionDto { };
            string key = "StripeCurrentPeriodEnd";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddUserSubsciption(newUserSubsciption);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertyStripeCurrentPeriodEndnvalidFormatGuid_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newPrivacyPolicy = new AddUserSubsciptionDto { };
            string key = "StripeCurrentPeriodEnd";
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            string messageError = $"The JSON value could not be converted to System.DateTime.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddUserSubsciption(newPrivacyPolicy);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertyStripeCustomerIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newUserSubsciption = new AddUserSubsciptionDto { };
            string key = "StripeCustomerId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddUserSubsciption(newUserSubsciption);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertyStripeSubscriptionIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newUserSubsciption = new AddUserSubsciptionDto { };
            string key = "StripeSubscriptionId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddUserSubsciption(newUserSubsciption);

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
        public async Task AddUserSubsciption_WhenUserSubscriptionPropertyStripePriceIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var newUserSubsciption = new AddUserSubsciptionDto { };
            string key = "StripePriceId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddUserSubsciption(newUserSubsciption);

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
        public async Task AddUserSubscription_WhenAddFails_ShouldReturnInternalServerError()
        {
            // Arrange
            var newUserSubscription = new AddUserSubsciptionDto { };
            var userSubscription = new UserSubscription { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _subRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<UserSubscription>(newUserSubscription)).Returns(userSubscription);
            A.CallTo(() => _userSubRepo.Add(userSubscription)).Returns(Task.FromResult<UserSubscription>(null));

            // Act
            var result = await _controller.AddUserSubsciption(newUserSubscription);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the user subscription.");
        }
        [Fact]
        public async Task AddUserSubsciption_WhenUserIdDontExist_ShouldReturnsBadREquest()
        {
            // Arrange
            var newUserSubsciption = new AddUserSubsciptionDto { };
            var userSubsciption = new UserSubscription { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(false));
            A.CallTo(() => _subRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<UserSubscription>(newUserSubsciption)).Returns(userSubsciption);
            A.CallTo(() => _userSubRepo.Add(userSubsciption)).Returns(Task.FromResult<UserSubscription>(userSubsciption));

            // Act
            var result = await _controller.AddUserSubsciption(newUserSubsciption);

            // Assert
            var statusCodeResult = result as BadRequestObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            statusCodeResult.Value.Should().Be("Information of user provided is invalid");
        }
        [Fact]
        public async Task AddUserSubsciption_WhenSubsciptionPlanIdDontExist_ShouldReturnsBadREquest()
        {
            // Arrange
            var newUserSubsciption = new AddUserSubsciptionDto { };
            var userSubsciption = new UserSubscription { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult<bool>(true));
            A.CallTo(() => _subRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult<bool>(false));
            A.CallTo(() => _mapper.Map<UserSubscription>(newUserSubsciption)).Returns(userSubsciption);
            A.CallTo(() => _userSubRepo.Add(userSubsciption)).Returns(Task.FromResult<UserSubscription>(userSubsciption));

            // Act
            var result = await _controller.AddUserSubsciption(newUserSubsciption);

            // Assert
            var statusCodeResult = result as BadRequestObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            statusCodeResult.Value.Should().Be("Information of subscription plan provided is invalid");
        }

        [Fact]
        public async Task AddUserSubsciption_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newUserSubsciption = new AddUserSubsciptionDto { };
            var userSubsciption = new UserSubscription { };
            string messageException = "User SubScription existed";
            var message = "An error occurred while adding User Subscription into Database: " + messageException;

            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _subRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<UserSubscription>(newUserSubsciption)).Returns(userSubsciption);
            A.CallTo(() => _userSubRepo.Add(userSubsciption)).Throws(new Exception(messageException));


            // Act
            var result = await _controller.AddUserSubsciption(newUserSubsciption);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddUserSubsciption_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newUserSubsciption = new AddUserSubsciptionDto { };
            var userSubsciption = new UserSubscription { };
            var addedUserSubsciption = new UserSubscription { };
            A.CallTo(() => _userRepo.CheckContainsAsync(A<Expression<Func<User, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _subRepo.CheckContainsAsync(A<Expression<Func<SubscriptionPlan, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<UserSubscription>(newUserSubsciption)).Returns(userSubsciption);
            A.CallTo(() => _userSubRepo.Add(userSubsciption)).Returns(Task.FromResult<UserSubscription>(addedUserSubsciption));
            // Act
            var result = await _controller.AddUserSubsciption(newUserSubsciption);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionUserSubsciptionResponse;
            response.Message.Should().Be("User Subscription added successfully.");
            response.Id.Should().Be(addedUserSubsciption.Id);
        }




        #endregion

        #region UpdateUserSubsciption
        [Fact]
        public async Task UpdateUserSubsciption_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto();

            // Act
            var result = await controller.UpdateUserSubsciption(Guid.NewGuid(), updateUserSub);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task UpdateUserSubsciption_WhenUserSubscriptionPropertySubscriptionPlanIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "SubscriptionPlanId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenUserSubscriptionPropertySubscriptionPlanIdInvalidFormatGuid_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "SubscriptionPlanId";
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            string messageError = $"The value '{invalidGuid}' is not valid.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenUserSubscriptionPropertyStripeCurrentPeriodEndNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "StripeCurrentPeriodEnd";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenUserSubscriptionPropertyStripeCurrentPeriodEndnvalidFormatGuid_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "StripeCurrentPeriodEnd";
            string messageError = $"The JSON value could not be converted to System.DateTime.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenUserSubscriptionPropertyStripeCustomerIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "StripeCustomerId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenUserSubscriptionPropertyStripeSubscriptionIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "StripeSubscriptionId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenUserSubscriptionPropertyStripePriceIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "StripePriceId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenCustomerReviewPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            var updateUserSub = new UpdateUserSubsciptionDto { };
            var userSubId = Guid.NewGuid();
            string key = "id";
            string messageError = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.UpdateUserSubsciption(userSubId, updateUserSub);

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
        public async Task UpdateUserSubsciption_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var updateUserSub = new UpdateUserSubsciptionDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateUserSubsciption(differentId, updateUserSub);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("User Subscription ID information does not match");
        }

        [Fact]
        public async Task UpdateUserSubsciption_WhenUserSubsciptionDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var updateCustomerReview = new UpdateUserSubsciptionDto { Id = Guid.NewGuid() };
            A.CallTo(() => _userSubRepo.GetSingleByCondition(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Returns(Task.FromResult<UserSubscription>(null));

            // Act
            var result = await _controller.UpdateUserSubsciption(updateCustomerReview.Id, updateCustomerReview);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("User Subscription not found.");
        }
        [Fact]
        public async Task UpdateUserSubsciption_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var updateUserSub = new UpdateUserSubsciptionDto { Id = Guid.NewGuid() };
            var existUserSub = new UserSubscription { Id = updateUserSub.Id };
            A.CallTo(() => _userSubRepo.GetSingleByCondition(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existUserSub));
            A.CallTo(() => _mapper.Map(updateUserSub, existUserSub));
            A.CallTo(() => _userSubRepo.Update(existUserSub)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateUserSubsciption(updateUserSub.Id, updateUserSub);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionUserSubsciptionResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("User Subscription updated successfully.");
            response.Id.Should().Be(existUserSub.Id);
        }
        [Fact]
        public async Task UpdateUserSubsciption_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var updateUserSub = new UpdateUserSubsciptionDto { Id = Guid.NewGuid() };
            var existUserSub = new UserSubscription { Id = updateUserSub.Id };
            A.CallTo(() => _userSubRepo.GetSingleByCondition(A<Expression<Func<UserSubscription, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existUserSub));
            A.CallTo(() => _mapper.Map(updateUserSub, existUserSub));
            A.CallTo(() => _userSubRepo.Update(existUserSub)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateUserSubsciption(updateUserSub.Id, updateUserSub);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating user subscription: Update failed");
        }



        #endregion

        #region DeleteUserSubsciption
        [Fact]
        public async Task DeleteUserSubsciption_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new UserSubscriptionsController(null, null, null, null, _logger);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteUserSubsciption(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteUserSubsciption_WhenUserSubsciptionDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _userSubRepo.CheckContainsAsync(A<Expression<Func<UserSubscription, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteUserSubsciption(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Don't exist user subscription with id {id} to delete");
        }

        [Fact]
        public async Task DeleteUserSubsciption_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _userSubRepo.CheckContainsAsync(A<Expression<Func<UserSubscription, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _userSubRepo.DeleteMulti(A<Expression<Func<UserSubscription, bool>>>._)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteUserSubsciption(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionUserSubsciptionResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("User subscription deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DDeleteUserSubsciption_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _userSubRepo.CheckContainsAsync(A<Expression<Func<UserSubscription, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _userSubRepo.DeleteMulti(A<Expression<Func<UserSubscription, bool>>>._)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteUserSubsciption(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error deleting user subscription: Delete failed");
        }
        [Fact]
        public async Task DeleteUserSubsciption_WhenPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new UserSubscriptionsController(_userSubRepo, _userRepo, _subRepo, _mapper, _logger);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeleteUserSubsciption(Guid.NewGuid());

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

