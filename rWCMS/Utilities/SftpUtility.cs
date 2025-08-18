using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Threading.Tasks;

namespace rWCMS.Utilities
{
    public class SftpUtility
    {
        private readonly IConfiguration _configuration;

        public SftpUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SftpClient CreateSftpClient(string environment)
        {
            var sftpSettings = _configuration.GetSection($"SftpSettings{(environment == "Development" ? ":Development" : "")}");
            var host = sftpSettings["Host"];
            var port = int.Parse(sftpSettings["Port"]);
            var username = sftpSettings["Username"];
            var password = sftpSettings["Password"];
            return new SftpClient(host, port, username, password);
        }

        public async Task DeleteFileAsync(string path, string environment)
        {
            using var client = CreateSftpClient(environment);
            await Task.Run(() =>
            {
                client.Connect();
                if (client.Exists(path))
                {
                    client.DeleteFile(path);
                }
                client.Disconnect();
            });
        }

        public async Task DeleteDirectoryAsync(string path, string environment)
        {
            using var client = CreateSftpClient(environment);
            await Task.Run(() =>
            {
                client.Connect();
                if (client.Exists(path))
                {
                    client.DeleteDirectory(path);
                }
                client.Disconnect();
            });
        }

        public async Task RenameFileAsync(string oldPath, string newPath, string environment)
        {
            using var client = CreateSftpClient(environment);
            await Task.Run(() =>
            {
                client.Connect();
                if (client.Exists(oldPath))
                {
                    client.RenameFile(oldPath, newPath);
                }
                client.Disconnect();
            });
        }

        public async Task RenameDirectoryAsync(string oldPath, string newPath, string environment)
        {
            using var client = CreateSftpClient(environment);
            await Task.Run(() =>
            {
                client.Connect();
                if (client.Exists(oldPath))
                {
                    client.RenameFile(oldPath, newPath);
                }
                client.Disconnect();
            });
        }
    }
}