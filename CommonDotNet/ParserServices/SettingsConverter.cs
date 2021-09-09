using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonServices.ParserServices
{
    public class SettingsConverter<T> where T : class
    {
        private readonly Dictionary<string, string> environments;

        public SettingsConverter(string environmentValue)
        {
            try
            {
                environments = environmentValue
                    .Split(';')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0], y => y[1]);

                Model = Activator.CreateInstance<T>();

                Convert(Model);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("SettingConverter error: " + ex.InnerException?.Message);
            }
        }
        private void Convert(T model)
        {
            var modelProperties = typeof(T).GetProperties();

            foreach (var propertyInfo in modelProperties)
            {
                string propName = string.Intern(propertyInfo.Name);

                if (!environments.TryGetValue(propName, out var value)) 
                    continue;
                
                var type = model.GetType();
                var property = type.GetProperty(propName);

                property?.SetValue(model, value);
            }
        }
        public T Model { get; }
    }
}
