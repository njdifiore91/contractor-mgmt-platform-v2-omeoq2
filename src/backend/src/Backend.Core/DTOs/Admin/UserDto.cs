using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Core.DTOs.Admin
{
    /// <summary>
    /// Data Transfer Object for transferring user information between layers.
    /// Implements validation, security measures and proper documentation.
    /// </summary>
    [JsonSerializable(typeof(UserDto))]
    public class UserDto : ICloneable, IEquatable<UserDto>
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(256, ErrorMessage = "Email address cannot exceed 256 characters")]
        public string Email { get; set; }

        /// <summary>
        /// Indicates whether the user's email has been confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user
        /// </summary>
        [Required(ErrorMessage = "At least one role must be assigned")]
        public List<string> Roles { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp when the user account was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp of the user's last login, null if never logged in
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Indicates whether the user opts to receive email notifications
        /// </summary>
        public bool ReceiveEmails { get; set; }

        /// <summary>
        /// Initializes a new instance of the UserDto class with default values
        /// </summary>
        public UserDto()
        {
            Roles = new List<string>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            EmailConfirmed = false;
            ReceiveEmails = true;
        }

        /// <summary>
        /// Creates a deep copy of the UserDto instance
        /// </summary>
        /// <returns>A new UserDto instance with copied values</returns>
        public object Clone()
        {
            var cloned = new UserDto
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                EmailConfirmed = this.EmailConfirmed,
                IsActive = this.IsActive,
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                ReceiveEmails = this.ReceiveEmails,
                Roles = new List<string>(this.Roles)
            };
            return cloned;
        }

        /// <summary>
        /// Compares this UserDto instance with another for value equality
        /// </summary>
        /// <param name="other">The UserDto instance to compare with</param>
        /// <returns>True if the instances are equal, false otherwise</returns>
        public bool Equals(UserDto? other)
        {
            if (other is null) return false;
            
            return Id == other.Id &&
                   string.Equals(FirstName, other.FirstName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(LastName, other.LastName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase) &&
                   EmailConfirmed == other.EmailConfirmed &&
                   IsActive == other.IsActive &&
                   CreatedAt.Equals(other.CreatedAt) &&
                   ((LastLoginAt == null && other.LastLoginAt == null) || 
                    (LastLoginAt != null && LastLoginAt.Equals(other.LastLoginAt))) &&
                   ReceiveEmails == other.ReceiveEmails &&
                   Roles.Count == other.Roles.Count &&
                   Roles.TrueForAll(role => other.Roles.Contains(role, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as UserDto);
        }

        /// <summary>
        /// Serves as the default hash function
        /// </summary>
        /// <returns>A hash code for the current object</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add(FirstName, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(LastName, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Email, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(EmailConfirmed);
            hashCode.Add(IsActive);
            hashCode.Add(CreatedAt);
            hashCode.Add(LastLoginAt);
            hashCode.Add(ReceiveEmails);
            foreach (var role in Roles)
            {
                hashCode.Add(role, StringComparer.OrdinalIgnoreCase);
            }
            return hashCode.ToHashCode();
        }
    }
}