using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents an inspector entity with comprehensive tracking of personal information,
    /// professional status, assignments, and audit history.
    /// </summary>
    [Table("Inspectors")]
    public class Inspector
    {
        #region Properties

        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [PersonalData]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [PersonalData]
        public string LastName { get; set; }

        [MaxLength(50)]
        [PersonalData]
        public string MiddleName { get; set; }

        [MaxLength(50)]
        public string Nickname { get; set; }

        [Required]
        [EmailAddress]
        [PersonalData]
        public string Email { get; set; }

        [Phone]
        [PersonalData]
        public string Phone { get; set; }

        [Required]
        [PersonalData]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        public string InspectorId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [Required]
        [MaxLength(2)]
        public string State { get; set; }

        [MaxLength(100)]
        public string Title { get; set; }

        public List<string> Specialties { get; set; }

        public bool HasIssues { get; set; }

        public bool NeedsApproval { get; set; }

        public DateTime? MobilizationDate { get; set; }

        [MaxLength(50)]
        public string HireType { get; set; }

        [MaxLength(50)]
        public string Classification { get; set; }

        public bool CertificationRequired { get; set; }

        public List<string> RequiredCertifications { get; set; }

        [MaxLength(50)]
        public string Department { get; set; }

        [MaxLength(50)]
        public string Function { get; set; }

        [MaxLength(100)]
        public string ProjectLocation { get; set; }

        public DateTime? DemobilizationDate { get; set; }

        [MaxLength(200)]
        public string DemobilizationReason { get; set; }

        public List<int> DrugTestIds { get; set; }

        public DateTime? LastDrugTestDate { get; set; }

        [MaxLength(20)]
        public string LastDrugTestResult { get; set; }

        public List<int> AssignedEquipmentIds { get; set; }

        public List<string> AssignedCustomers { get; set; }

        public List<string> AssignedContracts { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string UpdatedBy { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Inspector class with default values and empty collections.
        /// </summary>
        public Inspector()
        {
            // Initialize collections
            Specialties = new List<string>();
            RequiredCertifications = new List<string>();
            DrugTestIds = new List<int>();
            AssignedEquipmentIds = new List<int>();
            AssignedCustomers = new List<string>();
            AssignedContracts = new List<string>();

            // Set default values
            HasIssues = false;
            NeedsApproval = false;
            CertificationRequired = false;

            // Initialize audit timestamps
            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates a properly formatted full name including optional middle name.
        /// </summary>
        /// <returns>Formatted full name with proper spacing and components.</returns>
        public string GetFullName()
        {
            var nameBuilder = new StringBuilder();
            nameBuilder.Append(FirstName);

            if (!string.IsNullOrWhiteSpace(MiddleName))
            {
                nameBuilder.Append(" ").Append(MiddleName);
            }

            nameBuilder.Append(" ").Append(LastName);
            return nameBuilder.ToString().Trim();
        }

        /// <summary>
        /// Records inspector mobilization with full audit trail.
        /// </summary>
        /// <param name="mobilizationDate">Date of mobilization</param>
        /// <param name="projectLocation">Location of the project</param>
        /// <param name="classification">Inspector classification</param>
        /// <param name="updatedBy">User performing the update</param>
        public void Mobilize(DateTime mobilizationDate, string projectLocation, string classification, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentNullException(nameof(updatedBy));

            MobilizationDate = mobilizationDate;
            ProjectLocation = projectLocation;
            Classification = classification;
            
            // Clear any existing demobilization data
            DemobilizationDate = null;
            DemobilizationReason = null;

            // Update audit trail
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Records inspector demobilization with reason and audit trail.
        /// </summary>
        /// <param name="demobilizationDate">Date of demobilization</param>
        /// <param name="reason">Reason for demobilization</param>
        /// <param name="updatedBy">User performing the update</param>
        public void Demobilize(DateTime demobilizationDate, string reason, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentNullException(nameof(updatedBy));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentNullException(nameof(reason));

            DemobilizationDate = demobilizationDate;
            DemobilizationReason = reason;

            // Update audit trail
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Updates drug test tracking information with audit trail.
        /// </summary>
        /// <param name="drugTestId">ID of the drug test</param>
        /// <param name="testDate">Date of the test</param>
        /// <param name="result">Test result</param>
        /// <param name="updatedBy">User performing the update</param>
        public void UpdateDrugTestReference(int drugTestId, DateTime testDate, string result, string updatedBy)
        {
            if (drugTestId <= 0)
                throw new ArgumentException("Invalid drug test ID", nameof(drugTestId));

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentNullException(nameof(updatedBy));

            if (string.IsNullOrWhiteSpace(result))
                throw new ArgumentNullException(nameof(result));

            DrugTestIds.Add(drugTestId);
            LastDrugTestDate = testDate;
            LastDrugTestResult = result;

            // Update audit trail
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        #endregion
    }
}