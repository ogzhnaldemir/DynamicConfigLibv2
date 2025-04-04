using DynamicConfigLib.Core;
using DynamicConfigLib.Core.Repositories;

namespace DynamicConfigLib.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine("   DİNAMİK KONFİGÜRASYON KÜTÜPHANESİ DEMO");
            Console.WriteLine("=================================================");
            Console.ResetColor();
            Console.WriteLine();

            try
            {
                // MongoDB bağlantı bilgileri
                string connectionString = "mongodb://localhost:27017";
                int refreshInterval = 10000; // Sunum için 10 saniye
                
                // Kullanıcıdan servis adını al
                Console.WriteLine("Lütfen test etmek istediğiniz servis/uygulama adını girin:");
                Console.Write("> ");
                
                string serviceName = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    Console.WriteLine("Servis adı boş olamaz.");
                    return;
                }

                Console.Clear();
                ShowHeader(serviceName);

                // ConfigurationReader örneği oluştur
                Console.WriteLine($"» ConfigurationReader oluşturuluyor...");
                Console.WriteLine($"  - Servis Adı: {serviceName}");
                Console.WriteLine($"  - Bağlantı: {connectionString}");
                Console.WriteLine($"  - Yenileme Aralığı: {refreshInterval}ms");
                
                var configReader = new ConfigurationReader(serviceName, connectionString, refreshInterval);
                Console.WriteLine("» ConfigurationReader başarıyla oluşturuldu\n");

                // MongoDB'den tüm konfigürasyonları dinamik olarak al
                Console.WriteLine($"» MongoDB'den {serviceName} için konfigürasyonlar alınıyor...");
                var repository = new MongoConfigRepository(connectionString, "DynamicConfigDb");
                
                // Servis için mevcut konfigürasyonları al
                var allServiceConfigs = await repository.GetConfigsForApplicationAsync(serviceName);
                var activeConfigKeys = allServiceConfigs.Select(c => c.Name).ToList();
                
                if (!activeConfigKeys.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n{serviceName} için henüz konfigürasyon kaydı bulunamadı.");
                    Console.WriteLine("Devam etmek için API kullanarak bazı konfigürasyonlar ekleyin.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"» {activeConfigKeys.Count} adet konfigürasyon bulundu.");
                }

                // İlk değer okuma
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[1] {serviceName} KONFİGÜRASYON DEĞERLERİ (İLK OKUMA)");
                Console.ResetColor();
                
                ReadAndDisplayConfigValues(configReader, allServiceConfigs);

                // API kullanımı için bilgiler
                Console.WriteLine("\n» API'yi kullanarak konfigürasyon değerlerini ekleyin/güncelleyin:");
                Console.WriteLine("  1. Swagger UI: https://localhost:5001/swagger");
                Console.WriteLine("  2. POST /api/Config veya PUT /api/Config/{id} kullanın");
                Console.WriteLine($"  3. ApplicationName değeri: {serviceName}");
               
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nDeğerleri ekledikten/güncelledikten sonra bir tuşa basın...");
                Console.ResetColor();
                Console.ReadKey();
                Console.WriteLine();

                // MongoDB'den güncel konfigürasyonları tekrar al
                allServiceConfigs = await repository.GetConfigsForApplicationAsync(serviceName);
                
                // Manuel yenileme sonrası değerler
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[2] {serviceName} KONFİGÜRASYON DEĞERLERİ (MANUEL YENİLEME SONRASI)");
                Console.ResetColor();
                
                ReadAndDisplayConfigValues(configReader, allServiceConfigs);

                // Dinamik güncelleme testi
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n» DİNAMİK GÜNCELLEME TESTİ");
                Console.ResetColor();
                Console.WriteLine("  API üzerinden bir konfigürasyon değerini güncelleyin");
                Console.WriteLine($"  {refreshInterval/1000} saniye içinde otomatik güncellenecek");
                
                // Geri sayım 
                for (int i = refreshInterval/1000; i > 0; i--)
                {
                    Console.Write($"\r  Kalan süre: {i} saniye   ");
                    Thread.Sleep(1000);
                }
                
                // MongoDB'den güncel konfigürasyonları tekrar al
                allServiceConfigs = await repository.GetConfigsForApplicationAsync(serviceName);
                
                // Otomatik timer refresh sonrası
                Console.WriteLine("\n");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[3] {serviceName} KONFİGÜRASYON DEĞERLERİ (OTOMATİK YENİLEME SONRASI)");
                Console.ResetColor();
                
                ReadAndDisplayConfigValues(configReader, allServiceConfigs);

                // IsActive = 0 test
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n» IsActive = 0 TEST");
                Console.ResetColor();
                Console.WriteLine("  API üzerinden bir konfigürasyonun IsActive değerini 0 (false) yapın");
                Console.WriteLine($"  {refreshInterval/1000} saniye içinde değer görünmez olacak");
                
                // Geri sayım 
                for (int i = refreshInterval/1000; i > 0; i--)
                {
                    Console.Write($"\r  Kalan süre: {i} saniye   ");
                    Thread.Sleep(1000);
                }
                
                // MongoDB'den güncel konfigürasyonları tekrar al
                allServiceConfigs = await repository.GetConfigsForApplicationAsync(serviceName);
                
                // Pasif değerler sonrası
                Console.WriteLine("\n");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[4] {serviceName} KONFİGÜRASYON DEĞERLERİ (IsActive=0 SONRASI)");
                Console.ResetColor();
                
                ReadAndDisplayConfigValues(configReader, allServiceConfigs);

                // Tüm veritabanındaki konfigürasyonları göster
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n» Veritabanındaki Tüm Uygulamaların Konfigürasyonları");
                Console.ResetColor();
                Console.WriteLine("  (Her servis sadece kendi konfigürasyonlarını görebilir)");
                
                var allConfigs = await repository.GetAllConfigsNoFilterAsync();
                var groupedConfigs = allConfigs.GroupBy(c => c.ApplicationName);
                
                foreach (var group in groupedConfigs)
                {
                    Console.WriteLine();
                    Console.WriteLine($"  • {group.Key} ({group.Count()} konfigürasyon)");
                    
                    foreach (var config in group)
                    {
                        string statusIcon = config.IsActive ? "✓" : "✗";
                        Console.ForegroundColor = config.IsActive ? ConsoleColor.Green : ConsoleColor.DarkGray;
                        Console.WriteLine($"    {statusIcon} {config.Name,-15} = {config.Value} ({config.Type})");
                        Console.ResetColor();
                    }
                }
                
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"» DEMO: ConfigReader sadece '{serviceName}' için aktif konfigürasyonları görebilir:");
                Console.ResetColor();
                
                foreach (var group in groupedConfigs)
                {
                    if (group.Key != serviceName)
                    {
                        // Farklı bir uygulamanın konfigürasyonunu okumaya çalış
                        var otherAppConfig = group.FirstOrDefault();
                        if (otherAppConfig != null)
                        {
                            Console.Write($"  • {otherAppConfig.Name} ({group.Key}) = ");
                            try
                            {
                                var value = configReader.GetValue<string>(otherAppConfig.Name);
                                Console.WriteLine(value ?? "null");
                            }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine("<erişim yok>");
                                Console.ResetColor();
                            }
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTest tamamlandı. Çıkmak için bir tuşa basın...");
                Console.ResetColor();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"HATA: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                Console.WriteLine("Çıkmak için bir tuşa basın...");
                Console.ReadKey();
            }
        }

        static void ShowHeader(string serviceName)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine($"   {serviceName} - KONFİGÜRASYON TESTİ");
            Console.WriteLine("=================================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void ReadAndDisplayConfigValues(ConfigurationReader configReader, IEnumerable<Core.Models.ConfigEntry> configs)
        {
            Console.WriteLine();
            bool anyValueFound = false;

            if (configs == null || !configs.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("  Hiçbir aktif konfigürasyon değeri bulunamadı.");
                Console.WriteLine("  API kullanarak konfigürasyon ekleyebilirsiniz.");
                Console.ResetColor();
                return;
            }

            foreach (var config in configs)
            {
                try
                {
                    // Konfigürasyon tipine göre değeri al
                    switch (config.Type.ToLower())
                    {
                        case "string":
                            var stringValue = configReader.GetValue<string>(config.Name);
                            Console.WriteLine($"  • {config.Name,-15} = {stringValue} (string)");
                            anyValueFound = true;
                            break;
                            
                        case "int":
                        case "integer":
                            var intValue = configReader.GetValue<int>(config.Name);
                            Console.WriteLine($"  • {config.Name,-15} = {intValue} (int)");
                            anyValueFound = true;
                            break;
                            
                        case "bool":
                        case "boolean":
                            var boolValue = configReader.GetValue<bool>(config.Name);
                            Console.WriteLine($"  • {config.Name,-15} = {boolValue} (bool)");
                            anyValueFound = true;
                            break;
                            
                        case "double":
                        case "float":
                            var doubleValue = configReader.GetValue<double>(config.Name);
                            Console.WriteLine($"  • {config.Name,-15} = {doubleValue} (double)");
                            anyValueFound = true;
                            break;
                            
                        default:
                            // Tip bilinmiyorsa string olarak dene
                            var value = configReader.GetValue<string>(config.Name);
                            Console.WriteLine($"  • {config.Name,-15} = {value} (bilinmeyen tip: {config.Type})");
                            anyValueFound = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  • {config.Name,-15} = <hata: {ex.Message}>");
                    Console.ResetColor();
                }
            }

            if (!anyValueFound)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("  Hiçbir aktif konfigürasyon değeri okunamadı.");
                Console.WriteLine("  Konfigürasyonlara erişimde sorun olabilir.");
                Console.ResetColor();
            }
        }
    }
}
