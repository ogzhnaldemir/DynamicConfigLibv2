using DynamicConfigLib.Core;
using DynamicConfigLib.Core.Interfaces;
using DynamicConfigLib.Core.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ekle DynamicConfigLib
var mongoConnectionString = builder.Configuration["MongoConnection:ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoConnection:DatabaseName"] ?? "DynamicConfigDb";

// kaydet MongoConfigRepository
builder.Services.AddSingleton<IConfigRepository>(sp => 
    new MongoConfigRepository(
        mongoConnectionString, 
        mongoDatabaseName, 
        sp.GetService<ILogger<MongoConfigRepository>>()));

//singelton olarak kaydet
builder.Services.AddSingleton<IConfigurationReader>(sp => 
    new ConfigurationReader(
        "API-Service", 
        mongoConnectionString, 
        60000)); // 1 dakika yenileme

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Docker'da tarayıcı başlatmaya çalışmıyoruz
    // Swagger UI'a erişim URL'ini loglayalım
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Swagger UI is available at: http://localhost:8080/swagger");
    
    // Geliştirme ortamında tarayıcıyı otomatik başlat
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            // İstenen özel URL'i aç
            var customUrl = "http://localhost:5272/index.html";
            
            // URL'i varsayılan tarayıcıda aç
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = customUrl,
                UseShellExecute = true
            });
            
            // Swagger UI'ın da kullanılabilir olduğunu logla
            logger.LogInformation("Swagger UI is available at: {url}/swagger", 
                app.Configuration["ASPNETCORE_URLS"]?.Split(';').First() ?? "http://localhost:5000");
        }
        catch (Exception ex)
        {
            // Tarayıcı açılmazsa sessizce devam et
            logger.LogWarning("Tarayıcı açılamadı: {Message}", ex.Message);
        }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();


app.UseStaticFiles();

app.MapControllers();

// Ana sayfayı swagger'a yönlendirilen
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
