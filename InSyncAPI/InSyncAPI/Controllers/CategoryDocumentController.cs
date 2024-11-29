using AutoMapper;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositories;
using static InSyncAPI.Dtos.PageDto;
using System.Diagnostics;
using BusinessObjects.Models;
using static InSyncAPI.Dtos.CategoryDocumentDto;
using InSyncAPI.JwtServices;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryDocumentController : ControllerBase
    {
        private ICategoryDocumentRepository _cateRepo;
        private ILogger<CategoryDocumentController> _logger;
        private readonly string TAG = nameof(PagesController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private readonly string[] includes = new string[] { nameof(CategoryDocument.Documents) };

        public CategoryDocumentController(ICategoryDocumentRepository CateRepo, IMapper mapper
            , ILogger<CategoryDocumentController> logger)
        {
            _cateRepo = CateRepo;
            _mapper = mapper;
            _logger = logger;

        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<CategoryDocument>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetCategoryDocuments()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get category documents at {RequestTime}.", DateTime.UtcNow);

            if (_cateRepo == null || _mapper == null)
            {
                _logger.LogError("Category Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            try
            {
                var listCate = _cateRepo.GetAll();
                var response = OrderCategoryAndDocument(listCate).AsQueryable();
               

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved category documents in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving category documents. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving category documents: {ex.Message}");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewCategoryDocumentDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllCategoryDocument(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all category documents at {RequestTime}.", DateTime.UtcNow);

            if (_cateRepo == null || _mapper == null)
            {
                _logger.LogError("Category Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<CategoryDocument> listPage = new List<CategoryDocument>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listPage = _cateRepo.GetMulti(c => c.Title.ToLower().Contains(keySearch), includes);
                    total = listPage.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listPage = _cateRepo.GetMultiPaging(
                        c => c.Title.ToLower().Contains(keySearch),
                        out total, index.Value, size.Value, includes
                    );
                }
                listPage = OrderCategoryAndDocument(listPage);
                var response = _mapper.Map<IEnumerable<ViewCategoryDocumentDto>>(listPage);
                
                var responsePaging = new ResponsePaging<IEnumerable<ViewCategoryDocumentDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved category documents in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving category documents. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving category documents: {ex.Message}");
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewCategoryDocumentDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetCategoryDocumentById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get Category Document by ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_cateRepo == null || _mapper == null)
            {
                _logger.LogError("Category Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var CategoryDocument = await _cateRepo.GetSingleByCondition(c => c.Id.Equals(id), includes);
                if (CategoryDocument == null)
                {
                    _logger.LogWarning("No Category Document found with ID: {Id}.", id);
                    return NotFound("No Category Document has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewCategoryDocumentDto>(CategoryDocument);
                response.Documents = response.Documents.OrderBy(c => c.Order).ThenBy(c => c.Title);

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved Category Document with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving Category Document with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving Category Document: {ex.Message}");
            }
        }

        [AdminAuthorization]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCategoryDocumentResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddCategoryDocument([FromBody] AddCategoryDocumentDto newCate)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add a new Category Document at {RequestTime}.", DateTime.UtcNow);

            if (_cateRepo == null || _mapper == null)
            {
                _logger.LogError("Category Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            CategoryDocument cate = _mapper.Map<CategoryDocument>(newCate);
            cate.DateCreated = DateTime.UtcNow;
            cate.DateUpdated = DateTime.UtcNow;

            try
            {
                var response = await _cateRepo.Add(cate);
                if (response == null)
                {
                    _logger.LogError("Failed to add the Category Document.");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the Category Document.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added Category Document with ID: {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionCategoryDocumentResponse { Message = "Category Document added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error occurred while adding Category Document into Database. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while adding category document into Database: {ex.Message}");
            }
        }

        [AdminAuthorization]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCategoryDocumentResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateCategoryDocument([FromRoute] Guid id, [FromBody] UpdateCategoryDocumentDto updateCate)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update Category Document with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_cateRepo == null || _mapper == null)
            {
                _logger.LogError("Category Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateCate.Id)
            {
                return BadRequest("Category Document ID information does not match");
            }

            // Fetch the existing Category Document to ensure it exists
            var existingCate = await _cateRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingCate == null)
            {
                _logger.LogWarning("Category Document with ID: {Id} not found.", id);
                return NotFound("Category Document not found.");
            }

            existingCate.DateUpdated = DateTime.UtcNow;

            // Map the updated fields
            _mapper.Map(updateCate, existingCate);

            try
            {
                await _cateRepo.Update(existingCate);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated Category Document with ID: {Id} in {ElapsedMilliseconds}ms.", existingCate.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionCategoryDocumentResponse { Message = "Category Document updated successfully.", Id = existingCate.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating Category Document with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating Category Document: {ex.Message}");
            }
        }

        [AdminAuthorization]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCategoryDocumentResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteCategoryDocument([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to delete Category Document with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_cateRepo == null || _mapper == null)
            {
                _logger.LogError("Category Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkCateExist = await _cateRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkCateExist)
            {
                _logger.LogWarning("Category Document with ID: {Id} does not exist.", id);
                return NotFound($"Don't exist Category Document with ID {id} to delete");
            }

            try
            {
                await _cateRepo.DeleteCategoryDocument(id);
                stopwatch.Stop();
                _logger.LogInformation("Successfully deleted Category Document with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionCategoryDocumentResponse { Message = "Category Document deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting Category Document with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error deleting Category Document: {ex.Message}");
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("function_order")]
        public IEnumerable<CategoryDocument> OrderCategoryAndDocument(IEnumerable<CategoryDocument> categories)
        {
            foreach (var category in categories)
            {
                category.Documents = category.Documents.OrderBy(c => c.Order).ThenBy(c => c.Title).ToList();
            }
            categories = categories.OrderBy(c => c.Order).ThenBy(c => c.Title);
            return categories;
        }
    }
}
