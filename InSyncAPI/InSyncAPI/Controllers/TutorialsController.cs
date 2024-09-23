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
        [HttpGet("pagination")]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<Tutorial>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]

        public async Task<IActionResult> GetTutorials()
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            var response = _tutorialRepo.GetAll().AsQueryable();
            return Ok(response);
        }
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponsePaging<IEnumerable<ViewTutorialDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
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
            var responsePaging = new ResponsePaging<IEnumerable<ViewTutorialDto>>
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
        public async Task<IActionResult> GetTutorialById(Guid id)
        {
            if (_tutorialRepo == null || _mapper == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: "Application service has not been created");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTutorialResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]

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

            try
            {
                var response = await _tutorialRepo.Add(tutorial);
                if (response == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "Error occurred while adding the tutorial.");
                }
                return Ok(new ActionTutorialResponse { Message = "Tutorial added successfully.", Id = response.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        value: "An error occurred while adding Tutorial into Database " + ex.Message);
            }

        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTutorialResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

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
                return Ok(new ActionTutorialResponse { Message = "Tutorial updated successfully.", Id = existingTutorial.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating tutorial: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionTutorialResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
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

            try
            {
                await _tutorialRepo.DeleteMulti(c => c.Id.Equals(id));
                return Ok(new ActionTutorialResponse { Message = "Tutorial deleted successfully.", Id = id });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"Error delete tutorial: {ex.Message}");
            }
        }
    }


}

