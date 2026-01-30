using FlagDocUploader.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlagDocUploader.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<QHFolder> Folders { get; set; }
        public DbSet<QHDocument> Documents { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QHFolder>()
                .ToTable("tblFolders", "qhse");

            modelBuilder.Entity<QHDocument>()
                .ToTable("tblDocuments", "qhse");
        }
    }
}
