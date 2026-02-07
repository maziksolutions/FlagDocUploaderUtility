using FlagDocUploader.Data;
using FlagDocUploader.Data.Entity;
using FlagDocUploader.Helpers;
using FlagDocUploader.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Services
{
    public class ZipProcessingService : IZipProcessingService
    {
        private readonly IDocumentService _service;
        private readonly ILogger<ZipProcessingService> _logger;
        private readonly IFolderService _folderService;
        private const int BATCH_SIZE = 10;
        private readonly AppDbContext _context;
        public ZipProcessingService(IDocumentService service, AppDbContext context,ILogger<ZipProcessingService> logger, IFolderService folderService)
        {
            _service = service;
            _context = context;
            _logger = logger;
            _folderService = folderService;
        }

        public async Task<ProcessingResult> ProcessZipFileAsync(
            string zipFilePath,
            int workspaceId,
            int categoryId,
            string categoryName,
            int? subCategoryId,
            string subCategoryName,
            int userId,
            int? parentFolderId,
            IProgress<ProcessingProgress> progress,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ProcessingResult();
            
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogInformation($"Starting ZIP processing: {zipFilePath}");
                progress?.Report(new ProcessingProgress
                {
                    CurrentOperation = "Opening ZIP file...",
                    ProgressPercentage = 0
                });

                using (var fileStream = File.OpenRead(zipFilePath))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    var allEntries = archive.Entries.ToList();

                    // Count folders and files
                    var folderEntries = allEntries.Where(e => e.FullName.EndsWith("/")).ToList();
                    var fileEntries = allEntries.Where(e => !e.FullName.EndsWith("/") && e.Length > 0).ToList();

                    result.TotalFolders = folderEntries.Count + 1; // +1 for root
                    result.TotalFiles = fileEntries.Count;

                    progress?.Report(new ProcessingProgress
                    {
                        CurrentOperation = $"Found {result.TotalFolders} folders and {result.TotalFiles} files",
                        TotalFolders = result.TotalFolders,
                        TotalFiles = result.TotalFiles,
                        ProgressPercentage = 5
                    });

                    cancellationToken.ThrowIfCancellationRequested();

                    // Get root folder name
                    var rootEntry = allEntries.FirstOrDefault();
                    if (rootEntry == null)
                    {
                        throw new Exception("ZIP file is empty");
                    }

                    var rootFolderName = rootEntry.FullName.Split('/')[0];
                    var existByName=await _folderService.GetFolderByNameAsync(rootFolderName,categoryId,subCategoryId, parentFolderId);
                    if(existByName is not null)
                    {                        
                        var archived = await _service.ArchiveDocumentByFolderId(existByName.FolderId, userId, categoryId, subCategoryId);
                    }
                    // Create root folder
                    progress?.Report(new ProcessingProgress
                    {
                        CurrentOperation = $"Creating root folder: {rootFolderName}",
                        TotalFolders = result.TotalFolders,
                        TotalFiles = result.TotalFiles,
                        ProgressPercentage = 10
                    });

                    var hierarchyLevel = parentFolderId.HasValue
                        ? 2
                        : 0;

                    var rootFolder = new QHFolder
                    {
                        WorkspaceId = workspaceId,
                        ParentFolderId = parentFolderId,
                        Name = rootFolderName,
                        CategoryId = categoryId,
                        CategoryName = categoryName,
                        SubCategoryId = subCategoryId,
                        SubCategoryName = subCategoryName,
                        HierarchyLevel = hierarchyLevel,
                        Status = "Draft",
                        FolderOrder = await _service.GetNextFolderOrderAsync(workspaceId, parentFolderId),
                        DisplayOrder = await _service.GetNextDisplayOrderAsync(workspaceId, parentFolderId),
                        CreatedBy = userId,
                        OwnerUserId = userId,
                        RecDate = DateTime.UtcNow
                    };

                    rootFolder = await _service.AddFolderAsync(rootFolder);
                    var hierarchyPath = await _service.BuildHierarchyPathAsync(rootFolder.FolderId);
                    await _service.UpdateFolderHierarchyPathAsync(rootFolder.FolderId, hierarchyPath);

                    result.RootFolderId = rootFolder.FolderId;
                    result.ProcessedFolders = 1;

                    var folderMapping = new Dictionary<string, int>
                    {
                        { rootFolderName, rootFolder.FolderId }
                    };

                    cancellationToken.ThrowIfCancellationRequested();

                    // Process all folder entries
                    progress?.Report(new ProcessingProgress
                    {
                        CurrentOperation = "Creating folder structure...",
                        TotalFolders = result.TotalFolders,
                        TotalFiles = result.TotalFiles,
                        ProcessedFolders = result.ProcessedFolders,
                        ProgressPercentage = 20
                    });

                    foreach (var entry in allEntries.OrderBy(e => e.FullName))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var fullPath = entry.FullName.TrimEnd('/');
                        if (string.IsNullOrEmpty(fullPath)) continue;

                        var pathParts = fullPath.Split('/');
                        
                        for (int i = 1; i < pathParts.Length; i++)
                        {
                            var currentPath = string.Join("/", pathParts.Take(i + 1));
                            var parentPath = string.Join("/", pathParts.Take(i));
                            bool isFolder = entry.FullName.EndsWith("/") || i < pathParts.Length - 1;
                            if (isFolder && !folderMapping.ContainsKey(currentPath))
                            {
                                if (!folderMapping.TryGetValue(parentPath, out int parentFolderIdForCurrent))
                                {
                                    _logger.LogWarning($"Parent folder not found for path: {currentPath}, parent: {parentPath}");
                                    continue;
                                }                              

                                var newFolder = new QHFolder
                                {
                                    WorkspaceId = workspaceId,
                                    ParentFolderId = parentFolderIdForCurrent,
                                    Name = pathParts[i],
                                    CategoryId = categoryId,
                                    CategoryName = categoryName,
                                    SubCategoryId = subCategoryId,
                                    SubCategoryName = subCategoryName,
                                    HierarchyLevel = hierarchyLevel + i,
                                    Status = "Draft",
                                    FolderOrder = await _service.GetNextFolderOrderAsync(workspaceId, parentFolderIdForCurrent),
                                    DisplayOrder = await _service.GetNextDisplayOrderAsync(workspaceId, parentFolderIdForCurrent),
                                    CreatedBy = userId,
                                    OwnerUserId = userId,
                                    RecDate = DateTime.UtcNow
                                };

                                newFolder = await _service.AddFolderAsync(newFolder);
                                var folderHierarchyPath = await _service.BuildHierarchyPathAsync(newFolder.FolderId);
                                await _service.UpdateFolderHierarchyPathAsync(newFolder.FolderId, folderHierarchyPath);

                                folderMapping[currentPath] = newFolder.FolderId;
                                result.ProcessedFolders++;
                                _logger.LogInformation($"Created folder: {currentPath} with ID: {newFolder.FolderId}");
                                progress?.Report(new ProcessingProgress
                                {
                                    CurrentOperation = $"Creating folder: {pathParts[i]}",
                                    TotalFolders = result.TotalFolders,
                                    TotalFiles = result.TotalFiles,
                                    ProcessedFolders = result.ProcessedFolders,
                                    ProgressPercentage = 20 + (int)((result.ProcessedFolders / (double)result.TotalFolders) * 30)
                                });
                            }
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // Process files in batches
                    progress?.Report(new ProcessingProgress
                    {
                        CurrentOperation = "Processing files...",
                        TotalFolders = result.TotalFolders,
                        TotalFiles = result.TotalFiles,
                        ProcessedFolders = result.ProcessedFolders,
                        ProgressPercentage = 50
                    });

                    for (int i = 0; i < fileEntries.Count; i += BATCH_SIZE)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var batch = fileEntries.Skip(i).Take(BATCH_SIZE).ToList();
                        var documents = new List<QHDocument>();

                        foreach (var entry in batch)
                        {
                            var fullPath = entry.FullName.TrimEnd('/');
                            var pathParts = fullPath.Split('/');
                            var fileName = pathParts[pathParts.Length - 1];
                            var folderPath = string.Join("/", pathParts.Take(pathParts.Length - 1));

                            if (folderMapping.TryGetValue(folderPath, out int targetFolderId))
                            {
                                progress?.Report(new ProcessingProgress
                                {
                                    CurrentOperation = $"Processing file: {fileName}",
                                    TotalFolders = result.TotalFolders,
                                    TotalFiles = result.TotalFiles,
                                    ProcessedFolders = result.ProcessedFolders,
                                    ProcessedFiles = result.ProcessedFiles,
                                    CurrentFile = fileName,
                                    ProgressPercentage = 50 + (int)((result.ProcessedFiles / (double)result.TotalFiles) * 45)
                                });

                                byte[] fileBytes;
                                using (var entryStream = entry.Open())
                                using (var ms = new MemoryStream())
                                {
                                    await entryStream.CopyToAsync(ms, cancellationToken);
                                    fileBytes = ms.ToArray();
                                }

                                var extension = Path.GetExtension(fileName).ToLower();
                                var mimeType = MimeTypeHelper.GetMimeType(extension);

                                var document = new QHDocument
                                {
                                    FolderId = targetFolderId,
                                    Name = Path.GetFileNameWithoutExtension(fileName),
                                    FileName = fileName,
                                    FileSize = (int)entry.Length,
                                    MimeType = mimeType,
                                    CVFileBytes = fileBytes,
                                    DocumentType = "file",
                                    CategoryId = categoryId,
                                    CategoryName = categoryName,
                                    SubCategoryId = subCategoryId,
                                    SubCategoryName = subCategoryName,
                                    Status = "Published",
                                    DisplayOrder = await _service.GetNextDocumentDisplayOrderAsync(targetFolderId),
                                    CreatedBy = userId,
                                    OwnerUserId = userId,
                                    RecDate = DateTime.UtcNow,
                                    PublishedDate = DateTime.UtcNow,
                                    PublishedBy = userId
                                };

                                documents.Add(document);
                                result.ProcessedFiles++;
                            }
                        }

                        if (documents.Any())
                        {
                            await _service.AddDocumentsAsync(documents);
                        }

                        progress?.Report(new ProcessingProgress
                        {
                            CurrentOperation = $"Processed {result.ProcessedFiles} of {result.TotalFiles} files",
                            TotalFolders = result.TotalFolders,
                            TotalFiles = result.TotalFiles,
                            ProcessedFolders = result.ProcessedFolders,
                            ProcessedFiles = result.ProcessedFiles,
                            ProgressPercentage = 50 + (int)((result.ProcessedFiles / (double)result.TotalFiles) * 45)
                        });
                    }
                }
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Transaction committed successfully");
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                result.Success = true;
                result.Message = $"Successfully processed {result.ProcessedFolders} folders and {result.ProcessedFiles} files in {result.Duration.TotalSeconds:F2} seconds";

                progress?.Report(new ProcessingProgress
                {
                    CurrentOperation = "Complete!",
                    TotalFolders = result.TotalFolders,
                    TotalFiles = result.TotalFiles,
                    ProcessedFolders = result.ProcessedFolders,
                    ProcessedFiles = result.ProcessedFiles,
                    ProgressPercentage = 100
                });

                _logger.LogInformation(result.Message);
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(cancellationToken);
                result.Success = false;
                result.Message = "Operation was cancelled by user";
                _logger.LogWarning(result.Message);
            }
catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                result.Success = false;
                result.Error = ex;
                result.Message = $"Error processing ZIP file: {ex.Message}";
                _logger.LogError(ex, result.Message);
            }
            return result;
        }
    }
}
