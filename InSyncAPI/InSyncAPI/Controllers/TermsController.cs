using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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

        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(200, Type = typeof(IQueryable<Term>))]
        public async Task<IActionResult> GetTerms()
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _termRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("get-all-terms")]
        public async Task<IActionResult> GetAllTerms(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listTerms = _termRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, null);
            var response = _mapper.Map<IEnumerable<ViewTermDto>>(listTerms);
            return Ok(response);
        }


        [HttpGet("get-term/{id}")]
        public async Task<IActionResult> GetTermById(Guid id)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
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
        public async Task<IActionResult> AddTerm(AddTermsDto newTerm)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Term term = _mapper.Map<Term>(newTerm);
            term.DateCreated = DateTime.Now;

            var response = await _termRepo.Add(term);

            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Error occurred while adding the term.");
            }

            return Ok(new { message = "Term added successfully.", Id = response.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTerm(Guid id, UpdateTermsDto updateTerm)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                return Ok(new { message = "Term updated successfully.", Id = existingTerm.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating term: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTerm(Guid id)
        {
            if (_termRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var checkTermExist = await _termRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkTermExist)
            {
                return NotFound($"Dont exist term with id {id.ToString()} to delete");
            }
            await _termRepo.DeleteMulti(c => c.Id.Equals(id));
            return Ok(new { message = "Term deleted successfully.", Id = id });
        }
    }

}

