namespace TMobCaseStudy.Base.Web
{
    public sealed class MediaType
    {
        public static MediaType ApplicationJson = new MediaType("application/json");
        public static MediaType ApplicationBson = new MediaType("application/bson");
        public static MediaType ApplicationXml = new MediaType("application/xml");
        public static MediaType ApplicationProtoBuf = new MediaType("application/x-protobuf");
        public static MediaType TextXml = new MediaType("text/xml");

        private readonly string _mediaType;

        private MediaType(string mediaType)
        {
            _mediaType = mediaType;
        }

        public override string ToString()
        {
            return _mediaType;
        }
    }
}