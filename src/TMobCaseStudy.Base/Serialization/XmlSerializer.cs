using System.IO;

namespace TMobCaseStudy.Base.Serialization
{
    public class XmlSerializer : Serializer
    {
        public override void SerializeToStream<T>(T obj, Stream stream, bool leaveOpen)
        {
            if (obj == null) return;

            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                serializer.Serialize(stream, obj);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream.Close();
                }
            }
        }

        public override string SerializeToString<T>(T obj)
        {
            if (obj == null) return null;
            
            var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            using var sw = new StringWriter();
            serializer.Serialize(sw, obj);
            return sw.ToString();
        }

        public override T DeserializeFromStream<T>(Stream stream)
        {
            if (stream == null) return default(T);

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream);
        }
        
        public override T DeserializeFromString<T>(string str)
        {
            if (str == null) return default(T);

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using var sr = new StringReader(str);
            return (T)serializer.Deserialize(sr);
        }
    }
}
