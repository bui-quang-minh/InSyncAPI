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
    public class PrivacyPolicysController : ControllerBase
    {
        private IPrivacyPolicyRepository _privacyPolicyRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public PrivacyPolicysController(IPrivacyPolicyRepository privacyPolicyRepo, IMapper mapper)
        {
            _privacyPolicyRepo = privacyPolicyRepo;
            _mapper = mapper;
        }
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (_privacyPolicyRepo == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _privacyPolicyRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("get-all-privacy_policy")]
        public async Task<IActionResult> GetAllPrivacyPolicy(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listPrivacyPolicy = _privacyPolicyRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, null);
            var response = _mapper.Map<IEnumerable<ViewPrivacyPolicyDto>>(listPrivacyPolicy);
            return Ok(response);
        }


        [HttpGet("get-privacy-policy/{id}")]
        public async Task<IActionResult> GetPrivacyPolicyById(Guid id)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
           
            var privacyPolicy = await _privacyPolicyRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (privacyPolicy == null)
            {
                return NotFound("No privacy policy has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewPrivacyPolicyDto>(privacyPolicy);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddPrivacyPolicy(AddPrivacyPolicyDto newPrivacy)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PrivacyPolicy privacyPolicy = _mapper.Map<PrivacyPolicy>(newPrivacy);
            privacyPolicy.DateCreated = DateTime.Now;

            var response = await _privacyPolicyRepo.Add(privacyPolicy);

            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Error occurred while adding the private policy.");
            }

            return Ok(new { message = "Private policy added successfully.", Id = response.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrivacyPolicy(Guid id, UpdatePrivacyPolicyDto updatePrivacy)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updatePrivacy.Id)
            {
                return BadRequest("PrivacyPolicy ID information does not match");
            }

            // Fetch the existing customer review to ensure it exists
            var existingPrivacy = await _privacyPolicyRepo.GetSingleByCondition(c => c.Id.Equals(id));
            
            if (existingPrivacy == null)
            {
                return NotFound("Privacy policy not found.");
            }
            existingPrivacy.DateUpdated = DateTime.Now;
            // Map the updated fields
            _mapper.Map(updatePrivacy, existingPrivacy);

            try
            {
                await _privacyPolicyRepo.Update(existingPrivacy);
                return Ok(new { message = "Privacy policy updated successfully.", Id = existingPrivacy.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating privacy policy: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrivacyPolicy(Guid id)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var checkPolicyExist = await _privacyPolicyRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                return NotFound($"Dont exist privacy policy with id {id.ToString()} to delete");
            }
           await _privacyPolicyRepo.DeleteMulti(c => c.Id.Equals(id));
            return Ok(new { message = "Privacy policy deleted successfully.", Id = id });
        }
    }
}

