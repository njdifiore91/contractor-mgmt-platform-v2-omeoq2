using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Core.Entities;
using Backend.Core.Interfaces.Repositories;
using Backend.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;  // v6.0.0
using Microsoft.Extensions.Logging;         // v6.0.0

namespace Backend.API.Controllers
{
    /// <summary>
    /// Controller handling all inspector-related operations including search, mobilization,
    /// demobilization, drug testing, and equipment management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ApiVersion("1.0")]
    public class InspectorController : ControllerBase
    {
        private readonly IInspectorRepository _inspectorRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<InspectorController> _logger;
        private readonly IMemoryCache _cache;
        private const int CACHE_DURATION_MINUTES = 5;
        private const string SEARCH_CACHE_KEY_PREFIX = "inspector_search_";

        public InspectorController(
            IInspectorRepository inspectorRepository,
            IEmailService emailService,
            ILogger<InspectorController> logger,
            IMemoryCache cache)
        {
            _inspectorRepository = inspectorRepository ?? throw new ArgumentNullException(nameof(inspectorRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Searches for inspectors based on geographical location and additional filters.
        /// </summary>
        [HttpGet("search")]
        [Authorize(Policy = "ViewInspectors")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<SearchResult<Inspector>>> SearchAsync(
            [FromQuery] string zipCode,
            [FromQuery] int radiusMiles,
            [FromQuery] Dictionary<string, string> filters)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length != 5)
                {
                    return BadRequest("Invalid zip code format");
                }

                if (radiusMiles <= 0 || radiusMiles > 500)
                {
                    return BadRequest("Radius must be between 1 and 500 miles");
                }

                string cacheKey = $"{SEARCH_CACHE_KEY_PREFIX}{zipCode}_{radiusMiles}_{string.Join("_", filters)}";

                if (_cache.TryGetValue(cacheKey, out SearchResult<Inspector> cachedResult))
                {
                    _logger.LogInformation("Returning cached search results for {ZipCode}", zipCode);
                    return Ok(cachedResult);
                }

                var searchFilters = new SearchFilters { Filters = filters };
                var result = await _inspectorRepository.SearchInspectorsAsync(zipCode, radiusMiles, searchFilters);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("Completed search for inspectors near {ZipCode}, found {Count} results", 
                    zipCode, result.TotalCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching inspectors near {ZipCode}", zipCode);
                return StatusCode(500, "An error occurred while searching for inspectors");
            }
        }

        /// <summary>
        /// Mobilizes an inspector for a project with comprehensive tracking.
        /// </summary>
        [HttpPost("{id}/mobilize")]
        [Authorize(Policy = "ManageInspectors")]
        public async Task<ActionResult> MobilizeAsync(
            int id,
            [FromBody] MobilizationDetails details)
        {
            try
            {
                var inspector = await _inspectorRepository.GetByIdAsync(id);
                if (inspector == null)
                {
                    return NotFound($"Inspector with ID {id} not found");
                }

                var result = await _inspectorRepository.MobilizeInspectorAsync(id, details);
                if (!result.Success)
                {
                    return BadRequest(result.ValidationErrors);
                }

                await _emailService.SendMobilizationNotificationAsync(
                    inspector.Email,
                    details.ProjectName,
                    details.CustomerName,
                    details.MobilizationDate);

                _logger.LogInformation("Inspector {Id} mobilized successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mobilizing inspector {Id}", id);
                return StatusCode(500, "An error occurred during mobilization");
            }
        }

        /// <summary>
        /// Processes inspector demobilization with reason tracking.
        /// </summary>
        [HttpPost("{id}/demobilize")]
        [Authorize(Policy = "ManageInspectors")]
        public async Task<ActionResult> DemobilizeAsync(
            int id,
            [FromBody] DemobilizationDetails details)
        {
            try
            {
                var inspector = await _inspectorRepository.GetByIdAsync(id);
                if (inspector == null)
                {
                    return NotFound($"Inspector with ID {id} not found");
                }

                var result = await _inspectorRepository.DemobilizeInspectorAsync(id, details);
                if (!result.Success)
                {
                    return BadRequest(result.ValidationErrors);
                }

                _logger.LogInformation("Inspector {Id} demobilized successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error demobilizing inspector {Id}", id);
                return StatusCode(500, "An error occurred during demobilization");
            }
        }

        /// <summary>
        /// Manages drug test records for an inspector.
        /// </summary>
        [HttpPost("{id}/drugtests")]
        [Authorize(Policy = "ManageInspectors")]
        public async Task<ActionResult> AddDrugTestAsync(
            int id,
            [FromBody] DrugTestRecord testRecord)
        {
            try
            {
                var inspector = await _inspectorRepository.GetByIdAsync(id);
                if (inspector == null)
                {
                    return NotFound($"Inspector with ID {id} not found");
                }

                var result = await _inspectorRepository.ManageDrugTestAsync(id, testRecord);
                if (!result.Success)
                {
                    return BadRequest(result.ValidationErrors);
                }

                _logger.LogInformation("Drug test record added for inspector {Id}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding drug test for inspector {Id}", id);
                return StatusCode(500, "An error occurred while adding drug test record");
            }
        }

        /// <summary>
        /// Manages equipment assignments for an inspector.
        /// </summary>
        [HttpPost("{id}/equipment")]
        [Authorize(Policy = "ManageEquipment")]
        public async Task<ActionResult> AssignEquipmentAsync(
            int id,
            [FromBody] EquipmentAssignment assignment)
        {
            try
            {
                var inspector = await _inspectorRepository.GetByIdAsync(id);
                if (inspector == null)
                {
                    return NotFound($"Inspector with ID {id} not found");
                }

                var result = await _inspectorRepository.ManageEquipmentAssignmentAsync(id, assignment);
                if (!result.Success)
                {
                    return BadRequest(result.ValidationErrors);
                }

                _logger.LogInformation("Equipment assigned to inspector {Id}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning equipment to inspector {Id}", id);
                return StatusCode(500, "An error occurred during equipment assignment");
            }
        }
    }
}