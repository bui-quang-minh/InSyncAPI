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
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetCustomerReviews()
        {
            if (_customerReviewRepo == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _customerReviewRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("get-all-customer-review")]
        public async Task<IActionResult> GetAllCustomerReview(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listCustomerReview = _customerReviewRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, null);
            var response = _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(listCustomerReview);
            return Ok(response);
        }

        [HttpGet("get-customer-review-is-publish")]
        public async Task<IActionResult> GetAllCustomerReviewIsPublish(bool isPublish, int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listCustomerReview = _customerReviewRepo.GetMultiPaging(c => c.IsShow == isPublish, out int total, index.Value, size.Value, null);
            var response = _mapper.Map<IEnumerable<ViewCustomerReviewDto>>(listCustomerReview);
            return Ok(response);
        }

        [HttpGet("get-customer-review/{id}")]
        public async Task<IActionResult> GetCustomerReviewById(Guid id)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var customerReview = await _customerReviewRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if(customerReview == null)
            {
                return NotFound("No review has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewCustomerReviewDto>(customerReview);
                                                return Ok(response);
                                            }

        [HttpPost]
        public async Task<IActionResult> AddCustomerReview(AddCustomerReviewDto newReview)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CustomerReview customerReview = _mapper.Map<CustomerReview>(newReview);
            customerReview.DateCreated = DateTime.Now;
            customerReview.IsShow = false;

            var response = await _customerReviewRepo.Add(customerReview);

            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Error occurred while adding the customer review.");
            }

            return Ok(new { message = "Customer review added successfully.", Id = response.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerReview(Guid id, UpdateCustomerReviewDto updateReview)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            { 
                return BadRequest(ModelState);
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
              await  _customerReviewRepo.Update(existingReview);
                return Ok(new { message = "Customer review updated successfully.", Id = existingReview.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating customer review: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerReview(Guid id)
        {
            if (_customerReviewRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var checkReviewExist = await _customerReviewRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkReviewExist)
            {
                return NotFound($"Dont exist review with id {id.ToString()} to delete");
            }
           await  _customerReviewRepo.DeleteMulti(c => c.Id.Equals(id));
            return Ok(new { message = "Customer review deleted successfully.", Id = id });
        }

    }
}
