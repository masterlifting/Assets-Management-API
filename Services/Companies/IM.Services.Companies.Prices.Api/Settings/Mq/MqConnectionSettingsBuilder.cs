using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Companies.Prices.Api.Settings.Mq
{
    public class MqConnectionSettingsBuilder
    {
        public MqConnectionSettingsBuilder(string connectionString)
        {
            Dictionary<string, string> connectionSettings;

            try
            {
                connectionSettings = connectionString
                    .Split(';')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0], y => y[1]);
               
                ConnectionSettings = new()
                {
                    Server = connectionSettings[nameof(ConnectionSettings.Server)],
                    UserId = connectionSettings[nameof(ConnectionSettings.UserId)],
                    Password = connectionSettings[nameof(ConnectionSettings.Password)]
                };
            }
            catch
            {
                throw new KeyNotFoundException("Connectionstring is invalid");
            }
        }
        public MqConnectionSettings ConnectionSettings { get; }
    }
}
