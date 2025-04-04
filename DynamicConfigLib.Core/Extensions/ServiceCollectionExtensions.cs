using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace DynamicConfigLib.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
      
        /// <param name="services">The service collection</param>
        /// <param name="mongoConnectionString">MongoDB connection string</param>
        /// <param name="mongoDatabaseName">MongoDB database name</param>
        /// <param name="applicationName">Application name for configurations</param>
        /// <param name="refreshInterval">Refresh interval for configuration cache</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDynamicConfig(
            this IServiceCollection services,
            string mongoConnectionString,
            string mongoDatabaseName,
            string applicationName,
            TimeSpan? refreshInterval = null)
        {
            if (string.IsNullOrEmpty(mongoConnectionString))
                throw new ArgumentException("MongoDB connection string cannot be null or empty", nameof(mongoConnectionString));

            if (string.IsNullOrEmpty(mongoDatabaseName))
                throw new ArgumentException("MongoDB database name cannot be null or empty", nameof(mongoDatabaseName));

            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentException("Application name cannot be null or empty", nameof(applicationName));

            // Add memory cache if not already registered
            services.AddMemoryCache();

            // Register repository
            services.AddSingleton<IConfigRepository>(sp =>
            {
                var logger = sp.GetService<ILogger<MongoConfigRepository>>();
                return new MongoConfigRepository(mongoConnectionString, mongoDatabaseName, logger);
            });

            // Register config service as singleton
            services.AddSingleton<IConfigService>(sp =>
            {
                var repository = sp.GetRequiredService<IConfigRepository>();
                var cache = sp.GetRequiredService<IMemoryCache>();
                var logger = sp.GetService<ILogger<ConfigService>>();
                return new ConfigService(repository, cache, logger, applicationName, refreshInterval);
            });

            return services;
        }
    }
} 