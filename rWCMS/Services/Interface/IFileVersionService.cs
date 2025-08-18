using rWCMS.DTOs;
using rWCMS.Models;
using System.Threading.Tasks;

namespace rWCMS.Services.Interface
{
    public interface IFileVersionService
    {
        Task<FileVersion> GetFileVersionAsync(int fileVersionId);
        Task AddFileVersionAsync(AddFileVersionRequest request);
    }
}