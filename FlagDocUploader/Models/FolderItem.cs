using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Models
{
    public class FolderItem
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public int WorkspaceId { get; set; }
        public int? ParentFolderId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public int HierarchyLevel { get; set; }
        public string? HierarchyPath { get; set; }
        public int DisplayOrder { get; set; }
        // Display property for ComboBox
        public string DisplayText => FolderPath;

        public int? OwnerUserId { get; internal set; }

        public override string ToString() => DisplayText;
    }
}
