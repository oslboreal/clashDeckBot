using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace TWPoster
{
    public class UserConfiguration
    {
        public static string configurationFile = "config.json";

        public string CostumerKey { get; set; }
        public string CostumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public string AuthorizationToken { get; set; } // TODO : Implement.

        private static void SetConfiguration(string costumerkey, string costumersecret, string token, string tokenSecret)
        {
            UserConfiguration configuration = new UserConfiguration();
            configuration.AccessToken = token;
            configuration.AccessTokenSecret = tokenSecret;
            configuration.CostumerKey = costumerkey;
            configuration.CostumerSecret = costumersecret;

            // Serializes the file.
            string json = JsonConvert.SerializeObject(configuration, Formatting.Indented);

            File.WriteAllText(UserConfiguration.configurationFile, json);
        }

        public static UserConfiguration GetConfiguration()
        {
            // Reads all the text from phrases file..
            string content = File.ReadAllText(UserConfiguration.configurationFile);

            // Obtains phrases.
            UserConfiguration phrase = JsonConvert.DeserializeObject<UserConfiguration>(content);
            return phrase;
        }
    }
}
