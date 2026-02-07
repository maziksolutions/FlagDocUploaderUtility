using FlagDocUploader.Data.Entity;
using FlagDocUploader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Services
{
    public interface IFolderService
    {
        Task<List<FolderItem>> GetCircularFlagFoldersAsync();
        Task<List<FolderItem>> GetFoldersByFilterAsync(string categoryName, string subCategoryName);
        Task<QHFolder?> GetFolderByNameAsync(string FolderName, int CategoryId, int? SubCategoryId, int? ParentFolderId = 0);
    }
}
