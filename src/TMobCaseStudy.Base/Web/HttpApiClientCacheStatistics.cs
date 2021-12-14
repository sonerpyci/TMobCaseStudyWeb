using System.Collections.Generic;
using static TMobCaseStudy.Base.Caching.LRUCache;

namespace TMobCaseStudy.Base.Web
{
    public class HttpApiClientCacheStatistics
    {
        public decimal CacheSizeInMegabytes { get; set; }

        public decimal? HitRatio { get; set; }

        public int TotalHits { get; set; }

        public int TotalMisses { get; set; }

        public int TotalEvicts { get; set; }

        public IDictionary<string, KeyStatistics> StatisticsPerKey { get; set; }
    }
}
