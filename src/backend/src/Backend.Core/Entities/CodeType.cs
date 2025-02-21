// System v6.0.0
using System;
using System.Collections.Generic;
// System.ComponentModel.DataAnnotations v6.0.0
using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents a category or type of codes in the system that groups related codes together.
    /// Supports soft deletion, audit trailing, and comprehensive validation.
    /// </summary>
    public class CodeType
    {
        /// <summary>
        /// Unique identifier for the code type.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The name of the code type category.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Descriptive text explaining the purpose or usage of this code type.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Collection of codes belonging to this code type.
        /// </summary>
        public virtual ICollection<Code> Codes { get; set; }

        /// <summary>
        /// Indicates whether the code type is currently active in the system.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp when the code type was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the code type was last updated. Null if never updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the CodeType class with default values.
        /// </summary>
        public CodeType()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Codes = new List<Code>();
            UpdatedAt = null;
            Name = string.Empty;
            Description = string.Empty;
        }

        /// <summary>
        /// Adds a new code to the CodeType's collection.
        /// </summary>
        /// <param name="code">The code entity to add to this code type.</param>
        /// <exception cref="ArgumentNullException">Thrown when code parameter is null.</exception>
        public void AddCode(Code code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code), "Code cannot be null.");
            }

            code.CodeTypeId = this.Id;
            Codes.Add(code);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the CodeType as inactive for soft deletion.
        /// </summary>
        public void MarkInactive()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the CodeType's details and maintains audit trail.
        /// </summary>
        /// <param name="name">The new name for the code type.</param>
        /// <param name="description">The new description for the code type.</param>
        /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
        public void UpdateDetails(string name, string description)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            Name = name;
            Description = description ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}