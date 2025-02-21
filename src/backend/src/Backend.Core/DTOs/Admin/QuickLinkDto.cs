using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Core.Entities;

namespace Backend.Core.DTOs.Admin
{
    /// <summary>
    /// Data Transfer Object representing a quick link for API operations with comprehensive validation
    /// </summary>
    [Serializable]
    public class QuickLinkDto : IValidatableObject
    {
        /// <summary>
        /// Unique identifier for the quick link
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display text for the quick link that will be shown to users
        /// </summary>
        [Required(ErrorMessage = "Label is required")]
        [StringLength(100, ErrorMessage = "Label cannot exceed 100 characters")]
        public string Label { get; set; }

        /// <summary>
        /// URL that the quick link points to. Must be a valid URL as links open in new tabs
        /// </summary>
        [Required(ErrorMessage = "Link URL is required")]
        [StringLength(2000, ErrorMessage = "Link URL cannot exceed 2000 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string Link { get; set; }

        /// <summary>
        /// Display order for the quick link. Used to control the sequence of links shown to users
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative number")]
        public int Order { get; set; }

        /// <summary>
        /// Initializes a new instance of the QuickLinkDto class with default values
        /// </summary>
        public QuickLinkDto()
        {
            Id = 0;
            Label = string.Empty;
            Link = string.Empty;
            Order = 0;
        }

        /// <summary>
        /// Creates a DTO from a QuickLink entity with input validation
        /// </summary>
        /// <param name="entity">The QuickLink entity to convert</param>
        /// <returns>A new QuickLinkDto instance populated with validated entity data</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        public static QuickLinkDto FromEntity(QuickLink entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "QuickLink entity cannot be null");
            }

            return new QuickLinkDto
            {
                Id = entity.Id,
                Label = entity.Label?.Trim() ?? string.Empty,
                Link = entity.Link?.Trim() ?? string.Empty,
                Order = entity.Order
            };
        }

        /// <summary>
        /// Creates a QuickLink entity from this DTO with validation
        /// </summary>
        /// <returns>A new QuickLink entity instance populated with validated DTO data</returns>
        public QuickLink ToEntity()
        {
            return new QuickLink
            {
                Id = this.Id,
                Label = this.Label?.Trim() ?? string.Empty,
                Link = this.Link?.Trim() ?? string.Empty,
                Order = this.Order,
                IsActive = true // Default to active for new quick links
            };
        }

        /// <summary>
        /// Implements custom validation logic for the QuickLinkDto
        /// </summary>
        /// <param name="validationContext">The validation context</param>
        /// <returns>Collection of validation results</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validate Label is not just whitespace
            if (string.IsNullOrWhiteSpace(Label))
            {
                results.Add(new ValidationResult(
                    "Label cannot be empty or whitespace",
                    new[] { nameof(Label) }
                ));
            }

            // Validate Link is not just whitespace and is a valid URL
            if (string.IsNullOrWhiteSpace(Link))
            {
                results.Add(new ValidationResult(
                    "Link cannot be empty or whitespace",
                    new[] { nameof(Link) }
                ));
            }
            else if (!Uri.TryCreate(Link, UriKind.Absolute, out _))
            {
                results.Add(new ValidationResult(
                    "Link must be a valid absolute URL",
                    new[] { nameof(Link) }
                ));
            }

            // Additional order validation if needed beyond the Range attribute
            if (Order < 0)
            {
                results.Add(new ValidationResult(
                    "Order must be a non-negative number",
                    new[] { nameof(Order) }
                ));
            }

            return results;
        }
    }
}