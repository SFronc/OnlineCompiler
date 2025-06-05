using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Models;

namespace OnlineCompiler.Data
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options)
            : base(options)
        {
        }

        public DbSet<OnlineCompiler.Models.User> User { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.CompilationResult> CompilationResult { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.FileModel> FileModel { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.Library> Librarie { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.LibraryAccess> LibraryAccesse { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.Project> Project { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.ProjectLibrary> ProjectLibrarie { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.UserFile> UserFile { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.PublicFiles> PublicFiles { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.ImportFile> ImportFile { get; set; } = default!;
        public DbSet<OnlineCompiler.Models.ProjectCollaborator> ProjectCollaborators { get; set; } = default!;
    }
}
