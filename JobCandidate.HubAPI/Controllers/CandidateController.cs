using AutoMapper;
using JobCandidate.HubAPI.Entities;
using JobCandidate.HubAPI.Interfaces;
using JobCandidate.HubAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JobCandidate.HubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateRepository _repository;
        private readonly IMapper _mapper;

        public CandidateController(ICandidateRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        // 1. Get All Candidates
        /// <summary>
        /// Retrieves all candidates with optional search, sorting, and pagination.
        /// </summary>
        /// <returns>A list of candidates and the total count.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetCandidateInput input)
        {
            var result = await _repository.GetAllAsync(input.SearchText, input.Sorting, input.IsDescending, input.MaxResultCount, input.SkipCount);

            return Ok(new GetAllResponse
            {
                TotalCount = result.totalCount,
                Items = _mapper.Map<List<CandidateModel>>(result.candidates)
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var candidate = await _repository.GetByIdAsync(id);
                return Ok(_mapper.Map<CandidateModel>(candidate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // 3. Add or Update Candidate
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateModel input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var candidate = await _repository.GetCandidateByEmail(input.Email);
                if (candidate != null)
                {
                    // update candidate information
                    _mapper.Map(input, candidate);
                  await  _repository.UpdateAsync(candidate);
                }
                else
                {
                    await _repository.AddAsync(_mapper.Map<Candidate>(input));
                }
                return Ok($"Candidate saved successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(CreateModel input, Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var existingCandidate = await _repository.GetCandidateByEmail(input.Email);
                if (existingCandidate != null && existingCandidate.Id != id)
                {
                    return BadRequest("Email address already exist.");
                }
                var candidate = await _repository.GetByIdAsync(id);
                _mapper.Map(input, candidate);
                await _repository.UpdateAsync(candidate);
                return Ok($"Candidate updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 4. Delete Candidate by Email
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            try
            {
                var existingCandidate = await _repository.GetByIdAsync(id);
                if (existingCandidate == null)
                {
                    return NotFound($"Candidate not found with the specified id.");
                }
                await _repository.DeleteAsync(id);
                return Ok($"Candidate deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
