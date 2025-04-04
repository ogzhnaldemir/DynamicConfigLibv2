// Create dynamicConfig database and collection
db = db.getSiblingDB('dynamicConfig');

// Create ConfigEntries collection and add some sample data
db.createCollection('ConfigEntries');

db.ConfigEntries.insertMany([
    {
        Name: "MaxConnections",
        ApplicationName: "SampleApp",
        Type: "int",
        Value: "100",
        IsActive: true,
        LastUpdated: new Date(),
        ConfigId: 1
    },
    {
        Name: "ApiUrl",
        ApplicationName: "SampleApp",
        Type: "string",
        Value: "https://api.example.com",
        IsActive: true,
        LastUpdated: new Date(),
        ConfigId: 2
    },
    {
        Name: "EnableLogging",
        ApplicationName: "SampleApp",
        Type: "bool",
        Value: "true",
        IsActive: true,
        LastUpdated: new Date(),
        ConfigId: 3
    }
]); 