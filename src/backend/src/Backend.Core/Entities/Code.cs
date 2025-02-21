// System v6.0.0
using System;
// System.ComponentModel.DataAnnotations v6.0.0
using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents an individual code value within a code type category with comprehensive tracking and validation capabilities.
    /// This entity is managed by administrators and supports features like expiration, audit trails, and validation.
    /// </summary>
    public class Code
    {
        /// <summary>
        /// Unique identifier for the code.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The actual code value.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Value { get; set; }

        /// <summary>
        /// Descriptive text explaining the purpose or meaning of the code.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether this code can expire.
        /// </summary>
        public bool IsExpireable { get; set; }

        /// <summary>
        /// UTC timestamp when the code was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the code was last updated. Null if never updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the code expires. Only applicable if IsExpireable is true.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Indicates whether the code is currently active in the system.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Foreign key reference to the parent CodeType.
        /// </summary>
        [Required]
        public int CodeTypeId { get; set; }

        /// <summary>
        /// Navigation property to the parent CodeType.
        /// </summary>
        public virtual CodeType CodeType { get; set; }

        /// <summary>
        /// Initializes a new instance of the Code class with default values.
        /// </summary>
        public Code()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            IsExpireable = false;
            Value = string.Empty;
            Description = string.Empty;
        }

        /// <summary>
        /// Marks the code as expired if it is expireable.
        /// </summary>
        /// <param name="expirationDate">The UTC datetime when the code should expire.</param>
        /// <returns>True if expiration was successful, false if code is not expireable.</returns>
        public bool Expire(DateTime expirationDate)
        {
            if (!IsExpireable)
            {
                return false;
            }

            ExpiresAt = expirationDate;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
    }
}