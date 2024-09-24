using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;

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
        private string[] includes = new string[]
        {
            nameof(Asset.Project)
        };

        public AssetsController(IAssetRepository assestRepo,IProjectRepository projectRepo,
            IMapper mapper)
        {
            _assestRepo = assestRepo;
            _projectRepo = projectRepo;
            _mapper = mapper;
        }


        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Asset>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAssets()
        {
            if (_assestRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _assestRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewAssetDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllAsset(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_assestRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            
            var listAssets = _assestRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewAssetDto>>(listAssets);
            var responsePaging = new ResponsePaging<IEnumerable<ViewAssetDto>>() 
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }
        [HttpGet("asset-project/{idProject}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewAssetDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllAssetOfProject(Guid idProject, int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_assestRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listAssets = _assestRepo.GetMultiPaging(c => c.ProjectId.Equals(idProject), out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewAssetDto>>(listAssets);
            var responsePaging = new ResponsePaging<IEnumerable<ViewAssetDto>>()
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewAssetDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetAssetById(Guid id)
        {
            if (_assestRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var asset = await _assestRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
            if (asset == null)
            {
                return NotFound("No asset has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewAssetDto>(asset);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionAssetResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddAsset(AddAssetDto newAsset)
        {
            if (_assestRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkExistProject = await _projectRepo.CheckContainsAsync(p => p.Id.Equals(newAsset.ProjectId));
            if(!checkExistProject)
            {
                return NotFound("Project does not exist according to Asset information");
            }
            var asset = _mapper.Map<Asset>(newAsset);
            asset.DateCreated = DateTime.Now;
            try
            {
                var response = await _assestRepo.Add(asset);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the asset.");
                }
                return Ok(new ActionAssetResponse { Message = "Asset added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding Asset into Database " + ex.Message);
            }   
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionAssetResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateAsset(Guid id, UpdateAssetDto updateAsset)
        {
            if (_assestRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            if (id != updateAsset.Id)
            {
                return BadRequest("Asset ID information does not match");
            }

            // Fetch the existing customer review to ensure it exists
            var existingAsset = await _assestRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingAsset == null)
            {
                return NotFound("Asset not found.");
            }
            existingAsset.DateUdpated = DateTime.Now;
            // Map the updated fields
            _mapper.Map(updateAsset, existingAsset);

            try
            {
                await _assestRepo.Update(existingAsset);
                return Ok(new ActionAssetResponse { Message = "Asset updated successfully.", Id = existingAsset.Id });
            }
            catch (Exception ex)
            {
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
            if (_assestRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkAssetExist = await _assestRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkAssetExist)
            {
                return NotFound($"Dont exist asset with id {id.ToString()} to delete");
            }

            try
            {
                await _assestRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionAssetResponse { Message = "Asset deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"Error delete asset: {ex.Message}");
            }

        }
    }
}
