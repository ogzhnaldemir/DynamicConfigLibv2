namespace DynamicConfigLib.Core.Dto;

public class ConfigEntryCreateDto
{
    public int? ConfigId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public bool IsActive { get; set; }
    public string ApplicationName { get; set; }
}
