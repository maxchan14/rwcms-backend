using rWCMS.Models;
using System;
using System.Threading.Tasks;

namespace rWCMS.Repositories.Interface
{
    public interface ISystemSettingRepository
    {
        Task<SystemSetting> GetSettingAsync(int key);
        Task AddSettingAsync(SystemSetting setting);
        Task UpdateSettingAsync(SystemSetting setting);
        Task DeleteSettingAsync(int key);
        Task<DateTime?> GetLatestSystemPermissionUpdateAsync();
    }
}