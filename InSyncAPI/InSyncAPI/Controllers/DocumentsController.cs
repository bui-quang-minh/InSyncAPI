using AutoMapper;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;
using static InSyncAPI.Dtos.PageDto;
using System.Diagnostics;
using BusinessObjects.Models;
using static InSyncAPI.Dtos.DocumentDto;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private IDocumentRepository _documentRepo;
        private ILogger<DocumentsController> _logger;
        private readonly string TAG = nameof(DocumentsController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public DocumentsController(IDocumentRepository documentRepo, IMapper mapper
            , ILogger<DocumentsController> logger)
        {
            _documentRepo = documentRepo;
            _mapper = mapper;
            _logger = logger;

        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Document>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPages()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get documents at {RequestTime}.", DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            try
            {
                var response = _documentRepo.GetAll().AsQueryable();
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved documents in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving documents. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving documents: {ex.Message}");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewDocumentDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllPage(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all documents at {RequestTime}.", DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<Document> listPage = new List<Document>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listPage = _documentRepo.GetMulti(c => c.Title.ToLower().Contains(keySearch));
                    total = listPage.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listPage = _documentRepo.GetMultiPaging(
                        c => c.Title.ToLower().Contains(keySearch),
                        out total, index.Value, size.Value, null
                    );
                }

                var response = _mapper.Map<IEnumerable<ViewDocumentDto>>(listPage);
                var responsePaging = new ResponsePaging<IEnumerable<ViewDocumentDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved documents in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving documents. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving documents: {ex.Message}");
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewDocumentDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetPageById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get Document by ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var Document = await _documentRepo.GetSingleByCondition(c => c.Id.Equals(id));
                if (Document == null)
                {
                    _logger.LogWarning("No Document found with ID: {Id}.", id);
                    return NotFound("No Document has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewDocumentDto>(Document);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved Document with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving Document with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving Document: {ex.Message}");
            }
        }

        [HttpGet("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewDocumentDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetPageBySlug([FromRoute] string slug)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get Document by Slug: {Slug} at {RequestTime}.", slug, DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var Document = await _documentRepo.GetSingleByCondition(c => c.Slug.Equals(slug));
                if (Document == null)
                {
                    _logger.LogWarning("No Document found with Slug: {Slug}.", slug);
                    return NotFound("No Document has an Slug: " + slug);
                }

                var response = _mapper.Map<ViewDocumentDto>(Document);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved Document with Slug: {Id} in {ElapsedMilliseconds}ms.", slug, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving Document with Slug: {Slug}. Total time taken: {ElapsedMilliseconds}ms.", slug, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving Document: {ex.Message}");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddPage([FromBody] AddDocumentDto newPage)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add a new Document at {RequestTime}.", DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            Document Document = _mapper.Map<Document>(newPage);
            Document.DateCreated = DateTime.UtcNow;
            Document.DateUpdated = DateTime.UtcNow;

            try
            {
                var response = await _documentRepo.Add(Document);
                if (response == null)
                {
                    _logger.LogError("Failed to add the Document.");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the Document.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added Document with ID: {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPrivacyPolicyResponse { Message = "Document added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error occurred while adding Document into Database. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while adding Document into Database: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdatePage([FromRoute] Guid id, [FromBody] UpdateDocumentDto updatePage)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update Document with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updatePage.Id)
            {
                return BadRequest("Document ID information does not match");
            }

            // Fetch the existing Document to ensure it exists
            var existingPage = await _documentRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingPage == null)
            {
                _logger.LogWarning("Document with ID: {Id} not found.", id);
                return NotFound("Document not found.");
            }

            existingPage.DateUpdated = DateTime.UtcNow;

            // Map the updated fields
            _mapper.Map(updatePage, existingPage);

            try
            {
                await _documentRepo.Update(existingPage);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated Document with ID: {Id} in {ElapsedMilliseconds}ms.", existingPage.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPrivacyPolicyResponse { Message = "Document updated successfully.", Id = existingPage.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating Document with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating Document: {ex.Message}");
            }
        }

        [HttpPut("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdatePageBySlug([FromRoute] string slug, [FromBody] UpdateDocumentDto updatePage)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update Document with Slug: {Id} at {RequestTime}.", slug, DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Fetch the existing Document to ensure it exists
            var existingPage = await _documentRepo.GetSingleByCondition(c => c.Slug.Equals(slug));

            if (existingPage == null)
            {
                _logger.LogWarning("Document with Slug: {Slug} not found.", slug);
                return NotFound("Document not found.");
            }

            existingPage.DateUpdated = DateTime.UtcNow;

            // Map the updated fields
            _mapper.Map(updatePage, existingPage);

            try
            {
                await _documentRepo.Update(existingPage);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated Document with ID: {Id} in {ElapsedMilliseconds}ms.", existingPage.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPrivacyPolicyResponse { Message = "Document updated successfully.", Id = existingPage.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating Document with Slug: {Slug}. Total time taken: {ElapsedMilliseconds}ms.", slug, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating Document: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeletePage([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to delete Document with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_documentRepo == null || _mapper == null)
            {
                _logger.LogError("Document repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkPolicyExist = await _documentRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                _logger.LogWarning("Document with ID: {Id} does not exist.", id);
                return NotFound($"Don't exist Document with ID {id} to delete");
            }

            try
            {
                await _documentRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation("Successfully deleted Document with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPrivacyPolicyResponse { Message = "Document deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting Document with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error deleting Document: {ex.Message}");
            }
        }
    }
}

