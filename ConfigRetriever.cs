using Newtonsoft.Json;
using System;
using System.IO;

namespace capp1 { 
    class Config
    {
        public string email { get; set; }
        public string password { get; set; }
        public string server { get; set; }
        public string loadingFilename { get; set; }
        public string sheetFilename { get; set; }
        public string folder { get; set; }
    }

    class ConfigRetriever
    {
        private Config config;
        public string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public ConfigRetriever()
        {
            StreamReader jsonStream = File.OpenText($@"{userPath}\OneDrive\Scripts\resources\c#-berlys-config.json");
            var json = jsonStream.ReadToEnd();
            config = JsonConvert.DeserializeObject<Config>(json, new JsonSerializerSettings()
            { Culture = new System.Globalization.CultureInfo("es-ES") });
        }

        public Config Load()
        {
            return config;
        }

    }
}