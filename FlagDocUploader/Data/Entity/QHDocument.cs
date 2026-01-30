using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlagDocUploader.Data.Entity
{
    [Table("tblDocuments", Schema = "qhse")]
    public class QHDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }
        [Required]
        public int FolderId { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        public string Description { get; set; }

        [StringLength(100)]
        public string DocumentType { get; set; }
        public string CVContent { get; set; }
        [StringLength(500)]
        public string FilePath { get; set; }
        [StringLength(255)]
        public string FileName { get; set; }
        public int? FileSize { get; set; }
        [StringLength(100)]
        public string MimeType { get; set; }
        public byte[] CVFileBytes { get; set; }
        public byte[] CVThumbnailBytes { get; set; }
        public int RevisionNumber { get; set; } = 1;
        public DateTime? RevisionDate { get; set; }
        public int? VersionNumber { get; set; } = 1;

        [StringLength(7)]
        public string ColorCode { get; set; }
        public int CategoryId { get; set; }

        [StringLength(200)]
        public string CategoryName { get; set; }
        public int? SubCategoryId { get; set; }

        [StringLength(200)]
        public string SubCategoryName { get; set; }
        public bool IsFavorite { get; set; } = false;
        public DateTime? ExpireDate { get; set; }
        public bool IsExpired { get; set; } = false;
        public string CircularDetailsJson { get; set; }       
        public string AdditionalPublicationDetailsJson { get; set; }
        
        public string FleetAlertDetailsJson { get; set; }
       
        [StringLength(20)]
        public string Status { get; set; } = "draft"; // "draft", "pending", "approved", "rejected","cancel", "published" 
        [StringLength(500)]
        public string RejectionReason { get; set; }
        [StringLength(2000)]
        public string AllowedRoles { get; set; }
        [StringLength(2000)]
        public string AllowedUsers { get; set; }
        [StringLength(2000)]
        public string SharedWithUserId { get; set; }
        public int DisplayOrder { get; set; }
        public int? VesselId { get; set; }
        public int? OwnerUserId { get; set; }
        public bool? VisibleOnIndex { get; set; } = false;
        public bool? IsDeleted { get; set; } = false;
        public DateTime RecDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedDate { get; set; }
        public int? PublishedBy { get; set; }
        public int AccessCount { get; set; } = 0;
        [StringLength(500)]
        public string UDF1 { get; set; }
        [StringLength(500)]
        public string UDF2 { get; set; }
        public int? ParentDocumentId { get; set; }
        public bool IsTemporaryVersion { get; set; } = false;
        public int? RoleId { get; set; }
        public int? AssigneeUserId { get; set; }
        public DateTime? TargetTransitionDate { get; set; }
        public DateTime? TakenTransitionDate { get; set; }
        public string TechFromLink { get; set; }
    }

}
