// System v6.0.0
using System;
using System.Collections.Generic;
// System.ComponentModel.DataAnnotations v6.0.0
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// System.Text.Json v6.0.0
using System.Text.Json.Serialization;

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents a user in the system with comprehensive authentication, authorization,
    /// profile information and audit capabilities.
    /// </summary>
    [Table("Users")]
    public class User : IEquatable<User>
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// User's email address used for authentication and communication
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public string Email { get; set; }

        /// <summary>
        /// Indicates whether the user's email has been confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Hashed password for secure authentication
        /// </summary>
        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Security stamp used for tracking security-related changes
        /// </summary>
        [Required]
        [JsonIgnore]
        public string SecurityStamp { get; set; }

        /// <summary>
        /// Concurrency stamp for handling concurrent updates
        /// </summary>
        [Required]
        public string ConcurrencyStamp { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user
        /// </summary>
        [Required]
        public ICollection<string> Roles { get; set; }

        /// <summary>
        /// Collection of specific permissions assigned to the user
        /// </summary>
        [Required]
        public ICollection<string> Permissions { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Timestamp when the user account was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp of the user's last login
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Indicates whether the user has opted to receive emails
        /// </summary>
        public bool ReceiveEmails { get; set; }

        /// <summary>
        /// Timestamp of the user's last password change
        /// </summary>
        public DateTime? LastPasswordChangeAt { get; set; }

        /// <summary>
        /// Indicates whether the user must change their password on next login
        /// </summary>
        public bool RequirePasswordChange { get; set; }

        /// <summary>
        /// Initializes a new instance of the User class with secure defaults
        /// </summary>
        public User()
        {
            Roles = new HashSet<string>();
            Permissions = new HashSet<string>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            EmailConfirmed = false;
            ReceiveEmails = true;
            SecurityStamp = Guid.NewGuid().ToString();
            ConcurrencyStamp = Guid.NewGuid().ToString();
            RequirePasswordChange = true;
        }

        /// <summary>
        /// Implements secure comparison for User entities
        /// </summary>
        /// <param name="other">User instance to compare with</param>
        /// <returns>True if users are equal, false otherwise</returns>
        public bool Equals(User other)
        {
            if (other is null)
                return false;

            return Id == other.Id && 
                   SecurityStamp == other.SecurityStamp;
        }

        /// <summary>
        /// Overrides the default equality operator
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns>True if objects are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        /// <summary>
        /// Generates hash code for secure comparisons
        /// </summary>
        /// <returns>Hash code based on Id and SecurityStamp</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SecurityStamp);
        }
    }
}