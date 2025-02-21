using System;
using System.ComponentModel.DataAnnotations;
using Backend.Core.Entities;

namespace Backend.Core.DTOs.Equipment
{
    /// <summary>
    /// Data Transfer Object for equipment data transfer with enhanced validation and security
    /// </summary>
    [Serializable]
    [DataProtection]
    public class EquipmentDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Model { get; set; }

        [Required]
        [MaxLength(50)]
        public string SerialNumber { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Condition { get; set; }

        public bool IsOut { get; set; }

        public int? AssignedToInspectorId { get; set; }

        public string AssignedToInspectorName { get; set; }

        [Required]
        public int CompanyId { get; set; }

        public DateTime? AssignedDate { get; set; }

        [MaxLength(50)]
        public string AssignedCondition { get; set; }

        public DateTime? ReturnedDate { get; set; }

        [MaxLength(50)]
        public string ReturnedCondition { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; }

        [Required]
        [MaxLength(50)]
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for equipment assignment operations with enhanced validation
    /// </summary>
    [ValidateEquipmentAssignment]
    public class EquipmentAssignmentDto
    {
        [Required]
        public int EquipmentId { get; set; }

        [Required]
        public int InspectorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Condition { get; set; }

        [Required]
        public DateTime AssignmentDate { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for equipment return operations with comprehensive validation
    /// </summary>
    [ValidateEquipmentReturn]
    public class EquipmentReturnDto
    {
        [Required]
        public int EquipmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReturnCondition { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }
    }

    /// <summary>
    /// Custom validation attribute for equipment assignment operations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidateEquipmentAssignmentAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dto = (EquipmentAssignmentDto)value;

            if (dto.AssignmentDate > DateTime.UtcNow)
            {
                return new ValidationResult("Assignment date cannot be in the future.");
            }

            if (dto.EquipmentId <= 0)
            {
                return new ValidationResult("Invalid equipment ID.");
            }

            if (dto.InspectorId <= 0)
            {
                return new ValidationResult("Invalid inspector ID.");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Custom validation attribute for equipment return operations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidateEquipmentReturnAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dto = (EquipmentReturnDto)value;

            if (dto.ReturnDate > DateTime.UtcNow)
            {
                return new ValidationResult("Return date cannot be in the future.");
            }

            if (dto.EquipmentId <= 0)
            {
                return new ValidationResult("Invalid equipment ID.");
            }

            return ValidationResult.Success;
        }
    }
}