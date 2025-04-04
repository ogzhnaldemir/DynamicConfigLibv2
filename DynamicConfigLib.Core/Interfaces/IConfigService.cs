using DynamicConfigLib.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicConfigLib.Core.Interfaces
{//servis tanımlama t  generic parametre 
    public interface IConfigService
    {
     
        /// Belirli bir konfigürasyonu istenen tipte almak için generic bir metot tanımlandı. 
        
        T GetConfig<T>(string configName, T defaultValue = default);


        /// Mevcut uygulama için tüm aktif konfigürasyonları getiren bir metot eklendi.

        IEnumerable<ConfigEntry> GetAllConfigs();
        //coonfigentry nesnesi döndürür
      
     
  
        Task RefreshConfigsAsync();

        ///zorla yenileme
     
        void SetApplicationName(string applicationName);


        ///  Bu servis örneğinin hangi uygulama için çalışacağını ayarlayan  metot.

        string GetApplicationName();

        ///  Mevcut uygulama adını döndüren bir metot tanımlama.
    }
} 