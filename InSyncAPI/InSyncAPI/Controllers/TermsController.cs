using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Authentications;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Repositorys;
using System.Diagnostics;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermsController : ControllerBase
    {
        private ITermRepository _termRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private string TAG = "TermsController" + " - ";
        private ILogger<TermsController> _logger;

        public TermsController(ITermRepository termRepo, IMapper mapper, ILogger<TermsController> logger)
        {
            _termRepo = termRepo;
            _mapper = mapper;
            _logger = logger;
        }


        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Term>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetTerms()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to retrieve all terms at {RequestTime}", DateTime.UtcNow);

            if (_termRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Term repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            try
            {
                _logger.LogInformation(TAG + "Fetching terms from the database.");

                string[] includes = new string[] { };
                var response = _termRepo.GetAll(includes).AsQueryable();

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved {TermCount} terms from the database in {ElapsedMilliseconds}ms.",
                    response.Count(), stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving terms. Total time taken: {ElapsedMilliseconds}ms.",
                    stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "An error occurred while retrieving terms.");
            }
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewTermDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllTerms(int? index, int? size)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to retrieve all terms with index {Index}, size {Size} at {RequestTime}", index, size, DateTime.UtcNow);

            if (_termRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Term repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            try
            {
                IEnumerable<Term> listTerms = new List<Term>();
                int total = 0;

                if (index == null || size == null)
                {
                    listTerms = _termRepo.GetMulti(
                        c => true
                    );
                    total = listTerms.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listTerms = _termRepo.GetMultiPaging(c => true,
                        out total, index.Value, size.Value
                    );
                }

                var response = _mapper.Map<IEnumerable<ViewTermDto>>(listTerms);
                var responsePaging = new ResponsePaging<IEnumerable<ViewTermDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved terms with a total of {TotalTerms} terms in {ElapsedMilliseconds}ms.", total, stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving terms. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving terms.");
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewTermDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetTermById([FromRoute]Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to retrieve term with ID {TermId} at {RequestTime}", id, DateTime.UtcNow);

            if (_termRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Term repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Model state is invalid.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var term = await _termRepo.GetSingleByCondition(c => c.Id.Equals(id));

                if (term == null)
                {
                    _logger.LogWarning(TAG + "No term found with ID {TermId}.", id);
                    return NotFound("No term has an ID : " + id.ToString());
                }

                var response = _mapper.Map<ViewTermDto>(term);

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved term with ID {TermId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while retrieving the term with ID {TermId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the term.");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTermResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddTerm([FromBody] AddTermsDto newTerm)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to add a new term at {RequestTime}", DateTime.UtcNow);

            if (_termRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Term repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                Term term = _mapper.Map<Term>(newTerm);
                term.DateCreated = DateTime.UtcNow;
                term.DateUpdated = DateTime.UtcNow;

                _logger.LogInformation(TAG + "Attempting to add a new term to the database.");

                var response = await _termRepo.Add(term);

                if (response == null)
                {
                    _logger.LogError(TAG + "Failed to add the term to the database.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the term.");
                }

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully added a new term with ID {TermId} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionTermResponse { Message = "Term added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while adding the term. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding Term into Database: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTermResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateTerm([FromRoute] Guid id,[FromBody] UpdateTermsDto updateTerm)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to update term with ID {TermId} at {RequestTime}", id, DateTime.UtcNow);

            if (_termRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Term repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Model state is invalid.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateTerm.Id)
            {
                _logger.LogWarning(TAG + "Term ID in the request body ({BodyId}) does not match the URL parameter ({UrlId}).", updateTerm.Id, id);
                return BadRequest("Term ID information does not match");
            }

            try
            {
                // Fetch the existing term to ensure it exists
                var existingTerm = await _termRepo.GetSingleByCondition(c => c.Id.Equals(id));
                if (existingTerm == null)
                {
                    _logger.LogWarning(TAG + "No term found with ID {TermId}.", id);
                    return NotFound("Term not found.");
                }

                existingTerm.DateUpdated = DateTime.UtcNow;

                // Map the updated fields
                _mapper.Map(updateTerm, existingTerm);

                _logger.LogInformation(TAG + "Updating term with ID {TermId}.", id);
                await _termRepo.Update(existingTerm);

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully updated term with ID {TermId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionTermResponse { Message = "Term updated successfully.", Id = existingTerm.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while updating term with ID {TermId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating term: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTermResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeleteTerm([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received a request to delete term with ID {TermId} at {RequestTime}", id, DateTime.UtcNow);

            if (_termRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Term repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(TAG + "Model state is invalid.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkTermExist = await _termRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkTermExist)
            {
                _logger.LogWarning(TAG + "No term found with ID {TermId} to delete.", id);
                return NotFound($"Dont exist term with id {id.ToString()} to delete");
            }

            try
            {
                _logger.LogInformation(TAG + "Attempting to delete term with ID {TermId}.", id);
                await _termRepo.DeleteMulti(c => c.Id.Equals(id));

                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully deleted term with ID {TermId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionTermResponse { Message = "Term deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "An error occurred while deleting term with ID {TermId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting term: {ex.Message}");
            }
        }
    }

}

