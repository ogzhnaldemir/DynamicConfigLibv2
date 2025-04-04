using DynamicConfigLib.Api.Controllers;
using DynamicConfigLib.Core.Dto;
using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace DynamicConfigLib.Tests
{
    public class ConfigControllerTests
    {
        private readonly Mock<IConfigRepository> _mockRepository;
        private readonly Mock<IConfigurationReader> _mockConfigReader;
        private readonly Mock<ILogger<ConfigController>> _mockLogger;
        private readonly ConfigController _controller;

        public ConfigControllerTests()
        {
            _mockRepository = new Mock<IConfigRepository>();
            _mockConfigReader = new Mock<IConfigurationReader>();
            _mockLogger = new Mock<ILogger<ConfigController>>();
            
            _controller = new ConfigController(
                _mockRepository.Object,
                _mockConfigReader.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllConfigs_ShouldReturnConfigs_WhenRepositoryReturnsConfigs()
        {
            // Arrange
            var configs = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestConfig1",
                    ApplicationName = "TEST-APP",
                    Type = "string",
                    Value = "TestValue1",
                    IsActive = true
                },
                new ConfigEntry
                {
                    Id = "2",
                    Name = "TestConfig2",
                    ApplicationName = "TEST-APP",
                    Type = "int",
                    Value = "42",
                    IsActive = true
                }
            };

            // IConfigRepository'den GetAllConfigsNoFilterAsync metodu için mock ayarı
            _mockRepository
                .Setup(r => r.GetAllConfigsNoFilterAsync())
                .ReturnsAsync(configs);

            // Act
            var result = await _controller.GetAllConfigs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnConfigs = Assert.IsAssignableFrom<IEnumerable<ConfigEntry>>(okResult.Value);
            Assert.Equal(2, returnConfigs.Count());
        }

        [Fact]
        public async Task GetConfigsForApplication_ShouldReturnConfigs_WhenApplicationNameIsValid()
        {
            // Arrange
            string applicationName = "TEST-APP";
            var configs = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestConfig1",
                    ApplicationName = applicationName,
                    Type = "string",
                    Value = "TestValue1",
                    IsActive = true
                }
            };

            _mockRepository
                .Setup(r => r.GetConfigsForApplicationAsync(applicationName))
                .ReturnsAsync(configs);

            // Act
            var result = await _controller.GetConfigsForApplication(applicationName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnConfigs = Assert.IsAssignableFrom<IEnumerable<ConfigEntry>>(okResult.Value);
            Assert.Single(returnConfigs);
            Assert.Equal("TestConfig1", returnConfigs.First().Name);
        }

        [Fact]
        public async Task GetConfig_ShouldReturnConfig_WhenConfigExists()
        {
            // Arrange
            string applicationName = "TEST-APP";
            string configName = "TestConfig";
            var config = new ConfigEntry
            {
                Id = "1",
                Name = configName,
                ApplicationName = applicationName,
                Type = "string",
                Value = "TestValue",
                IsActive = true
            };

            _mockRepository
                .Setup(r => r.GetConfigAsync(configName, applicationName))
                .ReturnsAsync(config);

            // Act
            var result = await _controller.GetConfig(applicationName, configName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnConfig = Assert.IsType<ConfigEntry>(okResult.Value);
            Assert.Equal(configName, returnConfig.Name);
            Assert.Equal("TestValue", returnConfig.Value);
        }

        [Fact]
        public async Task GetConfig_ShouldReturnNotFound_WhenConfigDoesNotExist()
        {
            // Arrange
            string applicationName = "TEST-APP";
            string configName = "NonExistentConfig";

            _mockRepository
                .Setup(r => r.GetConfigAsync(configName, applicationName))
                .ReturnsAsync((ConfigEntry)null);

            // Act
            var result = await _controller.GetConfig(applicationName, configName);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddConfig_ShouldReturnCreatedConfig_WhenInputIsValid()
        {
            // Arrange
            var configDto = new ConfigEntryCreateDto
            {
                Name = "NewConfig",
                ApplicationName = "TEST-APP",
                Type = "string",
                Value = "NewValue",
                IsActive = true,
                ConfigId = 1
            };

            var createdConfig = new ConfigEntry
            {
                Id = "new-id",
                Name = configDto.Name,
                ApplicationName = configDto.ApplicationName,
                Type = configDto.Type,
                Value = configDto.Value,
                IsActive = configDto.IsActive,
                ConfigId = configDto.ConfigId
            };

            _mockRepository
                .Setup(r => r.AddConfigAsync(It.IsAny<ConfigEntry>()))
                .ReturnsAsync(createdConfig);

            // Act
            var result = await _controller.AddConfig(configDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnConfig = Assert.IsType<ConfigEntry>(createdAtActionResult.Value);
            Assert.Equal("NewConfig", returnConfig.Name);
            Assert.Equal("new-id", returnConfig.Id);
        }

        [Fact]
        public async Task UpdateConfig_ShouldReturnNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            string id = "existing-id";
            var configDto = new ConfigEntryDto
            {
                Name = "UpdatedConfig",
                ApplicationName = "TEST-APP",
                Type = "string",
                Value = "UpdatedValue",
                IsActive = true
            };

            _mockRepository
                .Setup(r => r.UpdateConfigAsync(It.IsAny<ConfigEntry>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateConfig(id, configDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateConfig_ShouldReturnNotFound_WhenUpdateFails()
        {
            // Arrange
            string id = "non-existent-id";
            var configDto = new ConfigEntryDto
            {
                Name = "UpdatedConfig",
                ApplicationName = "TEST-APP",
                Type = "string",
                Value = "UpdatedValue",
                IsActive = true
            };

            _mockRepository
                .Setup(r => r.UpdateConfigAsync(It.IsAny<ConfigEntry>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateConfig(id, configDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfig_ShouldReturnNoContent_WhenDeleteSucceeds()
        {
            // Arrange
            string id = "existing-id";

            _mockRepository
                .Setup(r => r.DeleteConfigAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteConfig(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteConfig_ShouldReturnNotFound_WhenDeleteFails()
        {
            // Arrange
            string id = "non-existent-id";

            _mockRepository
                .Setup(r => r.DeleteConfigAsync(id))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteConfig(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetConfigValue_ShouldReturnOk_WhenValueExists()
        {
            // Arrange
            string configName = "TestConfig";
            string stringValue = "TestValue";

            _mockConfigReader
                .Setup(r => r.GetValue<string>(configName))
                .Returns(stringValue);

            // Act
            var result = _controller.GetConfigValue(configName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            
            // JSON olarak parseleyip değerlere ulaşalım
            var jsonResult = JsonConvert.SerializeObject(okResult.Value);
            var resultObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResult);
            
            Assert.Equal(stringValue, resultObj["Value"]);
            Assert.Equal("string", resultObj["Type"]);
        }

        [Fact]
        public void GetConfigValue_ShouldReturnNotFound_WhenValueDoesNotExist()
        {
            // Arrange
            string configName = "NonExistentConfig";

            _mockConfigReader
                .Setup(r => r.GetValue<string>(configName))
                .Returns((string)null);

            _mockConfigReader
                .Setup(r => r.GetValue<int>(configName))
                .Returns(0);

            _mockConfigReader
                .Setup(r => r.GetValue<bool>(configName))
                .Returns(false);

            _mockConfigReader
                .Setup(r => r.GetValue<double>(configName))
                .Returns(0.0);

            // Act
            var result = _controller.GetConfigValue(configName);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
} 