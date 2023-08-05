using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using ExtraObjectiveSetup.Utils;
using GTFO.API.JSON.Converters;

namespace ExtraObjectiveSetup.JSON
{
    public static class EOSJson
    {
        private static readonly JsonSerializerOptions _setting = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,

        };


        static EOSJson()
        {
            _setting.Converters.Add(new JsonStringEnumConverter());
            if(MTFOPartialDataUtil.IsLoaded)
            {
                _setting.Converters.Add(MTFOPartialDataUtil.PersistentIDConverter);
                _setting.Converters.Add(MTFOPartialDataUtil.LocalizedTextConverter);
                EOSLogger.Log("PartialData support found!");
            }

            else
            {
                _setting.Converters.Add(new LocalizedTextConverter());
            }

            if(InjectLibUtil.IsLoaded)
            {
                _setting.Converters.Add(InjectLibUtil.InjectLibConnector);
                EOSLogger.Log("InjectLib (AWO) support found!");
            }
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _setting);
        }

        public static object Deserialize(Type type, string json)
        {
            return JsonSerializer.Deserialize(json, type, _setting);
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _setting);
        }
    }
}
