using FlagDocUploader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Services
{
    public interface IZipProcessingService
    {
        Task<ProcessingResult> ProcessZipFileAsync(
            string zipFilePath,
            int workspaceId,
            int categoryId,
            string categoryName,
            int? subCategoryId,
            string subCategoryName,
            int userId,
            int? parentFolderId,
            IProgress<ProcessingProgress> progress,
            CancellationToken cancellationToken);
    }
}
