using System;
using System.IO;
using System.Text;

namespace TMobCaseStudy.Base.Serialization
{
    public abstract class Serializer
    {
        public abstract void SerializeToStream<T>(T obj, Stream stream, bool leaveOpen = true);

        public virtual byte[] SerializeToBytes<T>(T obj)
        {
            using var ms = new MemoryStream();
            SerializeToStream(obj, ms, leaveOpen:true);
            ms.Flush();
            return ms.ToArray();
        }

        public virtual string SerializeToString<T>(T obj)
        {
            var bytes = SerializeToBytes(obj);
            return ToBase64String(bytes);
        }

        public abstract T DeserializeFromStream<T>(Stream stream);

        public virtual T DeserializeFromBytes<T>(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            return DeserializeFromStream<T>(ms);
        }

        public virtual T DeserializeFromString<T>(string str)
        {
            var bytes = FromBase64String(str);
            return DeserializeFromBytes<T>(bytes);
        }

        private static string ToBase64String(byte[] data)
        {
            return data == null ? null : Convert.ToBase64String(data);
        }

        private static byte[] FromBase64String(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : Convert.FromBase64String(str);
        }

        public static string GetString(byte[] data)
        {
            return data == null ? null : Encoding.UTF8.GetString(data);
        }

        public static byte[] GetBytes(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : Encoding.UTF8.GetBytes(str);
        }
    }
}
