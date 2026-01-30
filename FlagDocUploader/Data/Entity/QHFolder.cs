using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Data.Entity
{
    [Table("tblFolders", Schema = "qhse")]
    public class QHFolder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FolderId { get; set; }

        [Required]
        public int WorkspaceId { get; set; }
        public int? ParentFolderId { get; set; }
        [Required]
        [StringLength(500)]
        public string Name { get; set; }
        public string Description { get; set; }

        [StringLength(10)]
        public string ColorCode { get; set; }
        public int HierarchyLevel { get; set; } = 0;

        [StringLength(2000)]
        public string HierarchyPath { get; set; }

        public int CategoryId { get; set; }

        [StringLength(200)]
        public string CategoryName { get; set; }

        public int? SubCategoryId { get; set; }

        [StringLength(200)]
        public string SubCategoryName { get; set; }

        public bool IsFavorite { get; set; } = false;

        public bool? IsDeleted { get; set; } = false;

        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        public int FolderOrder { get; set; }

        [StringLength(2000)]
        public string AllowedRoles { get; set; }

        [StringLength(2000)]
        public string AllowedUsers { get; set; }

        [StringLength(2000)]
        public string SharedWithUserId { get; set; }
        public int DisplayOrder { get; set; }
        public int? VesselId { get; set; }
        public int? OwnerUserId { get; set; }
        public DateTime RecDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        [StringLength(500)]
        public string UDF1 { get; set; }
        [StringLength(500)]
        public string UDF2 { get; set; }

        [ForeignKey("ParentFolderId")]
        public virtual QHFolder ParentFolder { get; set; }
    }
}
