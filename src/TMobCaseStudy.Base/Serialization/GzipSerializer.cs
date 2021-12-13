using System.IO;
using System.IO.Compression;

namespace TMobCaseStudy.Base.Serialization
{
    public class GzipSerializer : Serializer
    {
        private readonly Serializer _serializer;
        private readonly CompressionLevel _compressionLevel;
        
        public GzipSerializer(Serializer serializer,
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            _serializer = serializer;
            _compressionLevel = compressionLevel;
        }

        public override void SerializeToStream<T>(T obj, Stream stream, bool leaveOpen)
        {
            if (obj == null) return;

            using var gzipStream = new GZipStream(stream, _compressionLevel, leaveOpen);
            _serializer.SerializeToStream(obj, gzipStream, leaveOpen);
        }
        
        public override T DeserializeFromStream<T>(Stream stream)
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            return _serializer.DeserializeFromStream<T>(gzipStream);
        }
    }
}
