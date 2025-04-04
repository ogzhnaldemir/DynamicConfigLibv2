using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Models;
using DynamicConfigLib.Core.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicConfigLib.Core
{
    public class ConfigurationReader : IConfigurationReader
    {
        private readonly IConfigRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConfigurationReader> _logger;
        private readonly System.Timers.Timer _refreshTimer;
        private readonly string _cacheKey = "ConfigEntries";
        private string _applicationName;
        private DateTime _lastRefreshed = DateTime.MinValue;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the ConfigurationReader
        /// </summary>
        /// <param name="applicationName">The name of the application using this configuration</param>
        /// <param name="connectionString">MongoDB connection string</param>
        /// <param name="refreshTimerIntervalInMs">Interval in milliseconds to check for configuration updates</param>
        public ConfigurationReader(string applicationName, string connectionString, int refreshTimerIntervalInMs = 300000)
        {
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentException("Application name cannot be null or empty", nameof(applicationName));

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            if (refreshTimerIntervalInMs <= 0)
                throw new ArgumentException("Refresh interval must be greater than zero", nameof(refreshTimerIntervalInMs));

            _applicationName = applicationName;
            
            _cache = new MemoryCache(new MemoryCacheOptions());
            
            
      _repository = new MongoConfigRepository(connectionString, "DynamicConfigDb");
            
           
            _refreshTimer = new System.Timers.Timer(refreshTimerIntervalInMs);
            _refreshTimer.Elapsed += async (sender, e) => await RefreshConfigsTimerCallback();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();

            
            Task.Run(async () => await RefreshConfigsAsync()).Wait();
        }

        /// <summary>
        /// Gets a configuration value by key
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <param name="key">The key of the configuration</param>
        /// <returns>The configuration value, or default if not found</returns>
        public T GetValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger?.LogWarning("Config key is null or empty");
                return default;
            }

            try
            {
                var configs = GetAllConfigs();
                var config = configs?.FirstOrDefault(c => c.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                
                if (config == null)
                {
                    _logger?.LogDebug("Config {ConfigName} not found for application {ApplicationName}, using default value", key, _applicationName);
                    return default;
                }

                if (!config.IsActive)
                {
                    _logger?.LogDebug("Config {ConfigName} is not active for application {ApplicationName}, using default value", key, _applicationName);
                    return default;
                }

                return config.GetTypedValue<T>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting config {ConfigName} for application {ApplicationName}", key, _applicationName);
                return default;
            }
        }

        public IEnumerable<ConfigEntry> GetAllConfigs()
        {
            if (string.IsNullOrEmpty(_applicationName))
            {
                _logger?.LogWarning("Application name not set. Unable to retrieve configurations.");
                return new List<ConfigEntry>();
            }

            if (!_cache.TryGetValue(_cacheKey, out List<ConfigEntry> cachedConfigs))
            {
                _logger?.LogDebug("Configs not found in cache for application {ApplicationName}, refreshing", _applicationName);
                Task.Run(async () => await RefreshConfigsAsync()).Wait();
                _cache.TryGetValue(_cacheKey, out cachedConfigs);
            }

            return cachedConfigs ?? new List<ConfigEntry>();
        }

        public async Task RefreshConfigsAsync()
        {
            if (string.IsNullOrEmpty(_applicationName))
            {
                _logger?.LogWarning("Application name not set. Unable to refresh configurations.");
                return;
            }

            try
            {
                _logger?.LogDebug("Refreshing configs for application {ApplicationName}", _applicationName);
                
                // aktif configlerı al
                var allConfigs = await _repository.GetConfigsForApplicationAsync(_applicationName);
                
                // configleri cachele
                _cache.Set(_cacheKey, allConfigs.ToList(), TimeSpan.FromDays(1));
                
                _lastRefreshed = DateTime.UtcNow;
                _logger?.LogDebug("Successfully refreshed {Count} configs for application {ApplicationName}", allConfigs.Count(), _applicationName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error refreshing configs for application {ApplicationName}", _applicationName);
                
            }
        }

        private async Task RefreshConfigsTimerCallback()
        {
            if (string.IsNullOrEmpty(_applicationName))
                return;

            try
            {
                _logger?.LogDebug("Timer triggered refresh for application {ApplicationName}", _applicationName);
                
                // sadece güncellenmiş configleri al
                var updatedConfigs = await _repository.GetUpdatedConfigsAsync(_applicationName, _lastRefreshed);
                
                if (updatedConfigs.Any())
                {
                    _logger?.LogInformation("Found {Count} updated configs for application {ApplicationName} since {LastRefresh}", 
                        updatedConfigs.Count(), _applicationName, _lastRefreshed);
                    
                    // refresh 
                    await RefreshConfigsAsync();
                }
                else
                {
                    _logger?.LogDebug("No updated configs found for application {ApplicationName} since {LastRefresh}", 
                        _applicationName, _lastRefreshed);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in refresh timer callback for application {ApplicationName}", _applicationName);
                
            }
        }
        
        public string GetApplicationName()
        {
            return _applicationName;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _refreshTimer?.Stop();
                    _refreshTimer?.Dispose();
                }

                _disposed = true;
            }
        }
    }
} 