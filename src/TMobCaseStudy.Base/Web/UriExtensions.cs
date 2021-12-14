using System;

namespace TMobCaseStudy.Base.Web
{
    public static class UriExtensions
    {
        public static string AbsoluteWithoutPort(this Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);
            uriBuilder.Port = -1;
            return uriBuilder.Uri.AbsoluteUri;
        }
    }
}
