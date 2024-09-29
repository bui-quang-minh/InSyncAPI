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
    public class CustomerReviewsController : ControllerBase
    {
        private ICustomerReviewRepository _customerReviewRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public CustomerReviewsController(ICustomerReviewRepository customerReviewRepo, IMapper mapper)
        {
            _customerReviewRepo = customerReviewRepo;
            _mapper = mapper;
        }
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<CustomerReview>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetCustomerReviews()
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _customerReviewRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewCustomerReviewDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllCustomerReview(string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            keySearch = keySearch.ToLower();
            var listCustomerReview = _customerReviewRepo.GetMultiPaging
            (c => c.JobTitle.ToLower().Contains(keySearch) || c.Name.ToLower().Contains(keySearch) ||c.Review.ToLower().Contains(keySearch)
            , out int total, index.Value, size.Value, null
            );
            var response = _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(listCustomerReview);
            var responsePaging = new ResponsePaging<IEnumerable<ViewCustomerReviewDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);

        }
        [HttpGet("pagination/is-publish")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewCustomerReviewDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllCustomerReviewIsPublish(bool isPublish,string? keySearch = "", int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listCustomerReview = _customerReviewRepo.GetMultiPaging
            (c => c.IsShow == isPublish 
            && (c.JobTitle.ToLower().Contains(keySearch) || c.Name.ToLower().Contains(keySearch) || c.Review.ToLower().Contains(keySearch))
            , out int total, index.Value, size.Value, null
            );
            var response = _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(listCustomerReview);
            var responsePaging = new ResponsePaging<IEnumerable<ViewCustomerReviewDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);

        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewCustomerReviewDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetCustomerReviewById(Guid id)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var customerReview = await _customerReviewRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (customerReview == null)
            {
                return NotFound("No review has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewCustomerReviewDto>(customerReview);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCustomerReviewResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddCustomerReview(AddCustomerReviewDto newReview)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
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
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the customer review.");
                }
                return Ok(new ActionCustomerReviewResponse { Message = "Customer review added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding Customer Review into Database" + ex.Message);
            }



        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCustomerReviewResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateCustomerReview(Guid id, UpdateCustomerReviewDto updateReview)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateReview.Id)
            {
                return BadRequest("Review ID information does not match");
            }

            // Fetch the existing customer review to ensure it exists
            var existingReview = await _customerReviewRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingReview == null)
            {
                return NotFound("Customer review not found.");
            }


            // Map the updated fields
            _mapper.Map(updateReview, existingReview);

            try
            {
                await _customerReviewRepo.Update(existingReview);
                return Ok(new ActionCustomerReviewResponse { Message = "Customer review updated successfully.", Id = existingReview.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating customer review: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionCustomerReviewResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteCustomerReview(Guid id)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkReviewExist = await _customerReviewRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkReviewExist)
            {
                return NotFound($"Dont exist review with id {id.ToString()} to delete");
            }
            try
            {
                await _customerReviewRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionCustomerReviewResponse { Message = "Customer review deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error delete Customer Review: {ex.Message}");
            }

        }

    }
}
