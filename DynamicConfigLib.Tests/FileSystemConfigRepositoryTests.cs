//using Xunit;
//using DynamicConfigLib.Core.Repositories;
//using DynamicConfigLib.Core.Models;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Moq;

//namespace DynamicConfigLib.Tests
//{
//    public class FileSystemConfigRepositoryTests : IDisposable
//    {
//        private readonly string _testConfigDir;
//        private readonly Mock<ILogger<FileSystemConfigRepository>> _mockLogger;
//        private readonly FileSystemConfigRepository _repository;

//        public FileSystemConfigRepositoryTests()
//        {
//            // Her test için benzersiz bir geçici klasör oluşturuyoruz
//            _testConfigDir = Path.Combine(Path.GetTempPath(), "DynamicConfigLibTests_" + Guid.NewGuid().ToString());
//            _mockLogger = new Mock<ILogger<FileSystemConfigRepository>>();
//            _repository = new FileSystemConfigRepository(_testConfigDir, _mockLogger.Object);
//        }

//        public void Dispose()
//        {
//            // Temizlik işlemlerini gerçekleştir
//            try
//            {
//                if (Directory.Exists(_testConfigDir))
//                {
//                    Directory.Delete(_testConfigDir, true);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Test temizleme hatası: {ex.Message}");
//            }
//        }

//        [Fact]
//        public void Constructor_ShouldThrowArgumentException_WhenDirectoryPathIsNull()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentException>(() => new FileSystemConfigRepository(null, _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_ShouldThrowArgumentException_WhenDirectoryPathIsEmpty()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentException>(() => new FileSystemConfigRepository("", _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_ShouldCreateDirectory_WhenItDoesNotExist()
//        {
//            // Arrange
//            var newPath = Path.Combine(_testConfigDir, "NewSubDir");
            
//            // Act
//            var repo = new FileSystemConfigRepository(newPath, _mockLogger.Object);
            
//            // Assert
//            Assert.True(Directory.Exists(newPath));
//            Assert.True(File.Exists(Path.Combine(newPath, "configs.json")));
//        }

//        [Fact]
//        public async Task GetConfigsForApplicationAsync_ShouldReturnEmptyList_WhenNoConfigsExist()
//        {
//            // Act
//            var result = await _repository.GetConfigsForApplicationAsync("TestApp");
            
//            // Assert
//            Assert.Empty(result);
//        }

//        [Fact]
//        public async Task AddConfigAsync_ShouldAddConfig_WhenConfigIsValid()
//        {
//            // Arrange
//            var configEntry = new ConfigEntry
//            {
//                Name = "TestConfig",
//                ApplicationName = "TestApp",
//                Type = "string",
//                Value = "TestValue",
//                IsActive = true
//            };
            
//            // Act
//            var result = await _repository.AddConfigAsync(configEntry);
            
//            // Assert
//            Assert.NotNull(result);
//            Assert.NotNull(result.Id);
//            Assert.Equal("TestConfig", result.Name);
            
//            // Verify it can be retrieved
//            var configs = await _repository.GetConfigsForApplicationAsync("TestApp");
//            Assert.Single(configs);
//            Assert.Equal("TestConfig", configs.First().Name);
//        }

//        [Fact]
//        public async Task GetConfigAsync_ShouldReturnConfig_WhenConfigExists()
//        {
//            // Arrange
//            var configEntry = new ConfigEntry
//            {
//                Name = "TestConfig",
//                ApplicationName = "TestApp",
//                Type = "string",
//                Value = "TestValue",
//                IsActive = true
//            };
            
//            await _repository.AddConfigAsync(configEntry);
            
//            // Act
//            var result = await _repository.GetConfigAsync("TestConfig", "TestApp");
            
//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal("TestConfig", result.Name);
//            Assert.Equal("TestValue", result.Value);
//        }

//        [Fact]
//        public async Task GetConfigAsync_ShouldReturnNull_WhenConfigDoesNotExist()
//        {
//            // Act
//            var result = await _repository.GetConfigAsync("NonExistentConfig", "TestApp");
            
//            // Assert
//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task UpdateConfigAsync_ShouldUpdateConfig_WhenConfigExists()
//        {
//            // Arrange
//            var configEntry = new ConfigEntry
//            {
//                Name = "TestConfig",
//                ApplicationName = "TestApp",
//                Type = "string",
//                Value = "TestValue",
//                IsActive = true
//            };
            
//            var addedConfig = await _repository.AddConfigAsync(configEntry);
//            addedConfig.Value = "UpdatedValue";
            
//            // Act
//            var updateResult = await _repository.UpdateConfigAsync(addedConfig);
            
//            // Assert
//            Assert.True(updateResult);
            
//            // Verify it was updated
//            var updatedConfig = await _repository.GetConfigAsync("TestConfig", "TestApp");
//            Assert.Equal("UpdatedValue", updatedConfig.Value);
//        }

//        [Fact]
//        public async Task UpdateConfigAsync_ShouldReturnFalse_WhenConfigDoesNotExist()
//        {
//            // Arrange
//            var nonExistentConfig = new ConfigEntry
//            {
//                Id = "non-existent-id",
//                Name = "NonExistentConfig",
//                ApplicationName = "TestApp",
//                Type = "string",
//                Value = "SomeValue",
//                IsActive = true
//            };
            
//            // Act
//            var result = await _repository.UpdateConfigAsync(nonExistentConfig);
            
//            // Assert
//            Assert.False(result);
//        }

//        [Fact]
//        public async Task DeleteConfigAsync_ShouldDeleteConfig_WhenConfigExists()
//        {
//            // Arrange
//            var configEntry = new ConfigEntry
//            {
//                Name = "TestConfig",
//                ApplicationName = "TestApp",
//                Type = "string",
//                Value = "TestValue",
//                IsActive = true
//            };
            
//            var addedConfig = await _repository.AddConfigAsync(configEntry);
            
//            // Act
//            var deleteResult = await _repository.DeleteConfigAsync(addedConfig.Id);
            
//            // Assert
//            Assert.True(deleteResult);
            
//            // Verify it was deleted
//            var configs = await _repository.GetConfigsForApplicationAsync("TestApp");
//            Assert.Empty(configs);
//        }

//        [Fact]
//        public async Task DeleteConfigAsync_ShouldReturnFalse_WhenConfigDoesNotExist()
//        {
//            // Act
//            var result = await _repository.DeleteConfigAsync("non-existent-id");
            
//            // Assert
//            Assert.False(result);
//        }

//        [Fact]
//        public async Task GetUpdatedConfigsAsync_ShouldReturnOnlyUpdatedConfigs()
//        {
//            // Arrange
//            var config1 = new ConfigEntry
//            {
//                Name = "Config1",
//                ApplicationName = "TestApp",
//                Type = "string",
//                Value = "Value1",
//                IsActive = true
//            };
            
//            await _repository.AddConfigAsync(config1);
            
//            var afterFirstAdd = DateTime.UtcNow;
//            await Task.Delay(1000); // 1 saniye bekle
            
//            var config2 = new ConfigEntry
//            {
//                Name = "Config2",
//                ApplicationName = "TestApp",
//                Type = "string",
//                Value = "Value2",
//                IsActive = true
//            };
            
//            await _repository.AddConfigAsync(config2);
            
//            // Act
//            var updatedConfigs = await _repository.GetUpdatedConfigsAsync("TestApp", afterFirstAdd);
            
//            // Assert
//            var configList = updatedConfigs.ToList();
//            Assert.Single(configList);
//            Assert.Equal("Config2", configList[0].Name);
//        }

//        [Fact]
//        public async Task GetAllConfigsNoFilterAsync_ShouldReturnAllConfigs()
//        {
//            // Arrange
//            var config1 = new ConfigEntry
//            {
//                Name = "Config1",
//                ApplicationName = "App1",
//                Type = "string",
//                Value = "Value1",
//                IsActive = true
//            };
            
//            var config2 = new ConfigEntry
//            {
//                Name = "Config2",
//                ApplicationName = "App2",
//                Type = "string",
//                Value = "Value2",
//                IsActive = false
//            };
            
//            await _repository.AddConfigAsync(config1);
//            await _repository.AddConfigAsync(config2);
            
//            // Act
//            var allConfigs = await _repository.GetAllConfigsNoFilterAsync();
            
//            // Assert
//            Assert.Equal(2, allConfigs.Count());
//            Assert.Contains(allConfigs, c => c.Name == "Config1" && c.ApplicationName == "App1");
//            Assert.Contains(allConfigs, c => c.Name == "Config2" && c.ApplicationName == "App2");
//        }
//    }
//} 