using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DynamicConfigLib.Core
{
    public class ConfigService : IConfigService, IDisposable
    {
        private readonly IConfigRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConfigService> _logger;
        private readonly System.Timers.Timer _refreshTimer;
        private readonly TimeSpan _refreshInterval;
        private readonly string _cacheKey = "ConfigEntries";
        private string _applicationName;
        private DateTime _lastRefreshed = DateTime.MinValue;
        private bool _disposed = false;

        public ConfigService(
            IConfigRepository repository,
            IMemoryCache cache,
            ILogger<ConfigService> logger = null,
            string applicationName = null,
            TimeSpan? refreshInterval = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
            _applicationName = applicationName;
            _refreshInterval = refreshInterval ?? TimeSpan.FromMinutes(5);

            //  timer 
            _refreshTimer = new System.Timers.Timer(_refreshInterval.TotalMilliseconds);
            _refreshTimer.Elapsed += async (sender, e) => await RefreshConfigsTimerCallback();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();

        
            Task.Run(async () => await RefreshConfigsAsync()).Wait();
        }

        public T GetConfig<T>(string configName, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(_applicationName))
            {
                _logger?.LogWarning("Application name not set. Unable to retrieve configuration.");
                return defaultValue;
            }

            if (string.IsNullOrEmpty(configName))
            {
                _logger?.LogWarning("Config name is null or empty");
                return defaultValue;
            }

            try
            {
                var configs = GetAllConfigs();
                var config = configs?.FirstOrDefault(c => c.Name.Equals(configName, StringComparison.OrdinalIgnoreCase));
                
                if (config == null)
                {
                    _logger?.LogDebug("Config {ConfigName} not found for application {ApplicationName}, using default value", configName, _applicationName);
                    return defaultValue;
                }

                if (!config.IsActive)
                {
                    _logger?.LogDebug("Config {ConfigName} is not active for application {ApplicationName}, using default value", configName, _applicationName);
                    return defaultValue;
                }

                return config.GetTypedValue<T>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting config {ConfigName} for application {ApplicationName}", configName, _applicationName);
                return defaultValue;
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
                
                // bütün aktif configleri al
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

                // Sadece son yenilemeden bu yana güncellenen config al
                var updatedConfigs = await _repository.GetUpdatedConfigsAsync(_applicationName, _lastRefreshed);
                
                if (updatedConfigs.Any())
                {
                    _logger?.LogInformation("Found {Count} updated configs for application {ApplicationName} since {LastRefresh}", 
                        updatedConfigs.Count(), _applicationName, _lastRefreshed);

                    // Tüm comfiglere sahip olduğumuzdan emin olmak için tam yenileme
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

        public void SetApplicationName(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentException("Application name cannot be null or empty", nameof(applicationName));

            if (_applicationName != applicationName)
            {
                _applicationName = applicationName;
                _logger?.LogInformation("Application name set to {ApplicationName}", _applicationName);
                // yeni uygulama için yenileme
                Task.Run(async () => await RefreshConfigsAsync()).Wait();
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