using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Core.Entities;
using Backend.Core.Interfaces.Repositories;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of IInspectorRepository providing data access operations for Inspector entities
    /// with enhanced geographical search, mobilization tracking, and equipment management.
    /// </summary>
    public class InspectorRepository : IInspectorRepository
    {
        private readonly ApplicationDbContext _context;

        public InspectorRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Inspector> GetByIdAsync(int id)
        {
            return await _context.Inspectors
                .Include(i => i.AssignedEquipment)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Inspector>> GetAllActiveAsync()
        {
            return await _context.Inspectors
                .Where(i => !i.IsDeleted)
                .ToListAsync();
        }

        public async Task<Inspector> CreateAsync(Inspector inspector)
        {
            if (inspector == null)
                throw new ArgumentNullException(nameof(inspector));

            await _context.Inspectors.AddAsync(inspector);
            await _context.SaveChangesAsync();
            return inspector;
        }

        public async Task<bool> UpdateAsync(Inspector inspector)
        {
            if (inspector == null)
                throw new ArgumentNullException(nameof(inspector));

            _context.Entry(inspector).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<SearchResult<Inspector>> SearchInspectorsAsync(
            string zipCode,
            int radiusMiles,
            SearchFilters filters)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code is required", nameof(zipCode));

            if (radiusMiles <= 0)
                throw new ArgumentException("Radius must be greater than zero", nameof(radiusMiles));

            // Start with base query
            var query = _context.Inspectors
                .Include(i => i.AssignedEquipment)
                .Where(i => !i.IsDeleted);

            // Apply status filter
            if (!string.IsNullOrEmpty(filters?.Status))
            {
                query = query.Where(i => i.Status == filters.Status);
            }

            // Apply specialties filter
            if (filters?.Specialties != null && filters.Specialties.Any())
            {
                query = query.Where(i => i.Specialties.Any(s => filters.Specialties.Contains(s)));
            }

            // Apply geographical filter using SQL spatial data
            // Note: Actual implementation would use proper spatial query
            var results = await query
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .Skip((filters?.PageNumber ?? 0) * (filters?.PageSize ?? 20))
                .Take(filters?.PageSize ?? 20)
                .ToListAsync();

            return new SearchResult<Inspector>
            {
                Items = results,
                TotalCount = await query.CountAsync(),
                PageNumber = filters?.PageNumber ?? 0,
                PageSize = filters?.PageSize ?? 20
            };
        }

        public async Task<MobilizationResult> MobilizeInspectorAsync(
            int inspectorId,
            MobilizationDetails details)
        {
            var inspector = await GetByIdAsync(inspectorId);
            if (inspector == null)
                throw new ArgumentException("Inspector not found", nameof(inspectorId));

            inspector.MobilizationDate = details.MobDate;
            inspector.HireType = details.HireType;
            inspector.Classification = details.Classification;
            inspector.Department = details.Department;
            inspector.Function = details.Function;
            inspector.ProjectLocation = details.Location;
            inspector.CertificationRequired = details.CertRequired;
            inspector.RequiredCertifications = details.CertsRequired;

            await _context.SaveChangesAsync();

            return new MobilizationResult
            {
                Success = true,
                InspectorId = inspectorId,
                MobilizationDate = details.MobDate
            };
        }

        public async Task<DemobilizationResult> DemobilizeInspectorAsync(
            int inspectorId,
            DemobilizationDetails details)
        {
            var inspector = await GetByIdAsync(inspectorId);
            if (inspector == null)
                throw new ArgumentException("Inspector not found", nameof(inspectorId));

            // Check for outstanding equipment
            var hasOutstandingEquipment = await _context.Equipment
                .AnyAsync(e => e.AssignedToInspectorId == inspectorId && e.IsOut);

            if (hasOutstandingEquipment)
            {
                return new DemobilizationResult
                {
                    Success = false,
                    Message = "Inspector has outstanding equipment that must be returned"
                };
            }

            inspector.DemobilizationDate = details.DemobDate;
            inspector.DemobilizationReason = details.DemobReason;
            inspector.Status = "Demobilized";

            await _context.SaveChangesAsync();

            return new DemobilizationResult
            {
                Success = true,
                InspectorId = inspectorId,
                DemobilizationDate = details.DemobDate
            };
        }

        public async Task<DrugTestResult> ManageDrugTestAsync(
            int inspectorId,
            DrugTestRecord testRecord)
        {
            var inspector = await GetByIdAsync(inspectorId);
            if (inspector == null)
                throw new ArgumentException("Inspector not found", nameof(inspectorId));

            var drugTest = new DrugTest
            {
                InspectorId = inspectorId,
                TestDate = testRecord.TestDate,
                TestType = testRecord.TestType,
                Frequency = testRecord.Frequency,
                Result = testRecord.Result,
                Comment = testRecord.Comment,
                Company = testRecord.Company,
                CreatedBy = testRecord.CreatedBy,
                ModifiedBy = testRecord.CreatedBy
            };

            await _context.DrugTests.AddAsync(drugTest);
            
            inspector.LastDrugTestDate = testRecord.TestDate;
            inspector.LastDrugTestResult = testRecord.Result;
            inspector.DrugTestIds.Add(drugTest.Id);

            await _context.SaveChangesAsync();

            return new DrugTestResult
            {
                Success = true,
                TestId = drugTest.Id,
                TestDate = drugTest.TestDate,
                Result = drugTest.Result
            };
        }

        public async Task<EquipmentAssignmentResult> ManageEquipmentAssignmentAsync(
            int inspectorId,
            EquipmentAssignment assignment)
        {
            var inspector = await GetByIdAsync(inspectorId);
            if (inspector == null)
                throw new ArgumentException("Inspector not found", nameof(inspectorId));

            var equipment = await _context.Equipment
                .FirstOrDefaultAsync(e => e.Id == assignment.EquipmentId);

            if (equipment == null)
                throw new ArgumentException("Equipment not found", nameof(assignment.EquipmentId));

            if (assignment.IsReturn)
            {
                equipment.RecordReturn(
                    assignment.Condition,
                    assignment.Date,
                    assignment.UpdatedBy);
            }
            else
            {
                equipment.AssignToInspector(
                    inspectorId,
                    assignment.Condition,
                    assignment.Date,
                    assignment.UpdatedBy);
            }

            await _context.SaveChangesAsync();

            return new EquipmentAssignmentResult
            {
                Success = true,
                EquipmentId = equipment.Id,
                AssignmentDate = assignment.Date,
                IsReturn = assignment.IsReturn
            };
        }

        public async Task<IEnumerable<EquipmentAssignment>> GetAssignedEquipmentAsync(int inspectorId)
        {
            return await _context.Equipment
                .Where(e => e.AssignedToInspectorId == inspectorId && e.IsOut)
                .Select(e => new EquipmentAssignment
                {
                    EquipmentId = e.Id,
                    Date = e.AssignedDate.Value,
                    Condition = e.AssignedCondition,
                    IsReturn = false
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DrugTestRecord>> GetDrugTestHistoryAsync(int inspectorId)
        {
            return await _context.DrugTests
                .Where(dt => dt.InspectorId == inspectorId && !dt.IsDeleted)
                .OrderByDescending(dt => dt.TestDate)
                .Select(dt => new DrugTestRecord
                {
                    TestDate = dt.TestDate,
                    TestType = dt.TestType,
                    Frequency = dt.Frequency,
                    Result = dt.Result,
                    Comment = dt.Comment,
                    Company = dt.Company,
                    CreatedBy = dt.CreatedBy
                })
                .ToListAsync();
        }
    }
}