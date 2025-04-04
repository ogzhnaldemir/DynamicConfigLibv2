using DynamicConfigLib.Core.Dto;
using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicConfigLib.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigRepository _repository;
    private readonly IConfigurationReader _configurationReader;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(
        IConfigRepository repository, 
        IConfigurationReader configurationReader,
        ILogger<ConfigController> logger)
    {
        _repository = repository;
        _configurationReader = configurationReader;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfigEntry>>> GetAllConfigs()
    {
        try
        {
            var allConfigs = await GetAllConfigsWithoutFilter();
            
            if (!allConfigs.Any())
            {
                return Ok(new List<ConfigEntry>());
            }

            return Ok(allConfigs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all configurations");
            return StatusCode(500, "An error occurred while retrieving configurations");
        }
    }

    private async Task<IEnumerable<ConfigEntry>> GetAllConfigsWithoutFilter()
    {
        try
        {
            var allConfigs = await _repository.GetAllConfigsNoFilterAsync();
            allConfigs = allConfigs.Where(a => a.IsActive);
            return allConfigs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all configs without filter");
            return new List<ConfigEntry>();
        }
    }

    [HttpGet]
    [Route("{applicationName}")]
    public async Task<ActionResult<IEnumerable<ConfigEntry>>> GetConfigsForApplication(string applicationName)
    {
        try
        {
            var configs = await _repository.GetConfigsForApplicationAsync(applicationName);
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configs for application {ApplicationName}", applicationName);
            return StatusCode(500, "An error occurred while retrieving configurations");
        }
    }

    [HttpGet]
    [Route("{applicationName}/{configName}")]
    public async Task<ActionResult<ConfigEntry>> GetConfig(string applicationName, string configName)
    {
        try
        {
            var config = await _repository.GetConfigAsync(configName, applicationName);
            if (config == null)
            {
                return NotFound();
            }
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting config {ConfigName} for application {ApplicationName}", configName, applicationName);
            return StatusCode(500, "An error occurred while retrieving the configuration");
        }
    }

    [HttpGet]
    [Route("value/{configName}")]
    public ActionResult<object> GetConfigValue(string configName)
    {
        try
        {
            var stringValue = _configurationReader.GetValue<string>(configName);
            if (stringValue != null)
            {
                return Ok(new { Value = stringValue, Type = "string" });
            }

            var intValue = _configurationReader.GetValue<int>(configName);
            if (intValue != default)
            {
                return Ok(new { Value = intValue, Type = "int" });
            }

            var boolValue = _configurationReader.GetValue<bool>(configName);
            if (boolValue)
            {
                return Ok(new { Value = boolValue, Type = "bool" });
            }

            var doubleValue = _configurationReader.GetValue<double>(configName);
            if (doubleValue != default)
            {
                return Ok(new { Value = doubleValue, Type = "double" });
            }

            return NotFound($"Configuration '{configName}' not found or is not active");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value for config {ConfigName}", configName);
            return StatusCode(500, "An error occurred while retrieving the configuration value");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ConfigEntry>> AddConfig(ConfigEntryCreateDto configDto)
    {
        try
        {
            var config = new ConfigEntry()
            {
                ApplicationName = configDto.ApplicationName,
                IsActive = configDto.IsActive,
                ConfigId = configDto.ConfigId,
                Name = configDto.Name,
                Type = configDto.Type,
                Value = configDto.Value,
            };
            var result = await _repository.AddConfigAsync(config);
            return CreatedAtAction(nameof(GetConfig), new { applicationName = config.ApplicationName, configName = config.Name }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding config {ConfigName}", configDto.Name);
            return StatusCode(500, "An error occurred while adding the configuration");
        }
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> UpdateConfig(string id, [FromBody] ConfigEntryDto configDto)
    {
        try
        {
           
            var config = new ConfigEntry
            {
                Id = id, 
                Name = configDto.Name,
                Type = configDto.Type,
                Value = configDto.Value,
                IsActive = configDto.IsActive,
                ApplicationName = configDto.ApplicationName
            };
            
            _logger.LogInformation("Updating config with ID: {Id}, Name: {Name}, Value: {Value}", 
                id, config.Name, config.Value);

            var success = await _repository.UpdateConfigAsync(config);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating config with name {Name}", configDto?.Name);
            return StatusCode(500, "An error occurred while updating the configuration");
        }
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteConfig(string id)
    {
        try
        {
            var success = await _repository.DeleteConfigAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting config with ID {ConfigId}", id);
            return StatusCode(500, "An error occurred while deleting the configuration");
        }
    }
}
