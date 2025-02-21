// System v6.0.0
using System;
// System.ComponentModel.DataAnnotations v6.0.0
using System.ComponentModel.DataAnnotations;

namespace Backend.Core.DTOs.Customer
{
    /// <summary>
    /// Data Transfer Object representing contract information for API operations.
    /// Handles contract data transfer between API and business layers with comprehensive validation.
    /// </summary>
    public class ContractDto
    {
        /// <summary>
        /// Unique identifier for the contract.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Contract ID")]
        [Display(Name = "Contract ID")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the contract. Must be between 2 and 100 characters and contain only
        /// letters, numbers, spaces, hyphens, and underscores.
        /// </summary>
        [Required(ErrorMessage = "Contract name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Contract name must be between 2 and 100 characters")]
        [Display(Name = "Contract Name")]
        [RegularExpression(@"^[a-zA-Z0-9\s-_]*$", ErrorMessage = "Contract name can only contain letters, numbers, spaces, hyphens, and underscores")]
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the contract is currently active.
        /// </summary>
        [Display(Name = "Is Active")]
        public bool Active { get; set; }

        /// <summary>
        /// Date and time when the contract was created.
        /// </summary>
        [Required]
        [Display(Name = "Creation Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Username or identifier of the user who created the contract.
        /// </summary>
        [Required(ErrorMessage = "Creator information is required")]
        [StringLength(50, ErrorMessage = "Creator name cannot exceed 50 characters")]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// ID of the customer associated with this contract.
        /// </summary>
        [Required(ErrorMessage = "Customer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Customer ID")]
        [Display(Name = "Customer ID")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Initializes a new instance of the ContractDto class with default values.
        /// </summary>
        public ContractDto()
        {
            Active = true;
            CreatedAt = DateTime.UtcNow;
        }
    }
}