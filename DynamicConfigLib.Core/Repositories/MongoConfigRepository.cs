using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicConfigLib.Core.Repositories
{
    public class MongoConfigRepository : IConfigRepository
    {
        private readonly IMongoCollection<ConfigEntry> _configs;
        private readonly ILogger<MongoConfigRepository> _logger;

        public MongoConfigRepository(string connectionString, string databaseName, ILogger<MongoConfigRepository> logger = null)
        {
            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                _configs = database.GetCollection<ConfigEntry>("ConfigEntries");
                _logger = logger;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize MongoDB connection");
                throw;
            }
        }

        public async Task<IEnumerable<ConfigEntry>> GetConfigsForApplicationAsync(string applicationName)
        {
            try
            {
                var filter = Builders<ConfigEntry>.Filter.Eq(c => c.ApplicationName, applicationName) &
                             Builders<ConfigEntry>.Filter.Eq(c => c.IsActive, true);
                return await _configs.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting configs for application {ApplicationName}", applicationName);
                return new List<ConfigEntry>();
            }
        }

        public async Task<ConfigEntry> GetConfigAsync(string configName, string applicationName)
        {
            try
            {
                var filter = Builders<ConfigEntry>.Filter.Eq(c => c.Name, configName) &
                             Builders<ConfigEntry>.Filter.Eq(c => c.ApplicationName, applicationName) &
                             Builders<ConfigEntry>.Filter.Eq(c => c.IsActive, true);
                return await _configs.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting config {ConfigName} for application {ApplicationName}", configName, applicationName);
                return null;
            }
        }

        public async Task<ConfigEntry> AddConfigAsync(ConfigEntry configEntry)
        {
            try
            {
                configEntry.LastUpdated = DateTime.UtcNow;
                await _configs.InsertOneAsync(configEntry);
                return configEntry;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding config {ConfigName}", configEntry.Name);
                return null;
            }
        }

        public async Task<bool> UpdateConfigAsync(ConfigEntry configEntry)
        {
            try
            {
                configEntry.LastUpdated = DateTime.UtcNow;
                var filter = Builders<ConfigEntry>.Filter.Eq(c => c.Id, configEntry.Id);
                var result = await _configs.ReplaceOneAsync(filter, configEntry);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating config {ConfigName}", configEntry.Name);
                return false;
            }
        }

        public async Task<bool> DeleteConfigAsync(string configId)
        {
            try
            {
                var filter = Builders<ConfigEntry>.Filter.Eq(c => c.Id, configId);
                var result = await _configs.DeleteOneAsync(filter);
                return result.IsAcknowledged && result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting config with ID {ConfigId}", configId);
                return false;
            }
        }

        public async Task<IEnumerable<ConfigEntry>> GetUpdatedConfigsAsync(string applicationName, DateTime since)
        {
            try
            {
                var filter = Builders<ConfigEntry>.Filter.Eq(c => c.ApplicationName, applicationName) &
                             Builders<ConfigEntry>.Filter.Gt(c => c.LastUpdated, since) &
                             Builders<ConfigEntry>.Filter.Eq(c => c.IsActive, true);
                return await _configs.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting updated configs for application {ApplicationName} since {Since}", applicationName, since);
                return new List<ConfigEntry>();
            }
        }

        public async Task<IEnumerable<ConfigEntry>> GetAllConfigsNoFilterAsync()
        {
            try
            {
                return await _configs.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all configs without filter");
                return new List<ConfigEntry>();
            }
        }
    }
} 