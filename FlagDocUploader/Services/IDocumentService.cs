using FlagDocUploader.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Services
{
   public interface IDocumentService
    {
        Task<bool> TestConnectionAsync();
        Task<QHFolder> AddFolderAsync(QHFolder folder);
        Task<QHDocument> AddDocumentAsync(QHDocument document);
        Task AddDocumentsAsync(List<QHDocument> documents);
        Task<int> GetNextFolderOrderAsync(int workspaceId, int? parentFolderId);
        Task<int> GetNextDisplayOrderAsync(int workspaceId, int? parentFolderId);
        Task<int> GetNextDocumentDisplayOrderAsync(int folderId);
        Task<int> GetHierarchyLevelAsync(int parentFolderId);
        Task<string> BuildHierarchyPathAsync(int folderId);
        Task UpdateFolderHierarchyPathAsync(int folderId, string hierarchyPath);
    }
}
