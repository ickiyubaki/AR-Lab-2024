using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Common.Scripts.Utils
{
    public static class JsonUtils
    {
        private static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static object DeserializeJsonFromStream(Type type, Stream stream,
            JsonSerializerSettings serializerSettings = null)
        {
            serializerSettings ??= DefaultJsonSerializerSettings;

            if (stream == null || stream.CanRead == false)
                return default;

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = JsonSerializer.Create(serializerSettings);
                var searchResult = js.Deserialize(jtr, type);
                return searchResult;
            }
        }

        public static T DeserializeJsonFromStream<T>(Stream stream, JsonSerializerSettings serializerSettings = null)
        {
            serializerSettings ??= DefaultJsonSerializerSettings;

            if (stream == null || stream.CanRead == false)
                return default;

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = JsonSerializer.Create(serializerSettings);
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        public static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }
    }
}