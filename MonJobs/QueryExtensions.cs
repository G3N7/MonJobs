using System.Collections.Generic;
using MongoDB.Driver;

namespace MonJobs
{
    internal static class QueryExtensions
    {
        public static FilterDefinition<Job> BuildFilters(this JobQuery query)
        {
            var filters = new List<FilterDefinition<Job>>();
            var builder = Builders<Job>.Filter;
            filters.Add(builder.Eq(x => x.QueueId, query.QueueId));

            foreach (var attribute in query.HasAttributes)
            {
                var hasAttribute = builder.Eq(x => x.Attributes[attribute.Key], attribute.Value);
                filters.Add(hasAttribute);
            }

            if (query.HasBeenAcknowledged.HasValue)
            {
                var hasBeenAcknowledged = builder.Not(builder.Eq(x => x.Acknowledgment, null));
                var hasNotBeenAcknowledged = builder.Eq(x => x.Acknowledgment, null);
                filters.Add(query.HasBeenAcknowledged.Value ? hasBeenAcknowledged : hasNotBeenAcknowledged);
            }

            if (query.HasResult.HasValue)
            {
                var hasFinished = builder.Not(builder.Eq(x => x.Result, null));
                var hasNoResults = builder.Eq(x => x.Result, null);
                filters.Add(query.HasResult.Value ? hasFinished : hasNoResults);
            }

            var matchesAllFilters = Builders<Job>.Filter.And(filters);
            return matchesAllFilters;
        }
        
    }
}