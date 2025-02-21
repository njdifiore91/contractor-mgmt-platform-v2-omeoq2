using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents physical equipment that can be assigned to inspectors and tracked within the system.
    /// Implements tracking for equipment assignment, condition changes, and return status.
    /// </summary>
    [Table("Equipment")]
    public class Equipment
    {
        #region Properties

        /// <summary>
        /// Unique identifier for the equipment
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Model name/number of the equipment
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Model { get; set; }

        /// <summary>
        /// Unique serial number of the equipment
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Detailed description of the equipment
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Current physical condition of the equipment
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Condition { get; set; }

        /// <summary>
        /// Indicates if the equipment is currently assigned out
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// ID of the inspector to whom the equipment is currently assigned
        /// </summary>
        public int? AssignedToInspectorId { get; set; }

        /// <summary>
        /// Date when the equipment was assigned to an inspector
        /// </summary>
        public DateTime? AssignedDate { get; set; }

        /// <summary>
        /// Condition of the equipment when it was assigned out
        /// </summary>
        [MaxLength(100)]
        public string AssignedCondition { get; set; }

        /// <summary>
        /// Date when the equipment was returned
        /// </summary>
        public DateTime? ReturnedDate { get; set; }

        /// <summary>
        /// Condition of the equipment when it was returned
        /// </summary>
        [MaxLength(100)]
        public string ReturnedCondition { get; set; }

        /// <summary>
        /// Timestamp of when the equipment record was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp of the last update to the equipment record
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Username of who created the equipment record
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Username of who last updated the equipment record
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Navigation property for the assigned inspector
        /// </summary>
        [ForeignKey(nameof(AssignedToInspectorId))]
        public virtual Inspector AssignedInspector { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Equipment class with default values
        /// </summary>
        public Equipment()
        {
            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
            IsOut = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Assigns the equipment to an inspector with condition and date tracking
        /// </summary>
        /// <param name="inspectorId">ID of the inspector receiving the equipment</param>
        /// <param name="condition">Condition of the equipment at assignment</param>
        /// <param name="assignedDate">Date of assignment</param>
        /// <param name="updatedBy">Username of person making the assignment</param>
        /// <exception cref="ArgumentException">Thrown when inspectorId is invalid or condition is empty</exception>
        /// <exception cref="ArgumentNullException">Thrown when updatedBy is null or empty</exception>
        public void AssignToInspector(int inspectorId, string condition, DateTime assignedDate, string updatedBy)
        {
            if (inspectorId <= 0)
                throw new ArgumentException("Invalid inspector ID", nameof(inspectorId));

            if (string.IsNullOrWhiteSpace(condition))
                throw new ArgumentException("Condition must be specified", nameof(condition));

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentNullException(nameof(updatedBy));

            AssignedToInspectorId = inspectorId;
            AssignedCondition = condition;
            AssignedDate = assignedDate;
            IsOut = true;
            ReturnedDate = null;
            ReturnedCondition = null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Records the return of equipment with condition and date tracking
        /// </summary>
        /// <param name="condition">Condition of the equipment upon return</param>
        /// <param name="returnedDate">Date of return</param>
        /// <param name="updatedBy">Username of person recording the return</param>
        /// <exception cref="ArgumentException">Thrown when condition is empty or equipment is not currently assigned</exception>
        /// <exception cref="ArgumentNullException">Thrown when updatedBy is null or empty</exception>
        public void RecordReturn(string condition, DateTime returnedDate, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(condition))
                throw new ArgumentException("Condition must be specified", nameof(condition));

            if (!IsOut)
                throw new InvalidOperationException("Equipment is not currently assigned out");

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentNullException(nameof(updatedBy));

            ReturnedCondition = condition;
            ReturnedDate = returnedDate;
            IsOut = false;
            AssignedToInspectorId = null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        #endregion
    }
}