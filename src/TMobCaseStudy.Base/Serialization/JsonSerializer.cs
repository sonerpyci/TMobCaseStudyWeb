using Newtonsoft.Json;
using System.IO;

namespace TMobCaseStudy.Base.Serialization
{
    public class JsonSerializer : Serializer
    {
        private readonly bool _shouldIgnoreNullValues;

        public JsonSerializer(bool shouldIgnoreNullValues = false)
        {
            _shouldIgnoreNullValues = shouldIgnoreNullValues;
        }

        public override void SerializeToStream<T>(T obj, Stream stream, bool leaveOpen)
        {
            if (obj == null) return;

            var jsonSerializer = GetNewtonsoftSerializer();

            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(stream);
                var jsonWriter = new JsonTextWriter(streamWriter);
                jsonSerializer.Serialize(jsonWriter, obj, obj.GetType());
                streamWriter.Flush();
            }
            finally
            {
                if (!leaveOpen)
                {
                    streamWriter?.Close();
                }
            }
        }

        public override string SerializeToString<T>(T obj)
        {
            if (obj == null) return null;

            var settings = GetJsonSerializerSettings();
            return JsonConvert.SerializeObject(obj, typeof(T), settings);
        }

        public override T DeserializeFromStream<T>(Stream stream)
        {
            var jsonSerializer = GetNewtonsoftSerializer();
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            return jsonSerializer.Deserialize<T>(jsonReader);
        }

        public override T DeserializeFromString<T>(string str)
        {
            if (str == null) return default(T);

            var settings = GetJsonSerializerSettings();
            return JsonConvert.DeserializeObject<T>(str, settings);
        }
        
        private Newtonsoft.Json.JsonSerializer GetNewtonsoftSerializer()
        {
            return new Newtonsoft.Json.JsonSerializer()
            {
                NullValueHandling = _shouldIgnoreNullValues
                    ? NullValueHandling.Ignore : NullValueHandling.Include
            };
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                NullValueHandling = _shouldIgnoreNullValues
                    ? NullValueHandling.Ignore : NullValueHandling.Include
            };
        }
    }
}
