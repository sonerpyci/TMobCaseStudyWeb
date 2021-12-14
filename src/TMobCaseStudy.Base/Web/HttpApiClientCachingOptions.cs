namespace TMobCaseStudy.Base.Web
{
    public class HttpApiClientCachingOptions
    {
        public HttpApiClientCachingOptions(long maxSizeInBytes)
        {
            MaxSizeInBytes = maxSizeInBytes;
        }

        public long MaxSizeInBytes { get; private set; }
        
        public override string ToString()
        {
            return string.Format("MaxSizeInBytes={0}", MaxSizeInBytes);
        }
    }
}
