DynamicConfigLib
DynamicConfigLib, .NET uygulamaları için dinamik olarak yapılandırma değerlerini saklamanıza ve yönetmenize olanak tanıyan bir kütüphanedir. Bu kütüphane, uygulama yeniden başlatmadan yapılandırma değerlerini güncelleyebilmenizi sağlar.
Özellikler
MongoDB desteğiyle yapılandırma değerlerini saklama (farklı depolama türlerine genişletilebilir)
Yapılandırma değerlerine tip güvenli erişim (string, int, bool, double)
Belirli aralıklarla yapılandırma değerlerini otomatik yenileme
Depolama erişilemez olduğunda son bilinen iyi yapılandırmaya geri dönme
Uygulamalar arası izolasyon sağlayan yapılandırma mekanizması
Yapılandırma değerlerini yönetmek için Web API ve UI
İsme göre yapılandırma değerlerini filtreleme

Projeler
DynamicConfigLib.Core: Yapılandırma okuyucu ve soyutlamaları içeren çekirdek kütüphane
DynamicConfigLib.Api: Yapılandırma değerlerini yönetmek için Web API ve kullanıcı arayüzü
DynamicConfigLib.SampleApp: Kütüphane kullanımını gösteren örnek konsol uygulaması
DynamicConfigLib.Tests: Kütüphane için birim testleri

Ön Gereksinimler
.NET 8.0 SDK
Docker ve Docker Compose (MongoDB için)

Kurulum
git clone https://github.com/yourusername/DynamicConfigLib.git
cd DynamicConfigLib
MongoDB varsayılan olarak 27017 portunda çalışacaktır.

!!Projeyi çalıştırmak için:
Solution Explorer'da solution'a sağ tıklayın
"Configure Startup Projects" seçeneğini tıklayın
"Multiple startup projects" seçeneğini işaretleyin
"DynamicConfigLib.Api" ve "DynamicConfigLib.SampleApp" projelerinin "Action" sütunundaki değeri "Start" olarak ayarlayın
"OK" düğmesine tıklayın ve F5 ile uygulamayı çalıştırın

API https://localhost:5001 ve http://localhost:5000 adreslerinde çalışacaktır.

Core Library: Yapılandırma değerlerini okumak ve yönetmek için gerekli arayüzler ve sınıflar
API: Yapılandırma değerlerini yönetmek için REST API ve web arayüzü

MongoDB Repository: Yapılandırma değerlerini saklama ve yönetme
Yapılandırma
appsettings.json dosyasında MongoDB bağlantı ayarlarını değiştirebilirsiniz:
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "DynamicConfig"
  }
}
