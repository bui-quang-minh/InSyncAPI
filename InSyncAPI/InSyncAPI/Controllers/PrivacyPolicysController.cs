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
        [HttpGet()]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<ViewPrivacyPolicyDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPrivacyPolicys()
        {
            if (_privacyPolicyRepo == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _privacyPolicyRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("pagination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewPrivacyPolicyDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetAllPrivacyPolicy(int? index, int? size, string? keySearch = "")
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            IEnumerable<PrivacyPolicy> listPrivacyPolicy = new List<PrivacyPolicy>();
            int total = 0;
            keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();
            if (index == null || size == null)
            {
                listPrivacyPolicy = _privacyPolicyRepo.GetMulti
                    (c => c.Title.ToLower().Contains(keySearch)
                    );
                total = listPrivacyPolicy.Count();
            }
            else
            {
                index = index.Value < 0 ? INDEX_DEFAULT : index;
                size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
                listPrivacyPolicy = _privacyPolicyRepo.GetMultiPaging
            (c => c.Title.ToLower().Contains(keySearch)
            , out total, index.Value, size.Value, null
            );
            }

            var response = _mapper.Map<IEnumerable<ViewPrivacyPolicyDto>>(listPrivacyPolicy);
            var responsePaging = new ResponsePaging<IEnumerable<ViewPrivacyPolicyDto>>
            {
                data = response,
                totalOfData = total
            };
            return Ok(responsePaging);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPrivacyPolicyDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> GetPrivacyPolicyById(Guid id)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

        public async Task<IActionResult> AddPrivacyPolicy(AddPrivacyPolicyDto newPrivacy)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
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
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the private policy.");
                }

                return Ok(new ActionPrivacyPolicyResponse { Message = "Private policy added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding privacy policy into Database " + ex.Message);
            }

        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdatePrivacyPolicy(Guid id, UpdatePrivacyPolicyDto updatePrivacy)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
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
                return Ok(new ActionPrivacyPolicyResponse { Message = "Privacy policy updated successfully.", Id = existingPrivacy.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating privacy policy: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionPrivacyPolicyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> DeletePrivacyPolicy(Guid id)
        {
            if (_privacyPolicyRepo == null || _mapper == null)
            {
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
                return NotFound($"Dont exist privacy policy with id {id.ToString()} to delete");
            }
            try
            {
                await _privacyPolicyRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionPrivacyPolicyResponse { Message = "Privacy policy deleted successfully.", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"Error delete term: {ex.Message}");
            }

        }
    }
}

