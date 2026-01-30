using FlagDocUploader.Data;
using FlagDocUploader.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(AppDbContext context, ILogger<DocumentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return false;
            }
        }

        public async Task<QHFolder> AddFolderAsync(QHFolder folder)
        {
            try
            {
                await _context.Folders.AddAsync(folder);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Folder {folder.Name} created with ID: {folder.FolderId}");
                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding folder: {folder.Name}");
                throw;
            }
        }

        public async Task<QHDocument> AddDocumentAsync(QHDocument document)
        {
            try
            {
                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync();
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding document: {document.Name}");
                throw;
            }
        }

        public async Task AddDocumentsAsync(List<QHDocument> documents)
        {
            try
            {
                await _context.Documents.AddRangeAsync(documents);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added {documents.Count} documents in batch");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding documents batch");
                throw;
            }
        }

        public async Task<int> GetNextFolderOrderAsync(int workspaceId, int? parentFolderId)
        {
            var maxOrder = await _context.Folders
                .Where(f => f.WorkspaceId == workspaceId && f.ParentFolderId == parentFolderId)
                .MaxAsync(f => (int?)f.FolderOrder) ?? 0;
            return maxOrder + 1;
        }

        public async Task<int> GetNextDisplayOrderAsync(int workspaceId, int? parentFolderId)
        {
            var maxOrder = await _context.Folders
                .Where(f => f.WorkspaceId == workspaceId && f.ParentFolderId == parentFolderId)
                .MaxAsync(f => (int?)f.DisplayOrder) ?? 0;
            return maxOrder + 1;
        }

        public async Task<int> GetNextDocumentDisplayOrderAsync(int folderId)
        {
            var maxOrder = await _context.Documents
                .Where(d => d.FolderId == folderId && d.IsDeleted == false)
                .MaxAsync(d => (int?)d.DisplayOrder) ?? 0;
            return maxOrder + 1;
        }

        public async Task<int> GetHierarchyLevelAsync(int parentFolderId)
        {
            var parentFolder = await _context.Folders.FirstOrDefaultAsync(f=>f.FolderId== parentFolderId && f.IsDeleted == false);
            return parentFolder?.HierarchyLevel ?? 0;
        }

        public async Task<string> BuildHierarchyPathAsync(int folderId)
        {
            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.FolderId == folderId);

            if (folder == null)
                return string.Empty;

            var pathParts = new List<string> { folder.FolderId.ToString() };
            var currentFolderId = folder.ParentFolderId;

            while (currentFolderId.HasValue)
            {
                var currentFolder = await _context.Folders.AsNoTracking()
                    .Select(f => new
                    {
                        f.FolderId,
                        f.ParentFolderId
                    })
                    .FirstOrDefaultAsync(f => f.FolderId == currentFolderId.Value);

                if (currentFolder != null)
                {
                    pathParts.Insert(0, currentFolder.FolderId.ToString());
                    currentFolderId = currentFolder.ParentFolderId;
                }
                else
                {
                    break;
                }
            }

            return string.Join("/", pathParts);
        }

        public async Task UpdateFolderHierarchyPathAsync(int folderId, string hierarchyPath)
        {
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder != null)
            {
                folder.HierarchyPath = hierarchyPath;
                await _context.SaveChangesAsync();
            }
        }
    }
}
