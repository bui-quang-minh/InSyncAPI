using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using InSyncAPI.JwtServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositories;
using System.Diagnostics;
using static InSyncAPI.Dtos.PageDto;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagesController : ControllerBase
    {
        private IPageRepository _pageRepo;
        private ILogger<PagesController> _logger;
        private readonly string TAG = nameof(PagesController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public PagesController(IPageRepository pageRepo, IMapper mapper
            , ILogger<PagesController> logger)
        {
            _pageRepo = pageRepo;
            _mapper = mapper;
            _logger = logger;

        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Page>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPages()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get pages at {RequestTime}.", DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            try
            {
                var response = _pageRepo.GetAll().AsQueryable();
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved pages in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving pages. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving pages: {ex.Message}");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewPageDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllPage(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all pages at {RequestTime}.", DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<Page> listPage = new List<Page>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listPage = _pageRepo.GetMulti(c => c.Title.ToLower().Contains(keySearch));
                    total = listPage.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listPage = _pageRepo.GetMultiPaging(
                        c => c.Title.ToLower().Contains(keySearch),
                        out total, index.Value, size.Value, null
                    );
                }

                var response = _mapper.Map<IEnumerable<ViewPageDto>>(listPage);
                var responsePaging = new ResponsePaging<IEnumerable<ViewPageDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved pages in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving pages. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving pages: {ex.Message}");
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPageDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetPageById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get Page by ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var Page = await _pageRepo.GetSingleByCondition(c => c.Id.Equals(id));
                if (Page == null)
                {
                    _logger.LogWarning("No Page found with ID: {Id}.", id);
                    return NotFound("No Page has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewPageDto>(Page);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved Page with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving Page with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving Page: {ex.Message}");
            }
        }

        [HttpGet("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPageDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetPageBySlug([FromRoute] string slug)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get Page by Slug: {Slug} at {RequestTime}.", slug, DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var Page = await _pageRepo.GetSingleByCondition(c => c.Slug.Equals(slug));
                if (Page == null)
                {
                    _logger.LogWarning("No Page found with Slug: {Slug}.", slug);
                    return NotFound("No Page has an Slug: " + slug);
                }

                var response = _mapper.Map<ViewPageDto>(Page);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved Page with Slug: {Id} in {ElapsedMilliseconds}ms.", slug, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving Page with Slug: {Slug}. Total time taken: {ElapsedMilliseconds}ms.", slug, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving Page: {ex.Message}");
            }
        }

        [AdminAuthorization]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddPage([FromBody] AddPageDto newPage)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add a new Page at {RequestTime}.", DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            Page Page = _mapper.Map<Page>(newPage);
            Page.DateCreated = DateTime.UtcNow;
            Page.DateUpdated = DateTime.UtcNow;

            try
            {
                var response = await _pageRepo.Add(Page);
                if (response == null)
                {
                    _logger.LogError("Failed to add the Page.");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the Page.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added Page with ID: {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPageResponse { Message = "Page added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error occurred while adding Page into Database. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while adding Page into Database: {ex.Message}");
            }
        }

        [AdminAuthorization]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdatePage([FromRoute] Guid id, [FromBody] UpdatePageDto updatePage)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update Page with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updatePage.Id)
            {
                return BadRequest("Page ID information does not match");
            }

            // Fetch the existing Page to ensure it exists
            var existingPage = await _pageRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingPage == null)
            {
                _logger.LogWarning("Page with ID: {Id} not found.", id);
                return NotFound("Page not found.");
            }

            existingPage.DateUpdated = DateTime.UtcNow;

            // Map the updated fields
            _mapper.Map(updatePage, existingPage);

            try
            {
                await _pageRepo.Update(existingPage);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated Page with ID: {Id} in {ElapsedMilliseconds}ms.", existingPage.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPageResponse { Message = "Page updated successfully.", Id = existingPage.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating Page with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating Page: {ex.Message}");
            }
        }

        [AdminAuthorization]
        [HttpPut("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdatePageBySlug([FromRoute] string slug, [FromBody] UpdatePageDto updatePage)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update Page with Slug: {Id} at {RequestTime}.", slug, DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            // Fetch the existing Page to ensure it exists
            var existingPage = await _pageRepo.GetSingleByCondition(c => c.Slug.Equals(slug) &&  c.Id.Equals(updatePage.Id));

            if (existingPage == null)
            {
                _logger.LogWarning("Page with Slug: {Slug} not found.", slug);
                return NotFound("Page not found.");
            }

            existingPage.DateUpdated = DateTime.UtcNow;

            // Map the updated fields
            _mapper.Map(updatePage, existingPage);

            try
            {
                await _pageRepo.Update(existingPage);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated Page with ID: {Id} in {ElapsedMilliseconds}ms.", existingPage.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPageResponse { Message = "Page updated successfully.", Id = existingPage.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating Page with Slug: {Slug}. Total time taken: {ElapsedMilliseconds}ms.", slug, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating Page: {ex.Message}");
            }
        }

        [AdminAuthorization]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPageResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeletePage([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to delete Page with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_pageRepo == null || _mapper == null)
            {
                _logger.LogError("Page repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkPolicyExist = await _pageRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                _logger.LogWarning("Page with ID: {Id} does not exist.", id);
                return NotFound($"Don't exist Page with ID {id} to delete");
            }

            try
            {
                await _pageRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation("Successfully deleted Page with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPageResponse { Message = "Page deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting Page with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error deleting Page: {ex.Message}");
            }
        }
    }
}

