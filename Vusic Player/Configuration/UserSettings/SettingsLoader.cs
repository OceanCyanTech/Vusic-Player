using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.UserSettings
{
    public static class SettingsLoader
    {
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        private static readonly string _folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VusicPlayer"); 

        private static readonly string _filePath = Path.Combine(_folderPath, "settings.json");

        public static async Task SaveSettingsAsync(SettingsValues settings)
        {
            if (settings == null)
                return;

            await _fileLock.WaitAsync();
            try
            {
                if (!Directory.Exists(_folderPath))
                    Directory.CreateDirectory(_folderPath);

                string json = JsonSerializer.Serialize(settings);
                if (string.IsNullOrEmpty(json) || json == "{}")
                    return;

                string tempPath = _filePath + ".tmp";
                await File.WriteAllTextAsync(tempPath, json, System.Text.Encoding.UTF8);
                File.Move(tempPath, _filePath, overwrite: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }
        public static async Task<SettingsValues> LoadSettingsAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new SettingsValues();
                }

                string json = await File.ReadAllTextAsync(_filePath);
                var settings = JsonSerializer.Deserialize<SettingsValues>(json);

                return settings ?? new SettingsValues();
            }
            catch
            {
                return new SettingsValues();
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }

}
