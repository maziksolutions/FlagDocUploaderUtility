using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Models
{
    public class ProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RootFolderId { get; set; }
        public int TotalFolders { get; set; }
        public int TotalFiles { get; set; }
        public int ProcessedFolders { get; set; }
        public int ProcessedFiles { get; set; }
        public TimeSpan Duration { get; set; }
        public Exception? Error { get; set; }
    }

    public class ProcessingProgress
    {
        public string CurrentOperation { get; set; } = string.Empty;
        public int TotalFolders { get; set; }
        public int TotalFiles { get; set; }
        public int ProcessedFolders { get; set; }
        public int ProcessedFiles { get; set; }
        public int ProgressPercentage { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
    }
}
