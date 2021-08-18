using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonServices.ParserServices
{
    public class SettingsConverter<T> where T : class
    {
        private Dictionary<string, string> environments;
        public SettingsConverter(string environmentValue)
        {
            if (environmentValue is null)
                throw new NullReferenceException("environment value is not set");

            Model = Activator.CreateInstance<T>();
            Convert(environmentValue, Model);
        }
        private void Convert(string environmentValue, T model)
        {
            try
            {
                environments = environmentValue
                    .Split(';')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0], y => y[1]);
            }
            catch
            {
                throw new KeyNotFoundException("environment value is invalid");
            }

            var modelProperties = typeof(T).GetProperties();

            for (int i = 0; i < modelProperties.Length; i++)
            {
                string propName = string.Intern(modelProperties[i].Name);

                if (environments.TryGetValue(propName, out string value))
                    model.GetType().GetProperty(propName).SetValue(model, value);
            }
        }
        public T Model { get; }
    }
}
