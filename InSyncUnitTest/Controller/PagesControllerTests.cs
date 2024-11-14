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
using System.Linq.Expressions;
using static InSyncAPI.Dtos.PageDto;

namespace InSyncUnitTest.Controller
{
    public class PagesControllerTests
    {
        private readonly IPageRepository _pageRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<PagesController> _logger;

        private readonly PagesController _controller;

        public PagesControllerTests()
        {
            _pageRepo = A.Fake<IPageRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<PagesController>>();
            _controller = new PagesController(_pageRepo, _mapper, _logger);
        }


        #region GetPages
        [Fact]
        public async Task GetPages_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new PagesController(null, null, _logger);

            // Act
            var result = await controller.GetPages();

            // Assert
            var statusCodeResult = result as ObjectResult;

            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetPages_WhenEnoughDependency_ShouldReturnsOkResultWithTerms()
        {

            // Arrange
            string[] includes = new string[] { };
            var term1 = new Page { };
            var term2 = new Page { };
            var pages = new List<Page> { term1, term2 };
            A.CallTo(() => _pageRepo.GetAll(includes)).Returns(pages.AsQueryable());

            // Act
            var result = await _controller.GetPages();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as IQueryable<Page>;
            returnedTerms.Should().NotBeNull();
            
        }
        [Fact]
        public async Task GetPages_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving pages.";
            A.CallTo(() => _pageRepo.GetAll(A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetPages();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error retrieving pages: " +  message);
        }
        #endregion

        #region GetAllPage
        [Fact]
        public async Task GetAllPage_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new PagesController(null, null, _logger);
            var result = await controller.GetAllPage(0, 2);


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllPage_WithValidInput_ShouldReturnOkResultWithTerms()
        {
            // Arrange
            var term1 = new Page { };
            var term2 = new Page { };
            var termView1 = new ViewPageDto { };
            var termView2 = new ViewPageDto { };
            IEnumerable<Page> pages = new List<Page> { term1, term2 };
            IEnumerable<ViewPageDto> viewPages = new List<ViewPageDto> { termView1, termView2 };
            int total;
            int index = 0, size = 2;
            string[] includes = new string[] { };
            A.CallTo(() => _pageRepo.GetMultiPaging(A<Expression<Func<Page, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(pages);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewPageDto>>(pages)).Returns(viewPages);

            // Act
            var result = await _controller.GetAllPage(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as ResponsePaging<IEnumerable<ViewPageDto>>;
            returnedTerms.Should().NotBeNull();
            returnedTerms.data.Should().BeEquivalentTo(viewPages);
            returnedTerms.data.Should().HaveCount(2);
        }
        [Fact]
        public async Task GetAllPage_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithTerms()
        {
            // Arrange
            var term1 = new Page { };
            var term2 = new Page { };
            var termView1 = new ViewPageDto { };
            var termView2 = new ViewPageDto { };
            IEnumerable<Page> pages = new List<Page> { term1, term2 };
            IEnumerable<ViewPageDto> viewPages = new List<ViewPageDto> { termView1, termView2 };
            int total;
            int index = -1, size = -3;
            string[] includes = new string[] { };
            A.CallTo(() => _pageRepo.GetMultiPaging(A<Expression<Func<Page, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(pages);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewPageDto>>(pages)).Returns(viewPages);

            // Act
            var result = await _controller.GetAllPage(index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as ResponsePaging<IEnumerable<ViewPageDto>>;
            returnedTerms.Should().NotBeNull();
            returnedTerms.data.Should().BeEquivalentTo(viewPages);
            returnedTerms.data.Should().HaveCount(2);
        }
        [Fact]
        public async Task GetAllPage_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithTerms()
        {
            // Arrange
            var term1 = new Page { };
            var term2 = new Page { };
            var termView1 = new ViewPageDto { };
            var termView2 = new ViewPageDto { };
            IEnumerable<Page> pages = new List<Page> { term1, term2 };
            IEnumerable<ViewPageDto> viewPages = new List<ViewPageDto> { termView1, termView2 };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _pageRepo.GetMulti(A<Expression<Func<Page, bool>>>._, A<string[]>._))
                .Returns(pages);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewPageDto>>(pages)).Returns(viewPages);

            // Act
            var result = await _controller.GetAllPage(null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerms = okResult.Value as ResponsePaging<IEnumerable<ViewPageDto>>;
            returnedTerms.Should().NotBeNull();
            returnedTerms.data.Should().BeEquivalentTo(viewPages);

        }
        [Fact]
        public async Task GetAllTerms_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving pages.";
            A.CallTo(() => _pageRepo.GetMulti(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetAllPage(null, null);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error retrieving pages: An error occurred while retrieving pages.");
        }

        #endregion

        #region GetPageById
        [Fact]
        public async Task GetPageById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new PagesController(null, null, _logger);
            //Act
            var result = await controller.GetPageById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetPageById_WhenPageDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var pageId = Guid.NewGuid();
            A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Page>(null));

            // Act
            var result = await _controller.GetPageById(pageId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No Page has an ID: {pageId}");
        }

        [Fact]
        public async Task GetPageById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new PagesController(_pageRepo, _mapper, _logger);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetPageById(Guid.Empty);

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
        public async Task GetPageById_WithIdExist_ShouldReturnPage()
        {
            // Arrange
            var pageId = Guid.NewGuid();
            var Page = new Page {};
            var viewPage = new ViewPageDto { };

            A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Returns(Task.FromResult(Page));
            A.CallTo(() => _mapper.Map<ViewPageDto>(Page)).Returns(viewPage);

            // Act
            var result = await _controller.GetPageById(pageId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerm = okResult.Value as ViewPageDto;
            returnedTerm.Should().NotBeNull();
            returnedTerm.Should().BeEquivalentTo(viewPage);
        }

        [Fact]
        public async Task GetPageById_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var message = "An error occurred while retrieving the Page.";
            A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetPageById(Guid.NewGuid());

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error retrieving Page: " + message);
        }
        #endregion

        #region GetPageBySlug
        [Fact]
        public async Task GetPageBySlug_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new PagesController(null, null, _logger);
            //Act
            var result = await controller.GetPageBySlug("");
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetPageBySlug_WhenPageDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var slug = "slug";
            A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Page>(null));

            // Act
            var result = await _controller.GetPageBySlug(slug);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No Page has an Slug: {slug}");
        }

        [Fact]
        public async Task GetPageBySlug_WithIdExist_ShouldReturnPage()
        {
            // Arrange
            var slug = "slug";
            var Page = new Page { };
            var viewPage = new ViewPageDto { };

            A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Returns(Task.FromResult(Page));
            A.CallTo(() => _mapper.Map<ViewPageDto>(Page)).Returns(viewPage);

            // Act
            var result = await _controller.GetPageBySlug(slug);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedTerm = okResult.Value as ViewPageDto;
            returnedTerm.Should().NotBeNull();
            returnedTerm.Should().BeEquivalentTo(viewPage);
        }

        [Fact]
        public async Task GetPageBySlug_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var slug = "slug";
            var message = "An error occurred while retrieving the Page.";
            A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Throws(new Exception(message));

            // Act
            var result = await _controller.GetPageBySlug(slug);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error retrieving Page: " + message);
        }
        #endregion

        #region AddPage
        [Fact]
        public async Task AddPage_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new PagesController(null, null, _logger);
            var newPage = new AddPageDto();

            // Act
            var result = await controller.AddPage(newPage);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddPage_WhenPagePropertySlugNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PagesController(_pageRepo, _mapper, _logger);
            var newPage = new AddPageDto {};
            controller.ModelState.AddModelError("Slug", "The Slug field is required.");

            // Act
            var result = await controller.AddPage(newPage);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Slug");
            errorResponse.Errors["Slug"].Should().Contain("The Slug field is required.");
        }
        [Fact]
        public async Task AddPage_WhenPagePropertySlugLengthLonger600_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PagesController(_pageRepo, _mapper, _logger);
            var newPage = new AddPageDto { };
            string key = "Slug";
            string messageError = $"The field {key} must be a string with a maximum length of 600.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddPage(newPage);

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
        public async Task AddPage_WhenTermPropertyQuestionNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PagesController(_pageRepo, _mapper, _logger);
            var newPage = new AddPageDto { };
            controller.ModelState.AddModelError("Title", "The Title field is required.");

            // Act
            var result = await controller.AddPage(newPage);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var errorResponse = badRequestResult.Value as ValidationProblemDetails;
            errorResponse.Should().NotBeNull();
            errorResponse.Title.Should().Be("One or more validation errors occurred.");
            errorResponse.Errors.Should().ContainKey("Title");
            errorResponse.Errors["Title"].Should().Contain("The Title field is required.");
           
        }
        [Fact]
        public async Task AddPage_WhenPagePropertyTitleLengthLonger500_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new PagesController(_pageRepo, _mapper, _logger);
            var newPage = new AddPageDto { };
            string key = "Title";
            string messageError = $"The field {key} must be a string with a maximum length of 500.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddPage(newPage);

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
        public async Task AddPage_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newPage = new AddPageDto { };
            var Page = new Page { };
            A.CallTo(() => _mapper.Map<Page>(newPage)).Returns(Page);
            A.CallTo(() => _pageRepo.Add(Page)).Returns(Task.FromResult<Page>(null));

            // Act
            var result = await _controller.AddPage(newPage);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the Page.");
        }
        [Fact]
        public async Task AddPage_WhenThrowException_ShouldReturnsInternalServerError()
        {
            // Arrange
            string messageException = "Id must be auto generator";
            var newPage = new AddPageDto { };
            var Page = new Page { Id = Guid.NewGuid(), DateCreated = DateTime.Now };
            A.CallTo(() => _mapper.Map<Page>(newPage)).Returns(Page);
            A.CallTo(() => _pageRepo.Add(Page)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddPage(newPage);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("An error occurred while adding Page into Database: " + messageException);
        }
        [Fact]
        public async Task AddPage_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newPage = new AddPageDto {};
            var addPage = new Page { };
            A.CallTo(() => _mapper.Map<Page>(newPage)).Returns(addPage);
            A.CallTo(() => _pageRepo.Add(addPage)).Returns(Task.FromResult(addPage));

            // Act
            var result = await _controller.AddPage(newPage);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionPageResponse;
            response.Message.Should().Be("Page added successfully.");
            response.Id.Should().Be(addPage.Id);
        }




        #endregion


        //#region UdpateTerm
        //[Fact]
        //public async Task UpdateTerm_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        //{
        //    // Arrange
        //    var controller = new PagesController(null, null, _logger);
        //    var newPage = new UpdateTermsDto();

        //    // Act
        //    var result = await controller.UpdateTerm(Guid.NewGuid(), newPage);

        //    // Assert
        //    var statusCodeResult = result as ObjectResult;
        //    statusCodeResult.Should().NotBeNull();
        //    statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        //    statusCodeResult.Value.Should().Be("Application service has not been created");
        //}
        //[Fact]
        //public async Task UpdateTerm_WhenTermPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        //{
        //    // Arrange
        //    var controller = new PagesController(_pageRepo, _mapper, _logger);
        //    var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid(), Question = "This is question." };
        //    controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

        //    // Act
        //    var result = await controller.UpdateTerm(Guid.NewGuid(), updateTerm);

        //    // Assert
        //    var badRequestResult = result as BadRequestObjectResult;
        //    badRequestResult.Should().NotBeNull();
        //    badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        //    var errorResponse = badRequestResult.Value as ValidationProblemDetails;
        //    errorResponse.Should().NotBeNull();
        //    errorResponse.Title.Should().Be("One or more validation errors occurred.");
        //    errorResponse.Errors.Should().ContainKey("id");
        //    errorResponse.Errors["id"].Should().Contain("The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");
        //}
        //[Fact]
        //public async Task UpdateTerm_WhenTermPropertyAnswerNull_ShouldReturnsBadRequest()
        //{
        //    // Arrange
        //    var controller = new PagesController(_pageRepo, _mapper, _logger);
        //    var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid(), Question = "This is question." };
        //    controller.ModelState.AddModelError("Answer", "The Answer field is required.");

        //    // Act
        //    var result = await controller.UpdateTerm(updateTerm.Id, updateTerm);

        //    // Assert
        //    var badRequestResult = result as BadRequestObjectResult;
        //    badRequestResult.Should().NotBeNull();
        //    badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        //    var errorResponse = badRequestResult.Value as ValidationProblemDetails;
        //    errorResponse.Should().NotBeNull();
        //    errorResponse.Title.Should().Be("One or more validation errors occurred.");
        //    errorResponse.Errors.Should().ContainKey("Answer");
        //    errorResponse.Errors["Answer"].Should().Contain("The Answer field is required.");
        //}
        //[Fact]
        //public async Task UpdateTerm_WhenTermPropertyQuestionNull_ShouldReturnsBadRequest()
        //{
        //    // Arrange
        //    var controller = new PagesController(_pageRepo, _mapper, _logger);
        //    var updateTerms = new UpdateTermsDto { Id = Guid.NewGuid(), Answer = "This is Answer" };
        //    controller.ModelState.AddModelError("Question", "The Question field is required.");

        //    // Act
        //    var result = await controller.UpdateTerm(updateTerms.Id, updateTerms);

        //    // Assert
        //    var badRequestResult = result as BadRequestObjectResult;
        //    badRequestResult.Should().NotBeNull();
        //    badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        //    var errorResponse = badRequestResult.Value as ValidationProblemDetails;
        //    errorResponse.Should().NotBeNull();
        //    errorResponse.Title.Should().Be("One or more validation errors occurred.");
        //    errorResponse.Errors.Should().ContainKey("Question");
        //    errorResponse.Errors["Question"].Should().Contain("The Question field is required.");
        //}
        //[Fact]
        //public async Task UpdateTerm_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        //{
        //    // Arrange
        //    var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid() };
        //    var differentId = Guid.NewGuid();

        //    // Act
        //    var result = await _controller.UpdateTerm(differentId, updateTerm);

        //    // Assert
        //    var badRequestResult = result as BadRequestObjectResult;
        //    badRequestResult.Should().NotBeNull();
        //    badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        //    badRequestResult.Value.Should().Be("Page ID information does not match");
        //}

        //[Fact]
        //public async Task UpdateTerm_WhenTermDoesNotExist_ShouldReturnsNotFound()
        //{
        //    // Arrange
        //    var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid() };
        //    A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Page>(null));

        //    // Act
        //    var result = await _controller.UpdateTerm(updateTerm.Id, updateTerm);

        //    // Assert
        //    var notFoundResult = result as NotFoundObjectResult;
        //    notFoundResult.Should().NotBeNull();
        //    notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        //    notFoundResult.Value.Should().Be("Page not found.");
        //}
        //[Fact]
        //public async Task UpdateTerm_WhenUpdateSucceeds_ShouldReturnsOkResult()
        //{
        //    // Arrange
        //    var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid(), Question = "This is question", Answer = "This is Answer" };
        //    var existingTerm = new Page { Id = updateTerm.Id };
        //    A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existingTerm));
        //    A.CallTo(() => _mapper.Map(updateTerm, existingTerm));
        //    A.CallTo(() => _pageRepo.Update(existingTerm)).Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _controller.UpdateTerm(updateTerm.Id, updateTerm);

        //    // Assert
        //    var okResult = result as OkObjectResult;
        //    okResult.Should().NotBeNull();
        //    okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        //    var response = okResult.Value as ActionPageResponse;
        //    response.Should().NotBeNull();
        //    response.Message.Should().Be("Page updated successfully.");
        //    response.Id.Should().Be(existingTerm.Id);
        //}
        //[Fact]
        //public async Task UpdateTerm_WhenUpdateFails_ShouldReturnsInternalServerError()
        //{
        //    // Arrange
        //    var updateTerm = new UpdateTermsDto { Id = Guid.NewGuid() };
        //    var existingTerm = new Page { Id = updateTerm.Id, DateUpdated = DateTime.Now };
        //    A.CallTo(() => _pageRepo.GetSingleByCondition(A<Expression<Func<Page, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existingTerm));
        //    A.CallTo(() => _pageRepo.Update(existingTerm)).Throws(new Exception("Update failed"));

        //    // Act
        //    var result = await _controller.UpdateTerm(updateTerm.Id, updateTerm);

        //    // Assert
        //    var statusCodeResult = result as ObjectResult;
        //    statusCodeResult.Should().NotBeNull();
        //    statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        //    statusCodeResult.Value.Should().Be("Error updating Page: Update failed");
        //}


        //#endregion

        //#region DeleteTerm
        //[Fact]
        //public async Task DeleteTerm_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        //{
        //    // Arrange
        //    var controller = new PagesController(null, null, _logger);
        //    var id = Guid.NewGuid();

        //    // Act
        //    var result = await controller.DeleteTerm(id);

        //    // Assert
        //    var statusCodeResult = result as ObjectResult;
        //    statusCodeResult.Should().NotBeNull();
        //    statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        //    statusCodeResult.Value.Should().Be("Application service has not been created");
        //}

        //[Fact]
        //public async Task DeleteTerm_WhenTermDoesNotExist_ShouldReturnsNotFound()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    A.CallTo(() => _pageRepo.CheckContainsAsync(A<Expression<Func<Page, bool>>>._)).Returns(Task.FromResult(false));

        //    // Act
        //    var result = await _controller.DeleteTerm(id);

        //    // Assert
        //    var notFoundResult = result as NotFoundObjectResult;
        //    notFoundResult.Should().NotBeNull();
        //    notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        //    notFoundResult.Value.Should().Be($"Dont exist Page with id {id.ToString()} to delete");




        //}

        //[Fact]
        //public async Task DeleteTerm_WhenDeleteSucceeds_ShouldReturnsOkResult()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    A.CallTo(() => _pageRepo.CheckContainsAsync(A<Expression<Func<Page, bool>>>._)).Returns(Task.FromResult(true));
        //    A.CallTo(() => _pageRepo.DeleteMulti(A<Expression<Func<Page, bool>>>._)).Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _controller.DeleteTerm(id);

        //    // Assert
        //    var okResult = result as OkObjectResult;
        //    okResult.Should().NotBeNull();
        //    okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        //    var response = okResult.Value as ActionPageResponse;
        //    response.Should().NotBeNull();
        //    response.Message.Should().Be("Page deleted successfully.");
        //    response.Id.Should().Be(id);
        //}

        //[Fact]
        //public async Task DeleteTerm_WhenDeleteFails_ShouldReturnsInternalServerError()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    A.CallTo(() => _pageRepo.CheckContainsAsync(A<Expression<Func<Page, bool>>>._)).Returns(Task.FromResult(true));
        //    A.CallTo(() => _pageRepo.DeleteMulti(A<Expression<Func<Page, bool>>>._)).Throws(new Exception("Delete failed"));

        //    // Act
        //    var result = await _controller.DeleteTerm(id);

        //    // Assert
        //    var statusCodeResult = result as ObjectResult;
        //    statusCodeResult.Should().NotBeNull();
        //    statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        //    statusCodeResult.Value.Should().Be("Error deleting Page: Delete failed");
        //}
        //[Fact]
        //public async Task DeleteTermTerm_WhenTermPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        //{
        //    // Arrange
        //    var controller = new PagesController(_pageRepo, _mapper, _logger);
        //    controller.ModelState.AddModelError("id", "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");

        //    // Act
        //    var result = await controller.DeleteTerm(Guid.NewGuid());

        //    // Assert
        //    var badRequestResult = result as BadRequestObjectResult;
        //    badRequestResult.Should().NotBeNull();
        //    badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        //    var errorResponse = badRequestResult.Value as ValidationProblemDetails;
        //    errorResponse.Should().NotBeNull();
        //    errorResponse.Title.Should().Be("One or more validation errors occurred.");
        //    errorResponse.Errors.Should().ContainKey("id");
        //    errorResponse.Errors["id"].Should().Contain("The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.");
        //}
        //#endregion
    }
}
