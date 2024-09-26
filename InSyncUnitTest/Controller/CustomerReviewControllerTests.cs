using AutoMapper;
using InSyncAPI.Controllers;
using Repositorys;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InSyncAPI.Dtos;
using System.Linq.Expressions;

namespace InSyncUnitTest.Controller
{
    public class CustomerReviewControllerTests
    {
        private ICustomerReviewRepository _customerReviewRepo;
        private IMapper _mapper;
        private CustomerReviewsController _controller;
        public CustomerReviewControllerTests()
        {
            _customerReviewRepo = A.Fake<ICustomerReviewRepository>();
            _mapper = A.Fake<IMapper>();
            _controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
        }

        #region GetCustomerReviews
        [Fact]
        public async Task GetCustomerReviews_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CustomerReviewsController(null, null);

            // Act
            var result = await controller.GetCustomerReviews();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetCustomerReviews_WithCustomerReviews_ShouldReturnsOkResult()
        {
            // Arrange
            var customerReviews = new[] { new CustomerReview(), new CustomerReview() }.AsQueryable();
            A.CallTo(() => _customerReviewRepo.GetAll(A<string[]>._)).Returns(customerReviews);

            // Act
            var result = await _controller.GetCustomerReviews();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as IQueryable<CustomerReview>;
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(customerReviews);
            response.Should().HaveCount(2);
        }

        #endregion

        #region GetAllCustomerReview
        [Fact]
        public async Task GetAllCustomerReview_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new CustomerReviewsController(null, null);
            var result = await controller.GetAllCustomerReview();


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllCustomerReview_WithValidInput_ShouldReturnOkResultWithCustomerReviews()
        {
            // Arrange

            IEnumerable<CustomerReview> customerReviews = new List<CustomerReview> { new CustomerReview(), new CustomerReview() };
            IEnumerable<ViewCustomerReviewDto> viewCustomerReviews = new List<ViewCustomerReviewDto> { new ViewCustomerReviewDto(), new ViewCustomerReviewDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _customerReviewRepo.GetMultiPaging(A<Expression<Func<CustomerReview, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(customerReviews);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(customerReviews)).Returns(viewCustomerReviews);

            // Act
            var result = await _controller.GetAllCustomerReview(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCustomerReviews = okResult.Value as ResponsePaging<IEnumerable<ViewCustomerReviewDto>>;
            returnedCustomerReviews.Should().NotBeNull();
            returnedCustomerReviews.data.Should().BeEquivalentTo(viewCustomerReviews);
        }
        [Fact]
        public async Task GetAllCustomerReview_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithCustomerReviews()
        {
            // Arrange

            IEnumerable<CustomerReview> custoemrReviews = new List<CustomerReview> { new CustomerReview(), new CustomerReview() };
            IEnumerable<ViewCustomerReviewDto> viewCustomerReviews = new List<ViewCustomerReviewDto> { new ViewCustomerReviewDto(), new ViewCustomerReviewDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _customerReviewRepo.GetMultiPaging(A<Expression<Func<CustomerReview, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(custoemrReviews);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(custoemrReviews)).Returns(viewCustomerReviews);

            // Act
            var result = await _controller.GetAllCustomerReview(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCustomerReviews = okResult.Value as ResponsePaging<IEnumerable<ViewCustomerReviewDto>>;
            returnedCustomerReviews.Should().NotBeNull();
            returnedCustomerReviews.data.Should().BeEquivalentTo(viewCustomerReviews);
        }

        [Fact]
        public async Task GetAllCustomerReview_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithCustomerReviews()
        {
            // Arrange

            IEnumerable<CustomerReview> custoemrReviews = new List<CustomerReview> { new CustomerReview(), new CustomerReview() };
            IEnumerable<ViewCustomerReviewDto> viewCustomerReviews = new List<ViewCustomerReviewDto> { new ViewCustomerReviewDto(), new ViewCustomerReviewDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _customerReviewRepo.GetMultiPaging(A<Expression<Func<CustomerReview, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(custoemrReviews);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(custoemrReviews)).Returns(viewCustomerReviews);

            // Act
            var result = await _controller.GetAllCustomerReview();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCustomerReviews = okResult.Value as ResponsePaging< IEnumerable<ViewCustomerReviewDto>>;
            returnedCustomerReviews.Should().NotBeNull();
            returnedCustomerReviews.data.Should().BeEquivalentTo(viewCustomerReviews);
        }


        #endregion

        #region GetCustomerReviewById
        [Fact]
        public async Task GetCustomerReviewById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new CustomerReviewsController(null, null);
            //Act
            var result = await controller.GetCustomerReviewById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetCustomerReviewById_WhenCustomerReviewDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var customerReviewId = Guid.NewGuid();
            A.CallTo(() => _customerReviewRepo.GetSingleByCondition(A<Expression<Func<CustomerReview, bool>>>._, A<string[]>._)).Returns(Task.FromResult<CustomerReview>(null));

            // Act
            var result = await _controller.GetCustomerReviewById(customerReviewId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No review has an ID : {customerReviewId}");
        }

        [Fact]
        public async Task GetCustomerReviewById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetCustomerReviewById(Guid.Empty);

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
        public async Task GetCustomerReviewById_WithIdExist_ShouldReturnOkResultWithCustomerReview()
        {
            // Arrange
            var customerReviewId = Guid.NewGuid();
            var customerReview = new CustomerReview();
            var viewCustomerReview = new ViewCustomerReviewDto();

            A.CallTo(() => _customerReviewRepo.GetSingleByCondition(A<Expression<Func<CustomerReview, bool>>>._, A<string[]>._)).Returns(Task.FromResult(customerReview));
            A.CallTo(() => _mapper.Map<ViewCustomerReviewDto>(customerReview)).Returns(viewCustomerReview);

            // Act
            var result = await _controller.GetCustomerReviewById(customerReviewId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewCustomerReviewDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewCustomerReview);
        }

        #endregion

        #region AddCustomerReview
        [Fact]
        public async Task AddCustomerReview_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CustomerReviewsController(null, null);
            var newCustomerReview = new AddCustomerReviewDto();

            // Act
            var result = await controller.AddCustomerReview(newCustomerReview);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddCustomerReview_WhenCustomerReviewPropertyNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new AddCustomerReviewDto { };
            controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await controller.AddCustomerReview(newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Name");
            errorResponse.Errors["Name"].Should().Contain("The Name field is required.");
        }
        public async Task AddCustomerReview_WhenCustomerReviewPropertyNameLengthLonger100_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new AddCustomerReviewDto { };
            string messageError = "The field Name must be a string with a maximum length of 100.";
            controller.ModelState.AddModelError("Name", messageError);

            // Act
            var result = await controller.AddCustomerReview(newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Name");
            errorResponse.Errors["Name"].Should().Contain(messageError);
        }
        [Fact]
        public async Task AddCustomerReview_WhenCustomerReviewPropertyJobTitleNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new AddCustomerReviewDto { };
            string message = "The JobTitle field is required.";
            string property = "JobTitle";
            controller.ModelState.AddModelError(property, message);

            // Act
            var result = await controller.AddCustomerReview(newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(message);
        }
        [Fact]
        public async Task AddCustomerReview_WhenCustomerReviewPropertyJobTitleLengthLonger100_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new AddCustomerReviewDto { };
            string property = "JobTitle";
            string messageError = "The field JobTitle must be a string with a maximum length of 150.";
            controller.ModelState.AddModelError(property, messageError);

            // Act
            var result = await controller.AddCustomerReview(newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(messageError);
        }

        [Fact]
        public async Task AddCustomerReview_WhenCustomerReviewPropertyReviewNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new AddCustomerReviewDto { };
            string message = "The Review field is required.";
            string property = "Review";
            controller.ModelState.AddModelError(property, message);

            // Act
            var result = await controller.AddCustomerReview(newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(message);
        }
        [Fact]
        public async Task AddCustomerReview_WhenCustomerReviewPropertyReviewLengthLonger200_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new AddCustomerReviewDto { };
            string property = "Review";
            string messageError = "The field Review must be a string with a maximum length of 200.";
            controller.ModelState.AddModelError(property, messageError);

            // Act
            var result = await controller.AddCustomerReview(newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(messageError);
        }


        [Fact]
        public async Task AddCustomerReview_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newCustomerReview = new AddCustomerReviewDto { };
            var customerReview = new CustomerReview { };
            A.CallTo(() => _mapper.Map<CustomerReview>(newCustomerReview)).Returns(customerReview);
            A.CallTo(() => _customerReviewRepo.Add(customerReview)).Returns(Task.FromResult<CustomerReview>(null));

            // Act
            var result = await _controller.AddCustomerReview(newCustomerReview);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the customer review.");
        }
        [Fact]
        public async Task AddCustomerReview_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newCustomerReview = new AddCustomerReviewDto { };
            var customerReview = new CustomerReview { };
            var message = "An error occurred while adding Customer Review into Database";
            A.CallTo(() => _mapper.Map<CustomerReview>(newCustomerReview)).Returns(customerReview);
            A.CallTo(() => _customerReviewRepo.Add(customerReview)).Throws(new Exception(message));

            // Act
            var result = await _controller.AddCustomerReview(newCustomerReview);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddCustomerReview_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newCustomerReview = new AddCustomerReviewDto { };
            var customerReview = new CustomerReview { };
            var addedCustomerReview = new CustomerReview { };
            A.CallTo(() => _mapper.Map<CustomerReview>(newCustomerReview)).Returns(customerReview);
            A.CallTo(() => _customerReviewRepo.Add(customerReview)).Returns(Task.FromResult(addedCustomerReview));

            // Act
            var result = await _controller.AddCustomerReview(newCustomerReview);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionCustomerReviewResponse;
            response.Message.Should().Be("Customer review added successfully.");
            response.Id.Should().Be(addedCustomerReview.Id);
        }




        #endregion

        #region UdpateCustomerReview
        [Fact]
        public async Task UpdatCustomerReviewCustomerReview_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CustomerReviewsController(null, null);
            var updateCustomerReview = new UpdateCustomerReviewDto();

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), updateCustomerReview);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task UpdateCustomerReview_WhenCustomerReviewPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var updateCustomerReview = new UpdateCustomerReviewDto();
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), updateCustomerReview);

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
        public async Task UpdateCustomerReview_WhenCustomerReviewPropertyNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var updateCustomerReview = new UpdateCustomerReviewDto { };
            controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), updateCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Name");
            errorResponse.Errors["Name"].Should().Contain("The Name field is required.");
        }
        [Fact]
        public async Task UpdateCustomerReview_WhenCustomerReviewPropertyNameLengthLonger100_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new UpdateCustomerReviewDto { };
            string messageError = "The field Name must be a string with a maximum length of 100.";
            controller.ModelState.AddModelError("Name", messageError);

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Name");
            errorResponse.Errors["Name"].Should().Contain(messageError);
        }
        [Fact]
        public async Task UpdateCustomerReview_WhenCustomerReviewPropertyJobTitleNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new UpdateCustomerReviewDto { };
            string message = "The JobTitle field is required.";
            string property = "JobTitle";
            controller.ModelState.AddModelError(property, message);

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(message);
        }
        [Fact]
        public async Task UpdateCustomerReview_WhenCustomerReviewPropertyJobTitleLengthLonger100_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new UpdateCustomerReviewDto { };
            string property = "JobTitle";
            string messageError = "The field JobTitle must be a string with a maximum length of 150.";
            controller.ModelState.AddModelError(property, messageError);

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(messageError);
        }

        
        [Fact]
        public async Task UpdateCustomerReview_WhenCustomerReviewPropertyReviewNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new UpdateCustomerReviewDto { };
            string message = "The Review field is required.";
            string property = "Review";
            controller.ModelState.AddModelError(property, message);

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(message);
        }
        [Fact]
        public async Task UpdateCustomerReview_WhenCustomerReviewPropertyReviewLengthLonger200_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            var newCustomerReview = new UpdateCustomerReviewDto { };
            string property = "Review";
            string messageError = "The field Review must be a string with a maximum length of 200.";
            controller.ModelState.AddModelError(property, messageError);

            // Act
            var result = await controller.UpdateCustomerReview(Guid.NewGuid(), newCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey(property);
            errorResponse.Errors[property].Should().Contain(messageError);
        }




        [Fact]
        public async Task UpdateCustomerReview_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var updateCustomerReview = new UpdateCustomerReviewDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateCustomerReview(differentId, updateCustomerReview);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Review ID information does not match");
        }

        [Fact]
        public async Task UpdateCustomerReview_WhenCustomerReviewDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var updateCustomerReview = new UpdateCustomerReviewDto { Id = Guid.NewGuid() };
            A.CallTo(() => _customerReviewRepo.GetSingleByCondition(A<Expression<Func<CustomerReview, bool>>>._, A<string[]>._)).Returns(Task.FromResult<CustomerReview>(null));

            // Act
            var result = await _controller.UpdateCustomerReview(updateCustomerReview.Id, updateCustomerReview);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Customer review not found.");
        }
        [Fact]
        public async Task UpdateCustomerReview_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var updateCustomerReview = new UpdateCustomerReviewDto { Id = Guid.NewGuid() };
            var existCustomerReview = new CustomerReview { Id = updateCustomerReview.Id };
            A.CallTo(() => _customerReviewRepo.GetSingleByCondition(A<Expression<Func<CustomerReview, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existCustomerReview));
            A.CallTo(() => _mapper.Map(updateCustomerReview, existCustomerReview));
            A.CallTo(() => _customerReviewRepo.Update(existCustomerReview)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateCustomerReview(updateCustomerReview.Id, updateCustomerReview);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionCustomerReviewResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Customer review updated successfully.");
            response.Id.Should().Be(existCustomerReview.Id);
        }
        [Fact]
        public async Task UpdateCustomerReview_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var updateCustomerReview = new UpdateCustomerReviewDto { Id = Guid.NewGuid() };
            var existCustomerReview = new CustomerReview { Id = updateCustomerReview.Id };
            A.CallTo(() => _customerReviewRepo.GetSingleByCondition(A<Expression<Func<CustomerReview, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existCustomerReview));
            A.CallTo(() => _mapper.Map(updateCustomerReview, existCustomerReview));
            A.CallTo(() => _customerReviewRepo.Update(existCustomerReview)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateCustomerReview(updateCustomerReview.Id, updateCustomerReview);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating customer review: Update failed");
        }


        #endregion
        #region DeleteCustomerReview
        [Fact]
        public async Task DeleteCustomerReview_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CustomerReviewsController(null, null);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteCustomerReview(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteCustomerReview_WhenCustomerReviewDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _customerReviewRepo.CheckContainsAsync(A<Expression<Func<CustomerReview, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteCustomerReview(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Dont exist review with id {id.ToString()} to delete");

        }

        [Fact]
        public async Task DeleteCustomerReview_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _customerReviewRepo.CheckContainsAsync(A<Expression<Func<CustomerReview, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _customerReviewRepo.DeleteMulti(A<Expression<Func<CustomerReview, bool>>>._)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCustomerReview(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionCustomerReviewResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Customer review deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DeleteCustomerReview_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _customerReviewRepo.CheckContainsAsync(A<Expression<Func<CustomerReview, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _customerReviewRepo.DeleteMulti(A<Expression<Func<CustomerReview, bool>>>._)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteCustomerReview(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error delete Customer Review: Delete failed");
        }
        [Fact]
        public async Task DeleteCustomerReview_WhenCustomerReviewPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CustomerReviewsController(_customerReviewRepo, _mapper);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeleteCustomerReview(Guid.NewGuid());

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
