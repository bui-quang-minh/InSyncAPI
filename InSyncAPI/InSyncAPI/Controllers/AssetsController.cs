using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.VisualBasic;
using Repositorys;
using System.Diagnostics;
using System.Linq;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private IAssetRepository _assestRepo;
        private IProjectRepository _projectRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private ILogger<AssetsController> _logger;
        private string[] includes = new string[]
        {
            nameof(Asset.Project)
        };
        private readonly string TAG = nameof(AssetsController) + " - ";

        public AssetsController(IAssetRepository assestRepo, IProjectRepository projectRepo,
            IMapper mapper, ILogger<AssetsController> logger)
        {
            _assestRepo = assestRepo;
            _projectRepo = projectRepo;
            _mapper = mapper;
            _logger = logger;
        }


        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Asset>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAssets()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to retrieve all assets at {RequestTime}", DateTime.UtcNow);

            if (_assestRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Asset repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            try
            {
                _logger.LogInformation(TAG + "Fetching assets from the database.");

                var response = _assestRepo.GetAll().AsQueryable();

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved {AssetCount} assets from the database in {ElapsedMilliseconds}ms.",
                    response.Count(), stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving assets. Total time taken: {ElapsedMilliseconds}ms.",
                    stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "An error occurred while retrieving assets.");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewAssetDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllAsset([FromQuery] int? index, [FromQuery] int? size, [FromQuery] string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to get all assets at {RequestTime}", DateTime.UtcNow);

            if (_assestRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Asset repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<Asset> listAssets = new List<Asset>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    _logger.LogInformation(TAG + "Fetching all assets with search key: {KeySearch}.", keySearch);
                    listAssets = _assestRepo.GetMulti(c => c.AssetName.ToLower().Contains(keySearch));
                    total = listAssets.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

                    _logger.LogInformation(TAG + "Fetching assets with pagination. Index: {Index}, Size: {Size}, KeySearch: {KeySearch}.",
                        index, size, keySearch);
                    listAssets = _assestRepo.GetMultiPaging(c => c.AssetName.ToLower().Contains(keySearch),
                        out total, index.Value, size.Value);
                }

                var response = _mapper.Map<IEnumerable<ViewAssetDto>>(listAssets);
                var responsePaging = new ResponsePaging<IEnumerable<ViewAssetDto>>()
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved {AssetCount} assets in {ElapsedMilliseconds}ms.",
                    total, stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving assets. Total time taken: {ElapsedMilliseconds}ms.",
                    stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "An error occurred while retrieving assets.");
            }
        }

        [HttpGet("asset-project/{idProject}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewAssetDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllAssetOfProject([FromRoute] Guid idProject, [FromQuery] int? index, [FromQuery] int? size, [FromQuery] string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to get all assets for project ID: {IdProject} at {RequestTime}", idProject, DateTime.UtcNow);

            if (_assestRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Asset repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Invalid model state for project ID: {IdProject}.", idProject);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            IEnumerable<Asset> listAssets = new List<Asset>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    _logger.LogInformation(TAG + "Fetching all assets for project ID: {IdProject} with search key: {KeySearch}.", idProject, keySearch);
                    listAssets = _assestRepo.GetMulti(c => c.ProjectId.Equals(idProject) && c.AssetName.ToLower().Contains(keySearch));
                    total = listAssets.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

                    _logger.LogInformation(TAG + "Fetching assets for project ID: {IdProject} with pagination. Index: {Index}, Size: {Size}, KeySearch: {KeySearch}.",
                        idProject, index, size, keySearch);
                    listAssets = _assestRepo.GetMultiPaging(c => c.ProjectId.Equals(idProject) && c.AssetName.ToLower().Contains(keySearch),
                        out total, index.Value, size.Value);
                }

                var response = _mapper.Map<IEnumerable<ViewAssetDto>>(listAssets);
                var responsePaging = new ResponsePaging<IEnumerable<ViewAssetDto>>()
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved {AssetCount} assets for project ID: {IdProject} in {ElapsedMilliseconds}ms.",
                    total, idProject, stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving assets for project ID: {IdProject}. Total time taken: {ElapsedMilliseconds}ms.",
                    idProject, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "An error occurred while retrieving assets.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewAssetDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetAssetById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to get asset by ID: {IdAsset} at {RequestTime}", id, DateTime.UtcNow);

            if (_assestRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Asset repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Invalid model state for asset ID: {IdAsset}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var asset = await _assestRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
                if (asset == null)
                {
                    _logger.LogWarning(TAG + "No asset found with ID: {IdAsset}.", id);
                    return NotFound("No asset has an ID : " + id.ToString());
                }

                var response = _mapper.Map<ViewAssetDto>(asset);
                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved asset ID: {IdAsset} in {ElapsedMilliseconds}ms.",
                    id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving asset ID: {IdAsset}. Total time taken: {ElapsedMilliseconds}ms.",
                    id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "An error occurred while retrieving asset.");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionAssetResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddAsset([FromBody] AddAssetDto newAsset)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to add a new asset at {RequestTime}.", DateTime.UtcNow);

            if (_assestRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Asset repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Invalid model state for new asset.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkExistProject = await _projectRepo.CheckContainsAsync(p => p.Id.Equals(newAsset.ProjectId));
            if (!checkExistProject)
            {
                _logger.LogWarning(TAG + "Project with ID {ProjectId} does not exist.", newAsset.ProjectId);
                return BadRequest("Project does not exist according to Asset information");
            }

            var asset = _mapper.Map<Asset>(newAsset);
            asset.DateCreated = DateTime.UtcNow;


            try
            {
                var response = await _assestRepo.Add(asset);
                if (response == null)
                {
                    _logger.LogError(TAG + "Error occurred while adding the asset.");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the asset.");
                }

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully added asset with ID: {AssetId} in {ElapsedMilliseconds}ms.",
                    response.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionAssetResponse { Message = "Asset added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while adding asset to the database. Total time taken: {ElapsedMilliseconds}ms.",
                    stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "An error occurred while adding Asset into Database.");
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionAssetResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateAsset(Guid id, UpdateAssetDto updateAsset)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received request to update asset with ID: {AssetId} at {RequestTime}.", id, DateTime.UtcNow);

            if (_assestRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Asset repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Invalid model state for asset update.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateAsset.Id)
            {
                _logger.LogWarning(TAG + "Asset ID information does not match. Expected: {ExpectedId}, Provided: {ProvidedId}", id, updateAsset.Id);
                return BadRequest("Asset ID information does not match");
            }

            var existingAsset = await _assestRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingAsset == null)
            {
                _logger.LogWarning(TAG + "Asset with ID {AssetId} not found.", id);
                return NotFound("Asset not found.");
            }

            existingAsset.DateUpdated = DateTime.UtcNow;
            _mapper.Map(updateAsset, existingAsset);

            try
            {
                await _assestRepo.Update(existingAsset);
                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully updated asset with ID: {AssetId} in {ElapsedMilliseconds}ms.", existingAsset.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionAssetResponse { Message = "Asset updated successfully.", Id = existingAsset.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "Error updating asset with ID: {AssetId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating asset: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionAssetResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteAsset(Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received request to delete asset with ID: {AssetId} at {RequestTime}.", id, DateTime.UtcNow);

            if (_assestRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Asset repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Invalid model state for asset deletion.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkAssetExist = await _assestRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkAssetExist)
            {
                _logger.LogWarning(TAG + "Asset with ID {AssetId} does not exist.", id);
                return NotFound($"Don't exist asset with id {id.ToString()} to delete");
            }

            try
            {
                await _assestRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully deleted asset with ID: {AssetId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionAssetResponse { Message = "Asset deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "Error deleting asset with ID: {AssetId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error delete asset: {ex.Message}");
            }
        }


    }
}

