using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.Core.DTOs.Admin
{
    /// <summary>
    /// Data transfer object for code type data that includes the type name and its associated codes.
    /// Used for transferring code type information between API layer and client applications.
    /// </summary>
    [Serializable]
    public class CodeTypeDto
    {
        /// <summary>
        /// Unique identifier for the code type.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the code type category.
        /// </summary>
        [Required(ErrorMessage = "Code type name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Code type name must be between 1 and 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Optional description of the code type category.
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the code type is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp when the code type was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the code type was last modified. Null if never modified.
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Collection of codes associated with this code type.
        /// </summary>
        public ICollection<CodeDto> Codes { get; set; }

        /// <summary>
        /// Initializes a new instance of the CodeTypeDto class.
        /// </summary>
        public CodeTypeDto()
        {
            Codes = new List<CodeDto>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Name = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// Data transfer object for individual code entries within a code type.
    /// Includes validation and proper nullability handling.
    /// </summary>
    [Serializable]
    public class CodeDto
    {
        /// <summary>
        /// Unique identifier for the code.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The actual code value.
        /// </summary>
        [Required(ErrorMessage = "Code value is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Code value must be between 1 and 100 characters")]
        public string Value { get; set; }

        /// <summary>
        /// Descriptive text explaining the purpose or meaning of the code.
        /// </summary>
        [Required(ErrorMessage = "Code description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether this code can expire.
        /// </summary>
        public bool IsExpireable { get; set; }

        /// <summary>
        /// Indicates whether the code is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp when the code expires. Only applicable if IsExpireable is true.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// UTC timestamp when the code was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the code was last modified. Null if never modified.
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the CodeDto class.
        /// </summary>
        public CodeDto()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            IsExpireable = false;
            Value = string.Empty;
            Description = string.Empty;
        }
    }
}