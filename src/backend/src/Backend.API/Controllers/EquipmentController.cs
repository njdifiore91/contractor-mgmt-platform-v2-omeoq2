using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.API.Filters;
using Backend.Core.DTOs.Equipment;
using Backend.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.API.Controllers
{
    /// <summary>
    /// API controller for managing equipment operations including listing, assignment, and return tracking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ILogger<EquipmentController> _logger;

        /// <summary>
        /// Initializes a new instance of the EquipmentController
        /// </summary>
        /// <param name="equipmentRepository">Repository for equipment operations</param>
        /// <param name="logger">Logger for operation tracking</param>
        public EquipmentController(
            IEquipmentRepository equipmentRepository,
            ILogger<EquipmentController> logger)
        {
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves equipment list for a specific company
        /// </summary>
        /// <param name="companyId">ID of the company</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of equipment for the company</returns>
        [HttpGet("company/{companyId}")]
        [RequirePermission("Edit Equipment")]
        [ResponseCache(Duration = 60)]
        [ProducesResponseType(typeof(IEnumerable<EquipmentDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetByCompanyAsync(
            [FromRoute] int companyId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving equipment for company {CompanyId}", companyId);

                if (companyId <= 0)
                {
                    _logger.LogWarning("Invalid company ID: {CompanyId}", companyId);
                    return BadRequest("Invalid company ID");
                }

                var filter = new EquipmentFilter();
                var equipment = await _equipmentRepository.GetByCompanyAsync(companyId, filter, cancellationToken);
                
                return Ok(equipment);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for company {CompanyId}", companyId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Assigns equipment to an inspector
        /// </summary>
        /// <param name="assignmentDto">Equipment assignment details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated equipment details</returns>
        [HttpPost("assign")]
        [RequirePermission("Edit Equipment")]
        [ProducesResponseType(typeof(EquipmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<EquipmentDto>> AssignToInspectorAsync(
            [FromBody] EquipmentAssignmentDto assignmentDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Assigning equipment {EquipmentId} to inspector {InspectorId}",
                    assignmentDto.EquipmentId,
                    assignmentDto.InspectorId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var equipment = await _equipmentRepository.AssignToInspectorAsync(
                    assignmentDto.EquipmentId,
                    assignmentDto.InspectorId,
                    assignmentDto.Condition,
                    assignmentDto.AssignmentDate,
                    cancellationToken);

                return Ok(equipment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid assignment parameters");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Equipment assignment conflict");
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized assignment attempt");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning equipment");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Records equipment return from an inspector
        /// </summary>
        /// <param name="returnDto">Equipment return details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated equipment details</returns>
        [HttpPost("return")]
        [RequirePermission("Edit Equipment")]
        [ProducesResponseType(typeof(EquipmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<EquipmentDto>> RecordReturnAsync(
            [FromBody] EquipmentReturnDto returnDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Recording return of equipment {EquipmentId}",
                    returnDto.EquipmentId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var equipment = await _equipmentRepository.RecordReturnAsync(
                    returnDto.EquipmentId,
                    returnDto.ReturnCondition,
                    returnDto.ReturnDate,
                    cancellationToken);

                return Ok(equipment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid return parameters");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Equipment return conflict");
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized return attempt");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording equipment return");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}