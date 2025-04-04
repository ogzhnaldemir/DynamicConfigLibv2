//using DynamicConfigLib.Core.Interfaces;
//using DynamicConfigLib.Core.Models;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace DynamicConfigLib.Core.Repositories
//{
//    public class FileSystemConfigRepository : IConfigRepository
//    {
//        private readonly string _configDirectoryPath;
//        private readonly string _mainConfigFile;
//        private readonly ILogger<FileSystemConfigRepository> _logger;
//        private readonly JsonSerializerOptions _jsonOptions;
        
//        /// <summary>
//        /// Initializes a new instance of the FileSystemConfigRepository
//        /// </summary>
//        /// <param name="configDirectoryPath">The directory where configuration files are stored</param>
//        /// <param name="logger">Optional logger instance</param>
//        public FileSystemConfigRepository(string configDirectoryPath, ILogger<FileSystemConfigRepository> logger = null)
//        {
//            if (string.IsNullOrEmpty(configDirectoryPath))
//                throw new ArgumentException("Configuration directory path cannot be null or empty", nameof(configDirectoryPath));
            
//            _configDirectoryPath = configDirectoryPath;
//            _mainConfigFile = Path.Combine(_configDirectoryPath, "configs.json");
//            _logger = logger;
//            _jsonOptions = new JsonSerializerOptions
//            {
//                WriteIndented = true
//            };
            
//            // Ensure directory exists
//            if (!Directory.Exists(_configDirectoryPath))
//            {
//                Directory.CreateDirectory(_configDirectoryPath);
//                _logger?.LogInformation("Created configuration directory at {DirectoryPath}", _configDirectoryPath);
//            }
            
//            // Initialize empty config file if it doesn't exist
//            if (!File.Exists(_mainConfigFile))
//            {
//                File.WriteAllText(_mainConfigFile, JsonSerializer.Serialize(new List<ConfigEntry>(), _jsonOptions));
//                _logger?.LogInformation("Initialized empty configuration file at {FilePath}", _mainConfigFile);
//            }
//        }
        
//        private async Task<List<ConfigEntry>> ReadAllConfigsAsync()
//        {
//            try
//            {
//                var content = await File.ReadAllTextAsync(_mainConfigFile);
//                var configs = JsonSerializer.Deserialize<List<ConfigEntry>>(content, _jsonOptions) ?? new List<ConfigEntry>();
//                return configs;
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error reading configurations from file {FilePath}", _mainConfigFile);
//                return new List<ConfigEntry>();
//            }
//        }
        
//        private async Task SaveAllConfigsAsync(List<ConfigEntry> configs)
//        {
//            try
//            {
//                var json = JsonSerializer.Serialize(configs, _jsonOptions);
//                await File.WriteAllTextAsync(_mainConfigFile, json);
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error saving configurations to file {FilePath}", _mainConfigFile);
//                throw;
//            }
//        }
        
//        public async Task<IEnumerable<ConfigEntry>> GetConfigsForApplicationAsync(string applicationName)
//        {
//            try
//            {
//                var configs = await ReadAllConfigsAsync();
//                return configs.Where(c => c.ApplicationName == applicationName && c.IsActive);
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error getting configs for application {ApplicationName}", applicationName);
//                return new List<ConfigEntry>();
//            }
//        }
        
//        public async Task<ConfigEntry> GetConfigAsync(string configName, string applicationName)
//        {
//            try
//            {
//                var configs = await ReadAllConfigsAsync();
//                return configs.FirstOrDefault(c => 
//                    c.Name.Equals(configName, StringComparison.OrdinalIgnoreCase) && 
//                    c.ApplicationName == applicationName && 
//                    c.IsActive);
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error getting config {ConfigName} for application {ApplicationName}", configName, applicationName);
//                return null;
//            }
//        }
        
//        public async Task<ConfigEntry> AddConfigAsync(ConfigEntry configEntry)
//        {
//            try
//            {
//                var configs = await ReadAllConfigsAsync();
                
//                // Generate an ID if not provided
//                if (string.IsNullOrEmpty(configEntry.Id))
//                {
//                    configEntry.Id = Guid.NewGuid().ToString();
//                }
                
//                configEntry.LastUpdated = DateTime.UtcNow;
//                configs.Add(configEntry);
                
//                await SaveAllConfigsAsync(configs);
//                return configEntry;
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error adding config {ConfigName}", configEntry.Name);
//                return null;
//            }
//        }
        
//        public async Task<bool> UpdateConfigAsync(ConfigEntry configEntry)
//        {
//            try
//            {
//                var configs = await ReadAllConfigsAsync();
//                var index = configs.FindIndex(c => c.Id == configEntry.Id);
                
//                if (index == -1)
//                {
//                    _logger?.LogWarning("Cannot update config {ConfigName}: not found", configEntry.Name);
//                    return false;
//                }
                
//                configEntry.LastUpdated = DateTime.UtcNow;
//                configs[index] = configEntry;
                
//                await SaveAllConfigsAsync(configs);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error updating config {ConfigName}", configEntry.Name);
//                return false;
//            }
//        }
        
//        public async Task<bool> DeleteConfigAsync(string configId)
//        {
//            try
//            {
//                var configs = await ReadAllConfigsAsync();
//                var index = configs.FindIndex(c => c.Id == configId);
                
//                if (index == -1)
//                {
//                    _logger?.LogWarning("Cannot delete config with ID {ConfigId}: not found", configId);
//                    return false;
//                }
                
//                configs.RemoveAt(index);
                
//                await SaveAllConfigsAsync(configs);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error deleting config with ID {ConfigId}", configId);
//                return false;
//            }
//        }
        
//        public async Task<IEnumerable<ConfigEntry>> GetUpdatedConfigsAsync(string applicationName, DateTime since)
//        {
//            try
//            {
//                var configs = await ReadAllConfigsAsync();
//                return configs.Where(c => 
//                    c.ApplicationName == applicationName && 
//                    c.LastUpdated > since && 
//                    c.IsActive);
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error getting updated configs for application {ApplicationName} since {Since}", applicationName, since);
//                return new List<ConfigEntry>();
//            }
//        }
        
//        public async Task<IEnumerable<ConfigEntry>> GetAllConfigsNoFilterAsync()
//        {
//            try
//            {
//                return await ReadAllConfigsAsync();
//            }
//            catch (Exception ex)
//            {
//                _logger?.LogError(ex, "Error getting all configs without filter");
//                return new List<ConfigEntry>();
//            }
//        }
//    }
//} 