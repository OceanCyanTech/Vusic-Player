using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vortice.XAudio2;

namespace Vusic_Player.Configuration.UserSettings
{
    public static class SettingsLoader
    {
        // This lock ensures only one part of the app writes/reads the file at a time
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        private static readonly string _folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VusicPlayer"); // Use your actual App Name here

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

                // Safety check: Don't overwrite good data with empty data
                if (string.IsNullOrEmpty(json) || json == "{}")
                    return;

                string tempPath = _filePath + ".tmp";

                // Use a Stream with a buffer for better performance on larger settings files
                await File.WriteAllTextAsync(tempPath, json, System.Text.Encoding.UTF8);

                // Overwrite the old file with the new one
                File.Move(tempPath, _filePath, overwrite: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
                // Consider if you want to re-throw or just log
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

                // If deserialization fails, return a fresh object but don't overwrite yet
                return settings ?? new SettingsValues();
            }
            catch
            {
                //Logger.Log($"Error loading settings: {ex.Message}", "SettingsHelper", Logger.LogLevelType.Error);
                return new SettingsValues();
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }

}
