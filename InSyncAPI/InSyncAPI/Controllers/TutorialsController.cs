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
    public class TutorialsController : ControllerBase
    {
        private ITutorialRepository _tutorialRepo;
        private const int ITEM_PAGES_DEFAULT = 10;
        private const int INDEX_DEFAULT = 0;
        private IMapper _mapper;

        public TutorialsController(ITutorialRepository tutorialRepo, IMapper mapper)
        {
            _tutorialRepo = tutorialRepo;
            _mapper = mapper;
        }
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _tutorialRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet("get-all-tutorials")]
        public async Task<IActionResult> GetAllTutorials(int? index = INDEX_DEFAULT, int? size = ITEM_PAGES_DEFAULT)
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            index = index.Value < 0 ? INDEX_DEFAULT : index;
            size = size.Value < 0 ? ITEM_PAGES_DEFAULT : size;

            var listTutorial = _tutorialRepo.GetMultiPaging(c => true, out int total, index.Value, size.Value, null);
            var response = _mapper.Map<IEnumerable<ViewTutorialDto>>(listTutorial);
            return Ok(response);
        }


        [HttpGet("get-tutorial/{id}")]
        public async Task<IActionResult> GetTutorialById(Guid id)
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var tutorial = await _tutorialRepo.GetSingleByCondition(c => c.Id.Equals(id));
            if (tutorial == null)
            {
                return NotFound("No tutorial has an ID : " + id.ToString());
            }
            var response = _mapper.Map<ViewTutorialDto>(tutorial);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddTutorial(AddTutorialDto newTutorial)
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Tutorial tutorial = _mapper.Map<Tutorial>(newTutorial);
            tutorial.DateCreated = DateTime.Now;

            var response = await _tutorialRepo.Add(tutorial);

            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Error occurred while adding the tutorial.");
            }

            return Ok(new { message = "Tutorial added successfully.", Id = response.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTutorial(Guid id, UpdateTutorialDto updateTutorial)
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Application service has not been created");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updateTutorial.Id)
            {
                return BadRequest("Tutorial ID information does not match");
            }

            
            var existingTutorial = await _tutorialRepo.GetSingleByCondition(c => c.Id.Equals(id));

            if (existingTutorial == null)
            {
                return NotFound("Tutorial not found.");
            }
            existingTutorial.DateUpdated = DateTime.Now;
            _mapper.Map(updateTutorial, existingTutorial);

            try
            {
                await _tutorialRepo.Update(existingTutorial);
                return Ok(new { message = "Tutorial updated successfully.", Id = existingTutorial.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTutorial(Guid id)
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }

            var checkPolicyExist = await _tutorialRepo.CheckContainsAsync(c => c.Id.Equals(id));
            if (!checkPolicyExist)
            {
                return NotFound($"Dont exist tutorial with id {id.ToString()} to delete");
            }
            await _tutorialRepo.DeleteMulti(c => c.Id.Equals(id));
            return Ok(new { message = "Tutorial deleted successfully.", Id = id });
        }
    }


}

