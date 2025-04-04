using DynamicConfigLib.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicConfigLib.Core.Interfaces
{
    public interface IConfigRepository
    {
      
        Task<IEnumerable<ConfigEntry>> GetConfigsForApplicationAsync(string applicationName);

        //Belirli bir uygulama için konfigürasyonları getirme
        Task<ConfigEntry> GetConfigAsync(string configName, string applicationName);

        //İsim ve uygulama ile belirli bir konfigürasyonu getirme
        Task<ConfigEntry> AddConfigAsync(ConfigEntry configEntry);

        //Belirli bir zamandan sonra güncellenmiş konfigürasyonları getirme
        Task<bool> UpdateConfigAsync(ConfigEntry configEntry);

        //Filtreleme olmadan tüm konfigürasyonları getirme
        Task<bool> DeleteConfigAsync(string configId);
        //Yeni bir konfigürasyon girdisi ekleme
          Task<IEnumerable<ConfigEntry>> GetUpdatedConfigsAsync(string applicationName, DateTime since);
        //Mevcut bir konfigürasyon girdisini güncelleme
        Task<IEnumerable<ConfigEntry>> GetAllConfigsNoFilterAsync();
        //D ile bir konfigürasyon girdisini silme
        //Tüm yöntemler asenkrondur (Task döndürür) ve ConfigEntry modeliyle çalışır.
    }
} 