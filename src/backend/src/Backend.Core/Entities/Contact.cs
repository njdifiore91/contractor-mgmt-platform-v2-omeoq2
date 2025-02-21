// System v6.0.0
using System;
// System.Collections.Generic v6.0.0
using System.Collections.Generic;
// System.ComponentModel.DataAnnotations v6.0.0
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents a contact person associated with a customer organization.
    /// Includes comprehensive contact information, audit tracking, and soft delete functionality.
    /// </summary>
    [Table("Contacts")]
    public class Contact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public int? ModifiedById { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? DeletedById { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }

        public virtual ICollection<Email> Emails { get; set; }

        public virtual ICollection<Phone> PhoneNumbers { get; set; }

        public virtual ICollection<Note> Notes { get; set; }

        public Contact()
        {
            Addresses = new HashSet<Address>();
            Emails = new HashSet<Email>();
            PhoneNumbers = new HashSet<Phone>();
            Notes = new HashSet<Note>();
            DateCreated = DateTime.UtcNow;
            IsDeleted = false;
            IsInactive = false;
            IsDeceased = false;
        }
    }

    [Table("Addresses")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        [Required]
        [ForeignKey("Contact")]
        public int ContactId { get; set; }

        public virtual Contact Contact { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public int? ModifiedById { get; set; }
    }

    [Table("Emails")]
    public class Email
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsPrimary { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(255)]
        public string EmailAddress { get; set; }

        [Required]
        [ForeignKey("Contact")]
        public int ContactId { get; set; }

        public virtual Contact Contact { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public int? ModifiedById { get; set; }
    }

    [Table("Phones")]
    public class Phone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        [Required]
        [ForeignKey("Contact")]
        public int ContactId { get; set; }

        public virtual Contact Contact { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public int? ModifiedById { get; set; }
    }

    [Table("Notes")]
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required(ErrorMessage = "Note text is required")]
        public string NoteText { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Contact")]
        public int ContactId { get; set; }

        public virtual Contact Contact { get; set; }

        public DateTime? DateModified { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public int? ModifiedById { get; set; }
    }
}