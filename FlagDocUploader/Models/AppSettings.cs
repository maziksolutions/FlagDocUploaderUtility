using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Models
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public ProcessingSettings Processing { get; set; } = new();
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }

    public class ProcessingSettings
    {
        public int BatchSize { get; set; } = 10;
        public int MaxConcurrentFiles { get; set; } = 5;
    }
}
