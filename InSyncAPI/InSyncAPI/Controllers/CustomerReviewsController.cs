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
    public class CustomerReviewsController : ControllerBase
    {
        private ICustomerReviewRepository _customerReviewRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;
        private ILogger<CustomerReviewsController> _logger;
        private readonly string TAG = nameof(CustomerReviewsController) + " - ";

        public CustomerReviewsController(ICustomerReviewRepository customerReviewRepo, IMapper mapper
            , ILogger<CustomerReviewsController> logger)
        {
            _customerReviewRepo = customerReviewRepo;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<CustomerReview>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetCustomerReviews()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(TAG + "Received request to get customer reviews at {RequestTime}.", DateTime.UtcNow);

            if (_customerReviewRepo == null || _mapper == null)
            {
                _logger.LogError(TAG + "Customer review repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            try
            {
                var response = await Task.FromResult(_customerReviewRepo.GetAll().AsQueryable());
                stopwatch.Stop();
                _logger.LogInformation(TAG + "Successfully retrieved customer reviews in {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, TAG + "Error retrieving customer reviews. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving customer reviews: {ex.Message}");
            }
        }

        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewCustomerReviewDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllCustomerReview(int? index, int? size, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all customer reviews at {RequestTime}.", DateTime.UtcNow);

            if (_customerReviewRepo == null || _mapper == null)
            {
                _logger.LogError("Customer review repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<CustomerReview> listCustomerReview = new List<CustomerReview>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listCustomerReview = _customerReviewRepo.GetMulti(c => c.JobTitle.ToLower().Contains(keySearch)
                    || c.Name.ToLower().Contains(keySearch)
                    || c.Review.ToLower().Contains(keySearch));
                    total = listCustomerReview.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listCustomerReview = _customerReviewRepo.GetMultiPaging(c => c.JobTitle.ToLower().Contains(keySearch)
                    || c.Name.ToLower().Contains(keySearch)
                    || c.Review.ToLower().Contains(keySearch),
                    out total, index.Value, size.Value, null);
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved customer reviews in {ElapsedMilliseconds}ms. Total records: {TotalRecords}.", stopwatch.ElapsedMilliseconds, total);

                var response = _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(listCustomerReview);
                var responsePaging = new ResponsePaging<IEnumerable<ViewCustomerReviewDto>>
                {
                    data = response,
                    totalOfData = total
                };

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving customer reviews. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving customer reviews: {ex.Message}");
            }
        }

        [HttpGet("pagination/is-publish")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewCustomerReviewDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllCustomerReviewIsPublish(int? index, int? size, bool? isPublish, string? keySearch = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get all published customer reviews at {RequestTime}.", DateTime.UtcNow);

            if (_customerReviewRepo == null || _mapper == null)
            {
                _logger.LogError("Customer review repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<CustomerReview> listCustomerReview = new List<CustomerReview>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();

            try
            {
                if (index == null || size == null)
                {
                    listCustomerReview = _customerReviewRepo.GetMulti(c =>
                        (isPublish == null || c.IsShow == isPublish) &&
                        (c.JobTitle.ToLower().Contains(keySearch) ||
                         c.Name.ToLower().Contains(keySearch) ||
                         c.Review.ToLower().Contains(keySearch))
                    );
                    total = listCustomerReview.Count();
                }
                else
                {
                    index = index.Value < 0 ? INDEX_DEFAULT : index;
                    size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                    listCustomerReview = _customerReviewRepo.GetMultiPaging(c =>
                        (isPublish == null || c.IsShow == isPublish) &&
                        (c.JobTitle.ToLower().Contains(keySearch) ||
                         c.Name.ToLower().Contains(keySearch) ||
                         c.Review.ToLower().Contains(keySearch)),
                        out total, index.Value, size.Value, null
                    );
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved customer reviews in {ElapsedMilliseconds}ms. Total records: {TotalRecords}.", stopwatch.ElapsedMilliseconds, total);

                var response = _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(listCustomerReview);
                var responsePaging = new ResponsePaging<IEnumerable<ViewCustomerReviewDto>>
                {
                    data = response,
                    totalOfData = total
                };

                return Ok(responsePaging);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving customer reviews. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving customer reviews: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewCustomerReviewDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetCustomerReviewById([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to get customer review by ID: {Id} at {RequestTime}.", id, DateTime.UtcNow);

            if (_customerReviewRepo == null || _mapper == null)
            {
                _logger.LogError("Customer review repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var customerReview = await _customerReviewRepo.GetSingleByCondition(c => c.Id.Equals(id));
                if (customerReview == null)
                {
                    _logger.LogWarning("No review found with ID: {Id}.", id);
                    return NotFound("No review has an ID: " + id.ToString());
                }

                var response = _mapper.Map<ViewCustomerReviewDto>(customerReview);
                stopwatch.Stop();
                _logger.LogInformation("Successfully retrieved customer review by ID: {Id} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving customer review by ID: {Id}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving customer review: {ex.Message}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCustomerReviewResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddCustomerReview([FromBody] AddCustomerReviewDto newReview)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to add a new customer review at {RequestTime}.", DateTime.UtcNow);

            if (_customerReviewRepo == null || _mapper == null)
            {
                _logger.LogError("Customer review repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the new customer review.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            CustomerReview customerReview = _mapper.Map<CustomerReview>(newReview);
            customerReview.DateCreated = DateTime.Now;
            customerReview.IsShow = false;

            try
            {
                var response = await _customerReviewRepo.Add(customerReview);

                if (response == null)
                {
                    _logger.LogError("Failed to add the customer review to the database.");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the customer review.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Successfully added customer review with ID: {Id} in {ElapsedMilliseconds}ms.", response.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionCustomerReviewResponse { Message = "Customer review added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error adding customer review to the database. Total time taken: {ElapsedMilliseconds}ms.", stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while adding Customer Review into Database: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCustomerReviewResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateCustomerReview([FromRoute] Guid id, [FromBody] UpdateCustomerReviewDto updateReview)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to update customer review with ID: {ReviewId} at {RequestTime}.", id, DateTime.UtcNow);

            if (_customerReviewRepo == null || _mapper == null)
            {
                _logger.LogError("Customer review repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the customer review update.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateReview.Id)
            {
                _logger.LogWarning("Review ID {Id} does not match the update review ID {UpdateId}.", id, updateReview.Id);
                return BadRequest("Review ID information does not match");
            }

            // Fetch the existing customer review to ensure it exists
            var existingReview = await _customerReviewRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingReview == null)
            {
                _logger.LogWarning("Customer review with ID {ReviewId} not found.", id);
                return NotFound("Customer review not found.");
            }

            // Map the updated fields
            _mapper.Map(updateReview, existingReview);

            try
            {
                await _customerReviewRepo.Update(existingReview);
                stopwatch.Stop();
                _logger.LogInformation("Successfully updated customer review with ID: {ReviewId} in {ElapsedMilliseconds}ms.", existingReview.Id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionCustomerReviewResponse { Message = "Customer review updated successfully.", Id = existingReview.Id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating customer review with ID: {ReviewId}. Total time taken: {ElapsedMilliseconds}ms.", existingReview.Id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating customer review: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCustomerReviewResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteCustomerReview([FromRoute] Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Received request to delete customer review with ID: {ReviewId} at {RequestTime}.", id, DateTime.UtcNow);

            if (_customerReviewRepo == null || _mapper == null)
            {
                _logger.LogError("Customer review repository or mapper is not initialized.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the customer review deletion.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var checkReviewExist = await _customerReviewRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkReviewExist)
            {
                _logger.LogWarning("Customer review with ID {ReviewId} does not exist.", id);
                return NotFound($"Don't exist review with id {id} to delete");
            }

            try
            {
                await _customerReviewRepo.DeleteMulti(c => c.Id.Equals(id));
                stopwatch.Stop();
                _logger.LogInformation("Successfully deleted customer review with ID: {ReviewId} in {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);

                return Ok(new ActionCustomerReviewResponse { Message = "Customer review deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting customer review with ID: {ReviewId}. Total time taken: {ElapsedMilliseconds}ms.", id, stopwatch.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error deleting Customer Review: {ex.Message}");
            }
        }


    }
}
