using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositorys;
using System.Diagnostics;

namespace InSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TutorialsController : ControllerBase
    {
        private ITutorialRepository _tutorialRepo;
        private ILogger<TutorialsController> _logger;
        private readonly string TAG = nameof(TutorialsController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public TutorialsController(ITutorialRepository tutorialRepo, IMapper mapper
            , ILogger<TutorialsController> logger)
        {
            _tutorialRepo = tutorialRepo;
            _mapper = mapper;
            _logger = logger;

        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Tutorial>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetTutorials()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to get tutorials at {RequestTime}", DateTime.UtcNow);

            if (_tutorialRepo == null || _mapper == null)
            {
                _logger.LogError("Tutorial repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            try
            {
                var response = _tutorialRepo.GetAll().AsQueryable();
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved tutorials in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving tutorials in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving tutorials: {ex.Message}");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewTutorialDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllTutorials(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to get all tutorials at {RequestTime}", DateTime.UtcNow);

            if (_tutorialRepo == null || _mapper == null)
            {
                _logger.LogError("Tutorial repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            IEnumerable<Tutorial> listTutorial = new List<Tutorial>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listTutorial = _tutorialRepo.GetMulti(c => c.Title.ToLower().Contains(keySearch));
                    total = listTutorial.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

                    listTutorial = _tutorialRepo.GetMultiPaging(c => c.Title.ToLower().Contains(keySearch), out total, index.Value, size.Value, null);
                }

                var response = _mapper.Map<IEnumerable<ViewTutorialDto>>(listTutorial);
                var responsePaging = new ResponsePaging<IEnumerable<ViewTutorialDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved tutorials in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving tutorials in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving tutorials: {ex.Message}");
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewCustomerReviewDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetTutorialById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received a request to get tutorial by ID: {TutorialId} at {RequestTime}", id, DateTime.UtcNow);

            if (_tutorialRepo == null || _mapper == null)
            {
                _logger.LogError("Tutorial repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for tutorial ID: {TutorialId}.", id);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var tutorial = await _tutorialRepo.GetSingleByCondition(c => c.Id.Equals(id));
                if (tutorial == null)
                {
                    _logger.LogInformation("No tutorial found with ID: {TutorialId}.", id);
                    return NotFound($"No tutorial has an ID: {id}");
                }

                var response = _mapper.Map<ViewTutorialDto>(tutorial);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved tutorial with ID: {TutorialId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving tutorial with ID: {TutorialId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving tutorial: {ex.Message}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTutorialResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddTutorial([FromBody] AddTutorialDto newTutorial)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add a new tutorial at {RequestTime}", DateTime.UtcNow);

            if (_tutorialRepo == null || _mapper == null)
            {
                _logger.LogError("Tutorial repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for new tutorial: {@NewTutorial}.", newTutorial);
                return BadRequest(ModelState);
            }

            Tutorial tutorial = _mapper.Map<Tutorial>(newTutorial);
            tutorial.DateCreated = DateTime.Now;

            try
            {
                var response = await _tutorialRepo.Add(tutorial);
                if (response == null)
                {
                    _logger.LogError("Failed to add tutorial: {@Tutorial}.", tutorial);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding the tutorial.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added tutorial with ID: {TutorialId} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionTutorialResponse { Message = "Tutorial added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error adding tutorial: {@Tutorial} in {ElapsedMilliseconds}ms.", tutorial, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding Tutorial into Database: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTutorialResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateTutorial([FromRoute] Guid id, [FromBody] UpdateTutorialDto updateTutorial)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update tutorial with ID: {TutorialId} at {RequestTime}", id, DateTime.UtcNow);

            if (_tutorialRepo == null || _mapper == null)
            {
                _logger.LogError("Tutorial repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for tutorial update: {@UpdateTutorial}.", updateTutorial);
                return BadRequest(ModelState);
            }

            if (id != updateTutorial.Id)
            {
                _logger.LogWarning("Tutorial ID mismatch: expected {ExpectedId}, received {ReceivedId}.", id, updateTutorial.Id);
                return BadRequest("Tutorial ID information does not match");
            }

            var existingTutorial = await _tutorialRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingTutorial == null)
            {
                _logger.LogWarning("Tutorial not found for ID: {TutorialId}.", id);
                return NotFound("Tutorial not found.");
            }

            existingTutorial.DateUpdated = DateTime.Now;
            _mapper.Map(updateTutorial, existingTutorial);

            try
            {
                await _tutorialRepo.Update(existingTutorial);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated tutorial with ID: {TutorialId} in {ElapsedMilliseconds}ms.", existingTutorial.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionTutorialResponse { Message = "Tutorial updated successfully.", Id = existingTutorial.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating tutorial with ID: {TutorialId} in {ElapsedMilliseconds}ms.", existingTutorial.Id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating tutorial: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTutorialResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteTutorial([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to delete tutorial with ID: {TutorialId} at {RequestTime}", id, DateTime.UtcNow);

            if (_tutorialRepo == null || _mapper == null)
            {
                _logger.LogError("Tutorial repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Application service has not been created");
            }

            var checkTutorialExist = await _tutorialRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkTutorialExist)
            {
                _logger.LogWarning("Attempted to delete a non-existent tutorial with ID: {TutorialId}.", id);
                return NotFound($"Don't exist tutorial with id {id} to delete");
            }

            try
            {
                await _tutorialRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation("Successfully deleted tutorial with ID: {TutorialId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionTutorialResponse { Message = "Tutorial deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting tutorial with ID: {TutorialId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error delete tutorial: {ex.Message}");
            }
        }

    }


}

