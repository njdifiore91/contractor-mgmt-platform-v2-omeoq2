using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents a drug test record for an inspector with comprehensive test details,
    /// metadata, and enhanced enterprise features for tracking and auditing.
    /// </summary>
    [Table("DrugTests")]
    [Index(nameof(InspectorId))]
    [Index(nameof(TestDate))]
    public class DrugTest
    {
        /// <summary>
        /// Unique identifier for the drug test record
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key reference to the Inspector entity
        /// </summary>
        [Required]
        public int InspectorId { get; set; }

        /// <summary>
        /// Navigation property to the associated Inspector
        /// </summary>
        [ForeignKey(nameof(InspectorId))]
        public virtual Inspector Inspector { get; set; }

        /// <summary>
        /// Date when the drug test was conducted
        /// </summary>
        [Required]
        public DateTime TestDate { get; set; }

        /// <summary>
        /// Type of drug test performed
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TestType { get; set; }

        /// <summary>
        /// Frequency of the drug test (e.g., Random, Annual, Pre-employment)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Frequency { get; set; }

        /// <summary>
        /// Result of the drug test
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Result { get; set; }

        /// <summary>
        /// Additional comments or notes about the drug test
        /// </summary>
        [MaxLength(500)]
        public string Comment { get; set; }

        /// <summary>
        /// Company that administered or requested the drug test
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Company { get; set; }

        /// <summary>
        /// Timestamp when the record was created
        /// </summary>
        [Required]
        public DateTime Created { get; set; }

        /// <summary>
        /// User who created the record
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Timestamp when the record was last modified
        /// </summary>
        [Required]
        public DateTime Modified { get; set; }

        /// <summary>
        /// User who last modified the record
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Soft delete flag for the record
        /// </summary>
        [Required]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Concurrency token for optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the DrugTest class with default values
        /// </summary>
        public DrugTest()
        {
            var now = DateTime.UtcNow;
            Created = now;
            Modified = now;
            IsDeleted = false;
        }
    }
}