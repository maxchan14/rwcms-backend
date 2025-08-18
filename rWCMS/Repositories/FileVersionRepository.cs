using Microsoft.EntityFrameworkCore;
using rWCMS.Data;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using System.Threading.Tasks;

namespace rWCMS.Repositories
{
    public class FileVersionRepository : IFileVersionRepository
    {
        private readonly rWCMSDbContext _context;

        public FileVersionRepository(rWCMSDbContext context)
        {
            _context = context;
        }

        public async Task<FileVersion> GetFileVersionAsync(int fileVersionId)
        {
            return await _context.FileVersions.FindAsync(fileVersionId);
        }

        public async Task AddFileVersionAsync(FileVersion fileVersion)
        {
            await _context.FileVersions.AddAsync(fileVersion);
            await _context.SaveChangesAsync();
        }
    }
}