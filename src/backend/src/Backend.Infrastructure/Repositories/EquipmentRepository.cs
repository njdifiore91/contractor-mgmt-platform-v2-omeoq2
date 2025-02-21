using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore; // v6.0.0
using Microsoft.Extensions.Logging; // v6.0.0
using Backend.Core.Entities;
using Backend.Core.Interfaces.Repositories;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of IEquipmentRepository for managing equipment data access operations
    /// with comprehensive validation, error handling, and audit trails.
    /// </summary>
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquipmentRepository> _logger;

        public EquipmentRepository(ApplicationDbContext context, ILogger<EquipmentRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Equipment> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving equipment with ID: {EquipmentId}", id);

            if (id <= 0)
                throw new ArgumentException("Invalid equipment ID", nameof(id));

            return await _context.Equipment
                .Include(e => e.AssignedInspector)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Equipment>> GetByCompanyAsync(
            int companyId,
            EquipmentFilter filter,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving equipment for company ID: {CompanyId}", companyId);

            if (companyId <= 0)
                throw new ArgumentException("Invalid company ID", nameof(companyId));

            var query = _context.Equipment
                .Include(e => e.AssignedInspector)
                .AsNoTracking();

            // Apply company filter
            query = query.Where(e => e.CompanyId == companyId);

            // Apply additional filters if provided
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Model))
                    query = query.Where(e => e.Model.Contains(filter.Model));

                if (!string.IsNullOrWhiteSpace(filter.SerialNumber))
                    query = query.Where(e => e.SerialNumber.Contains(filter.SerialNumber));

                if (filter.IsAssigned.HasValue)
                    query = query.Where(e => e.IsOut == filter.IsAssigned.Value);

                if (!string.IsNullOrWhiteSpace(filter.Condition))
                    query = query.Where(e => e.Condition == filter.Condition);

                if (filter.AssignedToInspectorId.HasValue)
                    query = query.Where(e => e.AssignedToInspectorId == filter.AssignedToInspectorId.Value);

                if (filter.AssignedAfter.HasValue)
                    query = query.Where(e => e.AssignedDate >= filter.AssignedAfter.Value);

                if (filter.AssignedBefore.HasValue)
                    query = query.Where(e => e.AssignedDate <= filter.AssignedBefore.Value);
            }

            // Apply default sorting
            query = query.OrderBy(e => e.Model).ThenBy(e => e.SerialNumber);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Equipment> AssignToInspectorAsync(
            int equipmentId,
            int inspectorId,
            string condition,
            DateTime assignedDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Assigning equipment {EquipmentId} to inspector {InspectorId}", equipmentId, inspectorId);

            if (equipmentId <= 0)
                throw new ArgumentException("Invalid equipment ID", nameof(equipmentId));

            if (inspectorId <= 0)
                throw new ArgumentException("Invalid inspector ID", nameof(inspectorId));

            if (string.IsNullOrWhiteSpace(condition))
                throw new ArgumentException("Condition must be specified", nameof(condition));

            if (assignedDate > DateTime.UtcNow)
                throw new ArgumentException("Assignment date cannot be in the future", nameof(assignedDate));

            var equipment = await _context.Equipment
                .Include(e => e.AssignedInspector)
                .FirstOrDefaultAsync(e => e.Id == equipmentId, cancellationToken);

            if (equipment == null)
                throw new InvalidOperationException($"Equipment with ID {equipmentId} not found");

            if (equipment.IsOut)
                throw new InvalidOperationException($"Equipment {equipmentId} is already assigned");

            var inspector = await _context.Inspectors
                .FirstOrDefaultAsync(i => i.Id == inspectorId, cancellationToken);

            if (inspector == null)
                throw new InvalidOperationException($"Inspector with ID {inspectorId} not found");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                equipment.AssignToInspector(inspectorId, condition, assignedDate, "System");
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Successfully assigned equipment {EquipmentId} to inspector {InspectorId}", equipmentId, inspectorId);
                return equipment;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error assigning equipment {EquipmentId} to inspector {InspectorId}", equipmentId, inspectorId);
                throw;
            }
        }

        public async Task<Equipment> RecordReturnAsync(
            int equipmentId,
            string condition,
            DateTime returnedDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Recording return of equipment {EquipmentId}", equipmentId);

            if (equipmentId <= 0)
                throw new ArgumentException("Invalid equipment ID", nameof(equipmentId));

            if (string.IsNullOrWhiteSpace(condition))
                throw new ArgumentException("Return condition must be specified", nameof(condition));

            if (returnedDate > DateTime.UtcNow)
                throw new ArgumentException("Return date cannot be in the future", nameof(returnedDate));

            var equipment = await _context.Equipment
                .Include(e => e.AssignedInspector)
                .FirstOrDefaultAsync(e => e.Id == equipmentId, cancellationToken);

            if (equipment == null)
                throw new InvalidOperationException($"Equipment with ID {equipmentId} not found");

            if (!equipment.IsOut)
                throw new InvalidOperationException($"Equipment {equipmentId} is not currently assigned");

            if (equipment.AssignedDate > returnedDate)
                throw new ArgumentException("Return date cannot be earlier than assignment date", nameof(returnedDate));

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                equipment.RecordReturn(condition, returnedDate, "System");
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Successfully recorded return of equipment {EquipmentId}", equipmentId);
                return equipment;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error recording return of equipment {EquipmentId}", equipmentId);
                throw;
            }
        }
    }
}