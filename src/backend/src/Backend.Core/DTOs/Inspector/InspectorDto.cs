using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Core.Entities;

namespace Backend.Core.DTOs.Inspector
{
    /// <summary>
    /// Data Transfer Object for inspector data that facilitates data transfer between 
    /// the API layer and client applications with comprehensive support for mobilization,
    /// demobilization, and contact tracking.
    /// </summary>
    public class InspectorDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string MiddleName { get; set; }

        [MaxLength(50)]
        public string Nickname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        public string InspectorId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [Required]
        [MaxLength(2)]
        public string State { get; set; }

        [MaxLength(100)]
        public string Title { get; set; }

        public List<string> Specialties { get; set; }

        public bool HasIssues { get; set; }

        public bool NeedsApproval { get; set; }

        public DateTime? MobilizationDate { get; set; }

        [MaxLength(50)]
        public string HireType { get; set; }

        [MaxLength(50)]
        public string Classification { get; set; }

        public bool CertificationRequired { get; set; }

        public List<string> RequiredCertifications { get; set; }

        [MaxLength(50)]
        public string Department { get; set; }

        [MaxLength(50)]
        public string Function { get; set; }

        [MaxLength(100)]
        public string ProjectLocation { get; set; }

        [MaxLength(100)]
        public string ProjectContact { get; set; }

        [MaxLength(100)]
        public string InvoiceContact { get; set; }

        [MaxLength(50)]
        public string ShipOpt { get; set; }

        public DateTime? DemobilizationDate { get; set; }

        [MaxLength(200)]
        public string DemobilizationReason { get; set; }

        [MaxLength(500)]
        public string DemobilizationNote { get; set; }

        public List<DrugTestDto> DrugTests { get; set; }

        public DateTime? LastDrugTestDate { get; set; }

        [MaxLength(20)]
        public string LastDrugTestResult { get; set; }

        public List<string> AssignedCustomers { get; set; }

        public List<string> AssignedContracts { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(50)]
        public string CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        [MaxLength(50)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Initializes a new instance of InspectorDto with empty collections
        /// </summary>
        public InspectorDto()
        {
            Specialties = new List<string>();
            RequiredCertifications = new List<string>();
            DrugTests = new List<DrugTestDto>();
            AssignedCustomers = new List<string>();
            AssignedContracts = new List<string>();
        }

        /// <summary>
        /// Creates a new DTO instance from an Inspector entity with validation
        /// </summary>
        /// <param name="entity">The source Inspector entity</param>
        /// <returns>A validated DTO instance populated with entity data</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        public static InspectorDto FromEntity(Inspector entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new InspectorDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                MiddleName = entity.MiddleName,
                Nickname = entity.Nickname,
                Email = entity.Email,
                Phone = entity.Phone,
                DateOfBirth = entity.DateOfBirth,
                InspectorId = entity.InspectorId,
                Status = entity.Status,
                State = entity.State,
                Title = entity.Title,
                Specialties = new List<string>(entity.Specialties),
                HasIssues = entity.HasIssues,
                NeedsApproval = entity.NeedsApproval,
                MobilizationDate = entity.MobilizationDate,
                HireType = entity.HireType,
                Classification = entity.Classification,
                CertificationRequired = entity.CertificationRequired,
                RequiredCertifications = new List<string>(entity.RequiredCertifications),
                Department = entity.Department,
                Function = entity.Function,
                ProjectLocation = entity.ProjectLocation,
                DemobilizationDate = entity.DemobilizationDate,
                DemobilizationReason = entity.DemobilizationReason,
                LastDrugTestDate = entity.LastDrugTestDate,
                LastDrugTestResult = entity.LastDrugTestResult,
                AssignedCustomers = new List<string>(entity.AssignedCustomers),
                AssignedContracts = new List<string>(entity.AssignedContracts),
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy
            };
        }

        /// <summary>
        /// Converts DTO to Inspector entity with validation
        /// </summary>
        /// <returns>A validated entity instance populated with DTO data</returns>
        public Inspector ToEntity()
        {
            // Validate required fields before conversion
            Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);

            return new Inspector
            {
                Id = this.Id,
                FirstName = this.FirstName.Trim(),
                LastName = this.LastName.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(this.MiddleName) ? null : this.MiddleName.Trim(),
                Nickname = string.IsNullOrWhiteSpace(this.Nickname) ? null : this.Nickname.Trim(),
                Email = this.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(this.Phone) ? null : this.Phone.Trim(),
                DateOfBirth = this.DateOfBirth,
                InspectorId = this.InspectorId.Trim(),
                Status = this.Status.Trim(),
                State = this.State.Trim(),
                Title = string.IsNullOrWhiteSpace(this.Title) ? null : this.Title.Trim(),
                Specialties = new List<string>(this.Specialties),
                HasIssues = this.HasIssues,
                NeedsApproval = this.NeedsApproval,
                MobilizationDate = this.MobilizationDate,
                HireType = string.IsNullOrWhiteSpace(this.HireType) ? null : this.HireType.Trim(),
                Classification = string.IsNullOrWhiteSpace(this.Classification) ? null : this.Classification.Trim(),
                CertificationRequired = this.CertificationRequired,
                RequiredCertifications = new List<string>(this.RequiredCertifications),
                Department = string.IsNullOrWhiteSpace(this.Department) ? null : this.Department.Trim(),
                Function = string.IsNullOrWhiteSpace(this.Function) ? null : this.Function.Trim(),
                ProjectLocation = string.IsNullOrWhiteSpace(this.ProjectLocation) ? null : this.ProjectLocation.Trim(),
                DemobilizationDate = this.DemobilizationDate,
                DemobilizationReason = string.IsNullOrWhiteSpace(this.DemobilizationReason) ? null : this.DemobilizationReason.Trim(),
                AssignedCustomers = new List<string>(this.AssignedCustomers),
                AssignedContracts = new List<string>(this.AssignedContracts),
                CreatedAt = this.CreatedAt,
                CreatedBy = this.CreatedBy,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = this.UpdatedBy
            };
        }
    }
}