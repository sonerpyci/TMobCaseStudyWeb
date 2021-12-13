using System.IO;

namespace TMobCaseStudy.Base.Serialization
{
    public class ProtoBufSerializer : Serializer
    {
        public override void SerializeToStream<T>(T obj, Stream stream, bool leaveOpen)
        {
            if (obj == null) return;

            try
            {
                ProtoBuf.Serializer.Serialize(stream, obj);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream.Close();
                }
            }
        }
        
        public override T DeserializeFromStream<T>(Stream stream)
        {
            return ProtoBuf.Serializer.Deserialize<T>(stream);
        }
    }
}
