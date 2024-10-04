using AutoMapper;
using BusinessObjects.Models;
using FakeItEasy;
using FluentAssertions;
using InSyncAPI.Controllers;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositorys;
using System.Linq.Expressions;
namespace InSyncUnitTest.Controller
{
    public class TermsControllerTests
    {
        private readonly ITermRepository _termRepo;
        private readonly IMapper _mapper;
        private readonly TermsController _controller;

        public TermsControllerTests()
        {
            _termRepo = A.Fake<ITermRepository>();
            _mapper = A.Fake<IMapper>();
            _controller = new TermsController(_termRepo, _mapper);
        }


        #region GetTerms
        [Fact]
        public async Task GetTerms_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new TermsController(null, null);

            // Act
            var result = await controller.GetTerms();

            // Assert
            var statusCodeResult = result as ObjectResult;

            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetTerms_WhenEnoughDependency_ShouldReturnsOkResultWithTerms()
        {

            // Arrange
            string[] includes = new string[] { };
            var term1 = new Term { Id = Guid.NewGuid(), Question = "This is question 1", Answer = "This is answer 1", DateCreated = DateTime.Now };
            var term2 = new Term { Id = Guid.NewGuid(), Question = "This is question 2", Answer = "This is answer 2", DateCreated = DateTime.Now };
            var terms = new List<Term> { term1, term2 };
            A.CallTo(() => _termRepo.GetAll(includes)).Returns(terms.AsQueryable());

            // Act
            var result = await _controller.GetTerms();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as IQueryable<Term>;
            returnedTerms.Should().NotBeNull();
            returnedTerms.Should().HaveCount(2);
        }
        #endregion

        #region GetAllTerms
        [Fact]
        public async Task GetAllTerms_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new TermsController(null, null);
            var result = await controller.GetAllTerms();


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllTerms_WithValidInput_ShouldReturnOkResultWithTerms()
        {
            // Arrange
            var term1 = new Term { Id = Guid.NewGuid(), Question = "This is question 1" };
            var term2 = new Term { Id = Guid.NewGuid(), Question = "This is question 2" };
            var termView1 = new ViewTermDto { Id = term1.Id, Question = term1.Question };
            var termView2 = new ViewTermDto { Id = term2.Id, Question = term2.Question };
            IEnumerable<Term> terms = new List<Term> { term1, term2 };
            IEnumerable<ViewTermDto> viewTerms = new List<ViewTermDto> { termView1, termView2 };
            int total;
            int index = 0, size = 2;
            string[] includes = new string[] { };
            A.CallTo(() => _termRepo.GetMultiPaging(A<Expression<Func<Term, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(terms);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewTermDto>>(terms)).Returns(viewTerms);

            // Act
            var result = await _controller.GetAllTerms("",index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as ResponsePaging<IEnumerable<ViewTermDto>>;
            returnedTerms.Should().NotBeNull();
            returnedTerms.data.Should().BeEquivalentTo(viewTerms);
            returnedTerms.data.Should().HaveCount(2);
        }
        [Fact]
        public async Task GetAllTerms_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithTerms()
        {
            // Arrange
            var term1 = new Term { Id = Guid.NewGuid(), Question = "This is question 1" };
            var term2 = new Term { Id = Guid.NewGuid(), Question = "This is question 2" };
            var termView1 = new ViewTermDto { Id = term1.Id, Question = term1.Question };
            var termView2 = new ViewTermDto { Id = term2.Id, Question = term2.Question };
            IEnumerable<Term> terms = new List<Term> { term1, term2 };
            IEnumerable<ViewTermDto> viewTerms = new List<ViewTermDto> { termView1, termView2 };
            int total;
            int index = -1, size = -3;
            string[] includes = new string[] { };
            A.CallTo(() => _termRepo.GetMultiPaging(A<Expression<Func<Term, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(terms);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewTermDto>>(terms)).Returns(viewTerms);

            // Act
            var result = await _controller.GetAllTerms("",index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as ResponsePaging<IEnumerable<ViewTermDto>>;
            returnedTerms.Should().NotBeNull();
            returnedTerms.data.Should().BeEquivalentTo(viewTerms);
            returnedTerms.data.Should().HaveCount(2);
        }
        [Fact]
        public async Task GetAllTerms_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithTerms()
        {
            // Arrange
            var term1 = new Term { };
            var term2 = new Term { };
            var termView1 = new ViewTermDto { };
            var termView2 = new ViewTermDto { };
            IEnumerable<Term> terms = new List<Term> { term1, term2 };
            IEnumerable<ViewTermDto> viewTerms = new List<ViewTermDto> { termView1, termView2 };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _termRepo.GetMultiPaging(A<Expression<Func<Term, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(terms);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewTermDto>>(terms)).Returns(viewTerms);

            // Act
            var result = await _controller.GetAllTerms();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as ResponsePaging<IEnumerable<ViewTermDto>>;
            returnedTerms.Should().NotBeNull();
            returnedTerms.data.Should().BeEquivalentTo(viewTerms);
            
        }


        #endregion

        #region GetTermById
        [Fact]
        public async Task GetTermById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new TermsController(null, null);
            //Act
            var result = await controller.GetTermById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetTermById_WhenTermDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var termId = Guid.NewGuid();
            A.CallTo(() => _termRepo.GetSingleByCondition(A<Expression<Func<Term, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Term>(null));

            // Act
            var result = await _controller.GetTermById(termId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No term has an ID : {termId}");
        }

        [Fact]
        public async Task GetTermById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new TermsController(_termRepo, _mapper);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetTermById(Guid.Empty);

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
        public async Task GetTermById_WithIdExist_ShouldReturnTerm()
        {
            // Arrange
            var termId = Guid.NewGuid();
            var term = new Term { Id = termId, Question = "This is a question" };
            var viewTerm = new ViewTermDto { Id = termId, Question = "This is a question" };

            A.CallTo(() => _termRepo.GetSingleByCondition(A<Expression<Func<Term, bool>>>._, A<string[]>._)).Returns(Task.FromResult(term));
            A.CallTo(() => _mapper.Map<ViewTermDto>(term)).Returns(viewTerm);

            // Act
            var result = await _controller.GetTermById(termId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerm = okResult.Value as ViewTermDto;
            returnedTerm.Should().NotBeNull();
            returnedTerm.Should().BeEquivalentTo(viewTerm);
        }

        #endregion

        #region AddTerm
        [Fact]
        public async Task AddTerm_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new TermsController(null, null);
            var newTerm = new AddTermsDto();

            // Act
            var result = await controller.AddTerm(newTerm);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddTerm_WhenTermPropertyAnswerNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new TermsController(_termRepo, _mapper);
            var newTerm = new AddTermsDto { Question = "This is question." };
            controller.ModelState.AddModelError("Answer", "The Answer field is required.");

            // Act
            var result = await controller.AddTerm(newTerm);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Answer");
            errorResponse.Errors["Answer"].Should().Contain("The Answer field is required.");
        }
        [Fact]
        public async Task AddTerm_WhenTermPropertyQuestionNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new TermsController(_termRepo, _mapper);
            var newTerm = new AddTermsDto { Answer = "This is Answer" };
            controller.ModelState.AddModelError("Question", "The Question field is required.");

            // Act
            var result = await controller.AddTerm(newTerm);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Question");
            errorResponse.Errors["Question"].Should().Contain("The Question field is required.");
            //badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            //var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            //errorResponse.Title.Should().Be("One or more validation errors occurred.");
            //errorResponse.Errors.Should().ContainKey("Answer");
            //errorResponse.Errors["Answer"].Should().Contain(("The Answer field is required."));
        }

        [Fact]
        public async Task AddTerm_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newTerm = new AddTermsDto { };
            var term = new Term { Id = Guid.NewGuid(), DateCreated = DateTime.Now };
            A.CallTo(() => _mapper.Map<Term>(newTerm)).Returns(term);
            A.CallTo(() => _termRepo.Add(term)).Returns(Task.FromResult<Term>(null));

            // Act
            var result = await _controller.AddTerm(newTerm);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the term.");
        }
        [Fact]
        public async Task AddTerm_WhenThrowException_ShouldReturnsInternalServerError()
        {
            // Arrange
            string messageException = "Id must be auto generator";
            var newTerm = new AddTermsDto { };
            var term = new Term { Id = Guid.NewGuid(), DateCreated = DateTime.Now };
            A.CallTo(() => _mapper.Map<Term>(newTerm)).Returns(term);
            A.CallTo(() => _termRepo.Add(term)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddTerm(newTerm);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("An error occurred while adding Term into Database " + messageException);
        }
        [Fact]
        public async Task AddTerm_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newTerm = new AddTermsDto { Question = "This is Question", Answer = "This is Answer" };
            var term = new Term { Question = "This is Question", Answer = "This is Answer", DateCreated = DateTime.Now };
            var addedTerm = new Term { Id = Guid.NewGuid(), Question = "This is Question", Answer = "This is Answer", DateCreated = term.DateCreated };
            A.CallTo(() => _mapper.Map<Term>(newTerm)).Returns(term);
            A.CallTo(() => _termRepo.Add(term)).Returns(Task.FromResult(addedTerm));

            // Act
            var result = await _controller.AddTerm(newTerm);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionTermResponse;
            response.Message.Should().Be("Term added successfully.");
            response.Id.Should().Be(addedTerm.Id);
        }




        #endregion


        #region UdpateTerm
        [Fact]
        public async Task UpdateTerm_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new TermsController(null, null);
            var newTerm = new UpdateTermsDto();

            // Act
            var result = await controller.UpdateTerm(Guid.NewGuid(), newTerm);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task UpdateTerm_WhenTermPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new TermsController(_termRepo, _mapper);
            var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid(), Question = "This is question." };
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.UpdateTerm(Guid.NewGuid(), updateTerm);

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
        public async Task UpdateTerm_WhenTermPropertyAnswerNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new TermsController(_termRepo, _mapper);
            var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid(), Question = "This is question." };
            controller.ModelState.AddModelError("Answer", "The Answer field is required.");

            // Act
            var result = await controller.UpdateTerm(updateTerm.Id, updateTerm);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Answer");
            errorResponse.Errors["Answer"].Should().Contain("The Answer field is required.");
        }
        [Fact]
        public async Task UpdateTerm_WhenTermPropertyQuestionNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new TermsController(_termRepo, _mapper);
            var updateTerms = new UpdateTermsDto { Id = Guid.NewGuid(), Answer = "This is Answer" };
            controller.ModelState.AddModelError("Question", "The Question field is required.");

            // Act
            var result = await controller.UpdateTerm(updateTerms.Id, updateTerms);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Question");
            errorResponse.Errors["Question"].Should().Contain("The Question field is required.");
        }
        [Fact]
        public async Task UpdateTerm_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateTerm(differentId, updateTerm);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Term ID information does not match");
        }

        [Fact]
        public async Task UpdateTerm_WhenTermDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid() };
            A.CallTo(() => _termRepo.GetSingleByCondition(A<Expression<Func<Term, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Term>(null));

            // Act
            var result = await _controller.UpdateTerm(updateTerm.Id, updateTerm);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Term not found.");
        }
        [Fact]
        public async Task UpdateTerm_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid(), Question = "This is question", Answer = "This is Answer" };
            var existingTerm = new Term { Id = updateTerm.Id };
            A.CallTo(() => _termRepo.GetSingleByCondition(A<Expression<Func<Term, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existingTerm));
            A.CallTo(() => _mapper.Map(updateTerm, existingTerm));
            A.CallTo(() => _termRepo.Update(existingTerm)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateTerm(updateTerm.Id, updateTerm);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionTermResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Term updated successfully.");
            response.Id.Should().Be(existingTerm.Id);
        }
        [Fact]
        public async Task UpdateTerm_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid() };
            var existingTerm = new Term { Id = updateTerm.Id, DateUpdated = DateTime.Now };
            A.CallTo(() => _termRepo.GetSingleByCondition(A<Expression<Func<Term, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existingTerm));
            A.CallTo(() => _termRepo.Update(existingTerm)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateTerm(updateTerm.Id, updateTerm);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating term: Update failed");
        }
        

        #endregion

        #region DeleteTerm
        [Fact]
        public async Task DeleteTerm_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new TermsController(null, null);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteTerm(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteTerm_WhenTermDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _termRepo.CheckContainsAsync(A<Expression<Func<Term, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteTerm(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Dont exist term with id {id.ToString()} to delete");




        }

        [Fact]
        public async Task DeleteTerm_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _termRepo.CheckContainsAsync(A<Expression<Func<Term, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _termRepo.DeleteMulti(A<Expression<Func<Term, bool>>>._)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteTerm(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionTermResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Term deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DeleteTerm_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _termRepo.CheckContainsAsync(A<Expression<Func<Term, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _termRepo.DeleteMulti(A<Expression<Func<Term, bool>>>._)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteTerm(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error delete term: Delete failed");
        }
        [Fact]
        public async Task DeleteTermTerm_WhenTermPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new TermsController(_termRepo, _mapper);
            controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

            // Act
            var result = await controller.DeleteTerm(Guid.NewGuid());

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
        #endregion
    }
}
