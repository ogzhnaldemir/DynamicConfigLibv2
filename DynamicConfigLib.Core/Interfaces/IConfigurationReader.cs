using System;
using System.Collections.Generic;
using DynamicConfigLib.Core.Models;

namespace DynamicConfigLib.Core.Interfaces
{
    public interface IConfigurationReader : IDisposable
    {
        /// <summary>
        /// Gets a configuration value by key
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <param name="key">The key of the configuration</param>
     
        T GetValue<T>(string key);

        
        IEnumerable<ConfigEntry> GetAllConfigs();
        
    
        Task RefreshConfigsAsync();
        
        /// <summary>
        /// Gets the application name associated with this configuration reader
        /// </summary>
        string GetApplicationName();
    }
} 