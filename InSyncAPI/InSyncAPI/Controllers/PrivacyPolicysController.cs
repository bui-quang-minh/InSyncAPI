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
    public class PrivacyPolicysController : ControllerBase
    {
        private IPrivacyPolicyRepository _privacyPolicyRepo;
        private ILogger<PrivacyPolicysController> _logger;
        private readonly string TAG = nameof(PrivacyPolicysController) + " - ";
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public PrivacyPolicysController(IPrivacyPolicyRepository privacyPolicyRepo, IMapper mapper
            , ILogger<PrivacyPolicysController> logger)
        {
            _privacyPolicyRepo = privacyPolicyRepo;
            _mapper = mapper;
            _logger = logger;

        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<ViewPrivacyPolicyDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPrivacyPolicys()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get privacy policies at {RequestTime}.", DateTime.UtcNow);

            if (_privacyPolicyRepo == null || _mapper == null)
            {
                _logger.LogError("Privacy policy repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            try
            {
                var response = _privacyPolicyRepo.GetAll().AsQueryable();
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved privacy policies in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving privacy policies. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving privacy policies: {ex.Message}");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewPrivacyPolicyDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllPrivacyPolicy(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all privacy policies at {RequestTime}.", DateTime.UtcNow);

            if (_privacyPolicyRepo == null || _mapper == null)
            {
                _logger.LogError("Privacy policy repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<PrivacyPolicy> listPrivacyPolicy = new List<PrivacyPolicy>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listPrivacyPolicy = _privacyPolicyRepo.GetMulti(c => c.Title.ToLower().Contains(keySearch));
                    total = listPrivacyPolicy.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listPrivacyPolicy = _privacyPolicyRepo.GetMultiPaging(
                        c => c.Title.ToLower().Contains(keySearch),
                        out total, index.Value, size.Value, null
                    );
                }

                var response = _mapper.Map<IEnumerable<ViewPrivacyPolicyDto>>(listPrivacyPolicy);
                var responsePaging = new ResponsePaging<IEnumerable<ViewPrivacyPolicyDto>>
                {
                    data = response,
                    totalOfData = total
                };

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved privacy policies in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving privacy policies. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving privacy policies: {ex.Message}");
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPrivacyPolicyDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetPrivacyPolicyById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get privacy policy by ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_privacyPolicyRepo == null || _mapper == null)
            {
                _logger.LogError("Privacy policy repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var privacyPolicy = await _privacyPolicyRepo.GetSingleByCondition(c => c.Id.Equals(id));
                if (privacyPolicy == null)
                {
                    _logger.LogWarning("No privacy policy found with ID: {Id}.", id);
                    return NotFound("No privacy policy has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewPrivacyPolicyDto>(privacyPolicy);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved privacy policy with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving privacy policy with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving privacy policy: {ex.Message}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddPrivacyPolicy([FromBody] AddPrivacyPolicyDto newPrivacy)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add a new privacy policy at {RequestTime}.", DateTime.UtcNow);

            if (_privacyPolicyRepo == null || _mapper == null)
            {
                _logger.LogError("Privacy policy repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            PrivacyPolicy privacyPolicy = _mapper.Map<PrivacyPolicy>(newPrivacy);
            privacyPolicy.DateCreated = DateTime.Now;

            try
            {
                var response = await _privacyPolicyRepo.Add(privacyPolicy);
                if (response == null)
                {
                    _logger.LogError("Failed to add the privacy policy.");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the privacy policy.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added privacy policy with ID: {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPrivacyPolicyResponse { Message = "Privacy policy added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error occurred while adding privacy policy into Database. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while adding privacy policy into Database: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdatePrivacyPolicy([FromRoute] Guid id, [FromBody] UpdatePrivacyPolicyDto updatePrivacy)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update privacy policy with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_privacyPolicyRepo == null || _mapper == null)
            {
                _logger.LogError("Privacy policy repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updatePrivacy.Id)
            {
                return BadRequest("Privacy Policy ID information does not match");
            }

            // Fetch the existing privacy policy to ensure it exists
            var existingPrivacy = await _privacyPolicyRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingPrivacy == null)
            {
                _logger.LogWarning("Privacy policy with ID: {Id} not found.", id);
                return NotFound("Privacy policy not found.");
            }

            existingPrivacy.DateUpdated = DateTime.Now;

            // Map the updated fields
            _mapper.Map(updatePrivacy, existingPrivacy);

            try
            {
                await _privacyPolicyRepo.Update(existingPrivacy);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated privacy policy with ID: {Id} in {ElapsedMilliseconds}ms.", existingPrivacy.Id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPrivacyPolicyResponse { Message = "Privacy policy updated successfully.", Id = existingPrivacy.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating privacy policy with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating privacy policy: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeletePrivacyPolicy([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to delete privacy policy with ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_privacyPolicyRepo == null || _mapper == null)
            {
                _logger.LogError("Privacy policy repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkPolicyExist = await _privacyPolicyRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                _logger.LogWarning("Privacy policy with ID: {Id} does not exist.", id);
                return NotFound($"Don't exist privacy policy with ID {id} to delete");
            }

            try
            {
                await _privacyPolicyRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation("Successfully deleted privacy policy with ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return Ok(new ActionPrivacyPolicyResponse { Message = "Privacy policy deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting privacy policy with ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error deleting privacy policy: {ex.Message}");
            }
        }
    }
}

