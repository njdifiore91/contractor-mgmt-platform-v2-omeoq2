using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Core.Entities;
using Backend.Core.DTOs.Equipment;

namespace Backend.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for managing equipment operations including listing, assignment, and return tracking
    /// with comprehensive security and audit support.
    /// </summary>
    public interface IEquipmentRepository
    {
        /// <summary>
        /// Retrieves equipment by its unique identifier with optional include parameters for related data.
        /// </summary>
        /// <param name="id">Unique identifier of the equipment</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>Equipment entity if found, null otherwise</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks required permissions</exception>
        Task<Equipment> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all equipment for a specific company with filtering and sorting options.
        /// Requires 'Edit Equipment' permission.
        /// </summary>
        /// <param name="companyId">ID of the company whose equipment to retrieve</param>
        /// <param name="filter">Filter criteria for equipment search</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>Filtered and sorted list of equipment belonging to the company</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks required permissions</exception>
        Task<IEnumerable<Equipment>> GetByCompanyAsync(
            int companyId,
            EquipmentFilter filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Assigns equipment to an inspector with condition tracking and validation.
        /// Creates an audit trail of the assignment.
        /// </summary>
        /// <param name="equipmentId">ID of the equipment to assign</param>
        /// <param name="inspectorId">ID of the inspector receiving the equipment</param>
        /// <param name="condition">Condition of the equipment at assignment</param>
        /// <param name="assignedDate">Date of assignment</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>Updated equipment entity after assignment</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when equipment is already assigned</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks required permissions</exception>
        Task<Equipment> AssignToInspectorAsync(
            int equipmentId,
            int inspectorId,
            string condition,
            DateTime assignedDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Records the return of equipment from an inspector with condition assessment.
        /// Creates an audit trail of the return.
        /// </summary>
        /// <param name="equipmentId">ID of the equipment being returned</param>
        /// <param name="condition">Condition of the equipment upon return</param>
        /// <param name="returnedDate">Date of return</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>Updated equipment entity after return</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when equipment is not currently assigned</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks required permissions</exception>
        Task<Equipment> RecordReturnAsync(
            int equipmentId,
            string condition,
            DateTime returnedDate,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Filter criteria for equipment queries
    /// </summary>
    public class EquipmentFilter
    {
        /// <summary>
        /// Filter by equipment model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Filter by serial number
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Filter by assignment status
        /// </summary>
        public bool? IsAssigned { get; set; }

        /// <summary>
        /// Filter by current condition
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Filter by assigned inspector ID
        /// </summary>
        public int? AssignedToInspectorId { get; set; }

        /// <summary>
        /// Include equipment assigned after this date
        /// </summary>
        public DateTime? AssignedAfter { get; set; }

        /// <summary>
        /// Include equipment assigned before this date
        /// </summary>
        public DateTime? AssignedBefore { get; set; }
    }
}