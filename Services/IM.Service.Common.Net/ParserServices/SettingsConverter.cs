using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Service.Common.Net.ParserServices;

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
        catch (Exception exception)
        {
            throw new KeyNotFoundException("SettingConverter error: " + exception.Message);
        }
    }
    private void Convert(T model)
    {
        var modelProperties = typeof(T).GetProperties();

        foreach (var propertyInfo in modelProperties)
        {
            var propName = string.Intern(propertyInfo.Name);

            if (!environments.TryGetValue(propName, out var value)) 
                continue;
                
            var type = model.GetType();
            var property = type.GetProperty(propName);

            property?.SetValue(model, value);
        }
    }
    public T Model { get; }
}