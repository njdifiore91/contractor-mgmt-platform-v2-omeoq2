using System;
using System.ComponentModel.DataAnnotations;
using Backend.Core.Entities;

namespace Backend.Core.DTOs.Inspector
{
    /// <summary>
    /// Data Transfer Object for drug test records that facilitates secure data transfer
    /// between the API layer and client applications with comprehensive validation.
    /// </summary>
    public class DrugTestDto
    {
        /// <summary>
        /// Unique identifier for the drug test record
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key reference to the Inspector entity
        /// </summary>
        [Required(ErrorMessage = "Inspector ID is required")]
        public int InspectorId { get; set; }

        /// <summary>
        /// Date when the drug test was conducted
        /// </summary>
        [Required(ErrorMessage = "Test date is required")]
        public DateTime TestDate { get; set; }

        /// <summary>
        /// Type of drug test performed
        /// </summary>
        [Required(ErrorMessage = "Test type is required")]
        [MaxLength(50, ErrorMessage = "Test type cannot exceed 50 characters")]
        public string TestType { get; set; }

        /// <summary>
        /// Frequency of the drug test (e.g., Random, Annual, Pre-employment)
        /// </summary>
        [Required(ErrorMessage = "Frequency is required")]
        [MaxLength(50, ErrorMessage = "Frequency cannot exceed 50 characters")]
        public string Frequency { get; set; }

        /// <summary>
        /// Result of the drug test
        /// </summary>
        [Required(ErrorMessage = "Result is required")]
        [MaxLength(50, ErrorMessage = "Result cannot exceed 50 characters")]
        public string Result { get; set; }

        /// <summary>
        /// Additional comments or notes about the drug test
        /// </summary>
        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }

        /// <summary>
        /// Company that administered or requested the drug test
        /// </summary>
        [Required(ErrorMessage = "Company is required")]
        [MaxLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string Company { get; set; }

        /// <summary>
        /// Timestamp when the record was created
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// User who created the record
        /// </summary>
        [MaxLength(50)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Timestamp when the record was last modified
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// User who last modified the record
        /// </summary>
        [MaxLength(50)]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Initializes a new instance of DrugTestDto with default values
        /// </summary>
        public DrugTestDto()
        {
            var now = DateTime.UtcNow;
            Created = now;
            Modified = now;
            TestType = string.Empty;
            Frequency = string.Empty;
            Result = string.Empty;
            Comment = string.Empty;
            Company = string.Empty;
            CreatedBy = "System";
            ModifiedBy = "System";
        }

        /// <summary>
        /// Creates a new DTO instance from a DrugTest entity with validation
        /// </summary>
        /// <param name="entity">The source DrugTest entity</param>
        /// <returns>A validated DTO instance populated with entity data</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        public static DrugTestDto FromEntity(DrugTest entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new DrugTestDto
            {
                Id = entity.Id,
                InspectorId = entity.InspectorId,
                TestDate = entity.TestDate,
                TestType = entity.TestType,
                Frequency = entity.Frequency,
                Result = entity.Result,
                Comment = entity.Comment ?? string.Empty,
                Company = entity.Company,
                Created = entity.Created,
                CreatedBy = entity.CreatedBy,
                Modified = entity.Modified,
                ModifiedBy = entity.ModifiedBy
            };
        }

        /// <summary>
        /// Converts DTO to DrugTest entity with validation
        /// </summary>
        /// <returns>A validated entity instance populated with DTO data</returns>
        public DrugTest ToEntity()
        {
            // Validate required fields before conversion
            Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);

            return new DrugTest
            {
                Id = this.Id,
                InspectorId = this.InspectorId,
                TestDate = this.TestDate,
                TestType = this.TestType.Trim(),
                Frequency = this.Frequency.Trim(),
                Result = this.Result.Trim(),
                Comment = string.IsNullOrWhiteSpace(this.Comment) ? null : this.Comment.Trim(),
                Company = this.Company.Trim(),
                Created = this.Created,
                CreatedBy = this.CreatedBy,
                Modified = DateTime.UtcNow,
                ModifiedBy = this.ModifiedBy
            };
        }
    }
}