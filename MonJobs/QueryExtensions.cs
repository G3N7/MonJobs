using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace MonJobs
{
    internal static class QueryExtensions
    {
        public static FilterDefinition<Job> BuildFilters(this JobQuery query)
        {
            var filters = new List<FilterDefinition<Job>>();
            var builder = Builders<Job>.Filter;
            filters.Add(builder.Eq(x => x.QueueId, query.QueueId));

            if (query.JobIds != null && query.JobIds.Any())
            {
                filters.Add(builder.In(x => x.Id, query.JobIds));
            }

            foreach (var attribute in query.HasAttributes)
            {
                var valueType = attribute.Value.GetType();
                if (typeof(IEnumerable).IsAssignableFrom(valueType) && valueType != typeof(string))
                {
                    var values = (IEnumerable)attribute.Value;

                    var orValueFilters = new List<FilterDefinition<Job>>();
                    foreach (var value in values)
                    {
                        var valueToCompare = value;
                        var jToken = value as JToken;
                        if (jToken != null)
                        {
                            switch (jToken.Type)
                            {
                                case JTokenType.String:
                                    valueToCompare = jToken.ToObject<string>();
                                    break;
                                case JTokenType.Integer:
                                    valueToCompare = jToken.ToObject<int>();
                                    break;
                                case JTokenType.Float:
                                    valueToCompare = jToken.ToObject<float>();
                                    break;
                                case JTokenType.Date:
                                    valueToCompare = jToken.ToObject<DateTime>();
                                    break;
                                case JTokenType.Boolean:
                                    valueToCompare = jToken.ToObject<bool>();
                                    break;
                                case JTokenType.Guid:
                                    valueToCompare = jToken.ToObject<Guid>();
                                    break;
                                case JTokenType.Uri:
                                    valueToCompare = jToken.ToObject<Uri>();
                                    break;
                                case JTokenType.TimeSpan:
                                    valueToCompare = jToken.ToObject<TimeSpan>();
                                    break;
                                case JTokenType.Null:
                                case JTokenType.Undefined:
                                    valueToCompare = null;
                                    break;
                                default:
                                    throw new NotSupportedException($"Currently monjob only supports primitive types (String, Integer, Date, Boolean, Guid, Uri, and Timespan are supported)");
                            }
                        }
                        var hasAttribute = builder.Eq(x => x.Attributes[attribute.Key], valueToCompare);
                        orValueFilters.Add(hasAttribute);
                    }
                    var anyOfTheseValuesFilter = builder.Or(orValueFilters);
                    filters.Add(anyOfTheseValuesFilter);
                }
                else
                {
                    var hasAttribute = builder.Eq(x => x.Attributes[attribute.Key], attribute.Value);
                    filters.Add(hasAttribute);
                }
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