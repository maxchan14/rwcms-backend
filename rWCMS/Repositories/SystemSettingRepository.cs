using Microsoft.EntityFrameworkCore;
using rWCMS.Data;
using rWCMS.Models;
using rWCMS.Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace rWCMS.Repositories
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly rWCMSDbContext _context;

        public SystemSettingRepository(rWCMSDbContext context)
        {
            _context = context;
        }

        public async Task<SystemSetting> GetSettingAsync(int key)
        {
            return await _context.SystemSettings.FindAsync(key);
        }

        public async Task AddSettingAsync(SystemSetting setting)
        {
            await _context.SystemSettings.AddAsync(setting);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSettingAsync(SystemSetting setting)
        {
            _context.SystemSettings.Update(setting);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSettingAsync(int key)
        {
            var setting = await GetSettingAsync(key);
            if (setting != null)
            {
                _context.SystemSettings.Remove(setting);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DateTime?> GetLatestSystemPermissionUpdateAsync()
        {
            var systemSetting = await _context.SystemSettings
                .OrderByDescending(s => s.SystemPermissionUpdate)
                .Select(s => new { s.SystemPermissionUpdate })
                .FirstOrDefaultAsync();
            return systemSetting?.SystemPermissionUpdate;
        }
    }
}