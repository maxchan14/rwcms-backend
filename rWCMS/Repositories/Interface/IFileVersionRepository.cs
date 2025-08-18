using rWCMS.Models;
using System.Threading.Tasks;

namespace rWCMS.Repositories.Interface
{
    public interface IFileVersionRepository
    {
        Task<FileVersion> GetFileVersionAsync(int fileVersionId);
        Task AddFileVersionAsync(FileVersion fileVersion);
    }
}