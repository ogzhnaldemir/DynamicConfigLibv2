using DynamicConfigLib.Core;
using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace DynamicConfigLib.Tests
{
    public class ConfigServiceTests
    {
        private readonly Mock<IConfigRepository> _mockRepository;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<ConfigService>> _mockLogger;
        private readonly string _applicationName = "TEST-APP";

        public ConfigServiceTests()
        {
            _mockRepository = new Mock<IConfigRepository>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<ConfigService>>();
            
            // Cache davranışını mockla
            var mockCacheEntry = new Mock<ICacheEntry>();
            
            _mockCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);
        }

        [Fact]
        public void GetConfig_ShouldReturnStringValue_WhenConfigExists()
        {
            
            var configEntries = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestConfig",
                    ApplicationName = _applicationName,
                    Type = "string",
                    Value = "TestValue",
                    IsActive = true
                }
            };

            object outValue = configEntries;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, _applicationName);

            // Act
            var result = configService.GetConfig<string>("TestConfig");

            // Assert
            Assert.Equal("TestValue", result);
        }

        [Fact]
        public void GetConfig_ShouldReturnIntValue_WhenConfigExists()
        {
            // Arrange
            var configEntries = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestIntConfig",
                    ApplicationName = _applicationName,
                    Type = "int",
                    Value = "42",
                    IsActive = true
                }
            };

            object outValue = configEntries;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, _applicationName);

            // Act
            var result = configService.GetConfig<int>("TestIntConfig");

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void GetConfig_ShouldReturnBoolValue_WhenConfigExists()
        {
            // Arrange
            var configEntries = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestBoolConfig",
                    ApplicationName = _applicationName,
                    Type = "bool",
                    Value = "true",
                    IsActive = true
                }
            };

            object outValue = configEntries;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, _applicationName);

            // Act
            var result = configService.GetConfig<bool>("TestBoolConfig");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetConfig_ShouldReturnDefaultValue_WhenConfigIsNotActive()
        {
            // Arrange
            var configEntries = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestConfig",
                    ApplicationName = _applicationName,
                    Type = "string",
                    Value = "TestValue",
                    IsActive = false
                }
            };

            object outValue = configEntries;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, _applicationName);

            // Act
            var result = configService.GetConfig<string>("TestConfig", "DefaultValue");

            // Assert
            Assert.Equal("DefaultValue", result);
        }

        [Fact]
        public void GetConfig_ShouldReturnDefaultValue_WhenConfigDoesNotExist()
        {
            // Arrange
            var configEntries = new List<ConfigEntry>();
            
            object outValue = configEntries;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, _applicationName);

            // Act
            var result = configService.GetConfig<string>("NonExistentConfig", "DefaultValue");

            // Assert
            Assert.Equal("DefaultValue", result);
        }

        [Fact]
        public void GetAllConfigs_ShouldReturnConfigs_WhenCacheExists()
        {
            // Arrange
            var configEntries = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestConfig1",
                    ApplicationName = _applicationName,
                    Type = "string",
                    Value = "TestValue1",
                    IsActive = true
                },
                new ConfigEntry
                {
                    Id = "2",
                    Name = "TestConfig2",
                    ApplicationName = _applicationName,
                    Type = "int",
                    Value = "42",
                    IsActive = true
                }
            };

            object outValue = configEntries;
            _mockCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, _applicationName);

            // Act
            var result = configService.GetAllConfigs();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("TestConfig1", result.First().Name);
            Assert.Equal("TestConfig2", result.Last().Name);
        }

        [Fact]
        public async Task RefreshConfigsAsync_ShouldUpdateCache_WhenCalled()
        {
            // Arrange
            var configEntries = new List<ConfigEntry>
            {
                new ConfigEntry
                {
                    Id = "1",
                    Name = "TestConfig",
                    ApplicationName = _applicationName,
                    Type = "string",
                    Value = "TestValue",
                    IsActive = true
                }
            };

            _mockRepository
                .Setup(r => r.GetConfigsForApplicationAsync(_applicationName))
                .ReturnsAsync(configEntries);

            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, _applicationName);

            // Act
            await configService.RefreshConfigsAsync();

            // Assert
            _mockRepository.Verify(r => r.GetConfigsForApplicationAsync(_applicationName), Times.AtLeastOnce);
        }

        [Fact]
        public void SetApplicationName_ShouldUpdateApplicationName_WhenCalled()
        {
            // Arrange
            var configService = new ConfigService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object, "OLD-APP");

            // Act
            configService.SetApplicationName("NEW-APP");

            // Assert
            Assert.Equal("NEW-APP", configService.GetApplicationName());
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ConfigService(null, _mockCache.Object, _mockLogger.Object, _applicationName));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenCacheIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ConfigService(_mockRepository.Object, null, _mockLogger.Object, _applicationName));
        }
    }

    public class ConfigurationReaderTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenApplicationNameIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ConfigurationReader(null, "mongodb://localhost:27017", 30000));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenApplicationNameIsEmpty()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ConfigurationReader("", "mongodb://localhost:27017", 30000));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ConfigurationReader("TEST-APP", null, 30000));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsEmpty()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ConfigurationReader("TEST-APP", "", 30000));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenRefreshIntervalIsZero()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ConfigurationReader("TEST-APP", "mongodb://localhost:27017", 0));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenRefreshIntervalIsNegative()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ConfigurationReader("TEST-APP", "mongodb://localhost:27017", -1000));
        }

        
    }

    public class ConfigEntryTests
    {
        [Fact]
        public void GetTypedValue_ShouldReturnString_WhenTypeIsString()
        {
            // Arrange
            var configEntry = new ConfigEntry
            {
                Name = "TestString",
                Type = "string",
                Value = "TestValue"
            };

            // Act
            var result = configEntry.GetTypedValue<string>();

            // Assert
            Assert.Equal("TestValue", result);
        }

        [Fact]
        public void GetTypedValue_ShouldReturnInt_WhenTypeIsInt()
        {
            // Arrange
            var configEntry = new ConfigEntry
            {
                Name = "TestInt",
                Type = "int",
                Value = "42"
            };

            // Act
            var result = configEntry.GetTypedValue<int>();

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void GetTypedValue_ShouldReturnBool_WhenTypeIsBool()
        {
            // Arrange
            var configEntry = new ConfigEntry
            {
                Name = "TestBool",
                Type = "bool",
                Value = "true"
            };

            // Act
            var result = configEntry.GetTypedValue<bool>();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetTypedValue_ShouldReturnDouble_WhenTypeIsDouble()
        {
            // Arrange
            var configEntry = new ConfigEntry
            {
                Name = "TestDouble",
                Type = "double",
                Value = "314"
            };

            // Act
            var result = configEntry.GetTypedValue<double>();

            // Assert
            Assert.Equal(314, result);
        }

        [Fact]
        public void GetTypedValue_ShouldThrowException_WhenTypeDoesNotMatch()
        {
            // Arrange
            var configEntry = new ConfigEntry
            {
                Name = "TestString",
                Type = "string",
                Value = "NotAnInt"
            };

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => configEntry.GetTypedValue<int>());
        }
    }
}