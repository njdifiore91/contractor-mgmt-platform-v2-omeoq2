using System.Threading.Tasks;
using System.Collections.Generic;
using Backend.Core.Entities;

namespace Backend.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface defining comprehensive data access operations for Inspector entities.
    /// Handles CRUD operations, geographical search, mobilization/demobilization tracking,
    /// drug test management, and equipment assignments.
    /// </summary>
    public interface IInspectorRepository
    {
        /// <summary>
        /// Performs geographical search for inspectors based on zip code and radius.
        /// </summary>
        /// <param name="zipCode">Target zip code for geographical search</param>
        /// <param name="radiusMiles">Search radius in miles</param>
        /// <param name="filters">Additional search filters including status, specialties, and availability</param>
        /// <returns>Paginated search results with matching inspectors and total count</returns>
        Task<SearchResult<Inspector>> SearchInspectorsAsync(
            string zipCode,
            int radiusMiles,
            SearchFilters filters);

        /// <summary>
        /// Records inspector mobilization with comprehensive tracking of hire details and certifications.
        /// </summary>
        /// <param name="inspectorId">ID of the inspector to mobilize</param>
        /// <param name="details">Mobilization details including project assignment and required certifications</param>
        /// <returns>Result containing mobilization status and any validation errors</returns>
        Task<MobilizationResult> MobilizeInspectorAsync(
            int inspectorId,
            MobilizationDetails details);

        /// <summary>
        /// Processes inspector demobilization with reason tracking and equipment return handling.
        /// </summary>
        /// <param name="inspectorId">ID of the inspector to demobilize</param>
        /// <param name="details">Demobilization details including reason, date and notes</param>
        /// <returns>Result containing demobilization status and pending actions</returns>
        Task<DemobilizationResult> DemobilizeInspectorAsync(
            int inspectorId,
            DemobilizationDetails details);

        /// <summary>
        /// Manages creation and updates of inspector drug test records.
        /// </summary>
        /// <param name="inspectorId">ID of the inspector</param>
        /// <param name="testRecord">Drug test record details including date, type, frequency, and results</param>
        /// <returns>Result containing test status and compliance information</returns>
        Task<DrugTestResult> ManageDrugTestAsync(
            int inspectorId,
            DrugTestRecord testRecord);

        /// <summary>
        /// Handles equipment assignment and return processing with condition tracking.
        /// </summary>
        /// <param name="inspectorId">ID of the inspector</param>
        /// <param name="assignment">Equipment assignment details including condition and dates</param>
        /// <returns>Result containing assignment status and equipment details</returns>
        Task<EquipmentAssignmentResult> ManageEquipmentAssignmentAsync(
            int inspectorId,
            EquipmentAssignment assignment);

        /// <summary>
        /// Retrieves an inspector by their unique identifier.
        /// </summary>
        /// <param name="id">Inspector ID</param>
        /// <returns>Inspector entity if found, null otherwise</returns>
        Task<Inspector> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all active inspectors.
        /// </summary>
        /// <returns>Collection of active inspector entities</returns>
        Task<IEnumerable<Inspector>> GetAllActiveAsync();

        /// <summary>
        /// Creates a new inspector record.
        /// </summary>
        /// <param name="inspector">Inspector entity to create</param>
        /// <returns>Created inspector with assigned ID</returns>
        Task<Inspector> CreateAsync(Inspector inspector);

        /// <summary>
        /// Updates an existing inspector record.
        /// </summary>
        /// <param name="inspector">Inspector entity with updated information</param>
        /// <returns>True if update successful, false otherwise</returns>
        Task<bool> UpdateAsync(Inspector inspector);

        /// <summary>
        /// Retrieves all equipment currently assigned to an inspector.
        /// </summary>
        /// <param name="inspectorId">ID of the inspector</param>
        /// <returns>Collection of assigned equipment details</returns>
        Task<IEnumerable<EquipmentAssignment>> GetAssignedEquipmentAsync(int inspectorId);

        /// <summary>
        /// Retrieves complete drug test history for an inspector.
        /// </summary>
        /// <param name="inspectorId">ID of the inspector</param>
        /// <returns>Collection of drug test records</returns>
        Task<IEnumerable<DrugTestRecord>> GetDrugTestHistoryAsync(int inspectorId);
    }

    /// <summary>
    /// Represents search result data with pagination support.
    /// </summary>
    /// <typeparam name="T">Type of entities in search results</typeparam>
    public class SearchResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}