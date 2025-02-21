using System;
using System.ComponentModel.DataAnnotations; // v6.0.0
using System.ComponentModel.DataAnnotations.Schema; // v6.0.0

namespace Backend.Core.Entities
{
    /// <summary>
    /// Represents a quick link that can be configured by administrators and displayed to users based on permissions.
    /// Quick links provide easy access to frequently used resources and open in new tabs.
    /// </summary>
    [Table("QuickLinks")]
    public class QuickLink
    {
        /// <summary>
        /// Unique identifier for the quick link
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        [Required(ErrorMessage = "Link is required")]
        [StringLength(2000, ErrorMessage = "Link cannot exceed 2000 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Link { get; set; }

        /// <summary>
        /// Display order for the quick link. Used to control the sequence of links shown to users
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative number")]
        public int Order { get; set; }

        /// <summary>
        /// Indicates whether the quick link is currently active and should be displayed
        /// </summary>
        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp when the quick link was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Username or identifier of the user who created the quick link
        /// </summary>
        [Required]
        [StringLength(256)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the quick link was last modified, null if never modified
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Username or identifier of the user who last modified the quick link
        /// </summary>
        [StringLength(256)]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Initializes a new instance of the QuickLink class with default values
        /// </summary>
        public QuickLink()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Order = 0;
            Label = string.Empty;
            Link = string.Empty;
            CreatedBy = string.Empty;
            ModifiedBy = string.Empty;
        }
    }
}