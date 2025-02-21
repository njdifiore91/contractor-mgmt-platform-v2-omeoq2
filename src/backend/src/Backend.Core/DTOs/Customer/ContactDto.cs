// System v6.0.0
using System;
// System.ComponentModel.DataAnnotations v6.0.0
using System.ComponentModel.DataAnnotations;
// System.Collections.Generic v6.0.0
using System.Collections.Generic;

namespace Backend.Core.DTOs.Customer
{
    /// <summary>
    /// Data transfer object for Contact entity with comprehensive validation and relationship management
    /// </summary>
    public class ContactDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [StringLength(10, ErrorMessage = "Suffix cannot exceed 10 characters")]
        public string Suffix { get; set; }

        [StringLength(50, ErrorMessage = "Nickname cannot exceed 50 characters")]
        public string Nickname { get; set; }

        public bool IsDeceased { get; set; }

        public bool IsInactive { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public int Rating { get; set; }

        [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string JobTitle { get; set; }

        public DateTime? Birthday { get; set; }

        public DateTime DateCreated { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public ICollection<AddressDto> Addresses { get; set; } = new List<AddressDto>();

        public ICollection<EmailDto> Emails { get; set; } = new List<EmailDto>();

        public ICollection<PhoneDto> PhoneNumbers { get; set; } = new List<PhoneDto>();

        public ICollection<NoteDto> Notes { get; set; } = new List<NoteDto>();
    }

    /// <summary>
    /// Data transfer object for Address entity with validation for required fields
    /// </summary>
    public class AddressDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Address type is required")]
        [StringLength(50)]
        public string AddressType { get; set; }

        [Required(ErrorMessage = "Address line 1 is required")]
        [StringLength(100)]
        public string Line1 { get; set; }

        [StringLength(100)]
        public string Line2 { get; set; }

        [StringLength(100)]
        public string Line3 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50)]
        public string State { get; set; }

        [Required(ErrorMessage = "ZIP code is required")]
        [StringLength(20)]
        public string Zip { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100)]
        public string Country { get; set; }

        public int ContactId { get; set; }
    }

    /// <summary>
    /// Data transfer object for Email entity with email format validation
    /// </summary>
    public class EmailDto
    {
        public int Id { get; set; }

        public bool IsPrimary { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(255)]
        public string EmailAddress { get; set; }

        public int ContactId { get; set; }
    }

    /// <summary>
    /// Data transfer object for Phone entity with phone number format validation
    /// </summary>
    public class PhoneDto
    {
        public int Id { get; set; }

        public bool IsPrimary { get; set; }

        [Required(ErrorMessage = "Phone type is required")]
        [StringLength(50)]
        public string PhoneType { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(10)]
        public string Extension { get; set; }

        public int ContactId { get; set; }
    }

    /// <summary>
    /// Data transfer object for Note entity with creation tracking
    /// </summary>
    public class NoteDto
    {
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        [Required(ErrorMessage = "Note text is required")]
        public string NoteText { get; set; }

        public int UserId { get; set; }

        public int ContactId { get; set; }
    }
}