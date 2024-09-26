using AutoMapper;
using BusinessObjects.Models;
using FakeItEasy;
using FluentAssertions;
using InSyncAPI.Controllers;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InSyncUnitTest.Controller
{
    public class PrivacyPolicyControllerTest
    {
        private IPrivacyPolicyRepository _privacyPolicyRepo;
        private IMapper _mapper;
        private PrivacyPolicysController _controller;
        public PrivacyPolicyControllerTest()
        {
            _privacyPolicyRepo = A.Fake<IPrivacyPolicyRepository>();
            _mapper = A.Fake<IMapper>();
            _controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
        }

        #region GetPrivacyPolicys
        [Fact]
        public async Task GetPrivacyPolicys_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new PrivacyPolicysController(null, null);

            // Act
            var result = await controller.GetPrivacyPolicys();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetPrivacyPolicys_WithPrivacyPolicys_ShouldReturnsOkResult()
        {
            // Arrange
            var privacyPolicys = new[] { new PrivacyPolicy(), new PrivacyPolicy() }.AsQueryable();
            A.CallTo(() => _privacyPolicyRepo.GetAll(A<string[]>._)).Returns(privacyPolicys);

            // Act
            var result = await _controller.GetPrivacyPolicys();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as IQueryable<PrivacyPolicy>;
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(privacyPolicys);
            response.Should().HaveCount(2);
        }

        #endregion

        #region GetAllPrivacyPolicy
        [Fact]
        public async Task GetAllPrivacyPolicy_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new PrivacyPolicysController(null, null);
            var result = await controller.GetAllPrivacyPolicy();


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllPrivacyPolicy_WithValidInput_ShouldReturnOkResultWithPrivacyPolicys()
        {
            // Arrange

            IEnumerable<PrivacyPolicy> privacyPolicys = new List<PrivacyPolicy> { new PrivacyPolicy(), new PrivacyPolicy() };
            IEnumerable<ViewPrivacyPolicyDto> viewPrivacyPolicys = new List<ViewPrivacyPolicyDto> { new ViewPrivacyPolicyDto(), new ViewPrivacyPolicyDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _privacyPolicyRepo.GetMultiPaging(A<Expression<Func<PrivacyPolicy, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(privacyPolicys);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewPrivacyPolicyDto>>(privacyPolicys)).Returns(viewPrivacyPolicys);

            // Act
            var result = await _controller.GetAllPrivacyPolicy(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedPrivacyPolicys = okResult.Value as ResponsePaging<IEnumerable<ViewPrivacyPolicyDto>>;
            returnedPrivacyPolicys.Should().NotBeNull();
            returnedPrivacyPolicys.data.Should().BeEquivalentTo(viewPrivacyPolicys);
        }
        [Fact]
        public async Task GetAllPrivacyPolicy_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithPrivacyPolicys()
        {
            // Arrange

            IEnumerable<PrivacyPolicy> privacyPolicys = new List<PrivacyPolicy> { new PrivacyPolicy(), new PrivacyPolicy() };
            IEnumerable<ViewPrivacyPolicyDto> viewPrivacyPolicys = new List<ViewPrivacyPolicyDto> { new ViewPrivacyPolicyDto(), new ViewPrivacyPolicyDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _privacyPolicyRepo.GetMultiPaging(A<Expression<Func<PrivacyPolicy, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(privacyPolicys);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewPrivacyPolicyDto>>(privacyPolicys)).Returns(viewPrivacyPolicys);

            // Act
            var result = await _controller.GetAllPrivacyPolicy(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedPrivacyPolicys = okResult.Value as ResponsePaging<IEnumerable<ViewPrivacyPolicyDto>>;
            returnedPrivacyPolicys.Should().NotBeNull();
            returnedPrivacyPolicys.data.Should().BeEquivalentTo(viewPrivacyPolicys);
        }

        [Fact]
        public async Task GetAllPrivacyPolicy_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithPrivacyPolicys()
        {
            // Arrange

            IEnumerable<PrivacyPolicy> privacyPolicys = new List<PrivacyPolicy> { new PrivacyPolicy(), new PrivacyPolicy() };
            IEnumerable<ViewPrivacyPolicyDto> viewPrivacyPolicys = new List<ViewPrivacyPolicyDto> { new ViewPrivacyPolicyDto(), new ViewPrivacyPolicyDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _privacyPolicyRepo.GetMultiPaging(A<Expression<Func<PrivacyPolicy, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(privacyPolicys);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewPrivacyPolicyDto>>(privacyPolicys)).Returns(viewPrivacyPolicys);

            // Act
            var result = await _controller.GetAllPrivacyPolicy();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedPrivacyPolicys = okResult.Value as ResponsePaging<IEnumerable<ViewPrivacyPolicyDto>>;
            returnedPrivacyPolicys.Should().NotBeNull();
            returnedPrivacyPolicys.data.Should().BeEquivalentTo(viewPrivacyPolicys);
        }


        #endregion

        #region GetPrivacyPolicyById
        [Fact]
        public async Task GetPrivacyPolicyById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new PrivacyPolicysController(null, null);
            //Act
            var result = await controller.GetPrivacyPolicyById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetPrivacyPolicyById_WhenPrivacyPolicyDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var privacyPolicyId = Guid.NewGuid();
            A.CallTo(() => _privacyPolicyRepo.GetSingleByCondition(A<Expression<Func<PrivacyPolicy, bool>>>._, A<string[]>._)).Returns(Task.FromResult<PrivacyPolicy>(null));

            // Act
            var result = await _controller.GetPrivacyPolicyById(privacyPolicyId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No privacy policy has an ID : {privacyPolicyId}");
        }

        [Fact]
        public async Task GetPrivacyPolicyById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetPrivacyPolicyById(Guid.Empty);

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
        public async Task GetPrivacyPolicyById_WithIdExist_ShouldReturnOkResultWithprivacyPolicy()
        {
            // Arrange
            var privacyPolicyId = Guid.NewGuid();
            var privacyPolicy = new PrivacyPolicy();
            var viewprivacyPolicy = new ViewPrivacyPolicyDto();

            A.CallTo(() => _privacyPolicyRepo.GetSingleByCondition(A<Expression<Func<PrivacyPolicy, bool>>>._, A<string[]>._)).Returns(Task.FromResult(privacyPolicy));
            A.CallTo(() => _mapper.Map<ViewPrivacyPolicyDto>(privacyPolicy)).Returns(viewprivacyPolicy);

            // Act
            var result = await _controller.GetPrivacyPolicyById(privacyPolicyId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewPrivacyPolicyDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewprivacyPolicy);
        }

        #endregion

        #region AddPrivacyPolicy
        [Fact]
        public async Task AddPrivacyPolicy_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new PrivacyPolicysController(null, null);
            var newPrivacyPolicy = new AddPrivacyPolicyDto();

            // Act
            var result = await controller.AddPrivacyPolicy(newPrivacyPolicy);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddPrivacyPolicy_WhenPrivacyPolicyPropertyNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
            var newPrivacyPolicy = new AddPrivacyPolicyDto { };
            string key = "Title";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddPrivacyPolicy(newPrivacyPolicy);

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
        public async Task AddPrivacyPolicy_WhenPrivacyPolicyPropertyNameLengthLonger300_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
            var newPrivacyPolicy = new AddPrivacyPolicyDto { };
            string key = "Title";
            string messageError = $"The field {key} must be a string with a maximum length of 100.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddPrivacyPolicy(newPrivacyPolicy);

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
        public async Task AddPrivacyPolicy_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newPrivacyPolicy = new AddPrivacyPolicyDto { };
            var PrivacyPolicy = new PrivacyPolicy { };
            A.CallTo(() => _mapper.Map<PrivacyPolicy>(newPrivacyPolicy)).Returns(PrivacyPolicy);
            A.CallTo(() => _privacyPolicyRepo.Add(PrivacyPolicy)).Returns(Task.FromResult<PrivacyPolicy>(null));

            // Act
            var result = await _controller.AddPrivacyPolicy(newPrivacyPolicy);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the private policy.");
        }
        [Fact]
        public async Task AddPrivacyPolicy_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newPrivacyPolicy = new AddPrivacyPolicyDto { };
            var PrivacyPolicy = new PrivacyPolicy { };
            string messageException = "Privacy existed";
            var message = "An error occurred while adding privacy policy into Database " + messageException;
            
            A.CallTo(() => _mapper.Map<PrivacyPolicy>(newPrivacyPolicy)).Returns(PrivacyPolicy);
            A.CallTo(() => _privacyPolicyRepo.Add(PrivacyPolicy)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddPrivacyPolicy(newPrivacyPolicy);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddPrivacyPolicy_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newPrivacyPolicy = new AddPrivacyPolicyDto { };
            var PrivacyPolicy = new PrivacyPolicy { };
            var addedPrivacyPolicy = new PrivacyPolicy { };
            A.CallTo(() => _mapper.Map<PrivacyPolicy>(newPrivacyPolicy)).Returns(PrivacyPolicy);
            A.CallTo(() => _privacyPolicyRepo.Add(PrivacyPolicy)).Returns(Task.FromResult(addedPrivacyPolicy));

            // Act
            var result = await _controller.AddPrivacyPolicy(newPrivacyPolicy);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionPrivacyPolicyResponse;
            response.Message.Should().Be("Private policy added successfully.");
            response.Id.Should().Be(addedPrivacyPolicy.Id);
        }




        #endregion

        #region UdpatePrivacyPolicy
        [Fact]
        public async Task UpdatPrivacyPolicyPrivacyPolicy_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new PrivacyPolicysController(null, null);
            var updatePrivacyPolicy = new UpdatePrivacyPolicyDto();

            // Act
            var result = await controller.UpdatePrivacyPolicy(Guid.NewGuid(), updatePrivacyPolicy);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task UpdatePrivacyPolicy_WhenPrivacyPolicyPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
            var updatePrivacyPolicy = new UpdatePrivacyPolicyDto();
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.UpdatePrivacyPolicy(Guid.NewGuid(), updatePrivacyPolicy);

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
        public async Task UpdatePrivacyPolicy_WhenPrivacyPolicyPropertyTitleNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
            var updatePrivacyPolicy = new UpdatePrivacyPolicyDto { };
            string key = "Title";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdatePrivacyPolicy(Guid.NewGuid(), updatePrivacyPolicy);

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
        public async Task UpdatePrivacyPolicy_WhenPrivacyPolicyPropertyTitleLengthLonger300_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
            var newPrivacyPolicy = new UpdatePrivacyPolicyDto { };
            string key = "Title";
            string messageError = $"The field {key} must be a string with a maximum length of 300.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.UpdatePrivacyPolicy(Guid.NewGuid(), newPrivacyPolicy);

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
        public async Task UpdatePrivacyPolicy_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var updatePrivacyPolicy = new UpdatePrivacyPolicyDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdatePrivacyPolicy(differentId, updatePrivacyPolicy);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Privacy Policy ID information does not match");
        }

        [Fact]
        public async Task UpdatePrivacyPolicy_WhenPrivacyPolicyDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var updatePrivacyPolicy = new UpdatePrivacyPolicyDto { Id = Guid.NewGuid() };
            A.CallTo(() => _privacyPolicyRepo.GetSingleByCondition(A<Expression<Func<PrivacyPolicy, bool>>>._, A<string[]>._)).Returns(Task.FromResult<PrivacyPolicy>(null));

            // Act
            var result = await _controller.UpdatePrivacyPolicy(updatePrivacyPolicy.Id, updatePrivacyPolicy);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Privacy policy not found.");
        }
        [Fact]
        public async Task UpdatePrivacyPolicy_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var updatePrivacyPolicy = new UpdatePrivacyPolicyDto { Id = Guid.NewGuid() };
            var existPrivacyPolicy = new PrivacyPolicy { Id = updatePrivacyPolicy.Id };
            A.CallTo(() => _privacyPolicyRepo.GetSingleByCondition(A<Expression<Func<PrivacyPolicy, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existPrivacyPolicy));
            A.CallTo(() => _mapper.Map(updatePrivacyPolicy, existPrivacyPolicy));
            A.CallTo(() => _privacyPolicyRepo.Update(existPrivacyPolicy)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdatePrivacyPolicy(updatePrivacyPolicy.Id, updatePrivacyPolicy);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionPrivacyPolicyResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Privacy policy updated successfully.");
            response.Id.Should().Be(existPrivacyPolicy.Id);
        }
        [Fact]
        public async Task UpdatePrivacyPolicy_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var updatePrivacyPolicy = new UpdatePrivacyPolicyDto { Id = Guid.NewGuid() };
            var existPrivacyPolicy = new PrivacyPolicy { Id = updatePrivacyPolicy.Id };
            A.CallTo(() => _privacyPolicyRepo.GetSingleByCondition(A<Expression<Func<PrivacyPolicy, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existPrivacyPolicy));
            A.CallTo(() => _mapper.Map(updatePrivacyPolicy, existPrivacyPolicy));
            A.CallTo(() => _privacyPolicyRepo.Update(existPrivacyPolicy)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdatePrivacyPolicy(updatePrivacyPolicy.Id, updatePrivacyPolicy);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating privacy policy: Update failed");
        }


        #endregion
        #region DeletePrivacyPolicy
        [Fact]
        public async Task DeletePrivacyPolicy_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new PrivacyPolicysController(null, null);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeletePrivacyPolicy(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeletePrivacyPolicy_WhenPrivacyPolicyDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _privacyPolicyRepo.CheckContainsAsync(A<Expression<Func<PrivacyPolicy, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeletePrivacyPolicy(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Dont exist privacy policy with id {id.ToString()} to delete");

        }

        [Fact]
        public async Task DeletePrivacyPolicy_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _privacyPolicyRepo.CheckContainsAsync(A<Expression<Func<PrivacyPolicy, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _privacyPolicyRepo.DeleteMulti(A<Expression<Func<PrivacyPolicy, bool>>>._)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeletePrivacyPolicy(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionPrivacyPolicyResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Privacy policy deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DeletePrivacyPolicy_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _privacyPolicyRepo.CheckContainsAsync(A<Expression<Func<PrivacyPolicy, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _privacyPolicyRepo.DeleteMulti(A<Expression<Func<PrivacyPolicy, bool>>>._)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeletePrivacyPolicy(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error delete term: Delete failed");
        }
        [Fact]
        public async Task DeletePrivacyPolicy_WhenPrivacyPolicyPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PrivacyPolicysController(_privacyPolicyRepo, _mapper);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeletePrivacyPolicy(Guid.NewGuid());

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
