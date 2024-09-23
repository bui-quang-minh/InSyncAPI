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

        public TermsController(ITermRepository termRepo, IMapper mapper)
        {
            _termRepo = termRepo;
            _mapper = mapper;
        }


        [HttpGet("odata")]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Term>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetTerms()
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            string[] includes = new string[] { };
            var response = _termRepo.GetAll(includes).AsQueryable();
            return Ok(response);
        }
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ViewTermDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetAllTerms(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;
            string[] includes = new string[] { };
            var listTerms = _termRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, includes);
            var response = _mapper.Map<IEnumerable<ViewTermDto>>(listTerms);
            return Ok(response);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewTermDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetTermById(Guid id)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var term = await _termRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (term == null)
            {
                return NotFound("No term has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewTermDto>(term);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTermResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> AddTerm(AddTermsDto newTerm)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            Term term = _mapper.Map<Term>(newTerm);
            term.DateCreated = DateTime.Now;

            try
            {
                var response = await _termRepo.Add(term);

                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the term.");
                }

                return Ok(new ActionTermResponse { Message = "Term added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding Term into Database " + ex.Message);
            }
            
        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTermResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdateTerm(Guid id, UpdateTermsDto updateTerm)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (id != updateTerm.Id)
            {
                return BadRequest("Term ID information does not match");
            }

            // Fetch the existing customer review to ensure it exists
            var existingTerm = await _termRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (existingTerm == null)
            {
                return NotFound("Term not found.");
            }
            existingTerm.DateUpdated = DateTime.Now;
            // Map the updated fields
            _mapper.Map(updateTerm, existingTerm);

            try
            {
                await _termRepo.Update(existingTerm);
                return Ok(new ActionTermResponse { Message = "Term updated successfully.", Id = existingTerm.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating term: {ex.Message}");
            }
        }

       
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTermResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteTerm(Guid id)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            var checkTermExist = await _termRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkTermExist)
            {
                return NotFound($"Dont exist term with id {id.ToString()} to delete");
            }
            try
            {
                await _termRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionTermResponse { Message = "Term deleted successfully.", Id = id });
            }
            catch(Exception ex)
            {
                 return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error delete term: {ex.Message}");
            }
            
        }
    }

}

