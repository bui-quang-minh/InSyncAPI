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
using static InSyncAPI.Dtos.CategoryDocumentDto;
using static InSyncAPI.Dtos.DocumentDto;

namespace InSyncUnitTest.Controller
{
    public class CategoryDocumentControllerTest
    {
        private ICategoryDocumentRepository _cateRepo;
        private IMapper _mapper;
        private ILogger<CategoryDocumentController> _logger;
        private CategoryDocumentController _controller;
        public CategoryDocumentControllerTest()
        {
            _cateRepo = A.Fake<ICategoryDocumentRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<CategoryDocumentController>>();
            _controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
        }

        #region GetCategoryDocuments
        [Fact]
        public async Task GetCategoryDocuments_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CategoryDocumentController(null, null, _logger);

            // Act
            var result = await controller.GetCategoryDocuments();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetCategoryDocuments_WithPrivacyPolicys_ShouldReturnsOkResult()
        {
            // Arrange
            var categoryDocuments = new[] { new CategoryDocument(), new CategoryDocument() }.AsQueryable();
            A.CallTo(() => _cateRepo.GetAll(A<string[]>._)).Returns(categoryDocuments);

            // Act
            var result = await _controller.GetCategoryDocuments();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as IQueryable<CategoryDocument>;
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(categoryDocuments);
            response.Should().HaveCount(2);
        }
        [Fact]
        public async Task GetCategoryDocuments_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving category documents.";
            var response = $"Error retrieving category documents: {message}";

            A.CallTo(() => _cateRepo.GetAll(A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetCategoryDocuments();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region GetAllCategoryDocument
        [Fact]
        public async Task GetAllCategoryDocument_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new CategoryDocumentController(null, null, _logger);
            var result = await controller.GetAllCategoryDocument(0, 2, "");


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllCategoryDocument_WithValidInput_ShouldReturnOkResultWithPrivacyPolicys()
        {
            // Arrange

            IEnumerable<CategoryDocument> categoryDocuments = new List<CategoryDocument> { new CategoryDocument(), new CategoryDocument() };
            IEnumerable<ViewCategoryDocumentDto> viewCategoryDocuments = new List<ViewCategoryDocumentDto> { new ViewCategoryDocumentDto(), new ViewCategoryDocumentDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _cateRepo.GetMultiPaging(A<Expression<Func<CategoryDocument, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(categoryDocuments);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewCategoryDocumentDto>>(categoryDocuments)).Returns(viewCategoryDocuments);

            // Act
            var result = await _controller.GetAllCategoryDocument(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedPrivacyPolicys = okResult.Value as ResponsePaging<IEnumerable<ViewCategoryDocumentDto>>;
            returnedPrivacyPolicys.Should().NotBeNull();
            returnedPrivacyPolicys.data.Should().BeEquivalentTo(viewCategoryDocuments);
        }
        [Fact]
        public async Task GetAllCategoryDocument_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithPrivacyPolicys()
        {
            // Arrange

            IEnumerable<CategoryDocument> categoryDocuments = new List<CategoryDocument> { new CategoryDocument(), new CategoryDocument() };
            IEnumerable<ViewCategoryDocumentDto> viewCategoryDocuments = new List<ViewCategoryDocumentDto> { new ViewCategoryDocumentDto(), new ViewCategoryDocumentDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _cateRepo.GetMultiPaging(A<Expression<Func<CategoryDocument, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(categoryDocuments);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewCategoryDocumentDto>>(categoryDocuments)).Returns(viewCategoryDocuments);

            // Act
            var result = await _controller.GetAllCategoryDocument(index, size, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedPrivacyPolicys = okResult.Value as ResponsePaging<IEnumerable<ViewCategoryDocumentDto>>;
            returnedPrivacyPolicys.Should().NotBeNull();
            returnedPrivacyPolicys.data.Should().BeEquivalentTo(viewCategoryDocuments);
        }

        [Fact]
        public async Task GetAllCategoryDocument_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithPrivacyPolicys()
        {
            // Arrange

            IEnumerable<CategoryDocument> categoryDocuments = new List<CategoryDocument> { new CategoryDocument(), new CategoryDocument() };
            IEnumerable<ViewCategoryDocumentDto> viewCategoryDocuments = new List<ViewCategoryDocumentDto> { new ViewCategoryDocumentDto(), new ViewCategoryDocumentDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _cateRepo.GetMulti(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._))
                .Returns(categoryDocuments);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewCategoryDocumentDto>>(categoryDocuments)).Returns(viewCategoryDocuments);

            // Act
            var result = await _controller.GetAllCategoryDocument(null, null, "");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedPrivacyPolicys = okResult.Value as ResponsePaging<IEnumerable<ViewCategoryDocumentDto>>;
            returnedPrivacyPolicys.Should().NotBeNull();
            returnedPrivacyPolicys.data.Should().BeEquivalentTo(viewCategoryDocuments);
        }
        [Fact]
        public async Task GetAllPrivacyPolicy_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving category documents.";
            var response = $"Error retrieving category documents: {message}";

            A.CallTo(() => _cateRepo.GetMulti(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllCategoryDocument(null, null, "");

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }


        #endregion

        #region GetCategoryDocumentById
        [Fact]
        public async Task GetCategoryDocumentById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new CategoryDocumentController(null, null, _logger);
            //Act
            var result = await controller.GetCategoryDocumentById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetCategoryDocumentById_WhenPrivacyPolicyDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var categoryDocumentId = Guid.NewGuid();
            A.CallTo(() => _cateRepo.GetSingleByCondition(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._)).Returns(Task.FromResult<CategoryDocument>(null));

            // Act
            var result = await _controller.GetCategoryDocumentById(categoryDocumentId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No Category Document has an ID: {categoryDocumentId}");
        }

        [Fact]
        public async Task GetCategoryDocumentById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetCategoryDocumentById(Guid.Empty);

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
        public async Task GetCategoryDocumentById_WithIdExist_ShouldReturnOkResultWithprivacyPolicy()
        {
            // Arrange
            var categoryDocumentId = Guid.NewGuid();
            var CategoryDocument = new CategoryDocument { Documents = new List<Document>() };
            var viewCategoryDocument = new ViewCategoryDocumentDto { Documents = new List<ViewDocumentDto>()};

            A.CallTo(() => _cateRepo.GetSingleByCondition(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._)).Returns(Task.FromResult(CategoryDocument));
            A.CallTo(() => _mapper.Map<ViewCategoryDocumentDto>(CategoryDocument)).Returns(viewCategoryDocument);

            // Act
            var result = await _controller.GetCategoryDocumentById(categoryDocumentId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewCategoryDocumentDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewCategoryDocument);
        }

        [Fact]
        public async Task GetCategoryDocumentById_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving category documents.";
            var response = $"Error retrieving Category Document: {message}";

            A.CallTo(() => _cateRepo.GetSingleByCondition(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetCategoryDocumentById(Guid.NewGuid());

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(response);
        }
        #endregion

        #region AddCategoryDocument
        [Fact]
        public async Task AddCategoryDocument_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CategoryDocumentController(null, null, _logger);
            var newCategoryDocument = new AddCategoryDocumentDto();

            // Act
            var result = await controller.AddCategoryDocument(newCategoryDocument);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddCategoryDocument_WhenCategoryDocumentPropertyTitleNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var newCategoryDocument = new AddCategoryDocumentDto { };
            string key = "Title";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddCategoryDocument(newCategoryDocument);

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
        public async Task AddCategoryDocument_WhenCategoryDocumentPropertyTitleLengthLonger500_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var newCategoryDocument = new AddCategoryDocumentDto { };
            string key = "Title";
            string messageError = $"The field {key} must be a string with a maximum length of 500.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddCategoryDocument(newCategoryDocument);

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
        public async Task AddCategoryDocument_WhenCategoryDocumentPropertyOrderNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var newCategoryDocument = new AddCategoryDocumentDto { };
            string key = "Order";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddCategoryDocument(newCategoryDocument);

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
        public async Task AddCategoryDocument_WhenCategoryDocumentPropertyOrderOutOfRange0toIntMaxValue_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var newCategoryDocument = new AddCategoryDocumentDto { };
            string key = "Order";
            string messageError = $"The field {key} must be in range 0 to 2147483647.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddCategoryDocument(newCategoryDocument);

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
        public async Task AddCategoryDocument_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newCategoryDocument = new AddCategoryDocumentDto { };
            var CategoryDocument = new CategoryDocument { };
            A.CallTo(() => _mapper.Map<CategoryDocument>(newCategoryDocument)).Returns(CategoryDocument);
            A.CallTo(() => _cateRepo.Add(CategoryDocument)).Returns(Task.FromResult<CategoryDocument>(null));

            // Act
            var result = await _controller.AddCategoryDocument(newCategoryDocument);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the Category Document.");
        }
        [Fact]
        public async Task AddCategoryDocument_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newCategoryDocument = new AddCategoryDocumentDto { };
            var CategoryDocument = new CategoryDocument { };
            string messageException = "Category Document existed";
            var message = "An error occurred while adding category document into Database: " + messageException;

            A.CallTo(() => _mapper.Map<CategoryDocument>(newCategoryDocument)).Returns(CategoryDocument);
            A.CallTo(() => _cateRepo.Add(CategoryDocument)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddCategoryDocument(newCategoryDocument);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddCategoryDocument_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newCategoryDocument = new AddCategoryDocumentDto { };
            var addedCategory = new CategoryDocument { };
            A.CallTo(() => _mapper.Map<CategoryDocument>(newCategoryDocument)).Returns(addedCategory);
            A.CallTo(() => _cateRepo.Add(addedCategory)).Returns(Task.FromResult(addedCategory));

            // Act
            var result = await _controller.AddCategoryDocument(newCategoryDocument);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionCategoryDocumentResponse;
            response.Message.Should().Be("Category Document added successfully.");
            response.Id.Should().Be(addedCategory.Id);
        }




        #endregion

        #region UpdateCategoryDocument
        [Fact]
        public async Task UpdateCategoryDocument_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CategoryDocumentController(null, null, _logger);
            var UpdateCategoryDocument = new UpdateCategoryDocumentDto();

            // Act
            var result = await controller.UpdateCategoryDocument(Guid.NewGuid(), UpdateCategoryDocument);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task UpdateCategoryDocument_WhenCategoryDocumentPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var UpdateCategoryDocument = new UpdateCategoryDocumentDto();
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.UpdateCategoryDocument(Guid.NewGuid(), UpdateCategoryDocument);

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
        public async Task UpdateCategoryDocument_WhenCategoryDocumentPropertyTitleNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var UpdateCategoryDocument = new UpdateCategoryDocumentDto { };
            string key = "Title";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateCategoryDocument(Guid.NewGuid(), UpdateCategoryDocument);

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
        public async Task UpdateCategoryDocument_WhenCategoryDocumentPropertyTitleLengthLonger500_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var newCategoryDocument = new UpdateCategoryDocumentDto { };
            string key = "Title";
            string messageError = $"The field {key} must be a string with a maximum length of 500.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.UpdateCategoryDocument(Guid.NewGuid(), newCategoryDocument);

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
        public async Task UpdateCategoryDocument_WhenCategoryDocumentPropertyOrderNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var newCategoryDocument = new UpdateCategoryDocumentDto { };
            string key = "Order";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.UpdateCategoryDocument(Guid.NewGuid(), newCategoryDocument);

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
        public async Task UpdateCategoryDocument_WhenCategoryDocumentPropertyOrderOutOfRange0toIntMaxValue_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            var newCategoryDocument = new UpdateCategoryDocumentDto { };
            string key = "Order";
            string messageError = $"The field {key} must be in range 0 to 2147483647.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.UpdateCategoryDocument(Guid.NewGuid(),newCategoryDocument);

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
        public async Task UpdateCategoryDocument_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var UpdateCategoryDocument = new UpdateCategoryDocumentDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateCategoryDocument(differentId, UpdateCategoryDocument);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Category Document ID information does not match");
        }

        [Fact]
        public async Task UpdateCategoryDocument_WhenPrivacyPolicyDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var UpdateCategoryDocument = new UpdateCategoryDocumentDto { Id = Guid.NewGuid() };
            A.CallTo(() => _cateRepo.GetSingleByCondition(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._)).Returns(Task.FromResult<CategoryDocument>(null));

            // Act
            var result = await _controller.UpdateCategoryDocument(UpdateCategoryDocument.Id, UpdateCategoryDocument);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Category Document not found.");
        }
        [Fact]
        public async Task UpdateCategoryDocument_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var UpdateCategoryDocument = new UpdateCategoryDocumentDto { Id = Guid.NewGuid() };
            var existPrivacyPolicy = new CategoryDocument { Id = UpdateCategoryDocument.Id };
            A.CallTo(() => _cateRepo.GetSingleByCondition(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existPrivacyPolicy));
            A.CallTo(() => _mapper.Map(UpdateCategoryDocument, existPrivacyPolicy));
            A.CallTo(() => _cateRepo.Update(existPrivacyPolicy)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateCategoryDocument(UpdateCategoryDocument.Id, UpdateCategoryDocument);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionCategoryDocumentResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Category Document updated successfully.");
            response.Id.Should().Be(existPrivacyPolicy.Id);
        }
        [Fact]
        public async Task UpdateCategoryDocument_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var UpdateCategoryDocument = new UpdateCategoryDocumentDto { Id = Guid.NewGuid() };
            var existPrivacyPolicy = new CategoryDocument { Id = UpdateCategoryDocument.Id };
            A.CallTo(() => _cateRepo.GetSingleByCondition(A<Expression<Func<CategoryDocument, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existPrivacyPolicy));
            A.CallTo(() => _mapper.Map(UpdateCategoryDocument, existPrivacyPolicy));
            A.CallTo(() => _cateRepo.Update(existPrivacyPolicy)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateCategoryDocument(UpdateCategoryDocument.Id, UpdateCategoryDocument);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating Category Document: Update failed");
        }


        #endregion

        #region DeleteCategoryDocument
        [Fact]
        public async Task DeleteCategoryDocument_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new CategoryDocumentController(null, null, _logger);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteCategoryDocument(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteCategoryDocument_WhenPrivacyPolicyDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _cateRepo.CheckContainsAsync(A<Expression<Func<CategoryDocument, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteCategoryDocument(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Don't exist Category Document with ID {id} to delete");

        }

        [Fact]
        public async Task DeleteCategoryDocument_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _cateRepo.CheckContainsAsync(A<Expression<Func<CategoryDocument, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _cateRepo.DeleteCategoryDocument(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCategoryDocument(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionCategoryDocumentResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Category Document deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DeleteCategoryDocument_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _cateRepo.CheckContainsAsync(A<Expression<Func<CategoryDocument, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _cateRepo.DeleteCategoryDocument(id)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteCategoryDocument(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error deleting Category Document: Delete failed");
        }
        [Fact]
        public async Task DeleteCategoryDocument_WhenPrivacyPolicyPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new CategoryDocumentController(_cateRepo, _mapper, _logger);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeleteCategoryDocument(Guid.NewGuid());

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
