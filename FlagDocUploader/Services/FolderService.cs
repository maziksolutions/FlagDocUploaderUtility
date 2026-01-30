using FlagDocUploader.Data;
using FlagDocUploader.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Services
{
    public class FolderService : IFolderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FolderService> _logger;

        public FolderService(AppDbContext context, ILogger<FolderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<FolderItem>> GetCircularFlagFoldersAsync()
        {
            return await GetFoldersByFilterAsync("Circular", "Circular Flag");
        }

        public async Task<List<FolderItem>> GetFoldersByFilterAsync(string categoryName, string subCategoryName)
        {
            try
            {
                var folders = await _context.Folders
                    .Where(f => f.CategoryName == categoryName &&
                               f.SubCategoryName == subCategoryName &&
                               f.ParentFolderId == null &&
                               f.IsDeleted == false)
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new FolderItem
                    {
                        FolderId = f.FolderId,
                        FolderName = f.Name,
                        WorkspaceId = f.WorkspaceId,
                        ParentFolderId = f.ParentFolderId,
                        CategoryId = f.CategoryId,
                        CategoryName = f.CategoryName,
                        SubCategoryId = f.SubCategoryId,
                        SubCategoryName = f.SubCategoryName,
                        HierarchyLevel = f.HierarchyLevel,
                        HierarchyPath = f.HierarchyPath,
                        DisplayOrder = f.DisplayOrder
                    })
                    .ToListAsync();                
                _logger.LogInformation($"Retrieved {folders.Count} folders for {categoryName} - {subCategoryName}");

                return folders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving folders for {categoryName} - {subCategoryName}");
                throw;
            }
        }

        
    }
}
