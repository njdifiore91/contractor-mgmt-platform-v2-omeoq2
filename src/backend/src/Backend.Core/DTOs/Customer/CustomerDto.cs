// System v6.0.0
using System;
using System.Collections.Generic;
// System.ComponentModel.DataAnnotations v6.0.0
using System.ComponentModel.DataAnnotations;
// System.Text.Json v6.0.0
using System.Text.Json.Serialization;

namespace Backend.Core.DTOs.Customer
{
    /// <summary>
    /// Data transfer object representing customer information with comprehensive validation 
    /// and relationship management. Facilitates customer data transfer between API and client applications.
    /// </summary>
    [Serializable]
    public class CustomerDto
    {
        /// <summary>
        /// Unique identifier for the customer.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        /// <summary>
        /// Name of the customer organization.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        /// <summary>
        /// Unique identifier code for the customer.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Code { get; set; }

        /// <summary>
        /// Indicates whether the customer record is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp when the customer record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Username or identifier of the user who created the customer record.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Collection of contacts associated with this customer.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public ICollection<ContactDto> Contacts { get; set; }

        /// <summary>
        /// Collection of contracts associated with this customer.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public ICollection<ContractDto> Contracts { get; set; }

        /// <summary>
        /// Initializes a new instance of the CustomerDto class with default values
        /// and collection initialization.
        /// </summary>
        public CustomerDto()
        {
            Contacts = new List<ContactDto>();
            Contracts = new List<ContractDto>();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            CreatedBy = string.Empty;
        }
    }
}