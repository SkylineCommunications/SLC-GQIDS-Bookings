namespace GQI_Get_Bookings_Count_1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Elasticsearch.Net;
    using Nest;
    using Newtonsoft.Json;

    public static class ElasticSearch
    {
        public static IEnumerable<Booking> GetBookings(string uri)
        {
            var connectionPool = new SingleNodeConnectionPool(new Uri(uri + ":9200"));
            var settings = new ConnectionSettings(connectionPool);
            var elasticClient = new ElasticClient(settings);

            var alias = "dms-active-creservationinstance";

            return ElasticSearch.GetAllDocumentsInAlias<Booking>(elasticClient, alias);
        }

        private static IEnumerable<T> GetAllDocumentsInAlias<T>(ElasticClient elasticClient, string alias, string scrollTimeout = "2m", int scrollSize = 1000) where T : class
        {
            var indices = elasticClient.GetIndicesPointingToAlias(alias).ToList();
            foreach (var index in indices)
            {
                var documents = ElasticSearch.GetAllDocumentsInIndex<object>(elasticClient, index, scrollTimeout, scrollSize);
                foreach (var document in documents)
                {
                    var obj = JsonConvert.DeserializeObject<T>(document.ToString());
                    yield return obj;
                }
            }
        }

        private static IEnumerable<T> GetAllDocumentsInIndex<T>(ElasticClient elasticClient, string indexName, string scrollTimeout, int scrollSize) where T : class
        {
            ISearchResponse<T> initialResponse = elasticClient.Search<T>(s =>
            s.Index(indexName)
                .From(0)
                .Take(scrollSize)
                .Query(q => q.MatchAll())
                .Scroll(scrollTimeout));

            if (!initialResponse.IsValid || string.IsNullOrEmpty(initialResponse.ScrollId))
                throw new IOException(initialResponse.ServerError.Error.Reason);

            foreach (var document in initialResponse.Documents)
            {
                yield return document;
            }

            string scrollid = initialResponse.ScrollId;
            bool isScrollSetHasData = true;
            while (isScrollSetHasData)
            {
                ISearchResponse<T> loopingResponse = elasticClient.Scroll<T>(scrollTimeout, scrollid);
                if (loopingResponse.IsValid)
                {
                    foreach (var document in loopingResponse.Documents)
                    {
                        yield return document;
                    }

                    scrollid = loopingResponse.ScrollId;
                }

                isScrollSetHasData = loopingResponse.Documents.Any();
            }

            elasticClient.ClearScroll(new ClearScrollRequest(scrollid));
        }
    }
}
