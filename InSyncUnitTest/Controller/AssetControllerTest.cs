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
    public class AssetControllerTest
    {
        private IAssetRepository _assetRepo;
        private IProjectRepository _projectRepo;
        private IMapper _mapper;
        private AssetsController _controller;
        public AssetControllerTest()
        {
            _assetRepo = A.Fake<IAssetRepository>();
            _projectRepo = A.Fake<IProjectRepository>();
            _mapper = A.Fake<IMapper>();
            _controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
        }

        #region GetAssets
        [Fact]
        public async Task GetAssets_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new AssetsController(null,null, null);

            // Act
            var result = await controller.GetAssets();

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task GetAssets_WithAssets_ShouldReturnsOkResult()
        {
            // Arrange
            var Assets = new[] { new Asset(), new Asset() }.AsQueryable();
            A.CallTo(() => _assetRepo.GetAll(A<string[]>._)).Returns(Assets);

            // Act
            var result = await _controller.GetAssets();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as IQueryable<Asset>;
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(Assets);
            response.Should().HaveCount(2);
        }

        #endregion

        #region GetAllAsset
        [Fact]
        public async Task GetAllAsset_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new AssetsController(null,null, null);
            var result = await controller.GetAllAsset();


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllAsset_WithValidInput_ShouldReturnOkResultWithAssets()
        {
            // Arrange

            IEnumerable<Asset> Assets = new List<Asset> { new Asset(), new Asset() };
            IEnumerable<ViewAssetDto> viewAssets = new List<ViewAssetDto> { new ViewAssetDto(), new ViewAssetDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _assetRepo.GetMultiPaging(A<Expression<Func<Asset, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Assets);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewAssetDto>>(Assets)).Returns(viewAssets);

            // Act
            var result = await _controller.GetAllAsset("",index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedAssets = okResult.Value as ResponsePaging<IEnumerable<ViewAssetDto>>;
            returnedAssets.Should().NotBeNull();
            returnedAssets.data.Should().BeEquivalentTo(viewAssets);
        }
        [Fact]
        public async Task GetAllAsset_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithAssets()
        {
            // Arrange

            IEnumerable<Asset> Assets = new List<Asset> { new Asset(), new Asset() };
            IEnumerable<ViewAssetDto> viewAssets = new List<ViewAssetDto> { new ViewAssetDto(), new ViewAssetDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _assetRepo.GetMultiPaging(A<Expression<Func<Asset, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Assets);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewAssetDto>>(Assets)).Returns(viewAssets);

            // Act
            var result = await _controller.GetAllAsset("",index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedAssets = okResult.Value as ResponsePaging<IEnumerable<ViewAssetDto>>;
            returnedAssets.Should().NotBeNull();
            returnedAssets.data.Should().BeEquivalentTo(viewAssets);
        }

        [Fact]
        public async Task GetAllAsset_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithAssets()
        {
            // Arrange

            IEnumerable<Asset> Assets = new List<Asset> { new Asset(), new Asset() };
            IEnumerable<ViewAssetDto> viewAssets = new List<ViewAssetDto> { new ViewAssetDto(), new ViewAssetDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _assetRepo.GetMultiPaging(A<Expression<Func<Asset, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Assets);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewAssetDto>>(Assets)).Returns(viewAssets);

            // Act
            var result = await _controller.GetAllAsset();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedAssets = okResult.Value as ResponsePaging<IEnumerable<ViewAssetDto>>;
            returnedAssets.Should().NotBeNull();
            returnedAssets.data.Should().BeEquivalentTo(viewAssets);
        }


        #endregion

        #region GetAssetById
        [Fact]
        public async Task GetAssetById_WithDependencyNull_ShouldReturnInternalServer()
        {
            //Arrange
            var controller = new AssetsController(null,null, null);
            //Act
            var result = await controller.GetAssetById(Guid.NewGuid());
            //Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAssetById_WhenAssetDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var AssetId = Guid.NewGuid();
            A.CallTo(() => _assetRepo.GetSingleByCondition(A<Expression<Func<Asset, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Asset>(null));

            // Act
            var result = await _controller.GetAssetById(AssetId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"No asset has an ID : {AssetId}");
        }

        [Fact]
        public async Task GetAssetById_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo,_projectRepo, _mapper);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetAssetById(Guid.Empty);

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
        public async Task GetAssetById_WithIdExist_ShouldReturnOkResultWithAsset()
        {
            // Arrange
            var AssetId = Guid.NewGuid();
            var Asset = new Asset();
            var viewAsset = new ViewAssetDto();

            A.CallTo(() => _assetRepo.GetSingleByCondition(A<Expression<Func<Asset, bool>>>._, A<string[]>._)).Returns(Task.FromResult(Asset));
            A.CallTo(() => _mapper.Map<ViewAssetDto>(Asset)).Returns(viewAsset);

            // Act
            var result = await _controller.GetAssetById(AssetId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedCR = okResult.Value as ViewAssetDto;
            returnedCR.Should().NotBeNull();
            returnedCR.Should().BeEquivalentTo(viewAsset);
        }

        #endregion
        #region GetAllAssetOfProject
        [Fact]
        public async Task GetAllAssetOfProject_WhenDependencyAreNull_ShouldReturnInternalServerError()
        {
            var controller = new AssetsController(null, null, null);
            var result = await controller.GetAllAssetOfProject(Guid.NewGuid());


            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        [Fact]
        public async Task GetAllAssetOfProject_WithValidInput_ShouldReturnOkResultWithAssets()
        {
            // Arrange

            IEnumerable<Asset> Assets = new List<Asset> { new Asset(), new Asset() };
            IEnumerable<ViewAssetDto> viewAssets = new List<ViewAssetDto> { new ViewAssetDto(), new ViewAssetDto() };
            int total;
            int index = 0, size = 2;
            A.CallTo(() => _assetRepo.GetMultiPaging(A<Expression<Func<Asset, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Assets);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewAssetDto>>(Assets)).Returns(viewAssets);

            // Act
            var result = await _controller.GetAllAssetOfProject(Guid.NewGuid(),"", index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedAssets = okResult.Value as ResponsePaging<IEnumerable<ViewAssetDto>>;
            returnedAssets.Should().NotBeNull();
            returnedAssets.data.Should().BeEquivalentTo(viewAssets);
        }
        [Fact]
        public async Task GetAllAssetOfProject_WithSizeOrIndexIsNegativeInteger_ShouldReturnOkResultWithAssets()
        {
            // Arrange

            IEnumerable<Asset> Assets = new List<Asset> { new Asset(), new Asset() };
            IEnumerable<ViewAssetDto> viewAssets = new List<ViewAssetDto> { new ViewAssetDto(), new ViewAssetDto() };
            int total;
            int index = -1, size = -3;
            A.CallTo(() => _assetRepo.GetMultiPaging(A<Expression<Func<Asset, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Assets);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewAssetDto>>(Assets)).Returns(viewAssets);

            // Act
            var result = await _controller.GetAllAssetOfProject(Guid.NewGuid(),"", index, size);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedAssets = okResult.Value as ResponsePaging<IEnumerable<ViewAssetDto>>;
            returnedAssets.Should().NotBeNull();
            returnedAssets.data.Should().BeEquivalentTo(viewAssets);
        }

        [Fact]
        public async Task GetAllAssetOfProject_WithSizeOrIndexNoParameterInApi_ShouldReturnOkResultWithAssets()
        {
            // Arrange

            IEnumerable<Asset> Assets = new List<Asset> { new Asset(), new Asset() };
            IEnumerable<ViewAssetDto> viewAssets = new List<ViewAssetDto> { new ViewAssetDto(), new ViewAssetDto() };
            int total;
            string[] includes = new string[] { };
            A.CallTo(() => _assetRepo.GetMultiPaging(A<Expression<Func<Asset, bool>>>._, out total, A<int>._, A<int>._, A<string[]>._))
                .Returns(Assets);
            A.CallTo(() => _mapper.Map<IEnumerable<ViewAssetDto>>(Assets)).Returns(viewAssets);

            // Act
            var result = await _controller.GetAllAssetOfProject(Guid.NewGuid());

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedAssets = okResult.Value as ResponsePaging<IEnumerable<ViewAssetDto>>;
            returnedAssets.Should().NotBeNull();
            returnedAssets.data.Should().BeEquivalentTo(viewAssets);
        }
        [Fact]
        public async Task GetAllAssetOfProject_WithIdProjectInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetAllAssetOfProject(Guid.Empty);

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

        #endregion



        #region AddAsset
        [Fact]
        public async Task AddAsset_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new AssetsController(null,null, null);
            var newAsset = new AddAssetDto();

            // Act
            var result = await controller.AddAsset(newAsset);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task AddAsset_WhenAssetPropertyProjectIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo,_projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "ProjectId";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task AddAsset_WithProjectIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetAssetById(Guid.Empty);

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
        public async Task AddAsset_WhenAssetPropertyAssestNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "AssestName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task AddAsset_WhenAssetPropertyAssetNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo,_projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "AssestName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task AddAsset_WhenAssetPropertyTypeLengthLonger50_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "Type";
            string messageError = $"The field {key} must be a string with a maximum length of 50.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task AddAsset_WhenAssetPropertyFilePathNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "FilePath";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task AddAsset_WhenInformationProjectInvalide_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newAsset = new AddAssetDto { };
            var Asset = new Asset { };
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(false));
            A.CallTo(() => _mapper.Map<Asset>(newAsset)).Returns(Asset);
            A.CallTo(() => _assetRepo.Add(Asset)).Returns(Task.FromResult<Asset>(null));

            // Act
            var result = await _controller.AddAsset(newAsset);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            statusCodeResult.Value.Should().Be("Project does not exist according to Asset information");
        }




        [Fact]
        public async Task AddAsset_WhenAddFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newAsset = new AddAssetDto { };
            var Asset = new Asset { };
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project,bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Asset>(newAsset)).Returns(Asset);
            A.CallTo(() => _assetRepo.Add(Asset)).Returns(Task.FromResult<Asset>(null));

            // Act
            var result = await _controller.AddAsset(newAsset);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error occurred while adding the asset.");
        }
        [Fact]
        public async Task AddAsset_WhenOccurredException_ShouldReturnsInternalServerError()
        {
            // Arrange
            var newAsset = new AddAssetDto { };
            var Asset = new Asset { };
            string messageException = "Privacy existed";
            var message = "An error occurred while adding Asset into Database " + messageException;
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Asset>(newAsset)).Returns(Asset);
            A.CallTo(() => _assetRepo.Add(Asset)).Throws(new Exception(messageException));

            // Act
            var result = await _controller.AddAsset(newAsset);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be(message);
        }
        [Fact]
        public async Task AddAsset_WhenAddSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var newAsset = new AddAssetDto { };
            var Asset = new Asset { };
            var addedAsset = new Asset { };
            A.CallTo(() => _projectRepo.CheckContainsAsync(A<Expression<Func<Project, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _mapper.Map<Asset>(newAsset)).Returns(Asset);
            A.CallTo(() => _assetRepo.Add(Asset)).Returns(Task.FromResult(addedAsset));

            // Act
            var result = await _controller.AddAsset(newAsset);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionAssetResponse;
            response.Message.Should().Be("Asset added successfully.");
            response.Id.Should().Be(addedAsset.Id);
        }




        #endregion

        #region UdpateAsset
        [Fact]
        public async Task UpdatAssetAsset_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new AssetsController(null,null, null);
            var updateAsset = new UpdateAssetDto();

            // Act
            var result = await controller.UpdateAsset(Guid.NewGuid(), updateAsset);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }
        public async Task UpdatAssetAsset_WhenAssetPropertyIdNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "Id";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task UpdatAssetAsset_WithIdInValidFomat_ReturnBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var invalidGuid = "e4d34798-4c18-4ca4-9014-191492e3b90"; // GUID sai định dạng
            controller.ModelState.AddModelError("id", $"The value '{invalidGuid}' is not valid.");

            // Act
            var result = await controller.GetAssetById(Guid.Empty);

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
        public async Task UpdatAssetAsset_WhenAssetPropertyAssestNameNull_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "AssestName";
            string message = $"The {key} field is required.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task UpdatAssetAsset_WhenAssetPropertyAssetNameLengthLonger255_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "AssestName";
            string messageError = $"The field {key} must be a string with a maximum length of 255.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task UpdatAssetAsset_WhenAssetPropertyTypeLengthLonger50_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo, _projectRepo, _mapper);
            var newAsset = new AddAssetDto { };
            string key = "Type";
            string messageError = $"The field {key} must be a string with a maximum length of 50.";
            controller.ModelState.AddModelError(key, messageError);

            // Act
            var result = await controller.AddAsset(newAsset);

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
        public async Task UpdatAssetAsset_WhenIdDoesNotMatch_ShouldReturnsBadRequest()
        {
            // Arrange
            var updateAsset = new UpdateAssetDto { Id = Guid.NewGuid() };
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateAsset(differentId, updateAsset);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Asset ID information does not match");
        }

        [Fact]
        public async Task UpdatAssetAsset_WhenAssetDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var updateAsset = new UpdateAssetDto { Id = Guid.NewGuid() };
            A.CallTo(() => _assetRepo.GetSingleByCondition(A<Expression<Func<Asset, bool>>>._, A<string[]>._)).Returns(Task.FromResult<Asset>(null));

            // Act
            var result = await _controller.UpdateAsset(updateAsset.Id, updateAsset);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Asset not found.");
        }
        [Fact]
        public async Task UpdatAssetAsset_WhenUpdateSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var updateAsset = new UpdateAssetDto { Id = Guid.NewGuid() };
            var existAsset = new Asset { Id = updateAsset.Id };
            A.CallTo(() => _assetRepo.GetSingleByCondition(A<Expression<Func<Asset, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existAsset));
            A.CallTo(() => _mapper.Map(updateAsset, existAsset));
            A.CallTo(() => _assetRepo.Update(existAsset)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateAsset(updateAsset.Id, updateAsset);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionAssetResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Asset updated successfully.");
            response.Id.Should().Be(existAsset.Id);
        }
        [Fact]
        public async Task UpdatAssetAsset_WhenUpdateFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var updateAsset = new UpdateAssetDto { Id = Guid.NewGuid() };
            var existAsset = new Asset { Id = updateAsset.Id };
            A.CallTo(() => _assetRepo.GetSingleByCondition(A<Expression<Func<Asset, bool>>>._, A<string[]>._))
                .Returns(Task.FromResult(existAsset));
            A.CallTo(() => _mapper.Map(updateAsset, existAsset));
            A.CallTo(() => _assetRepo.Update(existAsset)).Throws(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateAsset(updateAsset.Id, updateAsset);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error updating asset: Update failed");
        }


        #endregion

        #region DeleteAsset
        [Fact]
        public async Task DeleteAsset_WhenDependenciesAreNull_ShouldReturnsInternalServerError()
        {
            // Arrange
            var controller = new AssetsController(null,null, null);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteAsset(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task DeleteAsset_WhenAssetDoesNotExist_ShouldReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _assetRepo.CheckContainsAsync(A<Expression<Func<Asset, bool>>>._)).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteAsset(id);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Dont exist asset with id {id.ToString()} to delete");

        }

        [Fact]
        public async Task DeleteAsset_WhenDeleteSucceeds_ShouldReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _assetRepo.CheckContainsAsync(A<Expression<Func<Asset, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _assetRepo.DeleteMulti(A<Expression<Func<Asset, bool>>>._)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAsset(id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = okResult.Value as ActionAssetResponse;
            response.Should().NotBeNull();
            response.Message.Should().Be("Asset deleted successfully.");
            response.Id.Should().Be(id);
        }

        [Fact]
        public async Task DeleteAsset_WhenDeleteFails_ShouldReturnsInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            A.CallTo(() => _assetRepo.CheckContainsAsync(A<Expression<Func<Asset, bool>>>._)).Returns(Task.FromResult(true));
            A.CallTo(() => _assetRepo.DeleteMulti(A<Expression<Func<Asset, bool>>>._)).Throws(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteAsset(id);

            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            statusCodeResult.Value.Should().Be("Error delete asset: Delete failed");
        }
        [Fact]
        public async Task DeleteAsset_WhenAssetPropertyIdInvalidFomat_ShouldReturnsBadRequest()
        {
            // Arrange
            var controller = new AssetsController(_assetRepo,_projectRepo, _mapper);
            string key = "id";
            string message = "The value 'e4d34798-4c18-4ca4-9014-191492e3b90' is not valid.";
            controller.ModelState.AddModelError(key, message);

            // Act
            var result = await controller.DeleteAsset(Guid.NewGuid());

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
