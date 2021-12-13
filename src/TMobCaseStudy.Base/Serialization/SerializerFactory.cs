using System;

namespace TMobCaseStudy.Base.Serialization
{
    public static class SerializerFactory
    {
        public static Serializer Get(SerializationType serializationType)
        {
            return serializationType switch
            {
                SerializationType.Json => new JsonSerializer(),
                SerializationType.Xml => new XmlSerializer(),
                SerializationType.ProtoBuf => new ProtoBufSerializer(),
                SerializationType.GzipJson => new GzipSerializer(new JsonSerializer()),
                SerializationType.GzipXml => new GzipSerializer(new XmlSerializer()),
                SerializationType.GzipProtoBuf => new GzipSerializer(new ProtoBufSerializer()),
                _ => throw new ArgumentOutOfRangeException("SerializationType is invalid: " + serializationType)
            };
        }
    }
}